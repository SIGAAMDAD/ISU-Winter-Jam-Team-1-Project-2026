using Game.Mobs;
using Game.Player.Weapons;
using Godot;
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
			
			CallDeferred( MethodName.SetPhysicsProcess, false );

			var freezeArea = GetNode<Area2D>( "FreezeArea" );
			Godot.Collections.Array<Area2D> bodies = freezeArea.GetOverlappingAreas();

			_victims.EnsureCapacity( bodies.Count );
			for ( int i = 0; i < bodies.Count; i++ ) {
				if ( bodies[ i ] is MobBase enemy ) {
					_victims.Add( enemy );
					enemy.SetSpeed( Vector2.Zero );
					enemy.CreateTween().CallDeferred( Tween.MethodName.TweenProperty, enemy, ModulateNodePath, Colors.LightBlue, 1.0f );
				}
			}

			_killTimer.CallDeferred( Timer.MethodName.Start );
			_processTimer.CallDeferred( Timer.MethodName.Start );
			
			SetDeferred( PropertyName.Visible, false );
		}

		/*
		===============
		OnReleaseVictims
		===============
		*/
		private void OnReleaseVictims() {
			for ( int i = 0; i < _victims.Count; i++ ) {
				var enemy = _victims[ i ];
				if ( !IsInstanceValid( enemy ) ) {
					continue;
				}

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
				var victim = _victims[ i ];
				if ( !IsInstanceValid( victim ) ) {
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