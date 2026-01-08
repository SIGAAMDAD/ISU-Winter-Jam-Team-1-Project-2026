using Game.Player.Upgrades;
using Godot;

namespace Game.Player.UserInterface.UpgradeInterface {
	public partial class UpgradeButtonNode : HBoxContainer {
		[Export]
		public UpgradeType Type;
		[Export]
		public string StatName;
		[Export]
		public Texture2D Icon;
	};
};