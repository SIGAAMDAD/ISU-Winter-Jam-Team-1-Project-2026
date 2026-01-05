using Game.Common;
using Nomad.Core.Events;
using Godot;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	AnnouncementLabel
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class AnnouncementLabel {
		private const string WAVE_COMPLETED_TEXT = "WAVE DONE";
		private const string WAVE_START_TEXT = "NEW WAVE";

		private readonly Label _label;
		private readonly Timer _showTimer;

		/*
		===============
		AnnouncementLabel
		===============
		*/
		public AnnouncementLabel( Label label, IGameEventRegistryService eventFactory ) {
			_label = label;
			_showTimer = new Timer() {
				Name = nameof( _showTimer ),
				WaitTime = 1.5f,
				OneShot = true
			};
			_showTimer.Connect( Timer.SignalName.Timeout, Callable.From( OnShowTimerTimeout ) );
			_label.AddChild( _showTimer );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			var waveStarted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );
		}

		/*
		===============
		OnShowTimerTimeout
		===============
		*/
		private void OnShowTimerTimeout() {
			_label.CreateTween().TweenProperty( _label, "modulate", Colors.Transparent, 1.0f );
		}

		/*
		===============
		ShowLabel
		===============
		*/
		private void ShowLabel( string text ) {
			_label.Text = text;
			_label.Modulate = Colors.White;
			_showTimer.Start();
		}

		/*
		===============
		OnWaveCompleted
		===============
		*/
		private void OnWaveCompleted( in WaveChangedEventArgs args ) {
			ShowLabel( WAVE_COMPLETED_TEXT );
		}

		/*
		===============
		OnWaveStarted
		===============
		*/
		private void OnWaveStarted( in WaveChangedEventArgs args ) {
			ShowLabel( WAVE_START_TEXT );
		}
	};
};