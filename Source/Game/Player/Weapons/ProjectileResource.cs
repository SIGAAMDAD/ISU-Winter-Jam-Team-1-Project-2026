using Godot;

namespace Game.Player.Weapons {
	/*
	===================================================================================

	v

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class ProjectileResource : Resource {
		[Export]
		public AudioStream FlySound;
		[Export]
		public float CooldownTime = 1.0f;
		[Export]
		public float Speed = 10.0f;
		[Export]
		public float Damage = 10.0f;
		[Export]
		public bool HasSpriteOverride;
		[Export]
		public bool HasAutoAttackOverride;
	};
};
