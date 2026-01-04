using Godot;

/// <summary>
/// Handles pause menu operations.
/// </summary>
public partial class PauseMenu : CanvasLayer
{
    /// <summary>
    ///
    /// </summary>
    public override void _Ready()
    {
        base._Ready();

        HookButtons();
    }

    /// <summary>
    ///
    /// </summary>
    private void HookButtons()
    {
        Button resumeGameButton = GetNode<Button>("%ResumeGame");
        resumeGameButton.Connect(Button.SignalName.Pressed, Callable.From(OnResumeGame));

        Button quitGameButton = GetNode<Button>("%ExitGame");
        quitGameButton.Connect(Button.SignalName.Pressed, Callable.From(OnQuitGame));
    }

    /// <summary>
    ///
    /// </summary>
    private void OnResumeGame()
    {
        GetTree().Paused = false;
    }

    /// <summary>
    ///
    /// </summary>
    private void OnQuitGame()
    {
        OnResumeGame();
        GetTree().ChangeSceneToFile("res://Source/Menus/MainMenu.tscn");
    }
}
