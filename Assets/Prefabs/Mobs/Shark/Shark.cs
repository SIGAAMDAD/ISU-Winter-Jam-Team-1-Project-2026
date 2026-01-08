using Game.Mobs;
using Godot;

namespace Prefabs {
	/*
	===================================================================================
	
	Shark
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class Shark : MobBase {
		private static readonly StringName @WindupAnimationName = "windup";

		private readonly Timer _checkAttackTimer = new Timer() {
			WaitTime = 1.5f,
			OneShot = false,
			Autostart = true
		};

		/*
		===============
		OnCheckAttackTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnCheckAttackTimeout() {
			if ( GlobalPosition.DistanceTo( _target.GlobalPosition ) > 40.0f ) {
				return;
			}

			_animation.Play( WindupAnimationName );
		}

		/*
		===============
		OnAnimationFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnAnimationFinished() {
			if ( _animation.Animation == WindupAnimationName ) {
				
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

			_checkAttackTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnCheckAttackTimeout ) );
			_animation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnAnimationFinished ) );
		}
	};
};