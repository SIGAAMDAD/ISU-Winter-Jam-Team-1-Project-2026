using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Systems.Caching;

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
		private readonly AudioStreamPlayer2D _moveStream;
		private readonly AudioStreamPlayer2D _actionStream;

		private bool _isMoving = false;

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

			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/jetski.wav" ) ).Get( out var moveSound );

			_moveStream = owner.GetNode<AudioStreamPlayer2D>( "MoveStream" );
			_moveStream.Stream = moveSound;
			_moveStream.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( OnCheckMoveLoop ) );

			_actionStream = owner.GetNode<AudioStreamPlayer2D>( "ActionStream" );
		}

		/*
		===============
		OnCheckMoveLoop
		===============
		*/
		/// <summary>
		/// Checks if we're still moving, if so, loop the jetski sound effect
		/// </summary>
		private void OnCheckMoveLoop() {
			if ( _isMoving ) {
				_moveStream.Play();
			}
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
			_moveStream.Play();
			_isMoving = true;
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
			_moveStream.Stop();
			_isMoving = false;
		}
	};
};