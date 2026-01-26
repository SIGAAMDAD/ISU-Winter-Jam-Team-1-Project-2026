using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Memory;
using Prefabs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Game.Mobs {
	/*
	===================================================================================

	TideSpawner

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class TideSpawner : Node2D {
		public const int MAX_WAVE_EFFECTS = 100;

		[Export]
		private CollisionShape2D _worldBounds;
		[Export]
		private PackedScene[] _environmentalEffects;

		private int _waveNumber;
		private int _effectCount = 2;
		private int _spawnMinX;
		private int _spawnMinY;
		private int _spawnMaxX;
		private int _spawnMaxY;

		private Timer _spawnTimer;

		private BasicObjectPool<EffectBase>[] _effectPools;
		private readonly ConcurrentDictionary<int, EffectBase> _effectCache = new( MAX_WAVE_EFFECTS, MAX_WAVE_EFFECTS );

		/*
		===============
		OnSpawnTides
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnSpawnEnvironmentalEffects() {
			int effectTier = 1;
			if ( _waveNumber >= 3 ) {
				effectTier++;
			}
			if ( _waveNumber >= 7 ) {
				effectTier++;
			}
			if ( _waveNumber >= 15 ) {
				effectTier++;
			}

			for ( int t = 0; t < effectTier; t++ ) {
				int count = _effectCount / effectTier;

				for ( int i = 0; i < count; i++ ) {
					EffectBase effect = _effectPools[ i ].Rent();

					Vector2 spawnPoint = new Vector2(
						Random.Shared.Next( _spawnMinX, _spawnMaxX ),
						Random.Shared.Next( _spawnMinY, _spawnMaxY )
					);
					effect.GlobalPosition = spawnPoint;
					effect.Enable();
					_effectCache[ effect.EffectId ] = effect;
				}
			}
		}

		/*
		===============
		CreateEffect
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="effectType"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private EffectBase CreateEffect( int effectType ) {
			var effect = _environmentalEffects[ effectType ].Instantiate<EffectBase>();

			AddChild( effect );
			effect.Disable();

			return effect;
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
			_spawnTimer.Start();
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
			_waveNumber = args.NewWave;
			_effectCount = (int)( _waveNumber * 0.0625f + _effectCount );

			var children = GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[ i ] is EffectBase effect ) {
					for ( int p = 0; p < _environmentalEffects.Length; p++ ) {
						if ( _environmentalEffects[ p ].ResourcePath == effect.SceneFilePath ) {
							_effectPools[ p ].Return( effect );
							_effectCache.Remove( effect.EffectId, out _ );
							effect.Disable();
						}
					}
				}
			}

			_spawnTimer.Stop();
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
			_spawnMaxX = (int)args.Size.X;
			_spawnMaxY = (int)args.Size.Y;
		}

		/*
		===============
		OnEffectFinished
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnEffectFinished( in int args ) {
			var effect = _effectCache[ args ];
			for ( int p = 0; p < _environmentalEffects.Length; p++ ) {
				if ( _environmentalEffects[ p ].ResourcePath == effect.SceneFilePath ) {
					_effectPools[ p ].Return( effect );
					_effectCache.Remove( effect.EffectId, out _ );
					effect.Disable();
				}
			}
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

			_effectPools = new BasicObjectPool<EffectBase>[ _environmentalEffects.Length ];
			for ( int i = 0; i < _effectPools.Length; i++ ) {
				_effectPools[ i ] = new BasicObjectPool<EffectBase>(
					() => CreateEffect( i )
				);
			}

			if ( _worldBounds.Shape is RectangleShape2D shape ) {
				float sizeX = shape.Size.X * 0.5f;
				float sizeY = shape.Size.Y * 0.5f;
				Vector2 position = _worldBounds.GlobalPosition;

				_spawnMinX = (int)( position.X - sizeX );
				_spawnMinY = (int)( position.Y - sizeY );
				_spawnMaxX = (int)( position.X + sizeX );
				_spawnMaxY = (int)( position.Y + sizeY );
			} else {
				throw new InvalidOperationException( "WorldBounds contain a RectangleShape2D!" );
			}

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			var arenaSizeChanged = eventFactory.GetEvent<ArenaSizeChangedEventArgs>( nameof( WorldArea ), nameof( WorldArea.ArenaSizeChanged ) );
			arenaSizeChanged.Subscribe( this, OnArenaSizeChanged );

			var effectFinished = eventFactory.GetEvent<int>( nameof( EffectBase ), nameof( EffectBase.EffectFinished ) );
			effectFinished.Subscribe( this, OnEffectFinished );

			_spawnTimer = GetNode<Timer>( "SpawnTimer" );
			_spawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSpawnEnvironmentalEffects ) );
		}
	};
};
