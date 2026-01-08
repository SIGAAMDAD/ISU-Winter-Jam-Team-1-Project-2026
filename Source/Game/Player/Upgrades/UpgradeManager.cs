using Nomad.Core.Events;
using Prefabs;
using System.Collections.Generic;

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
		public const int MAX_UPGRADE_TIER = 4;
		private const float UPGRADE_TIER_COST_MULTIPLIER = 2.5f;
		private const float UPGRADE_TIER_ADD_MULTIPLIER = 1.5f;
		private const float UPGRADE_TIER_ADD_SPEED_MULTIPLIER = 1.25f;

		private float _moneyAmount = 0.0f;

		public IGameEvent<UpgradeBoughtEventArgs> UpgradeBought => _upgradeBought;
		private IGameEvent<UpgradeBoughtEventArgs> _upgradeBought;

		private readonly Dictionary<UpgradeType, int> _upgrades = new();

		private static readonly Dictionary<int, float> _tierCosts = new Dictionary<int, float> {
			[ 0 ] = 2.0f,
			[ 1 ] = 6.0f,
			[ 2 ] = 10.0f
		};
		private static readonly Dictionary<int, float> _tierIncreaseAmounts = new Dictionary<int, float> {
			[ 0 ] = 10.0f,
			[ 1 ] = 20.0f,
			[ 2 ] = 40.0f,
		};

		private readonly record struct UpgradeData(
			float Cost,
			float AddAmount
		);
		private readonly Dictionary<int, UpgradeData> _upgradeData = new Dictionary<int, UpgradeData>();

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

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );
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
		public int GetUpgradeTier( UpgradeType type ) {
			if ( _upgrades.TryGetValue( type, out int upgrade ) ) {
				return upgrade;
			}
			return 0;
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
		public bool UpgradeIsOwned( UpgradeType type ) {
			return _upgrades.ContainsKey( type );
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
				if ( tier >= MAX_UPGRADE_TIER ) {
					// already at max
					return false;
				}
				tier++;
			} else {
				tier = 1;
			}

			float cost = _tierCosts[ tier ];
			if ( _moneyAmount - cost < 0.0f ) {
				return false;
			}

			float addAmount = _tierIncreaseAmounts[ tier ];
			_upgrades[ type ] = tier;
			_upgradeBought.Publish( new UpgradeBoughtEventArgs( type, tier, addAmount, cost ) );

			return true;
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
		public float GetUpgradeCost( UpgradeType type, int tier ) {
			switch ( type ) {
				case UpgradeType.Armor:
					break;
				case UpgradeType.AttackDamage:
					break;
				case UpgradeType.AttackSpeed:
					break;
			}
			return 0.0f;
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