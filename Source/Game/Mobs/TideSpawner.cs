using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.ResourceCache;
using System;
using Systems.Caching;

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
		[Export]
		private CollisionShape2D _worldBounds;

		private int _spawnMinX;
		private int _spawnMinY;
		private int _spawnMaxX;
		private int _spawnMaxY;

		private Timer _spawnTimer;

		private readonly ICacheEntry<PackedScene, FilePath> _tidePrefab = SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Source/Game/Mobs/Wave.tscn" ) );

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
				_spawnMaxY = (int)position.Y;
			} else {
				throw new InvalidOperationException( "WorldBounds contain a RectangleShape2D!" );
			}

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_spawnTimer = GetNode<Timer>( "SpawnTimer" );
			_spawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSpawnTide ) );
		}

		/*
		===============
		OnSpawnTides
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnSpawnTide() {
			_tidePrefab.Get( out var scene );
			Tide tide = scene.Instantiate<Tide>();
			AddChild( tide );

			Vector2 spawnPoint = new Vector2(
				Random.Shared.Next( _spawnMinX, _spawnMaxX ),
				Random.Shared.Next( _spawnMinY, _spawnMaxY )
			);
			tide.GlobalPosition = spawnPoint;
		}

		/*
		===============
		OnWaveStarted
		===============
		*/
		private void OnWaveStarted( in WaveChangedEventArgs args ) {
			_spawnTimer.Start();
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			Godot.Collections.Array<Node> children = GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[ i ] is Tide tide ) {
					tide.QueueFree();
				}
			}

			_spawnTimer.Stop();
		}
	};
};