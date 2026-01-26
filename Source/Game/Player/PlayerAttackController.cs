using System;
using Game.Common;
using Game.Player.Upgrades;
using Game.Player.Weapons;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Prefabs;
using Systems.Caching;

namespace Game.Player {
	/*
	===================================================================================

	PlayerAttackController

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class PlayerAttackController {
		private static readonly StringName @PrevWeaponToggleBind = "prev_weapon";
		private static readonly StringName @NextWeaponToggleBind = "next_weapon";

		public const float BASE_WEAPON_COOLDOWN_TIME = 1.5f;
		public const float BASE_WEAPON_DAMAGE = 1.0f;

		private record HarpoonData(
			Timer CooldownTimer,
			bool Owned,
			PackedScene Prefab
		);

		[Flags]
		private enum FlagBits : byte {
			CanAttack = 1 << 0,
			IsAttacking = 1 << 1,
			ActiveWave = 1 << 2
		};

		private HarpoonType _currentHarpoon = HarpoonType.Default;
		private float _baseDamage = BASE_WEAPON_DAMAGE;
		private float _baseCooldown = BASE_WEAPON_COOLDOWN_TIME;
		private FlagBits _flags = FlagBits.CanAttack | FlagBits.ActiveWave;

		private readonly HarpoonData[] _harpoons = new HarpoonData[ ( int )HarpoonType.Count ];

		private readonly PlayerManager _owner;
		private readonly PlayerAnimator _animator;

		public IGameEvent<EmptyEventArgs> UseWeapon => _useWeapon;
		private readonly IGameEvent<EmptyEventArgs> _useWeapon;

		public IGameEvent<EmptyEventArgs> WeaponCooldownFinished => _weaponCooldownFinished;
		private readonly IGameEvent<EmptyEventArgs> _weaponCooldownFinished;

		public IGameEvent<PlayerHarpoonChangedEventArgs> HarpoonChanged => _harpoonChanged;
		private readonly IGameEvent<PlayerHarpoonChangedEventArgs> _harpoonChanged;

		public IGameEvent<HarpoonCooldownChangedEventArgs> HarpoonCooldownChanged => _harpoonCooldownChanged;
		private readonly IGameEvent<HarpoonCooldownChangedEventArgs> _harpoonCooldownChanged;

		/*
		===============
		PlayerAttackController
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="animator"></param>
		/// <param name="eventFactory"></param>
		public PlayerAttackController( PlayerManager owner, PlayerAnimator animator, IGameEventRegistryService eventFactory ) {
			_owner = owner;
			_animator = animator;

			for ( int i = 0; i < ( int )HarpoonType.Count; i++ ) {
				var cooldown = new Timer() {
					WaitTime = BASE_WEAPON_COOLDOWN_TIME,
					OneShot = true
				};
				cooldown.Connect( Timer.SignalName.Timeout, Callable.From( OnCooldownTimerTimeout ) );
				owner.AddChild( cooldown );

				_harpoons[ i ] = new HarpoonData(
					CooldownTimer: cooldown,
					Owned: false,
					Prefab: null
				);
			}

			_harpoons[ ( int )HarpoonType.Default ] = _harpoons[ (int)HarpoonType.Default ] with { Owned = true };

			var harpoonTypeChanged = eventFactory.GetEvent<HarpoonTypeChangedEventArgs>( nameof( PlayerAttackController ), nameof( PlayerStats.HarpoonTypeChanged ) );
			harpoonTypeChanged.Subscribe( this, OnHarpoonTypeChanged );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerAttackController ), nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_harpoonChanged = eventFactory.GetEvent<PlayerHarpoonChangedEventArgs>( nameof( PlayerAttackController ), nameof( HarpoonChanged ) );
			_weaponCooldownFinished = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerAttackController ), nameof( WeaponCooldownFinished ) );
			_useWeapon = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerAttackController ), nameof( UseWeapon ) );
			_harpoonCooldownChanged = eventFactory.GetEvent<HarpoonCooldownChangedEventArgs>( nameof( PlayerAttackController ), nameof( HarpoonCooldownChanged ) );

			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/Default/DefaultHarpoon.tscn" ) ).Get( out var defaultHarpoon );
			_harpoons[ ( int )HarpoonType.Default ] = _harpoons[ ( int )HarpoonType.Default ] with { Prefab = defaultHarpoon };

			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/Explosive/ExplosiveHarpoon.tscn" ) ).Get( out var explosiveHarpoon );
			_harpoons[ ( int )HarpoonType.ExplosiveHarpoon ] = _harpoons[ ( int )HarpoonType.ExplosiveHarpoon ] with { Prefab = explosiveHarpoon };

			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/Icy/IcyHarpoon.tscn" ) ).Get( out var icyHarpoon );
			_harpoons[ ( int )HarpoonType.IcyHarpoon ] = _harpoons[ ( int )HarpoonType.IcyHarpoon ] with { Prefab = icyHarpoon };

			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/Stationary/StationaryHarpoon.tscn" ) ).Get( out var stationaryHarpoon );
			_harpoons[ ( int )HarpoonType.StationaryHarpoon ] = _harpoons[ ( int )HarpoonType.StationaryHarpoon ] with { Prefab = stationaryHarpoon };
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
		public void FixedUpdate( float delta ) {
			if ( (_flags & FlagBits.ActiveWave) == 0 ) {
				return;
			}
			for ( int i = 0; i < ( int )HarpoonType.Count; i++ ) {
				float timeLeft = ( float )_harpoons[ i ].CooldownTimer.TimeLeft;
				if ( timeLeft > 0.0f ) {
					_harpoonCooldownChanged.Publish( new HarpoonCooldownChangedEventArgs( ( HarpoonType )i, ( float )( _harpoons[ i ].CooldownTimer.WaitTime - timeLeft ) ) );
				} else if ( ( int )_currentHarpoon == i ) {
					OnUseWeapon();
					_flags |= FlagBits.IsAttacking;
				}
			}
			if ( Input.IsActionJustPressed( PrevWeaponToggleBind ) ) {
				OnPrevWeaponToggle();
			}
			if ( Input.IsActionJustPressed( NextWeaponToggleBind ) ) {
				OnNextWeaponToggle();
			}
		}

		/*
		===============
		OnCooldownTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnCooldownTimerTimeout() {
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
			_harpoons[ ( int )args.Type ] = _harpoons[ (int)args.Type ] with { Owned = true };
			_currentHarpoon = args.Type;
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
			_flags &= ~FlagBits.ActiveWave;
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
			_flags |= FlagBits.ActiveWave;
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
			int current = ( int )_currentHarpoon;
			Projectile harpoon = _harpoons[ current ].Prefab.Instantiate<Projectile>();
			harpoon.Direction = _animator.Direction;
			harpoon.GlobalPosition = _animator.AimPosition;
			harpoon.RotationDegrees = _animator.AimAngle;
			harpoon.MoveDirection = _owner.GlobalPosition.DirectionTo( _owner.GetGlobalMousePosition() );
			harpoon.DamageScale = _baseDamage;

			_harpoons[ current ].CooldownTimer.WaitTime = _baseCooldown * harpoon.CooldownTime;
			_harpoons[ current ].CooldownTimer.Start();

			_owner.GetTree().Root.AddChild( harpoon );

			_useWeapon.Publish( EmptyEventArgs.Args );
		}

		/*
		===============
		OnNextWeaponToggle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnNextWeaponToggle() {
			SetHarpoonSlot( 1 );
		}

		/*
		===============
		OnPrevWeaponToggle
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnPrevWeaponToggle() {
			SetHarpoonSlot( -1 );
		}

		/*
		===============
		SetHarpoonSlot
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="direction"></param>
		private void SetHarpoonSlot( int direction ) {
			_currentHarpoon = ( HarpoonType )GetAvailableHarpoonType( direction );
			_harpoonChanged.Publish( new PlayerHarpoonChangedEventArgs( _currentHarpoon ) );
		}

		/*
		===============
		GetAvailableHarpoonType
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="direction"></param>
		/// <returns></returns>
		private int GetAvailableHarpoonType( int direction ) {
			int startIndex = ( int )_currentHarpoon;
			int harpoonTypeCount = (int)HarpoonType.Count;
			int index = startIndex + direction;
			if ( index < 0 ) {
				index = harpoonTypeCount - 1;
			}

			while ( index != startIndex ) {
				if ( _harpoons[ index ].Owned ) {
					break;
				}
				index += direction;
				if ( index >= harpoonTypeCount ) {
					index = 0;
				} else if ( index < 0 ) {
					index = harpoonTypeCount - 1;
				}
			}
			return index;
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
				_baseCooldown = args.Value;
			} else if ( args.StatId == PlayerStats.ATTACK_DAMAGE ) {
				_baseDamage = args.Value;
			}
		}
	};
};
