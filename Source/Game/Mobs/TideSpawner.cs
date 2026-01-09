using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

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
		[Export]
		private PackedScene[] _environmentalEffects;

		private int _waveNumber;
		private int _effectCount = 2;
		private int _spawnMinX;
		private int _spawnMinY;
		private int _spawnMaxX;
		private int _spawnMaxY;

		private Timer _spawnTimer;

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
				PackedScene scene = _environmentalEffects[ t ];
				for ( int i = 0; i < count; i++ ) {
					EffectBase effect = scene.Instantiate<EffectBase>();
					AddChild( effect );

					Vector2 spawnPoint = new Vector2(
						Random.Shared.Next( _spawnMinX, _spawnMaxX ),
						Random.Shared.Next( _spawnMinY, _spawnMaxY )
					);
					effect.GlobalPosition = spawnPoint;
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
			_waveNumber = args.NewWave;
			_effectCount = (int)( _waveNumber * 0.0625f + _effectCount );

			var children = GetChildren();
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[ i ] is Tide tide ) {
					tide.QueueFree();
				}
			}

			_spawnTimer.Stop();
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
				_spawnMaxY = (int)position.Y;
			} else {
				throw new InvalidOperationException( "WorldBounds contain a RectangleShape2D!" );
			}

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_spawnTimer = GetNode<Timer>( "SpawnTimer" );
			_spawnTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSpawnEnvironmentalEffects ) );
		}
	};
};