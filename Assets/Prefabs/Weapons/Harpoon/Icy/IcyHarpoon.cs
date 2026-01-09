using Game.Mobs;
using Game.Player.Weapons;
using Godot;
using System.Collections.Generic;
using System.ComponentModel;

namespace Prefabs {
	/*
	===================================================================================
	
	IcyHarpoon
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class IcyHarpoon : Projectile {
		private static readonly NodePath @ModulateNodePath = "modulate";

		private const float ICE_DAMAGE = 2.0f;

		private readonly List<MobBase> _victims = new List<MobBase>();

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

			SetProcess( false );

			var freezeArea = GetNode<Area2D>( "FreezeArea" );
			Godot.Collections.Array<Area2D> bodies = freezeArea.GetOverlappingAreas();

			_victims.EnsureCapacity( bodies.Count );
			for ( int i = 0; i < bodies.Count; i++ ) {
				if ( bodies[ i ] is MobBase enemy ) {
					_victims.Add( enemy );
					enemy.SetSpeed( Vector2.Zero );

					var tween = enemy.CreateTween();
					tween.TweenProperty( enemy, ModulateNodePath, Colors.LightBlue, 1.0f );
				}
			}

			_killTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnReleaseVictims ) );
			AddChild( _killTimer );
			_killTimer.Start();

			_processTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnDamageVictims ) );
			AddChild( _processTimer );
			_processTimer.Start();
		}

		/*
		===============
		OnReleaseVictims
		===============
		*/
		private void OnReleaseVictims() {
			for ( int i = 0; i < _victims.Count; i++ ) {
				var enemy = _victims[ i ];				

				enemy.Modulate = Colors.White;
				enemy.ResetSpeed();
			}
			_victims.Clear();

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
			for ( int i = 0; i < _victims.Count; i++ ) {
				if ( _victims[ i ] is MobBase enemy ) {
					enemy.Damage( ICE_DAMAGE );
				}
			}
		}
	};
};