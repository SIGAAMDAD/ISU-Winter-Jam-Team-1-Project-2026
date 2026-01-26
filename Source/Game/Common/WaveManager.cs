using Game.Mobs;
using Game.Player;
using Game.Player.UserInterface;
using Game.Systems;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Logger;

namespace Game.Common {
	/*
	===================================================================================

	WaveManager

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class WaveManager : Node {
		public const int MAX_WAVES = 20;

		[Export]
		private TideSpawner _tideSpawner;
		[Export]
		private MobSpawner _mobSpawner;

		public int CurrentWave => _currentWave;
		private int _currentWave = 0;

		private ILoggerCategory _category;
		private ILoggerService _logger;

		public IGameEvent<WaveChangedEventArgs> WaveCompleted => _waveCompleted;
		private IGameEvent<WaveChangedEventArgs> _waveCompleted;

		public IGameEvent<EmptyEventArgs> WaveStarted => _waveStarted;
		private IGameEvent<EmptyEventArgs> _waveStarted;

		public IGameEvent<EmptyEventArgs> GameCompleted => _gameCompleted;
		private IGameEvent<EmptyEventArgs> _gameCompleted;

		/*
		===============
		OnWaveTimerTimeout
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnWaveTimerTimeout( in EmptyEventArgs args ) {
			int oldWave = _currentWave;
			_currentWave++;
			if ( _currentWave >= MAX_WAVES ) {

			} else {
				_waveCompleted.Publish( new WaveChangedEventArgs( oldWave, _currentWave ) );
			}

			_logger.PrintLine( $"Wave completed, showing upgrade menu..." );
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
			if ( args.NewState == GameState.Level && args.OldState == GameState.UpgradeMenu ) {
				_waveStarted.Publish( EmptyEventArgs.Args );

				_logger.PrintLine( $"Upgrade shopping finished, resuming game loop & spawning new wave..." );
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

			var serviceLocator = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator;

			var eventFactory = serviceLocator.GetService<IGameEventRegistryService>();
			_waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveCompleted ) );
			_waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveStarted ) );
			_gameCompleted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( GameCompleted ) );

			var waveTimeout = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveTimer ), nameof( WaveTimer.WaveTimeout ) );
			waveTimeout.Subscribe( this, OnWaveTimerTimeout );

			_waveStarted.Publish( EmptyEventArgs.Args );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );

			_logger = serviceLocator.GetService<ILoggerService>();
			_category = _logger.CreateCategory( nameof( WaveManager ), LogLevel.Info, true );
		}
	};
};
