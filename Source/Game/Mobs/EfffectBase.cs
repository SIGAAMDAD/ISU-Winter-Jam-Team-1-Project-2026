using Game.Player;
using Godot;

namespace Game.Mobs {
	/*
	===================================================================================
	
	EffectBase
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class EffectBase : AnimatedSprite2D {
		/*
		===============
		OnPlayerEntered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OnPlayerEntered( PlayerManager player ) {
		}

		/*
		===============
		OnPlayerExited
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		protected virtual void OnPlayerExited( PlayerManager player ) {
		}

		/*
		===============
		OnBodyEntered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyEntered( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager player ) {
				OnPlayerEntered( player );
			}
		}

		/*
		===============
		OnBodyEntered
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bodyRid"></param>
		/// <param name="body"></param>
		/// <param name="bodyShapeIndex"></param>
		/// <param name="localShapeIndex"></param>
		private void OnBodyExited( Rid bodyRid, Node2D body, int bodyShapeIndex, int localShapeIndex ) {
			if ( body is PlayerManager player ) {
				OnPlayerExited( player );
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		public override void _Ready() {
			base._Ready();

			var collisionArea = GetNode<Area2D>( "Area2D" );
			collisionArea.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyEntered ) );
			collisionArea.Connect( Area2D.SignalName.BodyShapeExited, Callable.From<Rid, Node2D, int, int>( OnBodyExited ) );
		}
	};
};