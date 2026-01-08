using Godot;

namespace Prefabs {
	public partial class UpgradeBase : Resource {
		[Export]
		public Godot.Collections.Array<float> TierCosts;
		[Export]
		public Godot.Collections.Array<float> TierIncreaseAmounts;
	};
};