using System;
using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Common {
	public partial class MusicManager : Node {
		[Export]
		private AudioStream[] _introThemes;
		[Export]
		private AudioStream[] _combatThemes;

		private int _toggle = 0;

		private AudioStreamPlayer _audioPlayer;

		/*
		===============
		OnAudioFinished
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnAudioFinished() {
			if ( GameStateManager.Instance.GameState == GameState.UpgradeMenu ) {
				_audioPlayer.Stream = _introThemes[ _toggle ];
				_audioPlayer.Play();
			} else {
				_audioPlayer.Stream = _combatThemes[ _toggle ];
				_audioPlayer.Play();
			}
		}

		/*
		===============
		OnWaveStarted
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWaveStarted( in EmptyEventArgs args ) {
			_toggle = ( _toggle + 1 ) % _introThemes.Length;
			_audioPlayer.Stream = _introThemes[ _toggle ];
			_audioPlayer.Play();
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready() {
			base._Ready();

			if ( _introThemes.Length != _combatThemes.Length ) {
				throw new Exception( "Intro themes and combat themes must be the same length!" );
			}

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();

			var waveStarted = eventFactory.GetEvent<EmptyEventArgs>( nameof( WaveManager ), nameof( WaveManager.WaveStarted ) );
			waveStarted.Subscribe( this, OnWaveStarted );

			_audioPlayer = GetNode<AudioStreamPlayer>( nameof( AudioStreamPlayer ) );
			_audioPlayer.Connect( AudioStreamPlayer.SignalName.Finished, Callable.From( OnAudioFinished ) );
			OnWaveStarted( EmptyEventArgs.Args );
		}
	};
};
