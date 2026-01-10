using Game.Player.UserInterface.UpgradeInterface;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	HeadsUpDisplay
	
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
		private AnnouncementLabel _announcementLabel;

		private TextureRect _hurtBox;

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
			GetNode<Control>( "MainHUD" ).Visible = args.NewState == GameState.Level;
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
			if ( args.StatId == PlayerStats.HEALTH ) {
				_hurtBox.Visible = args.Value < 20.0f;
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _Ready() {
			base._Ready();

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			_healthBar = new HealthBar( GetNode<ProgressBar>( "%HealthBar" ), eventFactory );
			_waveUI = new WaveUI( GetNode<VBoxContainer>( "WaveDataContainer" ), eventFactory );
			_upgradeMenu = new UpgradeMenu( GetNode<CanvasLayer>( "UpgradeMenu" ), eventFactory );
			_statsList = new StatsList( GetNode<VBoxContainer>( "%StatsList" ), eventFactory );
			_moneyCounter = new MoneyCounter( GetNode<Label>( "%MoneyLabel" ), eventFactory );
			_announcementLabel = new AnnouncementLabel( GetNode<Label>( "%AnnouncementLabel" ), eventFactory );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );

			_hurtBox = GetNode<TextureRect>( "MainHUD/HurtBox" );
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public override void _ExitTree() {
			base._ExitTree();

			GameStateManager.GameStateChanged.Unsubscribe( this, OnGameStateChanged );
		}
	};
};
