using Game.Player.UserInterface.UpgradeInterface;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Events;
using Game.Player.UserInterface.Components;

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
		private WeaponSlotContainer _slotContainer;

		private TextureRect _hurtBox;

		private DisposableSubscription<StatChangedEventArgs> _playerStatChangedEvent;
		private DisposableSubscription<GameStateChangedEventArgs> _gameStateChangedEvent;

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
			_playerStatChangedEvent = new DisposableSubscription<StatChangedEventArgs>(
				eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) ),
				OnStatChanged
			);

			_healthBar = new HealthBar( GetNode<ProgressBar>( "%HealthBar" ), eventFactory );
			_waveUI = new WaveUI( GetNode<VBoxContainer>( "WaveDataContainer" ), eventFactory );
			_upgradeMenu = new UpgradeMenu( GetNode<CanvasLayer>( "UpgradeMenu" ), eventFactory );
			_statsList = new StatsList( GetNode<VBoxContainer>( "%StatsList" ), eventFactory );
			_moneyCounter = new MoneyCounter( GetNode<Label>( "%MoneyLabel" ), eventFactory );
			_announcementLabel = new AnnouncementLabel( GetNode<Label>( "%AnnouncementLabel" ), eventFactory );
			_slotContainer = new WeaponSlotContainer( GetNode<VBoxContainer>( "%WeaponSlotContainer" ), eventFactory );

			_gameStateChangedEvent = new DisposableSubscription<GameStateChangedEventArgs>(
				GameStateManager.GameStateChanged,
				OnGameStateChanged
			);

			_hurtBox = GetNode<TextureRect>( "MainHUD/HurtBox" );
		}
	};
};
