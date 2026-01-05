using Game.Common;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System.Collections.Generic;

namespace Game.Player {
	/*
	===================================================================================
	
	PlayerStats
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed class PlayerStats( PlayerManager player, IGameEventRegistryService eventFactory ) {
		public static readonly InternString HEALTH_ID = new( nameof( Health ) );
		public static readonly InternString SPEED_ID = new( nameof( Speed ) );
		public static readonly InternString DAMAGE_RESISTANCE_ID = new( nameof( DamageResistance ) );

		public float Speed => _statCache[ new( nameof( Speed ) ) ];
		public float Health => _statCache[ new( nameof( Health ) ) ];
		public float DamageResistance => _statCache[ new( nameof( DamageResistance ) ) ];

		private readonly Dictionary<InternString, float> _statCache = new Dictionary<InternString, float> {
			[ new( nameof( Speed ) ) ] = 100.0f,
			[ new( nameof( Health ) ) ] = 100.0f,
			[ new( nameof( DamageResistance ) ) ] = 0.95f
		};

		private readonly int _entityId = player.GetPath().GetHashCode();

		public IGameEvent<EntityTakeDamageEventArgs> TakeDamage => _takeDamage;
		private readonly IGameEvent<EntityTakeDamageEventArgs> _takeDamage = eventFactory.GetEvent<EntityTakeDamageEventArgs>( nameof( TakeDamage ) );

		public IGameEvent<StatChangedEventArgs> StatChanged => _statChanged;
		private readonly IGameEvent<StatChangedEventArgs> _statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( StatChanged ) );

		/*
		===============
		Damage
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void Damage( float value ) {
			float health = Health;
			health -= value;
			_statCache[ HEALTH_ID ] = health;

			_takeDamage.Publish( new EntityTakeDamageEventArgs( _entityId, value ) );
			_statChanged.Publish( new StatChangedEventArgs( HEALTH_ID, health ) );
		}
	};
};