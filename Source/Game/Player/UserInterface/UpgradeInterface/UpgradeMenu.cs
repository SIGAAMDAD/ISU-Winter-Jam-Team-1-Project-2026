using Godot;
using Game.Player.Upgrades;
using Game.Systems;
using Nomad.Core.Events;
using System.Collections.Generic;

namespace Game.Player.UserInterface.UpgradeInterface {
	/*
	===================================================================================
	
	UpgradeMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class UpgradeMenu {
		private readonly CanvasLayer _node;
		private readonly UpgradeManager _manager;
		private readonly Dictionary<UpgradeType, UpgradeButton> _buttons = new();

		private readonly Label _moneyLabel;

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public UpgradeMenu( CanvasLayer node, IGameEventRegistryService eventFactory ) {
			_node = node;
			_manager = new UpgradeManager( eventFactory );

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			_moneyLabel = _node.GetNode<Label>( "%MoneyLabel" );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );

			HookButtons();
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
				_moneyLabel.Text = $"{args.Value}";
			}
		}

		/*
		===============
		OnGameStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnGameStateChanged( in GameStateChangedEventArgs args ) {
			if ( args.NewState == GameState.UpgradeMenu && args.OldState == GameState.Level ) {
				_node.Visible = true;
				_node.ProcessMode = Node.ProcessModeEnum.Pausable;
			} else if ( args.NewState == GameState.Level && args.OldState == GameState.UpgradeMenu ) {
				_node.Visible = false;
				_node.ProcessMode = Node.ProcessModeEnum.Disabled;
			}
		}

		/*
		===============
		OnFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnFinished() {
			if ( GameStateManager.Instance.GameState == GameState.UpgradeMenu ) {
				GameStateManager.Instance.SetGameState( GameState.Level );
			}
		}

		/*
		===============
		HookButtons
		===============
		*/
		private void HookButtons() {
			var upgradeList = _node.GetNode<VBoxContainer>( "%UpgradeList" );

			var buttons = upgradeList.GetChildren();
			_buttons.EnsureCapacity( buttons.Count );
			for ( int i = 0; i < buttons.Count; i++ ) {
				if ( buttons[ i ] is UpgradeButtonNode node ) {
					_buttons[ node.Type ] = new UpgradeButton(
						node,
						node.Type,
						node.StatName,
						_manager
					);
				}
			}

			Button exitButton = _node.GetNode<Button>( "%FinishButton" );
			exitButton.Connect( Button.SignalName.Pressed, Callable.From( OnFinished ) );
		}
	};
};