using Godot;
using Game.Player.Upgrades;
using Game.Systems;
using Nomad.Core.Events;
using Game.Player;

namespace Game.Menus {
	/*
	===================================================================================
	
	UpgradeMenu
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class UpgradeMenu : Control {
		[Export]
		private UpgradeManager _upgradeManager;

		private float _moneyAmount = 0.0f;

		private Label _healthLabel;
		private Label _armorLabel;
		private Label _speedLabel;
		private Label _attackDamageLabel;
		private Label _attackSpeedLabel;
		private Label _moneyLabel;

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );

			HookButtons();
			HookLabels();
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
				Visible = true;
				ProcessMode = ProcessModeEnum.Pausable;
			} else if ( args.NewState == GameState.Level && args.OldState == GameState.UpgradeMenu ) {
				Visible = false;
				ProcessMode = ProcessModeEnum.Disabled;
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
				GD.Print( "Hiding upgrade menu..." );
				GameStateManager.Instance.SetGameState( GameState.Level );
			}
		}

		/*
		===============
		HookLabels
		===============
		*/
		private void HookLabels() {
			_healthLabel = GetNode<Label>( "%HealthLabel" );
			_armorLabel = GetNode<Label>( "%ArmorLabel" );
			_speedLabel = GetNode<Label>( "%SpeedLabel" );
			_attackDamageLabel = GetNode<Label>( "%AttackLabel" );
			_attackSpeedLabel = GetNode<Label>( "%AttackSpeedLabel" );
			_moneyLabel = GetNode<Label>( "%ExpLabel" );
		}

		/*
		===============
		HookButtons
		===============
		*/
		private void HookButtons() {
			Button finishButton = GetNode<Button>( "%FinishButton" );
			finishButton.Connect( Button.SignalName.Pressed, Callable.From( OnFinished ) );

			Button healthButton = GetNode<Button>( "%UpgradeHealth" );
			healthButton.Connect( Button.SignalName.Pressed, Callable.From( OnHealthUpgradePurchased ) );

			Button speedButton = GetNode<Button>( "%UpgradeSpeed" );
			speedButton.Connect( Button.SignalName.Pressed, Callable.From( OnSpeedUpgradePurchase ) );

			Button defenseButton = GetNode<Button>( "%UpgradeArmor" );
			defenseButton.Connect( Button.SignalName.Pressed, Callable.From( OnArmorUpgradePurchased ) );

			Button attackButton = GetNode<Button>( "%UpgradeAttackDamage" );
			attackButton.Connect( Button.SignalName.Pressed, Callable.From( OnAttackDamageUpgradePurchased ) );

			Button attackSpeedButton = GetNode<Button>( "%UpgradeAttackSpeed" );
			attackSpeedButton.Connect( Button.SignalName.Pressed, Callable.From( OnAttackSpeedUpgradePurchased ) );
		}

		/*
		===============
		OnUpgradePurchased
		===============
		*/
		private void OnUpgradePurchased( UpgradeType type ) {
			bool isUpgradeOwned = _upgradeManager.UpgradeIsOwned( type );

			if ( _moneyAmount > 0.0f && !isUpgradeOwned ) {
				_moneyAmount--;
				_upgradeManager.BuyUpgrade( type );
			} else if ( isUpgradeOwned ) {
				GD.Print( "You already have that upgrade" );
			} else {
				GD.Print( "Sorry but you don't have enough exp!" );
			}

			_moneyLabel.Text = $"MONEY: {_moneyAmount}";
		}

		/*
		===============
		OnHealthUpgradePurchased
		===============
		*/
		private void OnHealthUpgradePurchased() {
			OnUpgradePurchased( UpgradeType.MaxHealth );
			_healthLabel.Text = $"Currently max health is level {_upgradeManager.GetUpgradeTier( UpgradeType.MaxHealth )}";
		}

		/*
		===============
		OnSpeedUpgradePurchase
		===============
		*/
		private void OnSpeedUpgradePurchase() {
			OnUpgradePurchased( UpgradeType.Speed );
			_speedLabel.Text = $"Currently speed is level {_upgradeManager.GetUpgradeTier( UpgradeType.Speed )}";
		}

		/*
		===============
		OnArmorUpgradePurchased
		===============
		*/
		private void OnArmorUpgradePurchased() {
			OnUpgradePurchased( UpgradeType.Armor );
			_armorLabel.Text = $"Currently armor is level {_upgradeManager.GetUpgradeTier( UpgradeType.Armor )}";
		}

		/*
		===============
		OnAttackDamageUpgradePurchased
		===============
		*/
		private void OnAttackDamageUpgradePurchased() {
			OnUpgradePurchased( UpgradeType.AttackDamage );
			_attackDamageLabel.Text = $"Currently attack power is level {_upgradeManager.GetUpgradeTier( UpgradeType.AttackDamage )}";
		}

		/*
		===============
		OnAttackSpeedUpgradePurchased
		===============
		*/
		private void OnAttackSpeedUpgradePurchased() {
			OnUpgradePurchased( UpgradeType.AttackSpeed );
			_attackSpeedLabel.Text = "currently attack speed is level " + _upgradeManager.GetUpgradeTier( UpgradeType.AttackSpeed );
		}
	};
};