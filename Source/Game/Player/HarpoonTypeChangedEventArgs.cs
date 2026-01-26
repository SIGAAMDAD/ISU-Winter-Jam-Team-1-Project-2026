using Game.Player.Upgrades;

namespace Game.Player {
	public readonly record struct HarpoonTypeChangedEventArgs(
		HarpoonType Type
	);
};