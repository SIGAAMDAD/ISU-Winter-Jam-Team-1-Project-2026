using Game.Common;
using Game.Player;
using Game.Player.UserInterface;
using Game.Systems;
using Game.Systems.Caching;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
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
			Hurt = 1 << 1,
			PlayerIsInRange = 1 << 3
		};

		[Export]
		protected float _xpKillAmount = 1.0f;
		[Export]
		protected float _damageAmount = 0.0f;
		[Export]
		protected float _health = 100.0f;
		[Export]
		protected float _attackCooldown = 1.5f;
		[Export]
		protected Vector2 _speed = new Vector2( 10.0f, 10.0f );
		[Export]
		protected MobTierDefinition _tierData;

		private readonly AudioStream _hitMarkerSound;

		protected NavigationAgent2D _navigationAgent;
		protected CollisionShape2D _collisionShape;
		protected AudioStreamPlayer2D _audioStreamPlayer;
		protected AnimatedSprite2D _animation;
		protected PlayerManager _target;

		protected FlagBits _flags;
		protected int _mobId;

		protected float _targetDesiredDistance;
		protected Rid _agentRid;

		protected Vector2 _currentSpeed = Vector2.Zero;
		protected Vector2 _frameVelocity;

		protected IGameEvent<PlayerTakeDamageEventArgs> _damagePlayer;

		private readonly Timer _damageEffectTimer = new Timer() {
			WaitTime = 0.75f,
			OneShot = true
		};
		protected readonly Timer _cooldownTimer = new Timer();

		private Vector2 _nextPathPosition;

		private DamageNumberFactory _numberFactory;

		public IGameEvent<MobDieEventArgs> MobDie => _die;
		private IGameEvent<MobDieEventArgs> _die;

		public IGameEvent<MobTakeDamageEventArgs> TakeDamage => _takeDamage;
		private IGameEvent<MobTakeDamageEventArgs> _takeDamage;

		/*
		===============
		MobBase
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public MobBase() {
			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/hitmarker.wav" ) ).Get( out _hitMarkerSound);
		}

		/*
		===============
		Enable
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Enable() {
			ProcessMode = ProcessModeEnum.Pausable;
			_cooldownTimer.Start();
			_animation.Play( DefaultAnimationName );
			_navigationAgent.TargetPosition = _target.GlobalPosition;
			_collisionShape.Disabled = false;
			Visible = true;

			_flags &= ~FlagBits.Dead;
		}

		/*
		===============
		Disable
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Disable() {
			_collisionShape.Disabled = true;
			Visible = false;
			ProcessMode = ProcessModeEnum.Disabled;
		}

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

			_audioStreamPlayer.Stream = _hitMarkerSound;
			_audioStreamPlayer.PitchScale = (float)GD.RandRange( 1.0f, 1.08f );
			_audioStreamPlayer.Play();

			if ( _health <= 0.0f ) {
				_flags |= FlagBits.Dead;
				_animation.Play( DieAnimationName );
				_collisionShape.Disabled = true;
			}
			_animation.SpeedScale = 0.0f;
			_animation.Modulate = Colors.Red;
			_damageEffectTimer.Start();
			_flags |= FlagBits.Hurt;
			_takeDamage.Publish( new MobTakeDamageEventArgs( _mobId, amount ) );

			_numberFactory.Add( GlobalPosition, amount );
		}

		/*
		===============
		SetSpeed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="speed"></param>
		public void SetSpeed( Vector2 speed ) {
			_currentSpeed = speed;
			NavigationServer2D.AgentSetMaxSpeed( _agentRid, _currentSpeed.Length() );
		}

		/*
		===============
		ResetSpeed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void ResetSpeed() {
			_currentSpeed = _speed;
			NavigationServer2D.AgentSetMaxSpeed( _agentRid, _currentSpeed.Length() );
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
			_flags |= FlagBits.PlayerIsInRange;
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
			_animation.Modulate = Colors.White;
			_animation.SpeedScale = 1.0f;
			_flags &= ~FlagBits.Hurt;
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				_die.Publish( new MobDieEventArgs( _mobId, _xpKillAmount ) );
				_navigationAgent.TargetPosition = GlobalPosition;
				NavigationServer2D.AgentSetVelocityForced( _agentRid, Vector2.Zero );

				ProcessMode = ProcessModeEnum.Disabled;
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
			_cooldownTimer.Stop();
		}

		/*
		===============
		OnCooldownTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		protected virtual void OnCooldownTimerTimeout() {
			if ( (_flags & FlagBits.Dead) == 0 && GlobalPosition.DistanceTo( _navigationAgent.TargetPosition ) < _targetDesiredDistance ) {
				_damagePlayer.Publish( new PlayerTakeDamageEventArgs( _damageAmount ) );
			}
		}

		/*
		===============
		OnVelocityComputed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="velocity"></param>
		private void OnVelocityComputed( Vector2 velocity ) {
			GlobalPosition += velocity;
		}

		/*
		===============
		GetDamageNumberFactory
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void GetDamageNumberFactory() {
			_numberFactory = GetTree().Root.GetNode<DamageNumberFactory>( nameof( DamageNumberFactory ) );
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

			_cooldownTimer.WaitTime = _attackCooldown;
			_cooldownTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnCooldownTimerTimeout ) );
			AddChild( _cooldownTimer );

			_mobId = GetPath().GetHashCode();
			_target = GetNode<PlayerManager>( "/root/World/Player" );
			_collisionShape = GetNode<CollisionShape2D>( "CollisionShape2D" );

			var locator = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator;
			var eventFactory = locator.GetService<IGameEventRegistryService>();
			_takeDamage = eventFactory.GetEvent<MobTakeDamageEventArgs>( nameof( MobBase ), nameof( TakeDamage ) );
			_die = eventFactory.GetEvent<MobDieEventArgs>( nameof( MobBase ), nameof( MobDie ) );
			_damagePlayer = eventFactory.GetEvent<PlayerTakeDamageEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.TakeDamage ) );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			_audioStreamPlayer = new AudioStreamPlayer2D();
			AddChild( _audioStreamPlayer );

			_navigationAgent = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
			_navigationAgent.AvoidanceEnabled = true;
			_navigationAgent.MaxNeighbors = MobSpawner.MAX_WAVE_ENEMIES;
			_navigationAgent.MaxSpeed = _speed.LengthSquared();
			_navigationAgent.TimeHorizonAgents = 0.5f;
			_navigationAgent.Radius = 20.0f;
			_navigationAgent.ProcessThreadGroup = ProcessThreadGroupEnum.MainThread;
			_agentRid = _navigationAgent.GetRid();
			_navigationAgent.Connect( NavigationAgent2D.SignalName.TargetReached, Callable.From( OnTargetReached ) );
			_navigationAgent.Connect( NavigationAgent2D.SignalName.VelocityComputed, Callable.From<Vector2>( OnVelocityComputed ) );

			_targetDesiredDistance = _navigationAgent.TargetDesiredDistance;

			_animation = GetNode<AnimatedSprite2D>( nameof( AnimatedSprite2D ) );

			_damageEffectTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnResetColor ) );
			AddChild( _damageEffectTimer );

			_currentSpeed = _speed;
			CallDeferred( MethodName.GetDamageNumberFactory );
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
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}

			_nextPathPosition = _navigationAgent.GetNextPathPosition();
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
			EntityUtils.CalcSpeed( ref _frameVelocity, _currentSpeed, (float)delta, position.DirectionTo( _nextPathPosition ) );
			_navigationAgent.SetDeferred( NavigationAgent2D.PropertyName.TargetPosition, _target.GlobalPosition );
			NavigationServer2D.AgentSetVelocity( _agentRid, _frameVelocity );
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

			_takeDamage.Dispose();
			_takeDamage = null;

			_damagePlayer.Dispose();
			_damagePlayer = null;
		}
	};
};
