namespace Game.Systems {
	/// <summary>
	/// Enumeration marker for the current status of the game.
	/// </summary>
	/// <remarks>
	/// Add to this when needed.
	/// </remarks>
	public enum GameState : byte {
		/// <summary>
		/// We're in the title screen.
		/// </summary>
		TitleScreen,

		/// <summary>
		/// We're in a level.
		/// </summary>
		Level,

		/// <summary>
		/// The game's pause menu is being shown, and we've completely halted the main game loop.
		/// </summary>
		Paused,

		UpgradeMenu,

		Count
	};
};