public readonly record struct GameStateChangedEventArgs(
	GameState OldState,
	GameState NewState
);