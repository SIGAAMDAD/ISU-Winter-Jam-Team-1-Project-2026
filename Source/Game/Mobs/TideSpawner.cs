using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.ResourceCache;
using System;
using Systems.Caching;

namespace Game.Mobs {
	public partial class TideSpawner : Node2D {
		private float _spawnInterval = 30.5f;
		private WaveManager _waveManager;

		private readonly ICacheEntry<PackedScene, FilePath> _tidePrefab = SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Source/Game/Mobs/Wave.tscn" ) );
		private readonly Timer _spawnTimer = new Timer();

		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			var waveChanged = eventFactory.GetEvent<WaveChangedEventArgs>( "WaveChanged" );
			waveChanged.Subscribe( this, OnWaveChanged );

			_waveManager = GetNode<WaveManager>( "WaveManager" );

			_spawnTimer.Name = "SpawnTimer";
			_spawnTimer.WaitTime = _spawnInterval;
			_spawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSpawnTides ) );
			AddChild( _spawnTimer );
		}

		/*
		===============
		OnWaveChanged
		===============
		*/
		private void OnWaveChanged( in WaveChangedEventArgs args ) {
			_spawnInterval = Math.Min( 5.0f, _spawnInterval - 1.0f );

			_spawnTimer.Stop();
			_spawnTimer.WaitTime = _spawnInterval;
			_spawnTimer.Start();
		}

		/*
		===============
		OnSpawnTides
		===============
		*/
		private void OnSpawnTides() {
			int numTides = _waveManager.CurrentWave;

			for ( int i = 0; i < numTides; i++ ) {
				_tidePrefab.Get( out var scene );
				Tide tide = scene.Instantiate<Tide>();
				AddChild( tide );
			}
		}
	};
};