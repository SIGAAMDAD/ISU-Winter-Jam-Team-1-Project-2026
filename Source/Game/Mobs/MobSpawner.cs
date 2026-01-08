using Game.Common;
using Game.Systems;
using Game.Systems.Caching;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Logger;
using Nomad.Core.Util;
using System;
using System.Collections.Generic;
using Systems.Caching;

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
		private const int FIRST_WAVE_ENEMY_COUNT = 5;

		[Export]
		private CollisionShape2D _worldBounds;
		[Export]
		private PackedScene[] _enemyTypes;

		private int _waveNumber = 0;

		private int _spawnMinX;
		private int _spawnMinY;
		private int _spawnMaxX;
		private int _spawnMaxY;
		private int _enemyCount = FIRST_WAVE_ENEMY_COUNT;

		private ILoggerCategory _category;
		private ILoggerService _logger;

		private Timer _spawnTimer;
		private NavigationRegion2D _navRegion;

		/*
		===============
		OnSpawnEnemies
		===============
		*/
		private void OnSpawnEnemies() {
			SpawnBatch( _waveNumber );
		}

		/*
		===============
		SpawnBatch
		===============
		*/
		private void SpawnBatch( int waveNumber ) {
			int mobTier = 1;
			if ( waveNumber >= 3 ) {
				mobTier++;
			}
			if ( waveNumber >= 7 ) {
				mobTier++;
			}
			if ( waveNumber >= 15 ) {
				mobTier++;
			}

			_enemyCount += (int)( waveNumber * 0.0625f * _enemyCount );
			_logger.PrintLine( in _category, $"Spawning {_enemyCount} enemies in wave {waveNumber}..."  );

			for ( int t = 0; t < mobTier; t++ ) {
				// NOTE: this might need adjusting...
				int count = _enemyCount / mobTier;
				PackedScene mob = _enemyTypes[ t ];
				for ( int i = 0; i < count; i++ ) {
					MobBase node = mob.Instantiate<MobBase>();
					node.GlobalPosition = new Vector2(
						Random.Shared.Next( _spawnMinX, _spawnMaxX ),
						Random.Shared.Next( _spawnMinY, _spawnMinY )
					);
					_navRegion.AddChild( node );
				}
			}
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

			Godot.Collections.Array<Node> children = _navRegion.GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				_navRegion.RemoveChild( children[ i ] );
				children[ i ].QueueFree();
			}
			_spawnTimer.Stop();

			_waveNumber = args.NewWave;
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

			var serviceLocator = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator;

			_logger = serviceLocator.GetService<ILoggerService>();
			_category = _logger.CreateCategory( nameof( MobSpawner ), LogLevel.Info, true );

			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_spawnTimer = GetNode<Timer>( "SpawnInterval" );
			_spawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSpawnEnemies ) );

			_navRegion = GetNode<NavigationRegion2D>( "NavigationRegion2D" );
		}
	};
};