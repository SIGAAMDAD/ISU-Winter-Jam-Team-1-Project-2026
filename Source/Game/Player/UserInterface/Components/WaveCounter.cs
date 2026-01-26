using Game.Common;
using Nomad.Core.Events;
using Godot;
using System;
using Nomad.Events;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	WaveCounter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class WaveCounter : IDisposable {
		private readonly Label _countLabel;
		private readonly DisposableSubscription<WaveChangedEventArgs> _waveCompletedEvent;

		/*
		===============
		WaveCounter
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="label"></param>
		/// <param name="eventFactory"></param>
		public WaveCounter( Label label, IGameEventRegistryService eventFactory ) {
			_waveCompletedEvent = new DisposableSubscription<WaveChangedEventArgs>(
				eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) ),
				OnWaveChanged
			);

			_countLabel = label;
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
			_waveCompletedEvent.Dispose();
		}

		/*
		===============
		OnWaveChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveChanged( in WaveChangedEventArgs args ) {
			_countLabel.Text = $"WAVE {args.NewWave}";
		}
	};
};
