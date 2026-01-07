using Game.Common;
using Nomad.Core.Events;
using Godot;

namespace Game.Player.UserInterface {
	public sealed class WaveCounter {
		private readonly Label _countLabel;

		public WaveCounter( Label label, IGameEventRegistryService eventFactory ) {
			var waveChanged = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager.WaveStarted ) );
			waveChanged.Subscribe( this, OnWaveChanged );

			_countLabel = label;
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