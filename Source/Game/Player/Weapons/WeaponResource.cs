using Game.Player.Weapons;
using Godot;
using System;

namespace Game.Player {
	public partial class WeaponResource : Resource {
		[Export]
		public string Id = String.Empty;
		[Export]
		public float Damage = 0.0f;
		[Export]
		public float Cooldown = 0.0f;
		[Export]
		public float SplashRadius = 1.0f;
		[Export]
		public float Knockback = 1.0f;
		[Export]
		public float LifeSteal = 0.0f;
		[Export]
		public WeaponType Type = WeaponType.Ranged;
		[Export]
		public PackedScene[] ProjectPrefabs;
	};
};