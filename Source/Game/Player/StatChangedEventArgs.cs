using Nomad.Core.Util;

namespace Game.Player {
	public readonly record struct StatChangedEventArgs(
		InternString StatId,
		float Value
	);
};