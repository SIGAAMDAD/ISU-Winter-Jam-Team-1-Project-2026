using Game.Mobs;
using Godot;

namespace Prefabs {
	/*
	===================================================================================
	
	Jellyfish
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public partial class Jellyfish : MobBase {
		private static readonly NodePath ModulateNodePath = "modulate";

		/*
		===============
		Damage
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount"></param>
		public override void Damage( float amount ) {
			base.Damage( amount );

			if ( ( _flags & FlagBits.Dead ) != 0 ) {
				CreateTween().CallDeferred( Tween.MethodName.TweenProperty, this, ModulateNodePath, Colors.DimGray, 1.0f );
			}
		}
	};
};
