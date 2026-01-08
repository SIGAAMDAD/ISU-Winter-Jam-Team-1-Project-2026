using System;

namespace Game.Player {
	public interface IPlayerStatsProvider : IDisposable {
		float HealthRegen { get; }
		float Speed { get; }
		float Health { get; }
		float Armor { get; }
		float MaxHealth { get; }
		float AttackDamage { get; }
		float AttackSpeed { get; }
		float Money { get; }
	};
};