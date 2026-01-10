using Game.Player.Upgrades;
using Godot;
using Nomad.Core.Events;

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
		private static readonly StringName @HoverThemeName = "hover";

		private readonly UpgradeManager _manager;
		private readonly HarpoonUpgradeButtonNode _owner;
		private readonly Label _costLabel;

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
		public HarpoonUpgradeButton( HarpoonUpgradeButtonNode node, UpgradeManager manager, IGameEventRegistryService eventFactory ) {
			var button = node.GetNode<Button>( "Button" );
			button.Icon = node.Icon;
			button.Text = node.UpgradeName;

			button.AddThemeStyleboxOverride( NormalThemeName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/NotOwned.tres" ) );
			button.AddThemeStyleboxOverride( HoverThemeName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/HoverStyle.tres" ) );
			
			button.Connect( Button.SignalName.FocusEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.MouseEntered, Callable.From( OnFocused ) );
			button.Connect( Button.SignalName.Pressed, Callable.From( OnPressed ) );

			_costLabel = node.GetNode<Label>( "CostLabel" );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			_owner = node;
			_manager = manager;
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
				if ( !_manager.CanBuyUpgrade( _owner.Type ) ) {
					_costLabel.Modulate = Colors.Red;
				} else {
					_costLabel.Modulate = Colors.White;
				}
			}
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

			_owned = _manager.BuyUpgrade( _owner.Type );
			if ( _owned ) {
				_owner.RemoveThemeStyleboxOverride( HoverThemeName );
				_owner.RemoveThemeStyleboxOverride( NormalThemeName );
				_owner.AddThemeStyleboxOverride( NormalThemeName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/Owned.tres" ) );
				_owner.AddThemeStyleboxOverride( HoverThemeName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/Owned.tres" ) );

				_owner.Modulate = Colors.Green;
			}
		}
	};
};