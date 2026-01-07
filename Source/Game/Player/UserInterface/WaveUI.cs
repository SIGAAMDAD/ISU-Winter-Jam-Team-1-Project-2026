using Game.Common;
using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	WaveUI
	
	Description
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class WaveUI {
		private readonly WaveCounter _waveCounter;
		private readonly Label _waveTimer;

		/*
		===============
		WaveUI
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		/// <param name="eventFactory"></param>
		public WaveUI( VBoxContainer container, IGameEventRegistryService eventFactory ) {
			_waveCounter = new WaveCounter( container.GetNode<Label>( "WaveCounter" ), eventFactory );
			_waveTimer = container.GetNode<Label>( "WaveTimer" );

			var waveTimeChanged = eventFactory.GetEvent<WaveTimeChangedEventArgs>( nameof( WaveManager.WaveTimeChanged ) );
			waveTimeChanged.Subscribe( this, OnWaveTimeChanged );
		}

		/*
		===============
		OnWaveTimeChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveTimeChanged( in WaveTimeChangedEventArgs args ) {
			_waveTimer.Text = ( (int)args.Value ).ToString();
		}
	};
};