using System;
using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	WaveUI

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>
	/// <param name="container"></param>
	/// <param name="eventFactory"></param>

	public sealed class WaveUI( VBoxContainer container, IGameEventRegistryService eventFactory ) : IDisposable {
		private readonly WaveCounter _waveCounter = new WaveCounter( container.GetNode<Label>( "WaveCounter" ), eventFactory );
		private readonly WaveTimer _waveTimer = new WaveTimer( container.GetNode<Label>( "WaveTimer" ), eventFactory );

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose() {
			_waveCounter.Dispose();
			_waveTimer.Dispose();
		}
	};
};
