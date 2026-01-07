using Game.Systems;
using Godot;
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
	
	public sealed partial class UpgradeManager : CanvasLayer {
		public IGameEvent<UpgradeBoughtEventArgs> UpgradeBought => _upgradeBought;
		private IGameEvent<UpgradeBoughtEventArgs> _upgradeBought;

		private readonly Dictionary<UpgradeType, int> _upgrades = new();
		
		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_upgradeBought = eventFactory.GetEvent<UpgradeBoughtEventArgs>( nameof( UpgradeBought ) );
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
		/// Activates an upgrade
		/// </summary>
		/// <param name="type"></param>
		public void BuyUpgrade( UpgradeType type ) {
			if ( _upgrades.TryGetValue( type, out int tier ) ) {
				tier++;
			} else {
				tier = 1;
			}

			_upgrades[ type ] = tier;
			_upgradeBought.Publish( new UpgradeBoughtEventArgs( type, tier, 10.0f ) );
		}
	};
};