using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;
using System.Collections;

namespace Game.Mobs {
	/*
	===================================================================================
	
	MobBase
	
	===================================================================================
	*/
	/// <summary>
	/// The base object that all mobs inherit from.
	/// </summary>

	public partial class MobBase : CharacterBody2D {
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
		private float _health = 100.0f;
		[Export]
		private float _speed = 10.0f;

		private Vector2 _frameVelocity;

		protected NavigationAgent2D _navigationAgent;
		protected AnimatedSprite2D _animation;
		private EntityManager _entityManager;

		protected FlagBits _flags;
		protected int _mobId;

		private readonly Timer _damageEffectTimer = new Timer() {
			WaitTime = 0.75f,
			OneShot = true
		};

		public IGameEvent<MobDieEventArgs> Die => _die;
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
		public void Damage( float amount ) {
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}

			_health -= amount;
			if ( _health <= 0.0f ) {
				_flags |= FlagBits.Dead;
				_die.Publish( new MobDieEventArgs( _mobId ) );
				QueueFree();
				return;
			}
			_animation.SpeedScale = 0.0f;
			_animation.Modulate = Colors.Red;
			_damageEffectTimer.Start();
			_flags |= FlagBits.Hurt;
			_takeDamage.Publish( new MobTakeDamageEventArgs( _mobId, amount ) );
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
		}

		/*
		===============
		OnTargetReached
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnTargetReached() {
			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				return;
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
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			_mobId = GetPath().GetHashCode();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_takeDamage = eventFactory.GetEvent<MobTakeDamageEventArgs>( nameof( TakeDamage ) );
			_die = eventFactory.GetEvent<MobDieEventArgs>( nameof( Die ) );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			_navigationAgent = GetNode<NavigationAgent2D>( "NavigationAgent2D" );
			_navigationAgent.Connect( NavigationAgent2D.SignalName.TargetReached, Callable.From( OnTargetReached ) );

			_animation = GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );
			_entityManager = GetNode<EntityManager>( "/root/World/EntityManager" );

			_damageEffectTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnResetColor ) );
			AddChild( _damageEffectTimer );
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

			_navigationAgent.TargetPosition = _entityManager.TargetPosition;
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
			EntityUtils.CalcSpeed( ref _frameVelocity, _speed, (float)delta, position.DirectionTo( _navigationAgent.TargetPosition ) );
			position += _frameVelocity;
			SetDeferred( PropertyName.GlobalPosition, position );
		}
	};
};