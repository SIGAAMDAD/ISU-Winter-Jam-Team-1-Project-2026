using Game.Mobs;
using Godot;
using System;

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
		[Flags]
		private enum SharkFlags : byte {
			CanAttack = 1 << 0,
			IsAttacking = 1 << 1
		};

		private static readonly StringName @WindupAnimationName = "windup";

		private SharkFlags _sharkFlags = SharkFlags.CanAttack;

		private readonly Timer _checkAttackTimer;
		private readonly Timer _windupTimer;
		private readonly Timer _attackCooldownTimer;

		/*
		===============
		Shark
		===============
		*/
		/// <summary>
		/// Creates a Shark.
		/// </summary>
		public Shark() {
			_checkAttackTimer = new Timer() {
				WaitTime = 1.5f,
				OneShot = false
			};
			_windupTimer = new Timer() {
				WaitTime = 2.0f,
				OneShot = true
			};
			_attackCooldownTimer = new Timer() {
				WaitTime = 4.0f,
				OneShot = true
			};
		}

		/*
		===============
		OnTargetReached
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnTargetReached() {
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}

			_animation.CallDeferred( AnimatedSprite2D.MethodName.Play, DefaultAnimationName );
			_speed /= 4.0f;
			_sharkFlags &= ~( SharkFlags.IsAttacking | SharkFlags.CanAttack );

			_attackCooldownTimer.CallDeferred( Timer.MethodName.Start );

			SetProcess( true );
		}

		/*
		===============
		OnCheckAttackTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnCheckAttackTimeout() {
			if ( ( _flags & FlagBits.Dead ) != 0 || ( _sharkFlags & SharkFlags.CanAttack ) == 0 || GlobalPosition.DistanceTo( _target.GlobalPosition ) > 400.0f ) {
				return;
			}

			_animation.CallDeferred( AnimatedSprite2D.MethodName.Play, WindupAnimationName );
			_sharkFlags |= SharkFlags.IsAttacking;
			_windupTimer.CallDeferred( Timer.MethodName.Start );
			_checkAttackTimer.CallDeferred( Timer.MethodName.Start );

			_sharkFlags |= SharkFlags.IsAttacking;

			SetProcess( false );
		}

		/*
		===============
		OnWindupTimerTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnWindupTimerTimeout() {
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}

			_speed *= 4.0f;
			_navigationAgent.SetDeferred( NavigationAgent2D.PropertyName.TargetPosition, _target.GlobalPosition );
		}

		/*
		===============
		OnAttackCooldownTimerTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnAttackCooldownTimerTimeout() {
			_sharkFlags |= SharkFlags.CanAttack;
			_checkAttackTimer.CallDeferred( Timer.MethodName.Start );
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
			AddChild( _checkAttackTimer );
			_checkAttackTimer.Start();

			_windupTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnWindupTimerTimeout ) );
			AddChild( _windupTimer );

			_attackCooldownTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnAttackCooldownTimerTimeout ) );
			AddChild( _attackCooldownTimer );
		}
	};
};