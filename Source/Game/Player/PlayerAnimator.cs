using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

namespace Game.Player {
	public sealed class PlayerAnimator {
		private readonly StringName @UpAnimation = "up";
		private readonly StringName @DownAnimation = "down";
		private readonly StringName @HorizontalAnimation = "horizontal";

		private readonly PlayerManager _owner;
		private readonly AnimatedSprite2D _animations;

		private readonly GpuParticles2D _foamParticles;
		private readonly Marker2D _upFoamPosition;
		private readonly Marker2D _downFoamPosition;
		private readonly Marker2D _leftFoamPosition;
		private readonly Marker2D _rightFoamPosition;
		private Marker2D _currentFoamPosition;

		public IGameEvent<EmptyEventArgs> PlayerStartMoving => _playerStartMoving;
		private readonly IGameEvent<EmptyEventArgs> _playerStartMoving;

		public IGameEvent<EmptyEventArgs> PlayerStopMoving => _playerStopMoving;
		private readonly IGameEvent<EmptyEventArgs> _playerStopMoving;

		private bool _playerIsMoving = false;

		/*
		===============
		PlayerAnimator
		===============
		*/
		public PlayerAnimator( PlayerManager owner ) {
			_owner = owner;
			_animations = owner.GetNode<AnimatedSprite2D>( "AnimatedSprite2D" );

			_foamParticles = _animations.GetNode<GpuParticles2D>( "UpFoamMarker/FoamParticles" );
			_upFoamPosition = _animations.GetNode<Marker2D>( "UpFoamMarker" );
			_downFoamPosition = _animations.GetNode<Marker2D>( "DownFoamMarker" );
			_leftFoamPosition = _animations.GetNode<Marker2D>( "LeftFoamMarker" );
			_rightFoamPosition = _animations.GetNode<Marker2D>( "RightFoamMarker" );

			_currentFoamPosition = _upFoamPosition;

			var eventFactory = owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_playerStartMoving = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStartMoving ) );
			_playerStopMoving = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStopMoving ));
		}

		private void SetFoamPosition( float rotation, Marker2D newPosition ) {
			if ( _currentFoamPosition == newPosition ) {
				return;
			}
			_currentFoamPosition.RemoveChild( _foamParticles );
			newPosition.AddChild( _foamParticles );
			_currentFoamPosition = newPosition;
			_foamParticles.GlobalRotation = rotation;
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
			Vector2 velocity = _owner.Velocity;

			if ( Math.Abs( velocity.X ) > Math.Abs( velocity.Y ) ) {
				if ( velocity.X > 0.0f ) {
					_animations.Play( HorizontalAnimation );
					_animations.FlipH = false;

					SetFoamPosition( 90.0f, _rightFoamPosition );
				}
				if ( velocity.X < 0.0f ) {
					_animations.Play( HorizontalAnimation );
					_animations.FlipH = true;

					SetFoamPosition( -90.0f, _leftFoamPosition );
				}
			} else {
				if ( velocity.Y > 0.0f ) {
					_animations.Play( DownAnimation );

					SetFoamPosition( 180.0f, _downFoamPosition );
				}
				if ( velocity.Y < 0.0f ) {
					_animations.Play( UpAnimation );

					SetFoamPosition( 0.0f, _upFoamPosition );
				}
			}
			_foamParticles.Emitting = inputWasActive;
			if ( inputWasActive && !_playerIsMoving ) {
				_playerIsMoving = true;
				_playerStartMoving.Publish( new EmptyEventArgs() );
			} else if ( !inputWasActive && _playerIsMoving ) {
				_playerStopMoving.Publish( new EmptyEventArgs() );
				_playerIsMoving = false;
			}
		}
	};
};