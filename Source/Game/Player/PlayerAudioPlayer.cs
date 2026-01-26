using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Game.Systems.Caching;
using System;
using Game.Common;
using Game.Systems;

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
		private readonly AudioStreamPlayer2D _hitStream;

		private readonly AudioStream _useWeapon;
		private readonly AudioStream _switchWeapon;

		private readonly AudioStream _moveSound;
		private readonly AudioStream _hitMarker;

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
		/// <param name="controller"></param>
		/// <param name="animator"></param>
		public PlayerAudioPlayer( PlayerManager owner, PlayerAttackController controller, PlayerAnimator animator, IGameEventRegistryService eventFactory ) {
			animator.PlayerStartMoving.Subscribe( this, OnStartMoveSound );
			animator.PlayerStopMoving.Subscribe( this, OnStopMoveSound );
			controller.UseWeapon.Subscribe( this, OnWeaponUsed );
			controller.HarpoonChanged.Subscribe( this, OnWeaponSwitched );

			var playerDamage = eventFactory.GetEvent<PlayerTakeDamageEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.TakeDamage ) );
			playerDamage.Subscribe( this, OnDamagePlayer );

			var waveCompleted = eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) );
			waveCompleted.Subscribe( this, OnWaveCompleted );

			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/jetski.wav" ) ).Get( out _moveSound );
			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/player_hitmarker.wav" ) ).Get( out _hitMarker );
			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/harpoon.wav" ) ).Get( out _useWeapon );
			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/change_weapon.wav" ) ).Get( out _switchWeapon );

			_moveStream = owner.GetNode<AudioStreamPlayer2D>( "MoveStream" );
			_moveStream.Stream = _moveSound;
			_moveStream.Connect( AudioStreamPlayer2D.SignalName.Finished, Callable.From( OnCheckMoveLoop ) );

			_actionStream = owner.GetNode<AudioStreamPlayer2D>( "ActionStream" );

			_hitStream = new AudioStreamPlayer2D() {
				Stream = _hitMarker
			};
			owner.AddChild( _hitStream );
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
				_moveStream.Stream = _moveSound;
				_moveStream.Play();
			}
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
			_hitStream.Stop();
		}

		/*
		===============
		OnDamagePlayer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnDamagePlayer( in PlayerTakeDamageEventArgs args ) {
			_hitStream.Stream = _hitMarker;
			_hitStream.PitchScale = (float)GD.RandRange( 1.0f, 2.0f );
			_hitStream.Play();
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
			_moveStream.Stream = _moveSound;
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

		/*
		===============
		OnWeaponUsed
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWeaponUsed( in EmptyEventArgs args ) {
			_actionStream.Stream = _useWeapon;
			_actionStream.PitchScale = (float)GD.RandRange( 1.2f, 1.8f );
			_actionStream.Play();
		}

		/*
		===============
		OnWeaponSwitched
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWeaponSwitched( in PlayerHarpoonChangedEventArgs args ) {
			_actionStream.Stream = _switchWeapon;
			_actionStream.PitchScale = (float)GD.RandRange( 1.2f, 1.8f );
			_actionStream.Play();
		}
	};
};
