using Game.Mobs;
using Game.Player.Weapons;

namespace Prefabs {
	/*
	===================================================================================
	
	ExplosiveHarpoon
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class ExplosiveHarpoon : Projectile {
		protected override void OnEnemyHit( MobBase mob ) {
			base.OnEnemyHit( mob );
		}
	};
};