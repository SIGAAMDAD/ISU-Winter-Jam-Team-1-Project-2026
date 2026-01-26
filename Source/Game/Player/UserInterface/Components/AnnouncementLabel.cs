using Game.Common;
using Game.Systems;
using Nomad.Core.Events;
using Godot;
using Nomad.Events;
using System;
using Game.Systems.Caching;
using Nomad.Core.Util;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	AnnouncementLabel

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class AnnouncementLabel : IDisposable {
		private static readonly NodePath @ModulateNodePath = "modulate";

		private readonly Label _label;
		private readonly Callable _setUpgradeMenuCallback;

		private readonly DisposableSubscription<WaveChangedEventArgs> _waveCompletedEvent;
		private readonly DisposableSubscription<EmptyEventArgs> _playerDeathEvent;

		private readonly AudioStreamPlayer _audioStream;
		private readonly AudioStream _waveCompleted;

		private bool _isPlayerDead = false;

		/*
		===============
		AnnouncementLabel
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="label"></param>
		/// <param name="eventFactory"></param>
		public AnnouncementLabel( Label label, IGameEventRegistryService eventFactory ) {
			_label = label;

			_waveCompletedEvent = new DisposableSubscription<WaveChangedEventArgs>(
				eventFactory.GetEvent<WaveChangedEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveCompleted ) ),
				OnWaveCompleted
			);

			_playerDeathEvent = new DisposableSubscription<EmptyEventArgs>(
				eventFactory.GetEvent<EmptyEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.PlayerDeath ) ),
				OnPlayerDeath
			);

			var quitGameButton = label.GetNode<Button>( "QuitGameButton" );
			quitGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnQuitGame ) );

			AudioCache.Instance.GetCached( FilePath.FromResourcePath( "res://Assets/Audio/SoundEffects/wave_completed.wav" ) ).Get( out _waveCompleted );

			_audioStream = new AudioStreamPlayer();
			_label.AddChild( _audioStream );

			_setUpgradeMenuCallback = Callable.From( OnSetUpgradeMenu );
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
			_playerDeathEvent.Dispose();
		}

		/*
		===============
		OnQuitGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnQuitGame() {
			_label.GetTree().Quit();
		}

		/*
		===============
		OnSetUpgradeMenu
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnSetUpgradeMenu() {
			GameStateManager.Instance.SetGameState( GameState.UpgradeMenu );
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
			if ( _isPlayerDead ) {
				return;
			}

			_audioStream.Stream = _waveCompleted;
			_audioStream.Play();

			SetLabel( "WAVE FINISHED", _setUpgradeMenuCallback );
		}

		/*
		===============
		OnPlayerDeath
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnPlayerDeath( in EmptyEventArgs args ) {
			_isPlayerDead = true;

			SetLabel( "YOU DIED", null );
		}

		/*
		===============
		SetLabel
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="text"></param>
		/// <param name="finishedCallback"></param>
		private void SetLabel( string text, Callable? finishedCallback ) {
			_label.Text = text;

			var fadeTween = _label.CreateTween();
			fadeTween.TweenProperty( _label, ModulateNodePath, Colors.White, 0.25f );

			if ( !finishedCallback.HasValue ) {
				return;
			}

			fadeTween.TweenInterval( 1.0f );
			fadeTween.TweenProperty( _label, ModulateNodePath, Colors.Transparent, 0.5f );
			fadeTween.Connect( Tween.SignalName.Finished, finishedCallback.Value );
		}
	};
};
