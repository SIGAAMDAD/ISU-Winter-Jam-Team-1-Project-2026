using Godot;

namespace Game.Player.Weapons {
	public sealed partial class ProjectileNode : AnimatedSprite2D {
		public int ProjectileId => GetPath().GetHashCode();
	};
};
