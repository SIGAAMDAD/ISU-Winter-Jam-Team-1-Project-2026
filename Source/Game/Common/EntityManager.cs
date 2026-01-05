using Game.Player;
using Godot;
using System.Collections.Generic;

namespace Game.Common {
	public sealed partial class EntityManager : Node {
		[Export]
		private PlayerManager _player;

		public IReadOnlyDictionary<int, Node2D> EntityCache => _entityCache;
		private readonly Dictionary<int, Node2D> _entityCache = new Dictionary<int, Node2D>();

		public Vector2 TargetPosition => _player.GlobalPosition;

		/*
		===============
		RegisterEntity
		===============
		*/
		public void RegisterEntity( Node2D entity ) {
			_entityCache[ entity.GetPath().GetHashCode() ] = entity;
		}
	};
};