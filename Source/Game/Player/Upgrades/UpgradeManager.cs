using Godot;
using Nomad.Core.Events;
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
		) {
			public static readonly UpgradeData Default = new UpgradeData( 0.0f, 0.0f );
		};

		public const int MAX_UPGRADE_TIER = 5;

		private const float UPGRADE_TIER1_COST = 4.0f;
		private const float UPGRADE_TIER1_MULTIPLIER = 2.0f;

		private float _moneyAmount = 0.0f;

		private readonly Dictionary<UpgradeType, int> _upgrades = new();
		private readonly ImmutableDictionary<UpgradeType, ImmutableArray<UpgradeData>> _upgradeData;

		private readonly HashSet<HarpoonType> _harpoonUpgrades = new();
		private readonly ImmutableDictionary<HarpoonType, float> _harpoonData;

		public IGameEvent<UpgradeBoughtEventArgs> UpgradeBought => _upgradeBought;
		private readonly IGameEvent<UpgradeBoughtEventArgs> _upgradeBought;

		public IGameEvent<HarpoonTypeUpgradeBoughtEventArgs> HarpoonBought => _harpoonBought;
		private readonly IGameEvent<HarpoonTypeUpgradeBoughtEventArgs> _harpoonBought;

		public IGameEvent<EmptyEventArgs> BuyFailed => _buyFailed;
		private readonly IGameEvent<EmptyEventArgs> _buyFailed;

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
			_upgradeBought = eventFactory.GetEvent<UpgradeBoughtEventArgs>( nameof( UpgradeManager ), nameof( UpgradeBought ) );
			_harpoonBought = eventFactory.GetEvent<HarpoonTypeUpgradeBoughtEventArgs>( nameof( UpgradeManager ), nameof( HarpoonBought ) );
			_buyFailed = eventFactory.GetEvent<EmptyEventArgs>( nameof( UpgradeManager ), nameof( BuyFailed ) );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			_upgradeData = new Dictionary<UpgradeType, ImmutableArray<UpgradeData>> {
				[ UpgradeType.Armor ] = PopulateDefaultUpgradeData(),
				[ UpgradeType.MaxHealth ] = PopulateDefaultUpgradeData(),
				[ UpgradeType.HealthRegen ] = PopulateDefaultUpgradeData(),
				[ UpgradeType.Speed ] = PopulateDefaultUpgradeCostData( [ 0.0f, 1.05f, 1.5f, 2.75f, 3.5f ] ),
				[ UpgradeType.AttackDamage ] = PopulateDefaultUpgradeData(),
				[ UpgradeType.AttackSpeed ] = PopulateDefaultUpgradeCostData( [ 0.0f, 0.90f, 0.75f, 0.60f, 0.45f, 0.15f ] )
			}.ToImmutableDictionary();

			_harpoonData = new Dictionary<HarpoonType, float> {
				[ HarpoonType.ExplosiveHarpoon ] = 32.0f,
				[ HarpoonType.IcyHarpoon ] = 24.0f,
				[ HarpoonType.StationaryHarpoon ] = 28.0f,
			}.ToImmutableDictionary();
		}

		/*
		===============
		PopulateDefaultUpgradeData
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		private ImmutableArray<UpgradeData> PopulateDefaultUpgradeData() {
			var data = new UpgradeData[ MAX_UPGRADE_TIER ];
			data[ 0 ] = UpgradeData.Default;

			float lastCost = UPGRADE_TIER1_COST;
			for ( int i = 1; i < data.Length; i++ ) {
				data[ i ] = new UpgradeData( lastCost, UPGRADE_TIER1_MULTIPLIER * i );
				lastCost = data[ i ].Cost * 4.0f;
			}
			return [ ..data ];
		}

		/*
		===============
		PopulateDefaultUpgradeCostData
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="multipliers"></param>
		/// <returns></returns>
		private ImmutableArray<UpgradeData> PopulateDefaultUpgradeCostData( float[] multipliers ) {
			var data = new UpgradeData[ MAX_UPGRADE_TIER ];
			data[ 0 ] = UpgradeData.Default;

			float lastCost = UPGRADE_TIER1_COST;
			for ( int i = 1; i < data.Length; i++ ) {
				data[ i ] = new UpgradeData( lastCost, multipliers[ i ] );
				lastCost = data[ i ].Cost * 4.0f;
			}
			return [ ..data ];
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
		CanBuyUpgrade
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <param name="tier"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool CanBuyUpgrade( UpgradeType type, int tier ) {
			return !( _moneyAmount - _upgradeData[ type ][ tier ].Cost < 0.0f );
		}

		/*
		===============
		CanBuyUpgrade
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public bool CanBuyUpgrade( HarpoonType type ) {
			return !( _moneyAmount - _harpoonData[ type ] < 0.0f );
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
				tier++;
			} else {
				tier = 1;
			}

			if ( !CanBuyUpgrade( type, tier ) ) {
				_buyFailed.Publish( new EmptyEventArgs() );
				return false;
			}

			float cost = GetUpgradeCost( type, tier );
			_upgrades[ type ] = tier;
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

			if ( !CanBuyUpgrade( type ) ) {
				_buyFailed.Publish( new EmptyEventArgs() );
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
