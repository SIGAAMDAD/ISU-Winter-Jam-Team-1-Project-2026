using Game.Mobs;
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
		[Export]
		private TideSpawner _tideSpawner;
		[Export]
		private MobSpawner _mobSpawner;

		public int CurrentWave => _currentWave;
		private int _currentWave = 1;

		private ILoggerCategory _category;
		private ILoggerService _logger;

		public IGameEvent<WaveChangedEventArgs> WaveCompleted => _waveCompleted;
		private IGameEvent<WaveChangedEventArgs> _waveCompleted;

		public IGameEvent<EmptyEventArgs> WaveStarted => _waveStarted;
		private IGameEvent<EmptyEventArgs> _waveStarted;

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
			_waveCompleted.Publish( new WaveChangedEventArgs( oldWave, _currentWave ) );
			SetProcess( false );

			_logger.PrintLine( $"Wave completed, showing upgrade menu..." );

			GameStateManager.Instance.SetGameState( GameState.UpgradeMenu );
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
				_waveStarted.Publish( new EmptyEventArgs() );
				SetProcess( true );

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
			_waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveCompleted ) );
			_waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveStarted ) );

			var waveTimeout = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveTimer.WaveTimeout ) );
			waveTimeout.Subscribe( this, OnWaveTimerTimeout );

			_waveStarted.Publish( new EmptyEventArgs() );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );

			_logger = serviceLocator.GetService<ILoggerService>();
			_category = _logger.CreateCategory( nameof( WaveManager ), LogLevel.Info, true );
		}
	};
};