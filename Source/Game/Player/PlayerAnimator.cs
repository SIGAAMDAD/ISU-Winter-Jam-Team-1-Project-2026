using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerAnimator
	
	Description
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class PlayerAnimator {
		private static readonly StringName @UpHarpoonAnimation = "up_harpoon";
		private static readonly StringName @UpNoHarpoonAnimation = "up_noharpoon";
		private static readonly StringName @DownHarpoonAnimation = "down_harpoon";
		private static readonly StringName @DownNoHarpoonAnimation = "down_noharpoon";
		private static readonly StringName @HorizontalHarpoonAnimation = "horizontal_harpoon";
		private static readonly StringName @HorizontalNoHarpoonAnimation = "horizontal_noharpoon";

		private static readonly StringName @HorizontalAnimation = "horizontal";
		private static readonly StringName @UpAnimation = "up";
		private static readonly StringName @DownAnimation = "down";

		private const float UP_MIN = 315.0f;
		private const float UP_MAX = 45.0f;
		private const float RIGHT_MIN = 45.0f;
		private const float RIGHT_MAX = 135.0f;
		private const float DOWN_MIN = 135.0f;
		private const float DOWN_MAX = 225.0f;
		private const float LEFT_MIN = 225.0f;
		private const float LEFT_MAX = 315.0f;

		public PlayerDirection Direction => _direction;
		private PlayerDirection _direction;

		private readonly PlayerManager _owner;
		private readonly AnimatedSprite2D _animations;

		private readonly GpuParticles2D _foamParticles;
		private readonly AnimatedSprite2D _harpoonAnimation;

		private readonly Marker2D[] _foamPositions;
		private readonly Marker2D[] _harpoonPositions;

		private bool _playerIsMoving = false;
		private bool _isPlayerAttacking = false;

		private float _prevMouseAngle = 0.0f;
		private PlayerDirection _harpoonDirection;

		public IGameEvent<EmptyEventArgs> PlayerStartMoving => _playerStartMoving;
		private readonly IGameEvent<EmptyEventArgs> _playerStartMoving;

		public IGameEvent<EmptyEventArgs> PlayerStopMoving => _playerStopMoving;
		private readonly IGameEvent<EmptyEventArgs> _playerStopMoving;

		/*
		===============
		PlayerAnimator
		===============
		*/
		public PlayerAnimator( PlayerManager owner ) {
			_owner = owner;
			_animations = owner.GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );

			_foamParticles = _animations.GetNode<GpuParticles2D>( "UpFoamMarker/FoamParticles" );
			_harpoonAnimation = _animations.GetNode<AnimatedSprite2D>( "HarpoonSpawnUp/HarpoonCannon" );

			_foamPositions = new Marker2D[ (int)PlayerDirection.Count ];
			_foamPositions[ (int)PlayerDirection.North ] = _animations.GetNode<Marker2D>( "UpFoamMarker" );
			_foamPositions[ (int)PlayerDirection.South ] = _animations.GetNode<Marker2D>( "DownFoamMarker" );
			_foamPositions[ (int)PlayerDirection.West ] = _animations.GetNode<Marker2D>( "LeftFoamMarker" );
			_foamPositions[ (int)PlayerDirection.East ] = _animations.GetNode<Marker2D>( "RightFoamMarker" );

			_harpoonPositions = new Marker2D[ (int)PlayerDirection.Count ];
			_harpoonPositions[ (int)PlayerDirection.North ] = _animations.GetNode<Marker2D>( "HarpoonSpawnUp" );
			_harpoonPositions[ (int)PlayerDirection.South ] = _animations.GetNode<Marker2D>( "HarpoonSpawnDown" );
			_harpoonPositions[ (int)PlayerDirection.West ] = _animations.GetNode<Marker2D>( "HarpoonSpawnLeft" );
			_harpoonPositions[ (int)PlayerDirection.East ] = _animations.GetNode<Marker2D>( "HarpoonSpawnRight" );

			_harpoonDirection = PlayerDirection.North;

			var eventFactory = owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();

			var useWeapon = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerController.UseWeapon ) );
			useWeapon.Subscribe( this, OnUseWeapon );

			var weaponCooldownFinished = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerController.WeaponCooldownFinished ) );
			weaponCooldownFinished.Subscribe( this, OnWeaponCooldownFinished );

			_playerStartMoving = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStartMoving ) );
			_playerStopMoving = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStopMoving ) );
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="inputWasActive"></param>
		public void Update( float delta, bool inputWasActive ) {
			CalcCannonTransform();
			CalcFoamPosition();

			_foamParticles.Emitting = inputWasActive;
			if ( inputWasActive && !_playerIsMoving ) {
				_playerIsMoving = true;
				_playerStartMoving.Publish( new EmptyEventArgs() );
			} else if ( !inputWasActive && _playerIsMoving ) {
				_playerStopMoving.Publish( new EmptyEventArgs() );
				_playerIsMoving = false;
			}
		}

		/*
		===============
		SetHarpoonPosition
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="direction"></param>
		/// <param name="newPosition"></param>
		private void SetHarpoonPosition( PlayerDirection direction, Marker2D newPosition ) {
			_harpoonPositions[ (int)_harpoonDirection ].RemoveChild( _harpoonAnimation );
			newPosition.AddChild( _harpoonAnimation );
			_harpoonAnimation.RotationDegrees = 0.0f;
			_harpoonAnimation.Position = Vector2.Zero;
			_harpoonDirection = direction;
		}

		/*
		===============
		CalcCannonTransform
		===============
		*/
		private void CalcCannonTransform() {
			Vector2 playerPos = _harpoonAnimation.GlobalPosition;
			Vector2 mouseScreenPos = _owner.GetGlobalMousePosition();
			Vector2 direction = ( mouseScreenPos - playerPos ).Normalized();

			float angleRad = Mathf.Atan2( direction.Y, direction.X );
			float angleDeg = Mathf.RadToDeg( angleRad );

			// account for animation direction
			switch ( _harpoonDirection ) {
				case PlayerDirection.North:
					angleDeg += 90.0f;
					break;
				case PlayerDirection.South:
					angleDeg -= 90.0f;
					break;
				case PlayerDirection.East:
					angleDeg += 180.0f;
					break;
			}
			_harpoonAnimation.RotationDegrees = angleDeg;
		}

		/*
		===============
		AimHarpoon
		===============
		*/
		private void AimHarpoon( PlayerDirection quadrant, float length ) {
			switch ( quadrant ) {
				case PlayerDirection.North:
					_harpoonAnimation.Play( _isPlayerAttacking ? UpNoHarpoonAnimation : UpHarpoonAnimation );
					_harpoonAnimation.FlipV = false;
					break;
				case PlayerDirection.East:
					_harpoonAnimation.Play( _isPlayerAttacking ? HorizontalNoHarpoonAnimation : HorizontalHarpoonAnimation );
					_harpoonAnimation.FlipH = false;
					break;
				case PlayerDirection.South:
					_harpoonAnimation.Play( _isPlayerAttacking ? UpNoHarpoonAnimation : UpHarpoonAnimation );
					_harpoonAnimation.FlipV = true;
					break;
				case PlayerDirection.West:
					_harpoonAnimation.Play( _isPlayerAttacking ? HorizontalNoHarpoonAnimation : HorizontalHarpoonAnimation );
					_harpoonAnimation.FlipH = true;
					break;
			}
		}

		/*
		===============
		GetMouseQuadrant
		===============
		*/
		private PlayerDirection GetMouseQuadrant( float angleDeg ) {
			// Handle the wrap-around case for Up quadrant (315° to 45°)
			if ( ( angleDeg >= UP_MIN && angleDeg < 360f ) || ( angleDeg >= 0f && angleDeg < UP_MAX ) ) {
				return PlayerDirection.North;
			} else if ( angleDeg >= RIGHT_MIN && angleDeg < RIGHT_MAX ) {
				return PlayerDirection.East;
			} else if ( angleDeg >= DOWN_MIN && angleDeg < DOWN_MAX ) {
				return PlayerDirection.South;
			} else if ( angleDeg >= LEFT_MIN && angleDeg < LEFT_MAX ) {
				return PlayerDirection.West;
			}

			// Fallback (shouldn't reach here with proper normalization)
			return PlayerDirection.North;
		}

		/*
		===============
		SetFoamPosition
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="rotation"></param>
		/// <param name="newPosition"></param>
		private void SetFoamPosition( PlayerDirection direction, float rotation, Marker2D newPosition ) {
			_foamPositions[ (int)_direction ].RemoveChild( _foamParticles );
			newPosition.AddChild( _foamParticles );
			_direction = direction;
			_foamParticles.GlobalRotation = rotation;
		}

		/*
		===============
		CalcFoamPosition
		===============
		*/
		private void CalcFoamPosition() {
			Vector2 velocity = _owner.Velocity;
			PlayerDirection foamDirection = _direction;
			float foamRotation = _foamParticles.GlobalRotation;

			if ( Math.Abs( velocity.X ) > Math.Abs( velocity.Y ) ) {
				if ( velocity.X > 0.0f ) {
					_animations.Play( HorizontalAnimation );
					_animations.FlipH = false;

					foamRotation = 90.0f;
					foamDirection = PlayerDirection.East;
				}
				if ( velocity.X < 0.0f ) {
					_animations.Play( HorizontalAnimation );
					_animations.FlipH = true;

					foamRotation = -90.0f;
					foamDirection = PlayerDirection.West;
				}
			} else {
				if ( velocity.Y > 0.0f ) {
					_animations.Play( DownAnimation );

					foamRotation = 180.0f;
					foamDirection = PlayerDirection.South;
				}
				if ( velocity.Y < 0.0f ) {
					_animations.Play( UpAnimation );

					foamRotation = 0.0f;
					foamDirection = PlayerDirection.North;
				}
			}
			if ( foamDirection != _direction ) {
				AimHarpoon( foamDirection, 0.0f );
				SetHarpoonPosition( foamDirection, _harpoonPositions[ (int)foamDirection ] );
				SetFoamPosition( foamDirection, foamRotation, _foamPositions[ (int)foamDirection ] );
			}
		}

		/*
		===============
		OnWeaponCooldownFinished
		===============
		*/
		private void OnWeaponCooldownFinished( in EmptyEventArgs args ) {
			_isPlayerAttacking = false;
		}

		/*
		===============
		OnUseWeapon
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnUseWeapon( in EmptyEventArgs args ) {
			_isPlayerAttacking = true;
		}
	};
};