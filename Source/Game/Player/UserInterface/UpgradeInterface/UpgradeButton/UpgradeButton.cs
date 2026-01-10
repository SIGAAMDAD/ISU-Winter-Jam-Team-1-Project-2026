using Game.Player.Upgrades;
using Game.Player.UserInterface.UpgradeInterface;
using Godot;
using Nomad.Core.Events;
using System;

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
		private readonly UpgradeManager _manager;
		private readonly UpgradeButtonNode _owner;
		private readonly Button _button;
		private readonly Label _costLabel;
		private readonly Label _valueLabel;
		private readonly string _name;

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
		public UpgradeButton( UpgradeButtonNode node, UpgradeType type, string name, UpgradeManager manager, IGameEventRegistryService eventFactory ) {
			_button = node.GetNode<Button>( "Button" );
			_button.Icon = node.Icon;
			_button.Connect( Button.SignalName.FocusEntered, Callable.From( OnFocused ) );
			_button.Connect( Button.SignalName.MouseEntered, Callable.From( OnFocused ) );
			_button.Connect( Button.SignalName.Pressed, Callable.From( OnPressed ) );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			_owner = node;
			_costLabel = node.GetNode<Label>( "CostLabel" );
			_valueLabel = node.GetNode<Label>( "StatValueLabel" );
			_name = name;
			_manager = manager;
			SetTier( _manager.GetUpgradeTier( _owner.Type ) );
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
				if ( !_manager.CanBuyUpgrade( _owner.Type, _manager.GetUpgradeTier( _owner.Type ) + 1 ) ) {
					_costLabel.Modulate = Colors.Red;
				} else {
					_costLabel.Modulate = Colors.White;
				}
			}
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
			if ( tier == UpgradeManager.MAX_UPGRADE_TIER ) {
				_button.Text = $"{_name} [MAX UPGRADE TIER ({UpgradeManager.MAX_UPGRADE_TIER}])";
				_valueLabel.Text = $"+{_manager.GetUpgradeMultiplier( _owner.Type, tier )}";
				_costLabel.Text = String.Empty;
				return;
			}
			_button.Text = $"{_name} [{tier}]";
			_costLabel.Text = _manager.GetUpgradeCost( _owner.Type, tier + 1 ).ToString();
			_valueLabel.Text = $"+{_manager.GetUpgradeMultiplier( _owner.Type, tier + 1 )}";
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
			int tier = _manager.GetUpgradeTier( _owner.Type );
			_manager.BuyUpgrade( _owner.Type );
			SetTier( _manager.GetUpgradeTier( _owner.Type ) );
		}
	};
};