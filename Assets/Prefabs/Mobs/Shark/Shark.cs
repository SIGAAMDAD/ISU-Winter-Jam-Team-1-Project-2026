using Game.Common;
using Game.Mobs;
using Game.Player;
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

		private const float CHARGE_WINDUP_TIME = 1.5f;
		private const float CHARGE_COOLDOWN_TIME = 3.0f;

		[Flags]
		private enum SharkFlags : byte {
			CanAttack = 1 << 0,
			IsAttacking = 1 << 1
		};

		private SharkFlags _sharkFlags = SharkFlags.CanAttack;

		private Vector2 _chargeDirection = Vector2.Zero;
		private Vector2 _chargeDestination = Vector2.Zero;

		private readonly Timer _windupTimer = new Timer() {
			WaitTime = CHARGE_WINDUP_TIME,
			OneShot = true
		};
		private readonly Timer _attackCooldownTimer = new Timer() {
			WaitTime = CHARGE_COOLDOWN_TIME,
			OneShot = true
		};

		private bool IsAttacking => (_sharkFlags & SharkFlags.IsAttacking) != 0;
		private bool CanAttack => (_sharkFlags & SharkFlags.CanAttack) != 0;

		/*
		===============
		OnStopCharge
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnStopCharge() {
			_animation.Play( DefaultAnimationName );
			ResetSpeed();
			_sharkFlags &= ~(SharkFlags.IsAttacking | SharkFlags.CanAttack);

			_attackCooldownTimer.Start();

			Modulate = Colors.White;
		}

		/*
		===============
		OnHitTarget
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnHitTarget() {
			OnStopCharge();
			_damagePlayer.Publish( new PlayerTakeDamageEventArgs( _damageAmount ) );
		}

		/*
		===============
		OnCooldownTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected override void OnCooldownTimerTimeout() {
			if ( (_flags & FlagBits.Dead) != 0 || !CanAttack || IsAttacking || GlobalPosition.DistanceTo( _target.GlobalPosition ) > 400.0f ) {
				return;
			}

			_animation.Play( WindupAnimationName );
			_sharkFlags |= SharkFlags.IsAttacking;
			_sharkFlags &= ~SharkFlags.CanAttack;
			_windupTimer.Start();
			_cooldownTimer.Stop();

			_chargeDestination = _target.GlobalPosition;
			_chargeDirection = GlobalPosition.DirectionTo( _chargeDestination );

			CreateTween().TweenProperty( this, ModulateNodePath, Colors.Red, _windupTimer.WaitTime );
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
			if ( (_flags & FlagBits.Dead) != 0 ) {
				return;
			}

			_currentSpeed *= GlobalPosition.DistanceTo( _chargeDestination ) / 8.0f;
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
			_navigationAgent.TargetPosition = _target.GlobalPosition;
			_cooldownTimer.Start();
		}

		/*
		===============
		OnBodyShapeEntered
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyShapeEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager && IsAttacking ) {
				OnHitTarget();
			}
		}

		/*
		===============
		OnAreaShapeExited
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="areaRid"></param>
		/// <param name="area"></param>
		/// <param name="areaShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnAreaShapeExited( Rid areaRid, Area2D area, int areaShapeIndex, int localShapeIndex ) {
			if ( area is WorldArea && IsAttacking ) {
				OnStopCharge();
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

			_windupTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnWindupTimerTimeout ) );
			AddChild( _windupTimer );

			_attackCooldownTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnAttackCooldownTimerTimeout ) );
			AddChild( _attackCooldownTimer );

			Connect( Shark.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyShapeEntered ) );
			Connect( Shark.SignalName.AreaShapeExited, Callable.From<Rid, Area2D, int, int>( OnAreaShapeExited ) );
		}

		/*
		===============
		_PhysicsProcess
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		public override void _PhysicsProcess( double delta ) {
			if ( !IsAttacking ) {
				base._PhysicsProcess( delta );
			} else {
				Vector2 position = GlobalPosition;

				// NOTE: could just set the agent speed...
				EntityUtils.CalcSpeed( ref _frameVelocity, _currentSpeed, ( float )delta, _chargeDirection );

				position += _frameVelocity;
				SetDeferred( PropertyName.GlobalPosition, position );
				if ( GlobalPosition.DistanceTo( _chargeDestination ) < _targetDesiredDistance ) {
					OnStopCharge();
				}
			}
		}
	};
};
