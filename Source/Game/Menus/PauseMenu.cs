using Game.Systems;
using Godot;

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
		private GameState _prevState;

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
			GameStateManager.Instance.SetGameState( _prevState );
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
			GameStateManager.Instance.SetGameState( GameState.TitleScreen );
		}

		/*
		===============
		OnGameStateChanged
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		private void OnGameStateChanged( in GameStateChangedEventArgs args ) {
			if ( args.NewState == GameState.Paused ) {
				_prevState = args.OldState;
				GetTree().Paused = true;
			} else if ( args.OldState == GameState.Paused ) {
				GetTree().Paused = false;
			}
			Visible = GetTree().Paused;
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

			SetProcessUnhandledInput( true );
			SetProcess( false );
			SetPhysicsProcess( false );
			SetProcessInput( false );

			GameStateManager.GameStateChanged.Subscribe( this, OnGameStateChanged );

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
				if ( GameStateManager.Instance.GameState == GameState.Paused ) {
					GameStateManager.Instance.SetGameState( _prevState );
				} else {
					GameStateManager.Instance.SetGameState( GameState.Paused );
				}
			}
		}
	};
};