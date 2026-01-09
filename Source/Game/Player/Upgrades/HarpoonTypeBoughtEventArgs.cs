namespace Game.Player.Upgrades {
	public readonly record struct HarpoonTypeUpgradeBoughtEventArgs(
		HarpoonType Type,
		float Cost
	);
};