using Game.Common;
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
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class PlayerController {
		[Flags]
		private enum FlagBits : byte {
			CanMove = 1 << 0,
			CanAttack = 1 << 1,

			WaveActive = 1 << 2
		};

		private static readonly StringName @MoveEastBind = "move_east";
		private static readonly StringName @MoveWestBind = "move_west";
		private static readonly StringName @MoveNorthBind = "move_north";
		private static readonly StringName @MoveSouthBind = "move_south";
		private static readonly StringName @UseWeaponBind = "use_weapon";

		private readonly PackedScene[] _harpoonPrefabs = new PackedScene[ (int)PlayerDirection.Count ];

		private readonly PlayerStats _stats;
		private readonly PlayerManager _owner;
		private readonly PlayerAnimator _animator;
		private readonly Timer _weaponCooldown;

		private FlagBits _flags;
		private Vector2 _frameVelocity = Vector2.Zero;

		public IGameEvent<EmptyEventArgs> UseWeapon => _useWeapon;
		private readonly IGameEvent<EmptyEventArgs> _useWeapon;

		public IGameEvent<EmptyEventArgs> WeaponCooldownFinished => _weaponCooldownFinished;
		private readonly IGameEvent<EmptyEventArgs> _weaponCooldownFinished;

		/*
		===============
		PlayerController
		===============
		*/
		public PlayerController( PlayerManager owner, PlayerAnimator animator, PlayerStats stats ) {
			_stats = stats;
			_owner = owner;
			_animator = animator;

			var eventFactory = owner.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_useWeapon = eventFactory.GetEvent<EmptyEventArgs>( nameof( UseWeapon ) );
			_weaponCooldownFinished = eventFactory.GetEvent<EmptyEventArgs>( nameof( WeaponCooldownFinished ) );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_weaponCooldown = new Timer() {
				WaitTime = 1.5f
			};
			_weaponCooldown.Connect( Timer.SignalName.Timeout, Callable.From( OnWeaponCooldownFinished ) );
			owner.AddChild( _weaponCooldown );

			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/UpHarpoon.tscn" ) ).Get( out _harpoonPrefabs[ (int)PlayerDirection.North ] );
			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/RightHarpoon.tscn" ) ).Get( out _harpoonPrefabs[ (int)PlayerDirection.East ] );
			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/DownHarpoon.tscn" ) ).Get( out _harpoonPrefabs[ (int)PlayerDirection.South ] );
			SceneCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Prefabs/Weapons/Harpoon/LeftHarpoon.tscn" ) ).Get( out _harpoonPrefabs[ (int)PlayerDirection.West ] );
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
		public void Update( float delta, out bool inputWasActive ) {
			inputWasActive = false;

			// if we're not in an active wave, just ignore the call
			if ( ( _flags & FlagBits.WaveActive ) == 0 ) {
				return;
			}
			if ( ( _flags & FlagBits.CanMove ) != 0 ) {
				Vector2 inputVelocity = Input.GetVector( MoveWestBind, MoveEastBind, MoveNorthBind, MoveSouthBind );
				inputWasActive = inputVelocity != Vector2.Zero;

				EntityUtils.CalcSpeed( ref _frameVelocity, _stats.Speed, delta, inputVelocity );

				_owner.Velocity = _frameVelocity;
				_owner.MoveAndSlide();
				_frameVelocity = _owner.Velocity;
			}
			if ( ( _flags & FlagBits.CanAttack ) != 0 ) {
				OnUseWeapon();
				_flags &= ~FlagBits.CanAttack;
				_weaponCooldown.Start();
			}
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
			_weaponCooldownFinished.Publish( new EmptyEventArgs() );
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
			float angle = _owner.GlobalPosition.DirectionTo( _owner.GetGlobalMousePosition() ).Angle();

			Projectile harpoon = _harpoonPrefabs[ 0 ].Instantiate<Projectile>();
			harpoon.Direction = _animator.Direction;
			harpoon.GlobalPosition = _owner.GlobalPosition;
			harpoon.Rotation = angle;
			_owner.GetTree().Root.AddChild( harpoon );

			_useWeapon.Publish( new EmptyEventArgs() );
		}

		/*
		===============
		OnWaveStarted
		===============
		*/
		private void OnWaveStarted( in WaveChangedEventArgs args ) {
			_flags |= FlagBits.CanAttack | FlagBits.CanMove | FlagBits.WaveActive;
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			_flags &= ~( FlagBits.CanMove | FlagBits.WaveActive );
		}
	};
};