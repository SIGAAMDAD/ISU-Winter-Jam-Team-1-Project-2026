using System;
using System.Collections.Generic;
using Game.Common;
using Godot;

namespace Game.Mobs {
	/*
	===================================================================================

	MobWaveCalculator

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class MobWaveCalculator {
		private const int BASE_MAX_CONCURRENT = 5;
		private const float MAX_CONCURRENT_GROWTH_RATE = 0.25f;

		public class WaveSpawnData {
			public int WaveNumber;
			public int MaxConcurrent;
			public List<SpawnBatch> SpawnBatches = new();
			public Dictionary<int, int> TierDistribution = new();
		};

		public class SpawnBatch {
			public float Delay;
			public List<MobSpawn> Mobs = new();
		};

		public class MobSpawn {
			public int Tier;
			public int Count;
		};

		private readonly MobTierDefinition[] _tierDefinitions;

		/*
		===============
		MobWaveCalculator
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="definitions"></param>
		public MobWaveCalculator( MobTierDefinition[] definitions ) {
			_tierDefinitions = definitions;
		}

		/*
		===============
		GenerateAllWaves
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public WaveSpawnData[] GenerateAllWaves() {
			var waves = new WaveSpawnData[ WaveManager.MAX_WAVES ];

			for ( int wave = 1; wave <= WaveManager.MAX_WAVES; wave++ ) {
				waves[ wave - 1 ] = GenerateWave( wave );
			}

			return waves;
		}

		/*
		===============
		GenerateWave
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="waveNumber"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public WaveSpawnData GenerateWave( int waveNumber ) {
			if ( waveNumber < 1 || waveNumber > WaveManager.MAX_WAVES ) {
				throw new ArgumentOutOfRangeException( nameof( waveNumber ) );
			}

			var waveData = new WaveSpawnData {
				WaveNumber = waveNumber,
				MaxConcurrent = CalculateMaxConcurrent( waveNumber )
			};

			// Calculate total mobs for this wave (increases with wave number)
			int totalMobs = CalculateTotalMobs( waveNumber );

			// Calculate distribution across tiers
			waveData.TierDistribution = CalculateTierDistribution( waveNumber, totalMobs );

			// Create spawn batches based on distribution and concurrency limits
			waveData.SpawnBatches = CreateSpawnBatches( waveNumber, waveData.TierDistribution, waveData.MaxConcurrent );

			return waveData;
		}

		/// <summary>
		/// Debug/visualization method to print wave data
		/// </summary>
		public void PrintWaveData( WaveSpawnData waveData ) {
			GD.Print( $"=== Wave {waveData.WaveNumber} ===" );
			GD.Print( $"Max Concurrent: {waveData.MaxConcurrent}" );
			GD.Print( "Tier Distribution:" );

			foreach ( var kvp in waveData.TierDistribution ) {
				if ( kvp.Value > 0 )
					GD.Print( $"  Tier {kvp.Key}: {kvp.Value} mobs" );
			}

			GD.Print( $"Spawn Batches: {waveData.SpawnBatches.Count}" );

			for ( int i = 0; i < waveData.SpawnBatches.Count; i++ ) {
				var batch = waveData.SpawnBatches[ i ];
				GD.Print( $"  Batch {i + 1} (Delay: {batch.Delay:F1}s):" );

				foreach ( var mob in batch.Mobs ) {
					GD.Print( $"    Tier {mob.Tier} x{mob.Count}" );
				}
			}

			GD.Print();
		}

		/*
		===============
		CalculateTierDistribution
		===============
		*/
		/// <summary>
		/// Distribute total mobs across tiers
		/// </summary>
		private Dictionary<int, int> CalculateTierDistribution( int waveNumber, int totalMobs ) {
			var distribution = new Dictionary<int, int>();

			// Calculate weights for each tier based on wave progression
			float[] tierWeights = new float[ _tierDefinitions.Length ];
			float totalWeight = 0f;

			for ( int tier = 0; tier < _tierDefinitions.Length; tier++ ) {
				var definition = _tierDefinitions[ tier ];

				if ( waveNumber < definition.MinWaveToAppear ) {
					tierWeights[ tier ] = 0;
					continue;
				}

				// Calculate weight based on:
				// 1. Base weight (higher for lower tiers)
				// 2. Wave progression factor (increases with waves past appearance)
				// 3. Growth multiplier (how fast this tier scales)

				int wavesSinceAppearance = waveNumber - definition.MinWaveToAppear + 1;
				float progressionFactor = 1 + (wavesSinceAppearance * 0.2f); // 10% per wave past appearance

				tierWeights[ tier ] = definition.BaseWeight * progressionFactor * definition.GrowthMultiplier;
				totalWeight += tierWeights[ tier ];
			}

			// If no tiers are available (shouldn't happen), default to tier 1
			if ( totalWeight <= 0 ) {
				distribution[ 1 ] = totalMobs;
				return distribution;
			}

			// Distribute mobs based on weights
			int remainingMobs = totalMobs;

			for ( int tier = 0; tier < _tierDefinitions.Length; tier++ ) {
				if ( tierWeights[ tier ] <= 0 ) {
					distribution[ tier + 1 ] = 0;
					continue;
				}

				float proportion = tierWeights[ tier ] / totalWeight;
				int tierCount = ( int )(totalMobs * proportion);

				// Ensure at least 1 mob if this tier should appear
				if ( tierCount == 0 && tierWeights[ tier ] > 0 )
					tierCount = 1;

				// Cap by tier's max in concurrent (spread across batches)
				int maxPerWave = _tierDefinitions[ tier ].MaxInConcurrent * 3; // Allow up to 3 batches worth
				tierCount = Math.Min( tierCount, maxPerWave );

				distribution[ tier + 1 ] = tierCount;
				remainingMobs -= tierCount;
			}

			// Distribute any remaining mobs to highest available tier
			if ( remainingMobs > 0 ) {
				for ( int tier = _tierDefinitions.Length - 1; tier >= 0; tier-- ) {
					if ( distribution[ tier + 1 ] > 0 ) {
						distribution[ tier + 1 ] += remainingMobs;
						break;
					}
				}
			}

			// Apply wave-based scaling adjustments
			AdjustDistributionForWave( waveNumber, distribution );

			return distribution;
		}

		/// <summary>
		/// Create spawn batches considering concurrency limits
		/// </summary>
		private List<SpawnBatch> CreateSpawnBatches( int waveNumber, Dictionary<int, int> distribution, int maxConcurrent ) {
			var batches = new List<SpawnBatch>();

			// Determine number of batches based on wave intensity
			int batchCount = CalculateBatchCount( waveNumber );
			float waveDuration = CalculateWaveDuration( waveNumber );

			// Group mobs by tier and create initial batch assignments
			var tierGroups = new List<List<MobSpawn>>();

			foreach ( var kvp in distribution ) {
				if ( kvp.Value == 0 ) {
					continue;
				}

				int tier = kvp.Key;
				int count = kvp.Value;
				var definition = _tierDefinitions[ tier - 1 ];

				// Split tier mobs across multiple mini-groups for better pacing
				int mobsPerGroup = Math.Min( definition.MaxInConcurrent, count );
				int groups = ( int )Math.Ceiling( count / ( float )mobsPerGroup );

				for ( int g = 0; g < groups; g++ ) {
					int groupCount = Math.Min( mobsPerGroup, count - (g * mobsPerGroup) );
					if ( groupCount > 0 ) {
						tierGroups.Add( new List<MobSpawn> { new MobSpawn { Tier = tier, Count = groupCount } } );
					}
				}
			}

			// Distribute groups across batches
			float batchInterval = waveDuration / batchCount;

			for ( int batchIndex = 0; batchIndex < batchCount; batchIndex++ ) {
				var batch = new SpawnBatch {
					Delay = batchIndex * batchInterval
				};

				// Calculate how many mobs can be in this batch (considering concurrency)
				int batchCapacity = maxConcurrent;

				// Add tier groups to this batch
				int groupsPerBatch = ( int )Math.Ceiling( tierGroups.Count / ( float )batchCount );
				int startIdx = batchIndex * groupsPerBatch;
				int endIdx = Math.Min( startIdx + groupsPerBatch, tierGroups.Count );

				for ( int i = startIdx; i < endIdx && batchCapacity > 0; i++ ) {
					var group = tierGroups[ i ];
					foreach ( var mob in group ) {
						if ( mob.Count <= batchCapacity ) {
							batch.Mobs.Add( mob );
							batchCapacity -= mob.Count;
						} else {
							// Split mob group if too large for batch
							batch.Mobs.Add( new MobSpawn { Tier = mob.Tier, Count = batchCapacity } );
							// Remaining mobs will be added to overflow (next iteration)
							mob.Count -= batchCapacity;
							batchCapacity = 0;
						}
					}
				}

				if ( batch.Mobs.Count > 0 ) {
					batches.Add( batch );
				}
			}

			// Add a final "challenge" batch for the last 20% of waves
			if ( waveNumber >= 16 ) {
				batches.Add( CreateChallengeBatch( waveNumber, distribution, maxConcurrent ) );
			}

			return batches;
		}

		/*
		===============
		CalculateBatchCount
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="waveNumber"></param>
		/// <returns></returns>
		private int CalculateBatchCount( int waveNumber ) {
			// More batches in later waves for better pacing
			return Math.Min( 8, 2 + (waveNumber / 3) );
		}

		/*
		===============
		CalculateWaveDuration
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="waveNumber"></param>
		/// <returns></returns>
		private float CalculateWaveDuration( int waveNumber ) {
			// Longer waves as game progresses
			return 10.0f + ( waveNumber * 4.0f );
		}

		/// <summary>
		/// Additional wave-based adjustments to distribution
		/// </summary>
		private void AdjustDistributionForWave( int waveNumber, Dictionary<int, int> distribution ) {
			// Special wave adjustments (boss waves, difficulty spikes)
			if ( waveNumber % 5 == 0 ) // Every 5 waves
			{
				// Boost higher tiers on milestone waves
				for ( int tier = 4; tier <= 6; tier++ ) {
					if ( distribution.ContainsKey( tier ) && distribution[ tier ] > 0 ) {
						distribution[ tier ] = ( int )(distribution[ tier ] * 1.5f);
					}
				}
			}

			// Ensure tier progression feels meaningful
			if ( waveNumber >= 15 ) {
				// Reduce lower tier presence in late game
				for ( int tier = 1; tier <= 2; tier++ ) {
					if ( distribution.TryGetValue( tier, out int value ) ) {
						distribution[ tier ] = ( int )(value * 0.5f);
					}
				}
			}
		}

		/// <summary>
		/// Calculate max concurrent enemies for this wave
		/// </summary>
		private int CalculateMaxConcurrent( int waveNumber ) {
			// Exponential growth with diminishing returns
			float growthFactor = 1 + (MAX_CONCURRENT_GROWTH_RATE * (waveNumber - 1));
			int maxConcurrent = ( int )(BASE_MAX_CONCURRENT * growthFactor);

			// Minimum increase of 1 per wave, cap at reasonable number
			maxConcurrent = Math.Max( BASE_MAX_CONCURRENT + waveNumber - 1, maxConcurrent );
			maxConcurrent = Math.Min( maxConcurrent, 50 ); // Cap at 50

			return maxConcurrent;
		}

		/// <summary>
		/// Calculate total mobs for this wave
		/// </summary>
		private int CalculateTotalMobs( int waveNumber ) {
			// Base formula: total mobs = wave^1.5 + 5
			// This creates accelerating but manageable growth
			double wavePower = Math.Pow( waveNumber, 1.5 );
			int totalMobs = ( int )wavePower + 5;

			// Add wave-based scaling
			totalMobs += ( int )(waveNumber * 1.2f);

			// Ensure minimum growth
			totalMobs = Math.Max( totalMobs, waveNumber * 2 );

			return totalMobs;
		}

		private SpawnBatch CreateChallengeBatch( int waveNumber, Dictionary<int, int> distribution, int maxConcurrent ) {
			var batch = new SpawnBatch {
				Delay = CalculateWaveDuration( waveNumber ) * 0.8f // Last 20% of wave
			};

			// Create a challenging mix of higher tier mobs
			int challengeMobs = Math.Min( maxConcurrent / 2, 8 );

			for ( int tier = _tierDefinitions.Length; tier >= Math.Max( 1, _tierDefinitions.Length - 2 ); tier-- ) {
				if ( distribution.ContainsKey( tier ) && distribution[ tier ] > 0 && challengeMobs > 0 ) {
					int count = Math.Min( 3, challengeMobs );
					batch.Mobs.Add( new MobSpawn { Tier = tier, Count = count } );
					challengeMobs -= count;
				}
			}

			return batch;
		}
	};
};
