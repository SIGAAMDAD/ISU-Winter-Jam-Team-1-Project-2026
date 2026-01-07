using Game.Player.Upgrades;
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
		public int CurrentWave => _currentWave;
		private int _currentWave = 1;

		private Timer _waveTimer;

		private ILoggerCategory _category;
		private ILoggerService _logger;

		public IGameEvent<WaveChangedEventArgs> WaveCompleted => _waveCompleted;
		private IGameEvent<WaveChangedEventArgs> _waveCompleted;

		public IGameEvent<EmptyEventArgs> WaveStarted => _waveStarted;
		private IGameEvent<EmptyEventArgs> _waveStarted;

		public IGameEvent<WaveTimeChangedEventArgs> WaveTimeChanged => _waveTimeChanged;
		private IGameEvent<WaveTimeChangedEventArgs> _waveTimeChanged;

		/*
		===============
		OnWaveTimerTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnWaveTimerTimeout() {
			_waveTimer.Stop();

			int oldWave = _currentWave;
			_currentWave++;
			_waveCompleted.Publish( new WaveChangedEventArgs( oldWave, _currentWave ) );
			SetProcess( false );

			_logger.PrintLine( $"Wave completed, showing upgrade menu..." );
		}

		/*
		===============
		OnUpgradeMenuHide
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnUpgradeMenuHide( in EmptyEventArgs args ) {
			_waveTimer.Start();
			_waveStarted.Publish( new EmptyEventArgs() );
			SetProcess( true );

			_logger.PrintLine( $"Upgrade shopping finished, resuming game loop & spawning new wave..." );
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
			_waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveCompleted ) );
			_waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveStarted ) );
			_waveTimeChanged = eventFactory.GetEvent<WaveTimeChangedEventArgs>( nameof( WaveTimeChanged ) );

			_waveTimer = GetNode<Timer>( "WaveTimer" );
			_waveTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnWaveTimerTimeout ) );

			_waveStarted.Publish( new EmptyEventArgs() );

			var shoppingFinished = eventFactory.GetEvent<EmptyEventArgs>( nameof( UpgradeManager.ShoppingFinished ) );
			shoppingFinished.Subscribe( this, OnUpgradeMenuHide );

			_logger = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<ILoggerService>();
			_category = _logger.CreateCategory( nameof( WaveManager ), LogLevel.Info, true );
		}

		/*
		===============
		_Process
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public override void _Process( double delta ) {
			base._Process( delta );

			if ( ( Engine.GetProcessFrames() % 60 ) == 0 ) {
				_waveTimeChanged.Publish( new WaveTimeChangedEventArgs( (float)_waveTimer.TimeLeft ) );
			}
		}
	};
};