using Game.Player.Upgrades;

namespace Game.Player {
	public readonly record struct HarpoonCooldownChangedEventArgs(
		HarpoonType Type,
		float Progress
	);
};
