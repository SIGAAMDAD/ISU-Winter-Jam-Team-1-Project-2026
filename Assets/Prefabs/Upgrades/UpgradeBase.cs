using Godot;

namespace Prefabs {
	public partial class UpgradeBase : Resource {
		[Export]
		public float Tier1Cost;
		[Export]
		public float Tier2Cost;
		[Export]
		public float Tier3Cost;
		[Export]
		public float Tier4Cost;

		[Export]
		public float Tier1AddAmount;
		[Export]
		public float Tier2AddAmount;
		[Export]
		public float Tier3AddAmount;
		[Export]
		public float Tier4AddAmount;
	};
};