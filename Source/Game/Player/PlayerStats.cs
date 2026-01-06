using Game.Common;
using Game.Player.Upgrades;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;

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
		public const int MAX_WEAPON_SLOTS = 2;

		public static readonly InternString HEALTH = new( nameof( Health ) );
		public static readonly InternString SPEED = new( nameof( Speed ) );
		public static readonly InternString DAMAGE_RESISTANCE = new( nameof( DamageResistance ) );
		public static readonly InternString ATTACK_DAMAGE = new( nameof( AttackDamage ) );
		public static readonly InternString ARMOR = new( nameof( Armor ) );
		public static readonly InternString MAX_HEALTH = new( nameof( MaxHealth ) );

		public float Speed => _statCache[ SPEED ];
		public float Health => _statCache[ HEALTH ];
		public float DamageResistance => _statCache[ DAMAGE_RESISTANCE ];
		public float AttackDamage => _statCache[ ATTACK_DAMAGE ];
		public float Armor => _statCache[ ARMOR ];
		public float MaxHealth => _statCache[ MAX_HEALTH ];

		public WeaponSlot[] Slots => _slots;
		private readonly WeaponSlot[] _slots = new WeaponSlot[ MAX_WEAPON_SLOTS ];

		private readonly Dictionary<InternString, float> _statCache = new Dictionary<InternString, float> {
			[ SPEED ] = 100.0f,
			[ HEALTH ] = 100.0f,
			[ DAMAGE_RESISTANCE ] = 0.95f,
			[ ARMOR ] = 100.0f,
			[ ATTACK_DAMAGE ] = 10.0f,
			[ MAX_HEALTH ] = 100.0f
		};
		private readonly ImmutableDictionary<UpgradeType, InternString> _upgradeToStatId;

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

			_upgradeToStatId = new Dictionary<UpgradeType, InternString> {
				[ UpgradeType.Speed ] = SPEED,
				[ UpgradeType.MaxHealth ] = MAX_HEALTH,
				[ UpgradeType.Armor ] = ARMOR,
				[ UpgradeType.AttackDamage ] = ATTACK_DAMAGE,
			}.ToImmutableDictionary();
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
			InternString statId = _upgradeToStatId[ args.Type ];

			ref float value = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, statId, out _ );
			value += args.AddAmount;

			_statChanged.Publish( new StatChangedEventArgs( statId, value ) );
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