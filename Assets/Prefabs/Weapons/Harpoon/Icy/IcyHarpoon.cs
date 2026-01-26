using Game.Mobs;
using Game.Player;
using Game.Player.Weapons;
using Game.Systems.Caching;
using Godot;
using Nomad.Core.Util;
using System.Collections.Generic;

namespace Prefabs {
	/*
	===================================================================================

	IcyHarpoon

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class IcyHarpoon : Projectile {
		private static readonly NodePath @ModulateNodePath = "modulate";

		private const float ICE_DAMAGE = 2.0f;

		private MobBase[] _victims;

		private readonly Timer _killTimer = new Timer() {
			WaitTime = 2.0f,
			OneShot = true
		};
		private readonly Timer _processTimer = new Timer() {
			WaitTime = 1.0f,
			OneShot = false
		};

		/*
		===============
		OnEnemyHit
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="mob"></param>
		protected override void OnEnemyHit( MobBase mob ) {
			base.OnEnemyHit( mob );

			CallDeferred( ProjectileNode.MethodName.SetPhysicsProcess, false );

			var freezeArea = GetNode<Area2D>( "FreezeArea" );
			Godot.Collections.Array<Area2D> bodies = freezeArea.GetOverlappingAreas();

			_victims = new MobBase[ bodies.Count ];
			for ( int i = 0; i < bodies.Count; i++ ) {
				if ( bodies[ i ] is MobBase enemy ) {
					_victims[ i ] = enemy;
					enemy.SetSpeed( Vector2.Zero );
					enemy.CreateTween().CallDeferred( Tween.MethodName.TweenProperty, enemy, ModulateNodePath, Colors.LightBlue, 1.0f );
				}
			}

			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/ice_harpoon_freeze.wav" ) ).Get( out var freezeSound );
			var audioStream = new AudioStreamPlayer2D() {
				Stream = freezeSound
			};
			AddChild( audioStream );
			audioStream.Play();

			_killTimer.CallDeferred( Timer.MethodName.Start );
			_processTimer.CallDeferred( Timer.MethodName.Start );

			SetDeferred( ProjectileNode.PropertyName.Visible, false );
		}

		/*
		===============
		OnReleaseVictims
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnReleaseVictims() {
			for ( int i = 0; i < _victims.Length; i++ ) {
				var enemy = _victims[ i ];
				if ( !ProjectileNode.IsInstanceValid( enemy ) ) {
					continue;
				}

				enemy.Modulate = Colors.White;
				enemy.ResetSpeed();
			}
			_processTimer.Stop();

			QueueFree();
		}

		/*
		===============
		OnDamageVictims
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnDamageVictims() {
			for ( int i = 0; i < _victims.Length; i++ ) {
				var victim = _victims[ i ];
				if ( !ProjectileNode.IsInstanceValid( victim ) ) {
					continue;
				}
				if ( victim is MobBase enemy ) {
					enemy.Damage( ICE_DAMAGE );
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

			_processTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnDamageVictims ) );
			AddChild( _processTimer );

			_killTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReleaseVictims ) );
			AddChild( _killTimer );
		}
	};
};
