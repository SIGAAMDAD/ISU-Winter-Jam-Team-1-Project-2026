using Game.Common;
using Game.Player.Upgrades;
using Game.Player.Weapons;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System;
using Systems.Caching;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerController

	FIXME: this does too much
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class PlayerController {
		public const float BASE_WEAPON_COOLDOWN_TIME = 1.5f;

		[Flags]
		private enum FlagBits : byte {
			CanMove = 1 << 0,
			CanAttack = 1 << 1,

			WaveActive = 1 << 2,

			Dead = 1 << 3
		};

		private static readonly StringName @MoveEastBind = "move_east";
		private static readonly StringName @MoveWestBind = "move_west";
		private static readonly StringName @MoveNorthBind = "move_north";
		private static readonly StringName @MoveSouthBind = "move_south";

		private readonly PackedScene[] _harpoonPrefabs;

		private readonly Vector2 _startPosition;

		private readonly PlayerManager _owner;
		private readonly PlayerAnimator _animator;
		private readonly Timer _weaponCooldown;

		private HarpoonType _harpoonType;

		private FlagBits _flags = FlagBits.CanAttack | FlagBits.CanMove | FlagBits.WaveActive;
		private Vector2 _frameVelocity = Vector2.Zero;

		private float _movementSpeed = 0.0f;

		public IGameEvent<EmptyEventArgs> UseWeapon => _useWeapon;
		private readonly IGameEvent<EmptyEventArgs> _useWeapon;

		public IGameEvent<EmptyEventArgs> WeaponCooldownFinished => _weaponCooldownFinished;
		private readonly IGameEvent<EmptyEventArgs> _weaponCooldownFinished;

		/*
		===============
		PlayerController
		===============
		*/
		/// <summary>
		/// Creates a PlayerController
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="animator"></param>
		/// <param name="stats"></param>
		public PlayerController( PlayerManager owner, PlayerAnimator animator ) {
			_owner = owner;
			_animator = animator;

			var eventFactory = owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_useWeapon = eventFactory.GetEvent<EmptyEventArgs>( nameof( UseWeapon ) );
			_weaponCooldownFinished = eventFactory.GetEvent<EmptyEventArgs>( nameof( WeaponCooldownFinished ) );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			var harpoonType = eventFactory.GetEvent<HarpoonTypeChangedEventArgs>( nameof( PlayerStats.HarpoonTypeChanged ) );
			harpoonType.Subscribe( this, OnHarpoonTypeChanged );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			var playerDeath = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStats.PlayerDeath ) );
			playerDeath.Subscribe( this, OnPlayerDeath );

			_weaponCooldown = new Timer() {
				WaitTime = 1.5f
			};
			_weaponCooldown.Connect( Timer.SignalName.Timeout, Callable.From( OnWeaponCooldownFinished ) );
			owner.AddChild( _weaponCooldown );

			_startPosition = owner.GlobalPosition;

			_harpoonPrefabs = new PackedScene[ (int)HarpoonType.Count ];
			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/UpHarpoon.tscn" ) ).Get( out _harpoonPrefabs[ (int)HarpoonType.Default ] );
			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/Explosive/ExplosiveHarpoon.tscn" ) ).Get( out _harpoonPrefabs[ (int)HarpoonType.ExplosiveHarpoon ] );
			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/Icy/IcyHarpoon.tscn" ) ).Get( out _harpoonPrefabs[ (int)HarpoonType.IcyHarpoon ] );
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
			// if we're not in an active wave, just ignore the call
			if ( ( _flags & FlagBits.WaveActive ) == 0 || ( _flags & FlagBits.Dead ) != 0 ) {
				return;
			}
			if ( ( _flags & FlagBits.CanAttack ) != 0 ) {
				OnUseWeapon();
				_flags &= ~FlagBits.CanAttack;
				_weaponCooldown.Start();
			}
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
		OnWeaponCooldownFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnWeaponCooldownFinished() {
			_flags |= FlagBits.CanAttack;
			_weaponCooldownFinished.Publish( EmptyEventArgs.Args );
		}

		/*
		===============
		OnHarpoonTypeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private void OnHarpoonTypeChanged( in HarpoonTypeChangedEventArgs args ) {
			_harpoonType = args.Type;
		}

		/*
		===============
		OnUseWeapon
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnUseWeapon() {
			Projectile harpoon = _harpoonPrefabs[ (int)_harpoonType ].Instantiate<Projectile>();
			harpoon.Direction = _animator.Direction;
			harpoon.GlobalPosition = _animator.AimPosition;
			harpoon.RotationDegrees = _animator.AimAngle;
			harpoon.MoveDirection = _owner.GlobalPosition.DirectionTo( _owner.GetGlobalMousePosition() );
			_owner.GetTree().Root.AddChild( harpoon );

			_useWeapon.Publish( EmptyEventArgs.Args );
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
			_flags |= FlagBits.CanAttack | FlagBits.CanMove | FlagBits.WaveActive;
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
			if ( args.StatId == PlayerStats.ATTACK_SPEED ) {
				_weaponCooldown.WaitTime = args.Value;
			} else if ( args.StatId == PlayerStats.SPEED ) {
				_movementSpeed = args.Value;
			}
		}
	};
};