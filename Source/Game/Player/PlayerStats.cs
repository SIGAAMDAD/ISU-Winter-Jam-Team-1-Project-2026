using Game.Common;
using Game.Player.Upgrades;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System;
using System.Collections.Generic;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerStats
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class PlayerStats {
		private const int MAX_SLOTS = 2;

		public static readonly InternString HEALTH = new( nameof( Health ) );
		public static readonly InternString SPEED = new( nameof( Speed ) );
		public static readonly InternString DAMAGE_RESISTANCE = new( nameof( DamageResistance ) );
		public static readonly InternString ATTACK_DAMAGE = new( nameof( AttackDamage ));
		public static readonly InternString ARMOR = new( nameof( Armor ) );
		public static readonly InternString TOTAL_DAMAGE = new( nameof( TotalDamage ) );
		public static readonly InternString MAX_HEALTH = new( nameof( MaxHealth ) );

		public float Speed => _statCache[ SPEED ];
		public float Health => _statCache[ HEALTH ];
		public float DamageResistance => _statCache[ DAMAGE_RESISTANCE ];
		public float AttackDamage => _statCache[ ATTACK_DAMAGE ];
		public float Armor => _statCache[ ARMOR ];
		public float TotalDamage => _statCache[ TOTAL_DAMAGE ];
		public float MaxHealth => _statCache[ MAX_HEALTH ];

		public WeaponSlot[] Slots => _slots;
		private readonly WeaponSlot[] _slots = new WeaponSlot[ MAX_SLOTS ];

		public float this[ InternString statName ] {
			get {
				return _statCache[ statName ];
			}
			set {
				_statCache[ statName ] = value;
				_statChanged.Publish( new StatChangedEventArgs( statName, value ) );
			}
		}

		private readonly Dictionary<InternString, float> _statCache = new Dictionary<InternString, float> {
			[ SPEED ] = 100.0f,
			[ HEALTH ] = 100.0f,
			[ DAMAGE_RESISTANCE ] = 0.95f,
			[ ARMOR ] = 100.0f,
			[ TOTAL_DAMAGE ] = 1.0f,
			[ MAX_HEALTH ] = 100.0f
		};

		private readonly int _entityId;

		public IGameEvent<EntityTakeDamageEventArgs> TakeDamage => _takeDamage;
		private readonly IGameEvent<EntityTakeDamageEventArgs> _takeDamage;

		public IGameEvent<StatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<StatChangedEventArgs> _statChanged;

		public PlayerStats( PlayerManager player, UpgradeManager upgradeManager, IGameEventRegistryService eventFactory ) {
			_entityId = player.GetPath().GetHashCode();

			_takeDamage = eventFactory.GetEvent<EntityTakeDamageEventArgs>( nameof( TakeDamage ) );
			_statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( StatChanged ) );

			upgradeManager.UpgradeBought.Subscribe( this, OnUpgradeBought );
		}

		/*
		===============
		OnUpgradeBought
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnUpgradeBought( in UpgradeBoughtEventArgs args ) {
			// FINISH THIS!
		}

		/*
		===============
		Damage
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void Damage( float value ) {
			// TODO: clean this up
			float health = Health;
			health -= value;
			_statCache[ HEALTH ] = health;

			_takeDamage.Publish( new EntityTakeDamageEventArgs( _entityId, value ) );
			_statChanged.Publish( new StatChangedEventArgs( HEALTH, health ) );
		}
	};
};