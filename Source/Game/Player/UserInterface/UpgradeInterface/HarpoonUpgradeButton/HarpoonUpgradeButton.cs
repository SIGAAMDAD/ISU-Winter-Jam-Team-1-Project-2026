using Game.Player.Upgrades;
using Godot;

namespace Game.Player.UserInterface.UpgradeInterface {
	/*
	===================================================================================
	
	HarpoonUpgradeButton
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class HarpoonUpgradeButton {
		private static readonly StringName @NormalThemeName = "normal";

		private readonly HarpoonType _type;
		private readonly UpgradeManager _manager;
		private readonly HarpoonUpgradeButtonNode _owner;

		private bool _owned = false;

		/*
		===============
		HarpoonUpgradeButton
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		/// <param name="type"></param>
		/// <param name="manager"></param>
		public HarpoonUpgradeButton( HarpoonUpgradeButtonNode node, UpgradeManager manager ) {
			var button = node.GetNode<Button>( "Button" );
			button.Icon = node.Icon;
			button.Text = node.UpgradeName;
			button.Connect( Button.SignalName.FocusEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.MouseEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.Pressed, Callable.From( OnPressed ) );

			_owner = node;
			_manager = manager;
			_type = node.Type;

			var costLabel = node.GetNode<Label>( "CostLabel" );
			costLabel.Text = $"{_manager.GetUpgradeCost( _type )}";
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
			if ( _owned ) {
				return;
			}

			_owned = _manager.BuyUpgrade( _type );
			if ( _owned ) {
				_owner.RemoveThemeStyleboxOverride( NormalThemeName );
				_owner.AddThemeStyleboxOverride( NormalThemeName, ResourceLoader.Load<StyleBox>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/Owned.tres" ) );
			}
		}
	};
};