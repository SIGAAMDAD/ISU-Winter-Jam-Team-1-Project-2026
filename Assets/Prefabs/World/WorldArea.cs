using Game.Player.Weapons;
using Godot;

namespace Prefabs {
	public partial class WorldArea : Area2D {
		/*
		===============
		OnAreaShapeExited
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="areaRid"></param>
		/// <param name="area"></param>
		/// <param name="areaShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnAreaShapeExited( Rid areaRid, Area2D area, int areaShapeIndex, int localShapeIndex ) {
			if ( area is not null && area.GetParent() is Projectile projectile ) {
				// make sure we don't have any memory leaks
				projectile.QueueFree();
			}
		}

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

			Connect( SignalName.AreaShapeExited, Callable.From<Rid, Area2D, int, int>( OnAreaShapeExited ) );
		}
	};
};