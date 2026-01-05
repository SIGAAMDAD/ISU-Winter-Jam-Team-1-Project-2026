using Godot;
using System.Collections.Generic;

namespace Game.Common {
	public sealed partial class EntityManager : Node {
		public IReadOnlyDictionary<int, Node2D> EntityCache => _entityCache;
		private readonly Dictionary<int, Node2D> _entityCache = new Dictionary<int, Node2D>();

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