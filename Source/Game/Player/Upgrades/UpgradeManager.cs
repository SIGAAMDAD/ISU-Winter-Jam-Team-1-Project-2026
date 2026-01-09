using Godot;
using Nomad.Core.Events;
using Prefabs;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Game.Player.Upgrades {
	/*
	===================================================================================
	
	UpgradeManager
	
	===================================================================================
	*/
	/// <summary>
	/// Handles upgrade tiers and events.
	/// </summary>

	public sealed class UpgradeManager {
		private readonly record struct UpgradeData(
			float Cost,
			float AddAmount
		);

		public const int MAX_UPGRADE_TIER = 4;

		private const float UPGRADE_TIER1_MULTIPLIER = 2.0f;
		private const float UPGRADE_TIER2_MULTIPLIER = 3.0f;
		private const float UPGRADE_TIER3_MULTIPLIER = 4.0f;
		private const float UPGRADE_TIER4_MULTIPLIER = 5.0f;

		private const float UPGRADE_TIER1_COST = 4.0f;
		private const float UPGRADE_TIER2_COST = 12.0f;
		private const float UPGRADE_TIER3_COST = 36.0f;
		private const float UPGRADE_TIER4_COST = 108.0f;

		private float _moneyAmount = 0.0f;

		private readonly Dictionary<UpgradeType, int> _upgrades = new();
		private readonly ImmutableDictionary<UpgradeType, ImmutableDictionary<int, UpgradeData>> _upgradeData;

		private readonly HashSet<HarpoonType> _harpoonUpgrades = new();
		private readonly ImmutableDictionary<HarpoonType, float> _harpoonData;

		public IGameEvent<UpgradeBoughtEventArgs> UpgradeBought => _upgradeBought;
		private IGameEvent<UpgradeBoughtEventArgs> _upgradeBought;

		public IGameEvent<HarpoonTypeUpgradeBoughtEventArgs> HarpoonBought => _harpoonBought;
		private IGameEvent<HarpoonTypeUpgradeBoughtEventArgs> _harpoonBought;

		/*
		===============
		UpgradeManager
		===============
		*/
		/// <summary>
		/// Creates an UpgradeManager.
		/// </summary>
		/// <param name="eventFactory"></param>
		public UpgradeManager( IGameEventRegistryService eventFactory ) {
			_upgradeBought = eventFactory.GetEvent<UpgradeBoughtEventArgs>( nameof( UpgradeBought ) );
			_harpoonBought = eventFactory.GetEvent<HarpoonTypeUpgradeBoughtEventArgs>( nameof( HarpoonBought ) );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			//...
			_upgradeData = new Dictionary<UpgradeType, ImmutableDictionary<int, UpgradeData>> {
				[ UpgradeType.Armor ] = new Dictionary<int, UpgradeData> {
					[ 0 ] = new UpgradeData( 0.0f, 0.0f ),
					[ 1 ] = new UpgradeData( UPGRADE_TIER1_COST, UPGRADE_TIER1_MULTIPLIER ),
					[ 2 ] = new UpgradeData( UPGRADE_TIER2_COST, UPGRADE_TIER2_MULTIPLIER ),
					[ 3 ] = new UpgradeData( UPGRADE_TIER3_COST, UPGRADE_TIER3_MULTIPLIER ),
					[ 4 ] = new UpgradeData( UPGRADE_TIER4_COST, UPGRADE_TIER4_MULTIPLIER )
				}.ToImmutableDictionary(),
				[ UpgradeType.MaxHealth ] = new Dictionary<int, UpgradeData> {
					[ 0 ] = new UpgradeData( 0.0f, 0.0f ),
					[ 1 ] = new UpgradeData( UPGRADE_TIER1_COST, UPGRADE_TIER1_MULTIPLIER ),
					[ 2 ] = new UpgradeData( UPGRADE_TIER2_COST, UPGRADE_TIER2_MULTIPLIER ),
					[ 3 ] = new UpgradeData( UPGRADE_TIER3_COST, UPGRADE_TIER3_MULTIPLIER ),
					[ 4 ] = new UpgradeData( UPGRADE_TIER4_COST, UPGRADE_TIER4_MULTIPLIER )
				}.ToImmutableDictionary(),
				[ UpgradeType.HealthRegen ] = new Dictionary<int, UpgradeData> {
					[ 0 ] = new UpgradeData( 0.0f, 0.0f ),
					[ 1 ] = new UpgradeData( UPGRADE_TIER1_COST, UPGRADE_TIER1_MULTIPLIER ),
					[ 2 ] = new UpgradeData( UPGRADE_TIER2_COST, UPGRADE_TIER2_MULTIPLIER ),
					[ 3 ] = new UpgradeData( UPGRADE_TIER3_COST, UPGRADE_TIER3_MULTIPLIER ),
					[ 4 ] = new UpgradeData( UPGRADE_TIER4_COST, UPGRADE_TIER4_MULTIPLIER )
				}.ToImmutableDictionary(),
				[ UpgradeType.Speed ] = new Dictionary<int, UpgradeData> {
					[ 0 ] = new UpgradeData( 0.0f, 0.0f ),
					[ 1 ] = new UpgradeData( UPGRADE_TIER1_COST, 1.05f ),
					[ 2 ] = new UpgradeData( UPGRADE_TIER2_COST, 1.5f ),
					[ 3 ] = new UpgradeData( UPGRADE_TIER3_COST, 2.75f ),
					[ 4 ] = new UpgradeData( UPGRADE_TIER4_COST, 3.5f )
				}.ToImmutableDictionary(),
				[ UpgradeType.AttackDamage ] = new Dictionary<int, UpgradeData> {
					[ 0 ] = new UpgradeData( 0.0f, 0.0f ),
					[ 1 ] = new UpgradeData( UPGRADE_TIER1_COST, UPGRADE_TIER1_MULTIPLIER ),
					[ 2 ] = new UpgradeData( UPGRADE_TIER2_COST, UPGRADE_TIER2_MULTIPLIER ),
					[ 3 ] = new UpgradeData( UPGRADE_TIER3_COST, UPGRADE_TIER3_MULTIPLIER ),
					[ 4 ] = new UpgradeData( UPGRADE_TIER4_COST, UPGRADE_TIER4_MULTIPLIER )
				}.ToImmutableDictionary(),
				[ UpgradeType.AttackSpeed ] = new Dictionary<int, UpgradeData> {
					[ 0 ] = new UpgradeData( 0.0f, 0.0f ),
					[ 1 ] = new UpgradeData( UPGRADE_TIER1_COST, 0.75f ),
					[ 2 ] = new UpgradeData( UPGRADE_TIER2_COST, 0.60f ),
					[ 3 ] = new UpgradeData( UPGRADE_TIER3_COST, 0.45f ),
					[ 4 ] = new UpgradeData( UPGRADE_TIER4_COST, 0.15f )
				}.ToImmutableDictionary()
			}.ToImmutableDictionary();

			_harpoonData = new Dictionary<HarpoonType, float> {
				[ HarpoonType.ExplosiveHarpoon ] = 32.0f,
				[ HarpoonType.IcyHarpoon ] = 24.0f,
				[ HarpoonType.StationaryHarpoon ] = 28.0f,
			}.ToImmutableDictionary();
		}

		/*
		===============
		GetUpgrade
		===============
		*/
		/// <summary>
		/// Gets an upgrade's tier number.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public int GetUpgradeTier( UpgradeType type ) {
			return _upgrades.TryGetValue( type, out int upgrade ) ? upgrade : 0;
		}

		/*
		===============
		UpgradeIsOwned
		===============
		*/
		/// <summary>
		/// Returns the owner status of an upgrade.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool UpgradeIsOwned( UpgradeType type ) {
			return _upgrades.ContainsKey( type );
		}

		/*
		===============
		GetUpgradeCost
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="tier"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetUpgradeCost( UpgradeType type, int tier ) {
			return _upgradeData[ type ][ tier ].Cost;
		}

		/*
		===============
		GetUpgradeCost
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetUpgradeCost( HarpoonType type ) {
			return _harpoonData[ type ];
		}

		/*
		===============
		GetUpgradeMultiplier
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="tier"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public float GetUpgradeMultiplier( UpgradeType type, int tier ) {
			return _upgradeData[ type ][ tier ].AddAmount;
		}

		/*
		===============
		BuyUpgrade
		===============
		*/
		/// <summary>
		/// Activates an upgrade.
		/// </summary>
		/// <param name="type"></param>
		public bool BuyUpgrade( UpgradeType type ) {
			if ( _upgrades.TryGetValue( type, out int tier ) ) {
				if ( tier > MAX_UPGRADE_TIER ) {
					// already at max
					return false;
				}
			} else {
				tier = 1;
			}

			float cost = GetUpgradeCost( type, tier );
			if ( _moneyAmount - cost < 0.0f ) {
				return false;
			}

			_upgrades[ type ] = tier + 1;
			_upgradeBought.Publish( new UpgradeBoughtEventArgs( type, tier, cost, _upgradeData[ type ][ tier ].AddAmount ) );

			return true;
		}

		/*
		===============
		BuyUpgrade
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool BuyUpgrade( HarpoonType type ) {
			if ( _harpoonUpgrades.TryGetValue( type, out var bought ) ) {
				return false;
			}

			_harpoonUpgrades.Add( type );
			_harpoonBought.Publish( new HarpoonTypeUpgradeBoughtEventArgs( type, _harpoonData[ type ] ) );

			return true;
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
			if ( args.StatId == PlayerStats.MONEY ) {
				_moneyAmount = args.Value;
			}
		}
	};
};