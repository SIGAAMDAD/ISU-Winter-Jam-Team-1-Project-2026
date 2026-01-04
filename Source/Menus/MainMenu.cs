using System;
using Godot;

public partial class MainMenu : Control
{
    private TextureRect _buttonPressedTexture;
    private Timer _showPressedTextureTimer;
    private Action _buttonCallback;

    public override void _Ready()
    {
        base._Ready();

        _buttonPressedTexture = GetNode<TextureRect>("%ButtonPressed");
        _showPressedTextureTimer = new Timer()
        {
            WaitTime = 0.10f,
            OneShot = true
        };
        _showPressedTextureTimer.Connect(Timer.SignalName.Timeout, Callable.From(
            () =>
            {
                _buttonPressedTexture.Hide();
                _buttonCallback.Invoke();
            }
        ));
        AddChild(_showPressedTextureTimer);

        InitGPUParticles();
        InitAmbiencePlayer();
        HookButtons();
    }

    /// <summary>
    /// Initializes the background audio player.
    /// </summary>
    private void InitAmbiencePlayer()
    {
        // setup the player to loop
        AudioStreamPlayer audioPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
        audioPlayer.Connect(AudioStreamPlayer.SignalName.Finished, Callable.From(() => audioPlayer.Play()));
    }

    /// <summary>
    /// Initializes the rain particles.
    /// </summary>
    private void InitGPUParticles()
    {
        float windowSizeWidth = DisplayServer.WindowGetSize().X;

        GpuParticles2D rainParticles = GetNode<GpuParticles2D>("GPUParticles2D");
        rainParticles.VisibilityRect = new Rect2(
            new Vector2(0.0f, -100.0f),
            new Vector2(windowSizeWidth, 200.0f)
        );

        // size the process material appropriately based on the window's size
        if (rainParticles.ProcessMaterial is ParticleProcessMaterial material)
        {
            material.EmissionShapeOffset = new Vector3(windowSizeWidth / 2.0f, 0.0f, 0.0f);
            material.EmissionBoxExtents = new Vector3(windowSizeWidth / 2.0f, 0.0f, 0.0f);
        }
    }

    /// <summary>
    /// Hooks up button signals.
    /// </summary>
    private void HookButtons()
    {
        Button startGameButton = GetNode<Button>("%StartGame");
        startGameButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(OnStartGame)));

        Button creditsButton = GetNode<Button>("%Credits");
        creditsButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(OnShowCreditsMenu)));

        Button quitGameButton = GetNode<Button>("%QuitGame");
        quitGameButton.Connect(Button.SignalName.Pressed, Callable.From(() => OnButtonPressed(OnQuitGame)));
    }

    /// <summary>
    /// Shows a flash of lightning whenever a button is clicked.
    /// </summary>
    private void OnButtonPressed(Action callback)
    {
        _buttonPressedTexture.Show();
        _showPressedTextureTimer.Start();
        _buttonCallback = callback;
    }

    /// <summary>
    /// Shows the credits menu.
    /// </summary>
    private void OnShowCreditsMenu()
    {
        GetTree().ChangeSceneToFile("res://Source/Menus/CreditsMenu.tscn");
    }

    /// <summary>
    /// Loads the world scene and starts a new game.
    /// </summary>
    private void OnStartGame()
    {
        GetTree().ChangeSceneToFile("res://Assets/Prefabs/World/World.tscn");
    }

    /// <summary>
    /// Exits the game.
    /// </summary>
    private void OnQuitGame()
    {
        GetTree().Quit();
    }
};
