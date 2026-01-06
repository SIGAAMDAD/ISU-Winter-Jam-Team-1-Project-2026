using Game.Common;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Prefabs;
using System.Runtime.CompilerServices;
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
		private static readonly StringName @MoveEastBind = "move_east";
		private static readonly StringName @MoveWestBind = "move_west";
		private static readonly StringName @MoveNorthBind = "move_north";
		private static readonly StringName @MoveSouthBind = "move_south";
		private static readonly StringName @UseWeaponBind = "use_weapon";

		private static readonly Vector2I MIDDLE_SCREEN_POSITION = new Vector2I( 1280 / 2, 720 / 2 );

		public IGameEvent<EmptyEventArgs> UseWeapon => _useWeapon;
		private readonly IGameEvent<EmptyEventArgs> _useWeapon;

		public IGameEvent<EmptyEventArgs> WeaponCooldownFinished => _weaponCooldownFinished;
		private readonly IGameEvent<EmptyEventArgs> _weaponCooldownFinished;

		public IGameEvent<WeaponCooldownTimeChangedEventArgs> WeaponCooldownTimeChanged => _weaponCooldownTimeChanged;
		private readonly IGameEvent<WeaponCooldownTimeChangedEventArgs> _weaponCooldownTimeChanged;

		private readonly PackedScene[] _harpoonPrefabs = new PackedScene[ (int)PlayerDirection.Count ];

		private readonly PlayerStats _stats;
		private readonly PlayerManager _owner;
		private readonly PlayerAnimator _animator;
		private readonly Timer _weaponCooldown;

		private bool _canAttack = true;
		private Vector2 _frameVelocity = Vector2.Zero;

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
			_weaponCooldownTimeChanged = eventFactory.GetEvent<WeaponCooldownTimeChangedEventArgs>( nameof( WeaponCooldownTimeChanged ) );

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
			Vector2 inputVelocity = Input.GetVector( MoveWestBind, MoveEastBind, MoveNorthBind, MoveSouthBind );
			inputWasActive = inputVelocity != Vector2.Zero;

			EntityUtils.CalcSpeed( ref _frameVelocity, _stats.Speed, delta, inputVelocity );

			_owner.Velocity = _frameVelocity;
			_owner.MoveAndSlide();
			_frameVelocity = _owner.Velocity;

			if ( _canAttack ) {
				float angle = GetMouseAngle();

				OnUseWeapon( angle );
				_canAttack = false;
				_weaponCooldown.Start();
			} else if ( !_canAttack ) {
				_weaponCooldownTimeChanged.Publish( new WeaponCooldownTimeChangedEventArgs( 100.0f / (float)_weaponCooldown.TimeLeft ) );
			}
		}

		/*
		===============
		GetMouseAngle
		===============
		*/
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		private float GetMouseAngle() {
			return _owner.GetLocalMousePosition().AngleTo( MIDDLE_SCREEN_POSITION );
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
			_canAttack = true;
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
		private void OnUseWeapon( float angle ) {
			Harpoon harpoon = _harpoonPrefabs[ (int)_animator.Direction ].Instantiate<Harpoon>();
			harpoon.Direction = _animator.Direction;
			harpoon.GlobalPosition = _owner.GlobalPosition;
			harpoon.GlobalRotation = angle;
			_owner.GetTree().Root.AddChild( harpoon );

			_useWeapon.Publish( new EmptyEventArgs() );
		}
	};
};