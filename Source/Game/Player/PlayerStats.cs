using Game.Common;
using Game.Mobs;
using Game.Player.Upgrades;
using Game.Player.UserInterface.UpgradeInterface;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System;
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

	public sealed class PlayerStats : IPlayerStatsProvider {
		public static readonly InternString HEALTH = new( nameof( Health ) );
		public static readonly InternString SPEED = new( nameof( Speed ) );
		public static readonly InternString ATTACK_DAMAGE = new( nameof( AttackDamage ) );
		public static readonly InternString ATTACK_SPEED = new( nameof( AttackSpeed) );
		public static readonly InternString ARMOR = new( nameof( Armor ) );
		public static readonly InternString MAX_HEALTH = new( nameof( MaxHealth ) );
		public static readonly InternString MONEY = new( nameof( Money ) );
		public static readonly InternString HEALTH_REGEN = new( nameof( HealthRegen ) );

		public float HealthRegen => _statCache[ HEALTH_REGEN ];
		public float Speed => _statCache[ SPEED ];
		public float Health => _statCache[ HEALTH ];
		public float AttackDamage => _statCache[ ATTACK_DAMAGE ];
		public float AttackSpeed => _statCache[ ATTACK_SPEED ];
		public float Armor => _statCache[ ARMOR ];
		public float MaxHealth => _statCache[ MAX_HEALTH ];
		public float Money => _statCache[ MONEY ];

		private readonly Dictionary<InternString, float> _statCache = new Dictionary<InternString, float> {
			[ SPEED ] = 100.0f,
			[ HEALTH ] = 100.0f,
			[ HEALTH_REGEN ] = 1.0f,
			[ ARMOR ] = 0.0f,
			[ ATTACK_DAMAGE ] = 10.0f,
			[ MAX_HEALTH ] = 100.0f,
			[ MONEY ] = 0.0f,
		};
		private readonly ImmutableDictionary<UpgradeType, InternString> _upgradeToStatId;

		private readonly int _entityId;

		public IGameEvent<EntityTakeDamageEventArgs> TakeDamage => _takeDamage;
		private readonly IGameEvent<EntityTakeDamageEventArgs> _takeDamage;

		public IGameEvent<StatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<StatChangedEventArgs> _statChanged;

		/*
		===============
		PlayerStats
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="upgradeManager"></param>
		/// <param name="eventFactory"></param>
		public PlayerStats( PlayerManager player, IGameEventRegistryService eventFactory ) {
			_entityId = player.GetPath().GetHashCode();

			_takeDamage = eventFactory.GetEvent<EntityTakeDamageEventArgs>( nameof( TakeDamage ) );
			_statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( StatChanged ) );

			var mobDie = eventFactory.GetEvent<MobDieEventArgs>( nameof( MobBase.MobDie ) );
			mobDie.Subscribe( this, OnMobKilled );

			var upgradeBought = eventFactory.GetEvent<UpgradeBoughtEventArgs>( nameof( UpgradeManager.UpgradeBought ) );
			upgradeBought.Subscribe( this, OnUpgradeBought );

			_upgradeToStatId = new Dictionary<UpgradeType, InternString> {
				[ UpgradeType.Speed ] = SPEED,
				[ UpgradeType.MaxHealth ] = MAX_HEALTH,
				[ UpgradeType.HealthRegen ] = HEALTH_REGEN,
				[ UpgradeType.Armor ] = ARMOR,
				[ UpgradeType.AttackDamage ] = ATTACK_DAMAGE,
				[ UpgradeType.AttackSpeed ] = ATTACK_SPEED,
			}.ToImmutableDictionary();

			foreach ( var stat in _statCache ) {
				_statChanged.Publish( new StatChangedEventArgs( stat.Key, stat.Value ) );
			}
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			_statCache.Clear();
			_takeDamage.Dispose();
			_statChanged.Dispose();
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
			ref float health = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, HEALTH, out _ );
			float maxHealth = MaxHealth;

			if ( health >= maxHealth ) {
				return;
			}
			health = Math.Clamp( health + HealthRegen * delta, 0.0f, maxHealth );
		}

		/*
		===============
		OnMobKilled
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnMobKilled( in MobDieEventArgs args ) {
			float money = Money;
			money += args.XPAmount;
			_statCache[ MONEY ] = money;

			_statChanged.Publish( new StatChangedEventArgs( MONEY, money ) );
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

			ref float money = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, MONEY, out _ );
			money -= args.Cost;

			_statChanged.Publish( new StatChangedEventArgs( MONEY, money ) );
			_statChanged.Publish( new StatChangedEventArgs( statId, value ) );
		}
	};
};