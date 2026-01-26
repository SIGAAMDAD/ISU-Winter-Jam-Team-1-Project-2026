using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Player.Upgrades;
using Godot;

namespace Game.Player.Weapons {
	/*
	===================================================================================

	WeaponFactory

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed partial class WeaponFactory : Node {
		private readonly Dictionary<HarpoonType, ProjectileNode> _harpoonTypes;

		/*
		===============
		GetProjectileData
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public ProjectileNode GetProjectileData( HarpoonType type ) {
			return _harpoonTypes[ type ];
		}

		/*
		===============
		CreateProjectile
		===============
		*/
		/*
		public Projectile CreateProjectile( Vector2 position, float speed, float rotation ) {
			var projectile = new Projectile();
			projectile.Show( position, speed, rotation );

			return projectile;
		}
		*/

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready() {
			base._Ready();
		}
	};
};
