using Godot;
using Game.Player.Upgrades;
using Game.Systems;
using Nomad.Core.Events;
using System.Collections.Generic;
using Game.Systems.Caching;
using Nomad.Core.Util;

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
		private readonly Dictionary<UpgradeType, UpgradeButton> _statButtons = new();
		private readonly Dictionary<HarpoonType, HarpoonUpgradeButton> _harpoonButtons = new();

		private readonly AudioStreamPlayer _audioPlayer;

		private readonly AudioStream _buySound;
		private readonly AudioStream _cantBuySound;

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

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			var upgradeBought = eventFactory.GetEvent<UpgradeBoughtEventArgs>( nameof( UpgradeManager ), nameof( UpgradeManager.UpgradeBought ) );
			upgradeBought.Subscribe( this, OnUpgradeBought );

			var upgradeBuyFailed = eventFactory.GetEvent<EmptyEventArgs>( nameof( UpgradeManager ), nameof( UpgradeManager.BuyFailed ) );
			upgradeBuyFailed.Subscribe( this, OnUpgradeBuyFailed );

			var harpoonType = eventFactory.GetEvent<HarpoonTypeUpgradeBoughtEventArgs>( nameof( UpgradeManager ), nameof( UpgradeManager.HarpoonBought ) );
			harpoonType.Subscribe( this, OnHarpoonBought );

			_moneyLabel = _node.GetNode<Label>( "%MoneyLabel" );
			_audioPlayer = _node.GetNode<AudioStreamPlayer>( nameof( AudioStreamPlayer ) );

			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/BuyUpgrade.wav" ) ).Get( out _buySound );
			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/CantBuyUpgrade.wav" ) ).Get( out _cantBuySound );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );

			HookButtons();
		}

		/*
		===============
		OnUpgradeBought
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnUpgradeBought( in UpgradeBoughtEventArgs args ) {
			_audioPlayer.Stream = _buySound;
			_audioPlayer.Play();
		}

		/*
		===============
		OnHarpoonBought
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnHarpoonBought( in HarpoonTypeUpgradeBoughtEventArgs args ) {
			_audioPlayer.Stream = _buySound;
			_audioPlayer.Play();
		}

		/*
		===============
		OnUpgradeBuyFailed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnUpgradeBuyFailed( in EmptyEventArgs args ) {
			_audioPlayer.Stream = _cantBuySound;
			_audioPlayer.Play();
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
		/// <summary>
		///
		/// </summary>
		private void HookButtons() {
			var statUpgradeList = _node.GetNode<VBoxContainer>( "%StatUpgradeList" );
			var statButtons = statUpgradeList.GetChildren();
			_statButtons.EnsureCapacity( statButtons.Count );

			var eventFactory = _node.GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			for ( int i = 0; i < statButtons.Count; i++ ) {
				if ( statButtons[ i ] is UpgradeButtonNode node ) {
					_statButtons[ node.Type ] = new UpgradeButton(
						node,
						node.Type,
						node.StatName,
						_manager,
						eventFactory
					);
				}
			}

			var harpoonUpgradeList = _node.GetNode<VBoxContainer>( "%HarpoonUpgradeList" );
			var harpoonButtons = harpoonUpgradeList.GetChildren();
			_harpoonButtons.EnsureCapacity( harpoonButtons.Count );
			for ( int i = 0; i < harpoonButtons.Count; i++ ) {
				if ( harpoonButtons[ i ] is HarpoonUpgradeButtonNode node ) {
					_harpoonButtons[ node.Type ] = new HarpoonUpgradeButton(
						node,
						_manager,
						eventFactory
					);
				}
			}

			Button exitButton = _node.GetNode<Button>( "%FinishButton" );
			exitButton.Connect( Button.SignalName.Pressed, Callable.From( OnFinished ) );
		}
	};
};
