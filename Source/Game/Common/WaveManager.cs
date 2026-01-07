using Game.Systems;
using Godot;
using Nomad.Core.Events;

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

		public IGameEvent<WaveChangedEventArgs> WaveStarted => _waveStarted;
		private IGameEvent<WaveChangedEventArgs> _waveStarted;

		public IGameEvent<WaveChangedEventArgs> WaveCompleted => _waveCompleted;
		private IGameEvent<WaveChangedEventArgs> _waveCompleted;

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
			_waveStarted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveStarted ) );
			_waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveCompleted ) );
			_waveTimeChanged = eventFactory.GetEvent<WaveTimeChangedEventArgs>( nameof( WaveTimeChanged ) );

			_waveTimer = GetNode<Timer>( "WaveTimer" );
			_waveTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnWaveTimerTimeout ) );

			// start the wave
			_waveStarted.Publish( new WaveChangedEventArgs( _currentWave, _currentWave ) );
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