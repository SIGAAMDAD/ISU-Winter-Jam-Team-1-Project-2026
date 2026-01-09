using Game.Common;
using Game.Mobs;
using Game.Player.Upgrades;
using Game.Player.UserInterface;
using Game.Player.UserInterface.UpgradeInterface;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Prefabs;
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
		public static readonly InternString ATTACK_SPEED = new( nameof( AttackSpeed ) );
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

		private readonly PlayerManager _owner;

		private readonly Dictionary<InternString, float> _statCache = new Dictionary<InternString, float> {
			[ SPEED ] = 100.0f,
			[ HEALTH ] = 100.0f,
			[ HEALTH_REGEN ] = 1.0f,
			[ ARMOR ] = 0.0f,
			[ ATTACK_DAMAGE ] = 10.0f,
			[ ATTACK_SPEED ] = 1.25f,
			[ MAX_HEALTH ] = 100.0f,
			[ MONEY ] = 0.0f,
		};
		private readonly ImmutableDictionary<UpgradeType, InternString> _upgradeToStatId = new Dictionary<UpgradeType, InternString> {
			[ UpgradeType.Speed ] = SPEED,
			[ UpgradeType.MaxHealth ] = MAX_HEALTH,
			[ UpgradeType.HealthRegen ] = HEALTH_REGEN,
			[ UpgradeType.Armor ] = ARMOR,
			[ UpgradeType.AttackDamage ] = ATTACK_DAMAGE,
			[ UpgradeType.AttackSpeed ] = ATTACK_SPEED,
		}.ToImmutableDictionary();

		private readonly ImmutableDictionary<UpgradeType, float> _baseStatValues;

		public IGameEvent<PlayerTakeDamageEventArgs> TakeDamage => _takeDamage;
		private readonly IGameEvent<PlayerTakeDamageEventArgs> _takeDamage;

		public IGameEvent<StatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<StatChangedEventArgs> _statChanged;

		public IGameEvent<HarpoonType> HarpoonTypeChanged => _harpoonTypeChanged;
		private readonly IGameEvent<HarpoonType> _harpoonTypeChanged;

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
			_owner = player;

			_takeDamage = eventFactory.GetEvent<PlayerTakeDamageEventArgs>( nameof( TakeDamage ) );
			_takeDamage.Subscribe( this, OnDamageReceived );

			_statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( StatChanged ) );
			_harpoonTypeChanged = eventFactory.GetEvent<HarpoonType>( nameof( HarpoonTypeChanged ) );

			var mobDie = eventFactory.GetEvent<MobDieEventArgs>( nameof( MobBase.MobDie ) );
			mobDie.Subscribe( this, OnMobKilled );

			var upgradeBought = eventFactory.GetEvent<UpgradeBoughtEventArgs>( nameof( UpgradeManager.UpgradeBought ) );
			upgradeBought.Subscribe( this, OnUpgradeBought );

			var harpoonTypeBought = eventFactory.GetEvent<HarpoonTypeUpgradeBoughtEventArgs>( nameof( UpgradeManager.HarpoonBought ) );
			harpoonTypeBought.Subscribe( this, OnHarpoonTypeChanged );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_baseStatValues = new Dictionary<UpgradeType, float> {
				[ UpgradeType.Speed ] = _statCache[ SPEED ],
				[ UpgradeType.MaxHealth ] = _statCache[ MAX_HEALTH ],
				[ UpgradeType.HealthRegen ] = _statCache[ HEALTH_REGEN ],
				[ UpgradeType.Armor ] = _statCache[ ARMOR ],
				[ UpgradeType.AttackDamage ] = _statCache[ ATTACK_DAMAGE ],
				[ UpgradeType.AttackSpeed ] = _statCache[ ATTACK_SPEED ]
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
			health = Math.Clamp( health + HealthRegen, 0.0f, maxHealth );
		}

		/*
		===============
		OnDamageReceived
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		private void OnDamageReceived( in PlayerTakeDamageEventArgs args ) {
			// TODO: clean this up
			float health = Health;
			health -= args.Amount;
			_statCache[ HEALTH ] = health;

			_statChanged.Publish( new StatChangedEventArgs( HEALTH, health ) );

			var damageNumber = new DamageNumberLabel() {
				GlobalPosition = _owner.GlobalPosition,
				Value = args.Amount
			};
			_owner.GetTree().Root.CallDeferred( Node.MethodName.AddChild, damageNumber );
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
			ref float health = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, HEALTH, out _ );
			health = MaxHealth;
			_statChanged.Publish( new StatChangedEventArgs( HEALTH, health ) );
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
		OnHarpoonTypeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnHarpoonTypeChanged( in HarpoonTypeUpgradeBoughtEventArgs args ) {
			ref float money = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, MONEY, out _ );
			money -= args.Cost;

			_harpoonTypeChanged.Publish( args.Type );
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
			value = _baseStatValues[ args.Type ] * args.MultiplyAmount;

			ref float money = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, MONEY, out _ );
			money -= args.Cost;

			_statChanged.Publish( new StatChangedEventArgs( MONEY, money ) );
			_statChanged.Publish( new StatChangedEventArgs( statId, value ) );
		}
	};
};