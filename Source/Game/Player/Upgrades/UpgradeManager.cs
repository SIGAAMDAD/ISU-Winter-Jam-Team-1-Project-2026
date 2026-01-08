using Nomad.Core.Events;
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

		private float _moneyAmount = 0.0f;

		public IGameEvent<UpgradeBoughtEventArgs> UpgradeBought => _upgradeBought;
		private IGameEvent<UpgradeBoughtEventArgs> _upgradeBought;

		private readonly Dictionary<UpgradeType, int> _upgrades = new();
		
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
		public bool BuyUpgrade( UpgradeType type, float addAmount, float cost ) {
			if ( _moneyAmount - cost < 0.0f ) {
				return false;
			}

			if ( _upgrades.TryGetValue( type, out int tier ) ) {
				if ( tier >= MAX_UPGRADE_TIER ) {
					// already at max
					return false;
				}
				tier++;
			} else {
				tier = 1;
			}

			_upgrades[ type ] = tier;
			_upgradeBought.Publish( new UpgradeBoughtEventArgs( type, tier, addAmount, cost ) );

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