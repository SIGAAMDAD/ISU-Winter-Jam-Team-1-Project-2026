using System;
using Game.Player.Upgrades;
using Godot;
using Nomad.Core.Events;
using Nomad.Events;

namespace Game.Player.UserInterface.UpgradeInterface {
	/*
	===================================================================================

	HarpoonUpgradeButton

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class HarpoonUpgradeButton : IDisposable {
		private static readonly StringName @HoverStyleName = "hover";
		private static readonly StringName @PressedStyleName = "pressed";
		private static readonly StringName @NormalStyleName = "normal";

		private readonly UpgradeManager _manager;
		private readonly HarpoonUpgradeButtonNode _owner;
		private readonly Label _costLabel;
		private readonly Button _button;

		private readonly DisposableSubscription<StatChangedEventArgs> _statChangedEvent;

		private readonly Callable _onFocusedCallable;
		private readonly Callable _onPressedCallable;

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
			_button = node.GetNode<Button>( "Button" );
			_button.Icon = node.Icon;
			_button.Text = node.UpgradeName;

			_button.AddThemeStyleboxOverride( HoverStyleName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/HoverStyle.tres" ) );
			_button.AddThemeStyleboxOverride( PressedStyleName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/HoverStyle.tres" ) );
			_button.AddThemeStyleboxOverride( NormalStyleName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/NotOwned.tres" ) );

			_onFocusedCallable = Callable.From( OnFocused );
			_onPressedCallable = Callable.From( OnPressed );
			_button.Connect( Button.SignalName.FocusEntered, _onFocusedCallable );
			_button.Connect( Button.SignalName.MouseEntered, _onFocusedCallable );
			_button.Connect( Button.SignalName.Pressed, _onPressedCallable );

			_statChangedEvent = new DisposableSubscription<StatChangedEventArgs>(
				eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) ),
				OnStatChanged
			);

			_owner = node;
			_manager = manager;

			_costLabel = node.GetNode<Label>( "CostLabel" );
			_costLabel.Text = _manager.GetUpgradeCost( node.Type ).ToString();
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose() {
			_statChangedEvent.Dispose();
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
				_costLabel.Text = "OWNED";

				_button.Disconnect( Button.SignalName.Pressed, _onPressedCallable );
				_button.Disconnect( Button.SignalName.FocusEntered, _onFocusedCallable );
				_button.Disconnect( Button.SignalName.MouseEntered, _onFocusedCallable );

				_button.RemoveThemeStyleboxOverride( NormalStyleName );
				_button.RemoveThemeStyleboxOverride( HoverStyleName );
				_button.RemoveThemeStyleboxOverride( PressedStyleName );

				_button.AddThemeStyleboxOverride( HoverStyleName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/Owned.tres" ) );
				_button.AddThemeStyleboxOverride( PressedStyleName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/Owned.tres" ) );
				_button.AddThemeStyleboxOverride( NormalStyleName, ResourceLoader.Load<StyleBoxFlat>( "res://Source/Game/Player/UserInterface/UpgradeInterface/HarpoonUpgradeButton/Owned.tres" ) );
			}
		}
	};
};
