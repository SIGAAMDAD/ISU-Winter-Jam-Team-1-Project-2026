using Godot;
using Nomad.Core.Events;
using System.Collections.Generic;
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

		public IGameEvent<UpgradeBoughtEventArgs> UpgradeBought => _upgradeBought;
		private IGameEvent<UpgradeBoughtEventArgs> _upgradeBought;

		private readonly Dictionary<UpgradeType, int> _upgrades = new();
		private readonly Dictionary<int, UpgradeData> _upgradeData;

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

			_upgradeData = new Dictionary<int, UpgradeData> {
				[ 0 ] = new UpgradeData( UPGRADE_TIER1_COST, UPGRADE_TIER1_MULTIPLIER ),
				[ 1 ] = new UpgradeData( UPGRADE_TIER2_COST, UPGRADE_TIER2_MULTIPLIER ),
				[ 2 ] = new UpgradeData( UPGRADE_TIER3_COST, UPGRADE_TIER3_MULTIPLIER ),
				[ 3 ] = new UpgradeData( UPGRADE_TIER4_COST, UPGRADE_TIER4_MULTIPLIER ),
			};
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
			return _upgradeData[ tier ].Cost;
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
			return _upgradeData[ tier ].AddAmount;
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
				tier = 0;
			}

			float cost = GetUpgradeCost( type, tier );
			if ( _moneyAmount - cost < 0.0f ) {
				return false;
			}

			_upgrades[ type ] = tier + 1;
			_upgradeBought.Publish( new UpgradeBoughtEventArgs( type, tier, cost, _upgradeData[ tier ].AddAmount ) );

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