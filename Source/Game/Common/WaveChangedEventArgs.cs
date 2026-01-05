namespace Game.Common {
	public readonly record struct WaveChangedEventArgs(
		int OldWave,
		int NewWave
	);
};