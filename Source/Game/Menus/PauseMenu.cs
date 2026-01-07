using Game.Systems;
using Godot;
using Nomad.Core.Events;

namespace Game.Menus {
	/*
	===================================================================================

	PauseMenu

	===================================================================================
	*/
	/// <summary>
	/// Handles pause menu operations.
	/// </summary>

	public partial class PauseMenu : CanvasLayer {
		public IGameEvent<EmptyEventArgs> PauseGame => _pauseGame;
		private IGameEvent<EmptyEventArgs> _pauseGame;

		public IGameEvent<EmptyEventArgs> UnpauseGame => _unpauseGame;
		private IGameEvent<EmptyEventArgs> _unpauseGame;

		/*
		===============
		HookButtons
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void HookButtons() {
			Button resumeGameButton = GetNode<Button>( "%ResumeGame" );
			resumeGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnResumeGame ) );

			Button quitGameButton = GetNode<Button>( "%ExitGame" );
			quitGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnQuitGame ) );
		}

		/*
		===============
		OnResumeGame
		===============
		*/
		/// <summary>
		///
		/// </summary>
		private void OnResumeGame() {
			GetTree().Paused = false;
			_unpauseGame.Publish( new EmptyEventArgs() );
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
			OnResumeGame();
			GameStateManager.Instance.ActivateTitleScreen();
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

			var eventFactory = GetNode<NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_pauseGame = eventFactory.GetEvent<EmptyEventArgs>( nameof( PauseGame ) );
			_unpauseGame = eventFactory.GetEvent<EmptyEventArgs>( nameof( UnpauseGame ) );

			SetProcessUnhandledInput( true );
			SetProcess( false );
			SetPhysicsProcess( false );
			SetProcessInput( false );

			HookButtons();
		}

		/*
		===============
		_UnhandledInput
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="event"></param>
		public override void _UnhandledInput( InputEvent @event ) {
			base._UnhandledInput( @event );

			if ( Input.IsActionJustPressed( "pause" ) ) {
				
			}
		}
	};
};