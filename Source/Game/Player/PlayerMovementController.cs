using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using System;

namespace Game.Player {
	/*
	===================================================================================

	PlayerMovementController

	FIXME: this does too much

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class PlayerMovementController {
		[Flags]
		private enum FlagBits : byte {
			CanMove = 1 << 0,

			WaveActive = 1 << 2,

			Dead = 1 << 3
		};

		private static readonly StringName @MoveEastBind = "move_east";
		private static readonly StringName @MoveWestBind = "move_west";
		private static readonly StringName @MoveNorthBind = "move_north";
		private static readonly StringName @MoveSouthBind = "move_south";

		private readonly Vector2 _startPosition;

		private readonly PlayerManager _owner;
		private readonly PlayerAnimator _animator;
		private readonly PlayerAttackController _attackController;

		private FlagBits _flags = FlagBits.CanMove | FlagBits.WaveActive;
		private Vector2 _frameVelocity = Vector2.Zero;

		private float _movementSpeed = 0.0f;

		/*
		===============
		PlayerMovementController
		===============
		*/
		/// <summary>
		/// Creates a PlayerMovementController
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="animator"></param>
		public PlayerMovementController( PlayerManager owner, PlayerAnimator animator ) {
			_owner = owner;
			_animator = animator;

			var eventFactory = owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_attackController = new PlayerAttackController( owner, animator, eventFactory );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			var playerDeath = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.PlayerDeath ) );
			playerDeath.Subscribe( this, OnPlayerDeath );

			_startPosition = owner.GlobalPosition;
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
		public void Update( float delta ) {
		}

		/*
		===============
		FixedUpdate
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="delta"></param>
		/// <param name="inputWasActive"></param>
		public void FixedUpdate( float delta, out bool inputWasActive ) {
			inputWasActive = false;

			if ( ( _flags & FlagBits.WaveActive ) == 0 || ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}
			if ( ( _flags & FlagBits.CanMove ) != 0 ) {
				Vector2 inputVelocity = new Vector2(
					Input.GetAxis( MoveWestBind, MoveEastBind ),
					Input.GetAxis( MoveNorthBind, MoveSouthBind )
				);
				inputWasActive = inputVelocity != Vector2.Zero;

				EntityUtils.CalcSpeed( ref _frameVelocity, new Vector2( _movementSpeed, _movementSpeed ), delta, inputVelocity );

				_owner.Velocity = _frameVelocity;
				_owner.MoveAndSlide();
				_frameVelocity = _owner.Velocity;
			}
		}

		/*
		===============
		OnPlayerDeath
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnPlayerDeath( in EmptyEventArgs args ) {
			_flags |= FlagBits.Dead;
		}

		/*
		===============
		OnWaveStarted
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveStarted( in EmptyEventArgs args ) {
			_flags |= FlagBits.CanMove | FlagBits.WaveActive;
			_owner.GlobalPosition = _startPosition;
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
			_flags &= ~FlagBits.WaveActive;
		}

		/*
		===============
		OnStatChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnStatChanged( in StatChangedEventArgs args ) {
			if ( args.StatId == PlayerStats.SPEED ) {
				_movementSpeed = args.Value;
			}
		}
	};
};
