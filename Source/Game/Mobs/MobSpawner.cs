using Game.Common;
using Game.Player;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.Memory;
using Prefabs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Game.Mobs {
	/*
	===================================================================================

	MobSpawner

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class MobSpawner : Node2D {
		public const int FIRST_WAVE_ENEMY_COUNT = 5;
		public const int MAX_WAVE_ENEMIES = 1200;
		private const float MIN_MOB_DISTANCE = 40.0f;

		[Export]
		private CollisionShape2D _worldBounds;
		[Export]
		private PackedScene[] _enemyTypes;
		[Export]
		private MobTierDefinition[] _tierDefinitions;

		private int _waveNumber = 0;
		private int _batchCount = 0;

		private Vector2 _worldSize;

		private ILoggerCategory _category;
		private ILoggerService _logger;

		private NavigationRegion2D _navRegion;
		private SpatialPartition _spatialPartition;

		private MobWaveCalculator _waveCalculator;

		private BasicObjectPool<MobBase>[] _pools;

		private MobWaveCalculator.WaveSpawnData _currentWave;

		private readonly Timer _spawnTimer = new Timer();
		private readonly Dictionary<int, MobBase> _mobCache = new Dictionary<int, MobBase>( MAX_WAVE_ENEMIES );

		/*
		===============
		CreateMobOfType
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private MobBase CreateMobOfType( int type ) {
			var mob = _enemyTypes[ type ].Instantiate<MobBase>();

			_navRegion.AddChild( mob );
			mob.Disable();

			return mob;
		}

		/*
		===============
		TrySpawnSingleMob
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="enemyType"></param>
		/// <param name="mob"></param>
		/// <returns></returns>
		private bool TrySpawnSingleMob( int enemyType, out MobBase mob ) {
			mob = null;

			// Try multiple strategies
			if ( TrySpawnWithPoisson( out Vector2 position ) ) {
				mob = _pools[ enemyType ].Rent();
				mob.GlobalPosition = position;
				mob.Enable();

				// Register with spatial partition
				_spatialPartition.Add( mob, position );

				return true;
			}

			return false;
		}

		/*
		===============
		TrySpawnWithPoisson
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		private bool TrySpawnWithPoisson( out Vector2 position ) {
			position = Vector2.Zero;

			for ( int attempt = 0; attempt < 20; attempt++ ) {
				// Generate candidate position within world bounds
				Vector2 candidate = GetRandomPositionInBounds();

				// Check against spatial partition
				if ( _spatialPartition.IsPositionAvailable( candidate, MIN_MOB_DISTANCE ) ) {
					position = candidate;
					return true;
				}
			}

			return false;
		}

		/*
		===============
		GetRandomPositionInBounds
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private Vector2 GetRandomPositionInBounds() {
			float x = ( float )GD.RandRange( 0.0f, _worldSize.X );
			float y = ( float )GD.RandRange( 0.0f, _worldSize.Y );

			return new Vector2( x, y );
		}

		/*
		===============
		OnWaveStarted
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveStarted( in EmptyEventArgs args ) {
			_currentWave = _waveCalculator.GenerateWave( _waveNumber + 1 );
			_waveCalculator.PrintWaveData( _currentWave );

			_spawnTimer.Start();
		}

		/*
		===============
		SpawnWave
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void SpawnWave() {
			_waveCalculator.PrintWaveData( _currentWave );
			var batch = _currentWave.SpawnBatches[ _batchCount ];

			for ( int m = 0; m < batch.Mobs.Count; m++ ) {
				var mob = batch.Mobs[ m ];

				SpawnBatch( mob.Tier - 1, mob.Count );
			}

			if ( ++_batchCount >= _currentWave.SpawnBatches.Count ) {
				_spawnTimer.Stop();
			}
		}

		/*
		===============
		SpawnBatch
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="tier"></param>
		/// <param name="count"></param>
		private void SpawnBatch( int tier, int count ) {
			for ( int i = 0; i < count; i++ ) {
				if ( TrySpawnSingleMob( tier, out MobBase mob ) ) {
					_mobCache[ mob.GetPath().GetHashCode() ] = mob;
				}
			}
		}

		/*
		===============
		ClearMobs
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void ClearMobs() {
			_logger.PrintLine( in _category, $"Clearing mob cache..." );

			var children = _navRegion.GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[ i ] is MobBase mob ) {
					for ( int p = 0; p < _enemyTypes.Length; p++ ) {
						if ( _enemyTypes[ p ].ResourcePath == mob.SceneFilePath ) {
							_pools[ p ].Return( mob );
							_spatialPartition.Remove( mob );
							mob.Visible = false;
						}
					}
				}
			}
			_spawnTimer.Stop();
			_batchCount = 0;
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			ClearMobs();
			_waveNumber = args.NewWave;
		}

		/*
		===============
		OnPlayerDeath
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnPlayerDeath( in EmptyEventArgs args) {
			ClearMobs();
		}

		/*
		===============
		OnArenaSizeChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnArenaSizeChanged( in ArenaSizeChangedEventArgs args ) {
			_worldSize = args.Size;
		}

		/*
		===============
		OnMobDie
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnMobDie( in MobDieEventArgs args ) {
			MobBase mob = _mobCache[ args.MobId ];
			_spatialPartition.Remove( mob );
			_mobCache.Remove( args.MobId );
			mob.Disable();
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready() {
			base._Ready();

			var serviceLocator = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator;

			_logger = serviceLocator.GetService<ILoggerService>();
			_category = _logger.CreateCategory( nameof( MobSpawner ), LogLevel.Info, true );

			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();

			var arenaSizeChanged = eventFactory.GetEvent<ArenaSizeChangedEventArgs>( nameof( WorldArea ), nameof( WorldArea.ArenaSizeChanged ) );
			arenaSizeChanged.Subscribe( this, OnArenaSizeChanged );

			var mobDie = eventFactory.GetEvent<MobDieEventArgs>( nameof( MobBase ), nameof( MobBase.MobDie ) );
			mobDie.Subscribe( this, OnMobDie );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			var playerDie = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.PlayerDeath ) );
			playerDie.Subscribe( this, OnPlayerDeath );

			_navRegion = GetNode<NavigationRegion2D>( "NavigationRegion2D" );

			if ( _worldBounds.Shape is RectangleShape2D rectangleShape ) {
				_worldSize = rectangleShape.Size;
			} else {
				throw new InvalidCastException();
			}

			_waveCalculator = new MobWaveCalculator( _tierDefinitions );

			_spawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( SpawnWave ) );
			AddChild( _spawnTimer );

			_spatialPartition = new SpatialPartition( _worldSize, MIN_MOB_DISTANCE, eventFactory );

			_pools = new BasicObjectPool<MobBase>[ _enemyTypes.Length ];
			for ( int i = 0; i < _enemyTypes.Length; i++ ) {
				_pools[ i ] = new BasicObjectPool<MobBase>(
					() => CreateMobOfType( i ),
					128, MAX_WAVE_ENEMIES
				);
			}
		}
	};
};
