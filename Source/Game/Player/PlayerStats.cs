using Godot;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerStats
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed partial class PlayerStats : Node {
		public float Speed => _speed;
		private float _speed = 150.0f;
	};
};