using Nomad.Core.Events;
using Godot;
using System;
using Game.Common;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	WaveTimer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class WaveTimer : IDisposable {
		private readonly Timer _timer;
		private readonly Label _timerLabel;

		public IGameEvent<EmptyEventArgs> WaveTimeout => _waveTimeout;
		private readonly IGameEvent<EmptyEventArgs> _waveTimeout;

		/*
		===============
		WaveTimer
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="timerLabel"></param>
		/// <param name="eventFactory"></param>
		public WaveTimer( Label timerLabel, IGameEventRegistryService eventFactory ) {
			_waveTimeout = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveTimeout ) );

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnStartTimer );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			_timerLabel = timerLabel;

			_timer = new Timer() {
				Name = "WaveTimer",
				WaitTime = 15.0f,
				OneShot = true
			};
			_timer.Connect( Timer.SignalName.Timeout, Callable.From( OnWaveTimerTimeout ) );
			timerLabel.AddChild( _timer );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Dispose() {
			_waveTimeout.Dispose();
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public void Update() {
			if ( ( Engine.GetProcessFrames() % 60 ) != 0 ) {
				return;
			}
			_timerLabel.Text = ( (int)_timer.TimeLeft ).ToString();
		}

		/*
		===============
		OnWaveTimerTimeout
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnWaveTimerTimeout() {
			_waveTimeout.Publish( new EmptyEventArgs() );
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			if ( args.NewWave <= 5 ) {
				_timer.WaitTime += 10.0f;
			}
		}

		/*
		===============
		OnStartTimer
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnStartTimer( in EmptyEventArgs args ) {
			_timer.Start();
		}
	};
};