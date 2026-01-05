namespace Game.Common {
	public readonly record struct EntityTakeDamageEventArgs(
		int EntityId,
		float Amount
	);
};