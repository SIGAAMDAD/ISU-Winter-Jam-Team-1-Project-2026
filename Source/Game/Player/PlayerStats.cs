using Game.Common;
using Game.Mobs;
using Game.Player.Upgrades;
using Game.Player.UserInterface;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Events;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
		[Flags]
		private enum FlagBits : byte {
			CanDamage = 1 << 0
		};

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
			[ ATTACK_DAMAGE ] = PlayerAttackController.BASE_WEAPON_DAMAGE,
			[ ATTACK_SPEED ] = PlayerAttackController.BASE_WEAPON_COOLDOWN_TIME,
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

		private readonly DisposableSubscription<MobDieEventArgs> _mobDieSubscription;
		private readonly DisposableSubscription<UpgradeBoughtEventArgs> _upgradeBoughtSubscription;
		private readonly DisposableSubscription<HarpoonTypeUpgradeBoughtEventArgs> _harpoonTypeBoughtSubscription;
		private readonly DisposableSubscription<EmptyEventArgs> _waveStartedSubscription;

		private readonly Timer _healTimer;

		private FlagBits _flags = FlagBits.CanDamage;
		private DamageNumberFactory _numberFactory;

		public IGameEvent<PlayerTakeDamageEventArgs> TakeDamage => _takeDamage;
		private readonly IGameEvent<PlayerTakeDamageEventArgs> _takeDamage;

		public IGameEvent<StatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<StatChangedEventArgs> _statChanged;

		public IGameEvent<EmptyEventArgs> PlayerDeath => _playerDeath;
		private readonly IGameEvent<EmptyEventArgs> _playerDeath;

		public IGameEvent<HarpoonTypeChangedEventArgs> HarpoonTypeChanged => _harpoonTypeChanged;
		private readonly IGameEvent<HarpoonTypeChangedEventArgs> _harpoonTypeChanged;

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

			_healTimer = new Timer() {
				WaitTime = 1.0f,
				OneShot = false
			};
			_healTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnHealTimerTimeout ) );
			_owner.AddChild( _healTimer );
			_healTimer.Start();

			_takeDamage = eventFactory.GetEvent<PlayerTakeDamageEventArgs>( nameof( PlayerStats ), nameof( TakeDamage ) );
			_takeDamage.Subscribe( this, OnDamageReceived );

			_statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( StatChanged ) );
			_harpoonTypeChanged = eventFactory.GetEvent<HarpoonTypeChangedEventArgs>( nameof( PlayerStats ), nameof( HarpoonTypeChanged ) );
			_playerDeath = eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStats ), nameof( PlayerDeath ) );

			_mobDieSubscription = new DisposableSubscription<MobDieEventArgs>(
				eventFactory.GetEvent<MobDieEventArgs>( nameof( MobBase ), nameof( MobBase.MobDie ) ),
				OnMobKilled
			);
			_upgradeBoughtSubscription = new DisposableSubscription<UpgradeBoughtEventArgs>(
				eventFactory.GetEvent<UpgradeBoughtEventArgs>( nameof( PlayerStats ), nameof( UpgradeManager.UpgradeBought ) ),
				OnUpgradeBought
			);
			_harpoonTypeBoughtSubscription = new DisposableSubscription<HarpoonTypeUpgradeBoughtEventArgs>(
				eventFactory.GetEvent<HarpoonTypeUpgradeBoughtEventArgs>( nameof( PlayerStats ), nameof( UpgradeManager.HarpoonBought ) ),
				OnHarpoonTypeBought
			);
			_waveStartedSubscription = new DisposableSubscription<EmptyEventArgs>(
				eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveStarted ) ),
				OnWaveStarted
			);

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			Callable.From( GetDamageFactory ).CallDeferred();

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
			_mobDieSubscription.Dispose();
			_waveStartedSubscription.Dispose();
			_harpoonTypeChanged.Dispose();
			_upgradeBoughtSubscription.Dispose();
			_harpoonTypeBoughtSubscription.Dispose();
		}

		/*
		===============
		OnHealTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnHealTimerTimeout() {
			ref float health = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, HEALTH, out _ );
			float maxHealth = MaxHealth;

			if ( health >= maxHealth ) {
				return;
			}

			health = Math.Clamp( health + HealthRegen, 0.0f, maxHealth );
			_statChanged.Publish( new StatChangedEventArgs( HEALTH, health ) );
		}

		/*
		===============
		GetDamageFactory
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void GetDamageFactory() {
			_numberFactory = _owner.GetTree().Root.GetNode<DamageNumberFactory>( nameof( DamageNumberFactory ) );
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
			if ( ( _flags & FlagBits.CanDamage ) == 0 ) {
				return;
			}

			float armor = Armor;
			float amount = armor > 0.0f ? args.Amount / armor : args.Amount;

			// TODO: clean this up
			ref float health = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, HEALTH, out _ );
			health -= amount;
			if ( health <= 0.0f ) {
				_playerDeath.Publish( EmptyEventArgs.Args );
			}

			if ( args.Amount > 10.0f ) {
				HitStop( 0.1f );
			}

			_statChanged.Publish( new StatChangedEventArgs( HEALTH, health ) );
			_numberFactory.Add( _owner.GlobalPosition, amount );
		}

		/*
		===============
		HitStop
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		private async ValueTask HitStop( float duration ) {
			double originalTimeScale = Engine.TimeScale;
			Engine.TimeScale = 0.1f;
			await _owner.ToSignal( _owner.GetTree().CreateTimer( duration ), SceneTreeTimer.SignalName.Timeout );
			Engine.TimeScale = originalTimeScale;
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

			_flags |= FlagBits.CanDamage;
			_healTimer.Start();
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name=""></param>
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			_healTimer.Stop();
			_flags &= ~FlagBits.CanDamage;
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
			ref float money = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, MONEY, out _ );
			money += args.XPAmount;

			_statChanged.Publish( new StatChangedEventArgs( MONEY, money ) );
		}

		/*
		===============
		OnHarpoonTypeBought
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnHarpoonTypeBought( in HarpoonTypeUpgradeBoughtEventArgs args ) {
			ref float money = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, MONEY, out _ );
			money -= args.Cost;

			_harpoonTypeChanged.Publish( new HarpoonTypeChangedEventArgs( args.Type ) );
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
			value = _baseStatValues[ args.Type ] * args.MultiplyAmount;

			ref float money = ref CollectionsMarshal.GetValueRefOrAddDefault( _statCache, MONEY, out _ );
			money -= args.Cost;

			_statChanged.Publish( new StatChangedEventArgs( MONEY, money ) );
			_statChanged.Publish( new StatChangedEventArgs( statId, value ) );
		}
	};
};
