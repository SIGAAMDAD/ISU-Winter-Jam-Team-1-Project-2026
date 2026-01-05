using Game.Player;
using Godot;

namespace Game.Mobs {
	public partial class Tide : Sprite2D {
		public override void _Ready() {
			base._Ready();

			var collisionArea = GetNode<Area2D>( "CollisionBody" );
			collisionArea.Connect( Area2D.SignalName.BodyShapeEntered, Callable.From<Rid, Node2D, int, int>( OnBodyEntered ) );
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
				player.Damage( 20.0f );
				GetParent().CallDeferred( MethodName.RemoveChild, this );
				CallDeferred( MethodName.QueueFree );
			}
		}
	};
};