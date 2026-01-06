using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Mobs {
	public partial class MobBase : CharacterBody2D {
		[Export]
		private float _xpKillAmount = 1.0f;
		[Export]
		private float _damageAmount = 0.0f;
		[Export]
		private float _health = 100.0f;
		[Export]
		private float _speed = 10.0f;

		public IGameEvent<MobDieEventArgs> Die => _die;
		private IGameEvent<MobDieEventArgs> _die;

		public IGameEvent<EntityTakeDamageEventArgs> TakeDamage => _takeDamage;
		private IGameEvent<EntityTakeDamageEventArgs> _takeDamage;
		
		private Vector2 _frameVelocity;

		private NavigationAgent2D _navigationAgent;
		private AnimatedSprite2D _animation;
		private EntityManager _entityManager;

		private readonly Timer _damageEffectTimer = new Timer() {
			WaitTime = 0.75f,
			OneShot = true
		};

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
			_health -= amount;
			if ( _health <= 0.0f ) {
				GD.Print( "DEATH!" );
				return;
			}
			_animation.Modulate = Colors.Red;
			_damageEffectTimer.Start();
		}

		/*
		===============
		OnResetColor
		===============
		*/
		private void OnResetColor() {
			_animation.Modulate = Colors.White;
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
			GD.Print( "Target Reached!" );
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

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_takeDamage  = eventFactory.GetEvent<EntityTakeDamageEventArgs>( nameof( TakeDamage ) );
			_die = eventFactory.GetEvent<MobDieEventArgs>( nameof( Die ) );

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
			if ( ( Engine.GetProcessFrames() % 10 ) != 0 ) {
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