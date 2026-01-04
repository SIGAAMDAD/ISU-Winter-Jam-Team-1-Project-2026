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
		/// <summary>
		///
		/// </summary>
		public override void _Ready() {
			base._Ready();

			HookButtons();
		}

		/// <summary>
		///
		/// </summary>
		private void HookButtons() {
			Button resumeGameButton = GetNode<Button>( "%ResumeGame" );
			resumeGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnResumeGame ) );

			Button quitGameButton = GetNode<Button>( "%ExitGame" );
			quitGameButton.Connect( Button.SignalName.Pressed, Callable.From( OnQuitGame ) );
		}

		/// <summary>
		///
		/// </summary>
		private void OnResumeGame() {
			GetTree().Paused = false;
		}

		/// <summary>
		///
		/// </summary>
		private void OnQuitGame() {
			OnResumeGame();
			GameStateManager.Instance.ActivateTitleScreen();
		}
	};
};