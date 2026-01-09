using Game.Common;
using Game.Player;
using Game.Player.UserInterface;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

namespace Game.Mobs {
	/*
	===================================================================================
	
	MobBase
	
	===================================================================================
	*/
	/// <summary>
	/// The base object that all mobs inherit from.
	/// </summary>

	public partial class MobBase : Area2D {
		protected static readonly StringName @DefaultAnimationName = "default";
		protected static readonly StringName @DieAnimationName = "die";

		[Flags]
		protected enum FlagBits : uint {
			Dead = 1 << 0,
			Hurt = 1 << 1
		};

		[Export]
		private float _xpKillAmount = 1.0f;
		[Export]
		private float _damageAmount = 0.0f;
		[Export]
		protected float _health = 100.0f;
		[Export]
		protected float _speed = 10.0f;

		private Vector2 _frameVelocity;
		private Vector2 _nextPathPosition;

		protected NavigationAgent2D _navigationAgent;
		protected AnimatedSprite2D _animation;
		protected CollisionShape2D _collisionShape;
		protected PlayerManager _target;

		protected FlagBits _flags;
		protected int _mobId;

		protected IGameEvent<PlayerTakeDamageEventArgs> _damagePlayer;

		private readonly Timer _damageEffectTimer = new Timer() {
			WaitTime = 0.75f,
			OneShot = true
		};

		public IGameEvent<MobDieEventArgs> MobDie => _die;
		private IGameEvent<MobDieEventArgs> _die;

		public IGameEvent<MobTakeDamageEventArgs> TakeDamage => _takeDamage;
		private IGameEvent<MobTakeDamageEventArgs> _takeDamage;

		/*
		===============
		Damage
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		public virtual void Damage( float amount ) {
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}

			_health -= amount;
			if ( _health <= 0.0f ) {
				_flags |= FlagBits.Dead;
				_animation.CallDeferred( AnimatedSprite2D.MethodName.Play, DieAnimationName );
				_collisionShape.SetDeferred( CollisionShape2D.PropertyName.Disabled, true );
			}
			_animation.SetDeferred( AnimatedSprite2D.PropertyName.SpeedScale, 0.0f );
			_animation.SetDeferred( AnimatedSprite2D.PropertyName.Modulate, Colors.Red );
			_damageEffectTimer.CallDeferred( Timer.MethodName.Start );
			_flags |= FlagBits.Hurt;
			_takeDamage.Publish( new MobTakeDamageEventArgs( _mobId, amount ) );

			var damageNumber = new DamageNumberLabel() {
				GlobalPosition = GlobalPosition,
				Value = amount
			};
			GetTree().Root.CallDeferred( MethodName.AddChild, damageNumber );
		}

		/*
		===============
		OnTargetReached
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		protected virtual void OnTargetReached() {
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}
			_damagePlayer.Publish( new PlayerTakeDamageEventArgs( _damageAmount ) );
		}

		/*
		===============
		OnResetColor
		===============
		*/
		/// <summary>
		/// Resets the "hurt" color back to white.
		/// </summary>
		private void OnResetColor() {
			_animation.SetDeferred( AnimatedSprite2D.PropertyName.Modulate, Colors.White );
			_animation.SetDeferred( AnimatedSprite2D.PropertyName.SpeedScale, 1.0f );
			_flags &= ~FlagBits.Hurt;
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				_die.Publish( new MobDieEventArgs( _mobId, _xpKillAmount ) );
				CallDeferred( MethodName.QueueFree );
			}
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
			_flags |= FlagBits.Dead;
		}

		/*
		===============
		UpdateTargetPosition
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void UpdateTargetPosition() {
			_navigationAgent.TargetPosition = _target.GlobalPosition;
			_nextPathPosition = _navigationAgent.GetNextPathPosition();
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

			_mobId = GetPath().GetHashCode();
			_target = GetNode<PlayerManager>( "/root/World/Player" );
			_collisionShape = GetNode<CollisionShape2D>( "CollisionShape2D" );

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_takeDamage = eventFactory.GetEvent<MobTakeDamageEventArgs>( nameof( TakeDamage ) );
			_die = eventFactory.GetEvent<MobDieEventArgs>( nameof( MobDie ) );
			_damagePlayer = eventFactory.GetEvent<PlayerTakeDamageEventArgs>( nameof( PlayerStats.TakeDamage ) );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			_navigationAgent = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
			_navigationAgent.Connect( NavigationAgent2D.SignalName.TargetReached, Callable.From( OnTargetReached ) );
			_navigationAgent.ProcessThreadGroup = ProcessThreadGroupEnum.MainThread;

			_animation = GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );

			_damageEffectTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnResetColor ) );
			AddChild( _damageEffectTimer );

			ProcessThreadGroup = ProcessThreadGroupEnum.SubThread;
			ProcessThreadGroupOrder = 1;
		}

		/*
		===============
		_Process
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ) {
			base._Process( delta );

			// only process every 10 frames in case we've got a lot of baddies
			if ( ( _flags & FlagBits.Dead ) != 0 || ( Engine.GetProcessFrames() % 20 ) != 0 ) {
				return;
			}

			CallDeferred( MethodName.UpdateTargetPosition );
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
			base._PhysicsProcess( delta );

			Vector2 position = GlobalPosition;

			// NOTE: could just set the agent speed...
			EntityUtils.CalcSpeed( ref _frameVelocity, _speed, (float)delta, position.DirectionTo( _nextPathPosition ) );
			NavigationServer2D.AgentSetVelocityForced( _navigationAgent.GetRid(), _frameVelocity );
			position += _frameVelocity;
			SetDeferred( PropertyName.GlobalPosition, position );
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _ExitTree() {
			base._ExitTree();

			_target = null;
		}
	};
};