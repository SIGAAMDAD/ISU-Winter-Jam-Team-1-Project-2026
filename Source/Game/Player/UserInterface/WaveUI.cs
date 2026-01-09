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
		private readonly WaveTimer _waveTimer;

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
			_waveTimer = new WaveTimer( container.GetNode<Label>( "WaveTimer" ), eventFactory );
		}
	};
};