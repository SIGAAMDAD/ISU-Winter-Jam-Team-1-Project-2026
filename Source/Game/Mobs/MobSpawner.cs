using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

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
		[Export]
		private EntityManager _entityManager;
		[Export]
		private PackedScene[] _enemyTypes;

		private const int FIRST_WAVE_ENEMY_COUNT = 3;

		private int _enemyCount = FIRST_WAVE_ENEMY_COUNT;

		public IGameEvent<EmptyEventArgs> MobsCleared => _mobsCleared;
		private IGameEvent<EmptyEventArgs> _mobsCleared;

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

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			var _waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			_waveCompleted.Subscribe( this, OnWaveCompleted );

			var _waveStarted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveStarted ) );
			_waveStarted.Subscribe( this, OnWaveStarted );

			_mobsCleared = eventFactory.GetEvent<EmptyEventArgs>( nameof( MobsCleared ) );
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
		private void OnWaveStarted( in WaveChangedEventArgs args ) {
			// only spawn a beefcake every 5 waves
			bool shouldSpawnBoss = args.NewWave % 5 == 0;

			_enemyCount += (int)( args.NewWave * 0.0625f * _enemyCount );
			GD.Print( $"Spawning {_enemyCount} enemies..." );

			for ( int i = 0; i < _enemyCount; i++ ) {
				MobBase mob = _enemyTypes[ Random.Shared.Next( 0, _enemyTypes.Length - 1 ) ].Instantiate<MobBase>();
				mob.Die.Subscribe( this, OnMobDie );
				_entityManager.RegisterEntity( mob );
			}
		}

		/*
		===============
		OnMobDie
		===============
		*/
		private void OnMobDie( in MobDieEventArgs args ) {
			if ( _enemyCount-- <= 0 ) {
				_mobsCleared.Publish( new EmptyEventArgs() );
			}
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
		}
	};
};