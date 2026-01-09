using Game.Player.Upgrades;
using Godot;

namespace Game.Player.UserInterface.UpgradeInterface {
	/*
	===================================================================================
	
	HarpoonUpgradeButton
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class HarpoonUpgradeButtonNode : HBoxContainer {
		[Export]
		public HarpoonType Type;
		[Export]
		public string UpgradeName;
		[Export]
		public Texture2D Icon;
	};
};