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
		private static readonly StringName @WindupAnimationName = "windup";
		private static readonly NodePath @ModulateNodePath = "modulate";

		private const float CHARGE_WINDUP_TIME = 3.0f;
		private const float CHARGE_COOLDOWN_TIME = 6.0f;

		[Flags]
		private enum SharkFlags : byte {
			CanAttack = 1 << 0,
			IsAttacking = 1 << 1
		};

		private SharkFlags _sharkFlags = SharkFlags.CanAttack;

		private readonly Timer _checkAttackTimer = new Timer() {
			WaitTime = 0.5f,
			OneShot = false
		};
		private readonly Timer _windupTimer = new Timer() {
			WaitTime = CHARGE_WINDUP_TIME,
			OneShot = true
		};
		private readonly Timer _attackCooldownTimer = new Timer() {
			WaitTime = CHARGE_COOLDOWN_TIME,
			OneShot = true
		};

		/*
		===============
		OnTargetReached
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected override void OnTargetReached() {
			if ( ( _flags & FlagBits.Dead ) != 0 || ( _sharkFlags & SharkFlags.IsAttacking ) == 0 ) {
				return;
			}

			_animation.CallDeferred( AnimatedSprite2D.MethodName.Play, DefaultAnimationName );
			_currentSpeed = _speed;
			_sharkFlags &= ~( SharkFlags.IsAttacking | SharkFlags.CanAttack );

			_attackCooldownTimer.CallDeferred( Timer.MethodName.Start );

			SetDeferred( PropertyName.Modulate, Colors.White );

			CallDeferred( MethodName.SetProcess, true );
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

			CreateTween().CallDeferred( Tween.MethodName.TweenProperty, this, ModulateNodePath, Colors.Red, _windupTimer.WaitTime );

			_sharkFlags |= SharkFlags.IsAttacking;

			CallDeferred( MethodName.SetProcess, false );
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

			_currentSpeed = new Vector2( _currentSpeed.X * 2.0f, 0.0f );
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