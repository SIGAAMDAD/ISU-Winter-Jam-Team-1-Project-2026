using Godot;

namespace Game.Mobs {
	public partial class MobTierDefinition : Resource {
		[Export]
		public int Tier {get; private set; }
		[Export]
		public int MinWaveToAppear { get; private set; }
		[Export]
		public int MaxInConcurrent { get; private set; }
		[Export]
		public float BaseWeight { get; private set; }
		[Export]
		public float GrowthMultiplier { get; private set; }
	};
};
