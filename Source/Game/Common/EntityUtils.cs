using Godot;
using System;

namespace Game.Common {
	/*
	===================================================================================
	
	EntityUtils
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public static class EntityUtils {
		/*
		===============
		CalcSpeed
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="frameVelocity"></param>
		/// <param name="speed"></param>
		/// <param name="delta"></param>
		/// <param name="moveVector"></param>
		public static void CalcSpeed( ref Vector2 frameVelocity, Vector2 speed, float delta, Vector2 moveVector ) {
			Vector2 targetVelocity = moveVector * speed;
			frameVelocity += ( targetVelocity - frameVelocity ) * (float)( 1.0f - Math.Exp( -8.0f * delta ) );
		}
	};
};