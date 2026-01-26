using Game.Mobs;
using Godot;
using Nomad.Core.Events;

namespace Prefabs {
	/*
	===================================================================================

	Orca

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class Orca : MobBase {
		private static readonly StringName @SurfaceUpAnimationName = "surface_up";
		private static readonly StringName @SurfaceDownAnimationName = "surface_down";

		private readonly Timer _swimTimer = new Timer() {
			WaitTime = 4.0f,
			OneShot = true
		};
		private readonly Timer _surfaceTimer = new Timer() {
			WaitTime = 5.0f,
			OneShot = true
		};

		private TextureRect _waterSurface;

		/*
		===============
		OnSurfaceTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnSurfaceTimerTimeout() {
			_animation.Play( SurfaceDownAnimationName );
			_collisionShape.Disabled = true;
			ZIndex = -2;
		}

		/*
		===============
		OnSwimTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnSwimTimerTimeout() {
			_animation.Play( SurfaceUpAnimationName );
			_collisionShape.Disabled = false;
			ZIndex = 1;
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
			if ( _animation.Animation == SurfaceUpAnimationName ) {
				_surfaceTimer.Start();
			} else if ( _animation.Animation == SurfaceDownAnimationName ) {
				_swimTimer.Start();
				_animation.Play( DefaultAnimationName );
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

			ZIndex = -2;

			_swimTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSwimTimerTimeout ) );
			AddChild( _swimTimer );

			_surfaceTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnSurfaceTimerTimeout ) );
			AddChild( _surfaceTimer );

			_animation.Connect( AnimatedSprite2D.SignalName.AnimationFinished, Callable.From( OnAnimationFinished ) );
		}
	};
};
