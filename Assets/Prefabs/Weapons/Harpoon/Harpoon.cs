using Game.Mobs;
using Game.Player.Weapons;

namespace Prefabs {
	/*
	===================================================================================
	
	Harpoon
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class Harpoon : Projectile {
		/*
		===============
		OnEnemyHit
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="mob"></param>
		protected override void OnEnemyHit( MobBase mob ) {
			base.OnEnemyHit( mob );

			QueueFree();
		}
	};
};
