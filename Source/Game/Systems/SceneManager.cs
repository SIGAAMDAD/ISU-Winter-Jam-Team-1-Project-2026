using Game.Systems;
using Godot;

public partial class SceneManager : Node {
	public override void _Ready() {
		base._Ready();

		GameStateManager.GameStateChanged.Subscribe( this, OnStateChanged );
	}

	private void OnStateChanged( in GameStateChangedEventArgs args ) {
		if ( args.NewState == GameState.Level ) {
			GetTree().ChangeSceneToFile( "res://Assets/Prefabs/World/World.tscn" );
		} else if ( args.NewState == GameState.TitleScreen ) {
			GetTree().ChangeSceneToFile( "res://Source/Game/Menus/MainMenu.tscn" );
		}
	}
};
