using Game.Player.UserInterface.UpgradeInterface;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	HeadsUpDisplay
	
	Description
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class HeadsUpDisplay : CanvasLayer {
		private WaveUI _waveUI;
		private UpgradeMenu _upgradeMenu;

		private HealthBar _healthBar;
		private StatsList _statsList;
		private MoneyCounter _moneyCounter;

		/*
		===============
		OnGameStateChanged
		===============
		*/
		private void OnGameStateChanged( in GameStateChangedEventArgs args ) {
			GetNode<Control>( "MainHUD" ).Visible = args.NewState != GameState.UpgradeMenu;
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_healthBar = new HealthBar( GetNode<ProgressBar>( "%HealthBar" ), eventFactory );
			_waveUI = new WaveUI( GetNode<VBoxContainer>( "WaveDataContainer" ), eventFactory );
			_upgradeMenu = new UpgradeMenu( GetNode<CanvasLayer>( "UpgradeMenu" ), eventFactory );
			_statsList = new StatsList( GetNode<VBoxContainer>( "%StatsList" ), eventFactory );
			_moneyCounter = new MoneyCounter( GetNode<Label>( "%MoneyLabel" ), eventFactory );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			base._Process( delta );

			_waveUI.WaveTimer.Update();
		}
	};
};
