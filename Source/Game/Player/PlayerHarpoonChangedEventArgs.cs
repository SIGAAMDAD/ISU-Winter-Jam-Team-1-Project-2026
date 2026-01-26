using Game.Player.Upgrades;

namespace Game.Player {
	public readonly record struct PlayerHarpoonChangedEventArgs(
		HarpoonType Type
	);
};
