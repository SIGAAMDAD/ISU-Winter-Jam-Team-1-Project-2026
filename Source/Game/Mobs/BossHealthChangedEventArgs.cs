namespace Game.Mobs {
	public readonly record struct BossHealthChangedEventArgs(
		float Value // the boss's current health
	);
};