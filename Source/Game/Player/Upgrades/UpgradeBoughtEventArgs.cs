namespace Game.Player.Upgrades {
	public readonly record struct UpgradeBoughtEventArgs(
		UpgradeType Type,
		int CurrentTier,
		float Cost,
		float MultiplyAmount
	);
};