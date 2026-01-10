using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.Memory;
using Prefabs;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
		private const float MIN_MOB_DISTANCE = 80.0f;

		[Export]
		private CollisionShape2D _worldBounds;
		[Export]
		private PackedScene[] _enemyTypes;

		private int _maxEnemies = FIRST_WAVE_ENEMY_COUNT;
		private int _waveNumber = 0;
		private int _enemyCount = 0;

		private Vector2 _worldSize;

		private ILoggerCategory _category;
		private ILoggerService _logger;

		private Timer _spawnTimer;
		private NavigationRegion2D _navRegion;
		private SpatialPartition _spatialPartition;

		private BasicObjectPool<MobBase>[] _pools;
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
		OnSpawnEnemies
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSpawnEnemies() {
			if ( _enemyCount > _maxEnemies ) {
				return;
			}

			int enemyCount = _waveNumber > 0 ? _maxEnemies / _waveNumber : _maxEnemies;
			int mobTier = 1;
			if ( _waveNumber >= 3 ) {
				mobTier++;
			}
			if ( _waveNumber >= 7 ) {
				mobTier++;
			}
			if ( _waveNumber >= 15 ) {
				mobTier++;
			}

			_logger.PrintLine( in _category, $"Spawning {enemyCount} enemies in wave {_waveNumber}" );

			for ( int t = 0; t < mobTier; t++ ) {
				int numEnemies = enemyCount / ( t + 1 );

				for ( int i = 0; i < numEnemies; i++ ) {
					if ( TrySpawnSingleMob( t, out MobBase mob ) ) {
						_mobCache[ mob.GetPath().GetHashCode() ] = mob;
						_enemyCount++;
					}
				}
			}
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
			float x = GD.Randf() * _worldSize.X + _worldBounds.Position.X;
			float y = GD.Randf() * _worldSize.Y + _worldBounds.Position.Y;

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
			_enemyCount = 0;
			_spawnTimer.Stop();

			_waveNumber = args.NewWave;
			_maxEnemies += (int)( _waveNumber * 0.0625f + _maxEnemies );
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

			_enemyCount--;
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

			var arenaSizeChanged = eventFactory.GetEvent<ArenaSizeChangedEventArgs>( nameof( WorldArea.ArenaSizeChanged ) );
			arenaSizeChanged.Subscribe( this, OnArenaSizeChanged );

			var mobDie = eventFactory.GetEvent<MobDieEventArgs>( nameof( MobBase.MobDie ) );
			mobDie.Subscribe( this, OnMobDie );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_spawnTimer = GetNode<Timer>( "SpawnInterval" );
			_spawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSpawnEnemies ) );

			_navRegion = GetNode<NavigationRegion2D>( "NavigationRegion2D" );

			_spatialPartition = new SpatialPartition( new Vector2( 1280, 720 ), MIN_MOB_DISTANCE, eventFactory );

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