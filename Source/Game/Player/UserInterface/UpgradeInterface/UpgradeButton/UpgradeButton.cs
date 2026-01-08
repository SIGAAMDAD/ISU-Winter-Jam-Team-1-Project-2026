using Game.Player.Upgrades;
using Game.Player.UserInterface.UpgradeInterface;
using Godot;

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
		public UpgradeButton( UpgradeButtonNode node, UpgradeType type, string name, UpgradeManager manager ) {
			var button = node.GetNode<Button>( "Button" );
			button.Icon = node.Icon;
			button.Connect( Button.SignalName.FocusEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.MouseEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.Pressed, Callable.From( OnPressed ) );

			_owner = button;
			_costLabel = node.GetNode<Label>( "CostLabel" );
			_valueLabel = node.GetNode<Label>( "StatValueLabel" );
			_type = type;
			_name = name;
			_manager = manager;
			SetTier( _manager.GetUpgradeTier( _type ) + 1 ) ;
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
			_costLabel.Text = _manager.GetUpgradeCost( _type, tier ).ToString();
			_valueLabel.Text = $"+{_manager.GetUpgradeMultiplier( _type, tier )}";
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
			_manager.BuyUpgrade( _type );
			SetTier( _manager.GetUpgradeTier( _type ) );
		}
	};
};