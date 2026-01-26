using Nomad.Core.Events;
using Godot;
using System;
using Game.Common;
using Nomad.Events;

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

		private readonly DisposableSubscription<WaveChangedEventArgs> _waveCompletedEvent;
		private readonly DisposableSubscription<EmptyEventArgs> _waveStartedEvent;

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
			_waveTimeout = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveTimer ), nameof( WaveTimeout ) );

			_waveStartedEvent = new DisposableSubscription<EmptyEventArgs>(
				eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveStarted ) ),
				OnStartTimer
			);

			_waveCompletedEvent = new DisposableSubscription<WaveChangedEventArgs>(
				eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) ),
				OnWaveCompleted
			);

			_timerLabel = timerLabel;

			_timer = new Timer() {
				Name = "WaveTimer",
				WaitTime = 15.0f,
				OneShot = true
			};
			_timer.Connect( Timer.SignalName.Timeout, Callable.From( OnWaveTimerTimeout ) );
			timerLabel.AddChild( _timer );

			var updateTimer = new Timer() {
				WaitTime = 1.0f
			};
			updateTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnUpdateTimer ) );
			timerLabel.AddChild( updateTimer );
			updateTimer.Start();
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
			_timerLabel.Dispose();

			_waveStartedEvent.Dispose();
			_waveCompletedEvent.Dispose();
		}

		/*
		===============
		OnUpdateTimer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnUpdateTimer() {
			int timeLeft = (int)_timer.TimeLeft;

			if ( timeLeft < 5 ) {
				_timerLabel.Modulate = Colors.Red;
			} else {
				_timerLabel.Modulate = Colors.White;
			}
			_timerLabel.Text = timeLeft.ToString();
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
			_waveTimeout.Publish( EmptyEventArgs.Args );
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
				_timer.WaitTime = 10.0f + ( args.NewWave * 4.0f );
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
