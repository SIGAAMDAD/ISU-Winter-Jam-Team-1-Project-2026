using Game.Player.Upgrades;
using Game.Player.UserInterface.UpgradeInterface;
using Godot;
using Nomad.Core.Util;
using Prefabs;
using System;
using Systems.Caching;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	UpgradeButton
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class UpgradeButton {
		private readonly UpgradeType _type;
		private readonly UpgradeManager _manager;
		private readonly Button _owner;
		private readonly Label _costLabel;
		private readonly string _name;

		private readonly UpgradeBase _upgradeData;

		/*
		===============
		UpgradeButton
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="button"></param>
		/// <param name="manager"></param>
		public UpgradeButton( UpgradeButtonNode node, UpgradeType type, string name, UpgradeManager manager ) {
			var button = node.GetNode<Button>( "Button" );
			button.Icon = node.Icon;
			button.Connect( Button.SignalName.FocusEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.MouseEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.Pressed, Callable.From( OnPressed ) );

			ResourceCache.Instance.GetCached( FilePath.FromResourcePath( $"res://Assets/Upgrades/{type}.tres" ) ).Get( out var resource );
			if ( resource is UpgradeBase data ) {
				_upgradeData = data;
			} else {
				throw new InvalidCastException( "Upgrade resources must be UpgradeBase objects!" );
			}

			_owner = button;
			_costLabel = node.GetNode<Label>( "CostLabel" );
			_type = type;
			_name = name;
			_manager = manager;
			SetTier( _manager.GetUpgradeTier( _type ) ) ;
		}

		/*
		===============
		SetTier
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="tier"></param>
		public void SetTier( int tier ) {
			// FIXME: this should be an event
			_owner.Text = $"{_name} [{tier}]";
			_costLabel.Text = _upgradeData.TierCosts[ tier ].ToString();
		}

		/*
		===============
		OnFocused
		===============
		*/
		/// <summary>
		/// Callback for when the upgrade button is focused.
		/// </summary>
		private void OnFocused() {
		}

		/*
		===============
		OnPressed
		===============
		*/
		/// <summary>
		/// Callback for when the upgrade button is pressed.
		/// </summary>
		private void OnPressed() {
			int tier = _manager.GetUpgradeTier( _type );
			_manager.BuyUpgrade( _type, _upgradeData.TierIncreaseAmounts[ tier ], _upgradeData.TierCosts[ tier ] );
			SetTier( _manager.GetUpgradeTier( _type ) );
		}
	};
};