using Godot;
using Nomad.Core.Events;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerAudioPlayer
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class PlayerAudioPlayer {
		private readonly PlayerManager _owner;
		private readonly AudioStreamPlayer2D _streamPlayer;

		/*
		===============
		PlayerAudioPlayer
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="animator"></param>
		public PlayerAudioPlayer( PlayerManager owner, PlayerAnimator animator ) {
			animator.PlayerStartMoving.Subscribe( this, OnStartMoveSound );
			animator.PlayerStopMoving.Subscribe( this, OnStopMoveSound );
		}

		/*
		===============
		OnStartMoveSound
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnStartMoveSound( in EmptyEventArgs args ) {
			
		}

		/*
		===============
		OnStopMoveSound
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnStopMoveSound( in EmptyEventArgs args ) {
		}
	};
};