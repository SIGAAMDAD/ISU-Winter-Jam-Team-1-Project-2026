namespace Game.Mobs {
	public readonly record struct MobTakeDamageEventArgs(
		int MobId,
		float Amount
	);
};