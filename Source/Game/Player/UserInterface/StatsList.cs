using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System.Collections.Generic;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	StatsList
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class StatsList {
		private readonly Dictionary<InternString, StatValueContainer> _stats = new();

		/*
		===============
		StatsList
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="container"></param>
		/// <param name="eventFactory"></param>
		public StatsList( VBoxContainer container, IGameEventRegistryService eventFactory ) {
			_stats[ PlayerStats.MAX_HEALTH ] = container.GetNode<StatValueContainer>( "MaxHealth" );
			_stats[ PlayerStats.HEALTH_REGEN ] = container.GetNode<StatValueContainer>( "HealthRegen" );
			_stats[ PlayerStats.ARMOR ] = container.GetNode<StatValueContainer>( "Armor" );
			_stats[ PlayerStats.SPEED ] = container.GetNode<StatValueContainer>( "Speed" );
			_stats[ PlayerStats.ATTACK_DAMAGE ] = container.GetNode<StatValueContainer>( "AttackDamage" );
			_stats[ PlayerStats.ATTACK_SPEED ] = container.GetNode<StatValueContainer>( "AttackSpeed" );
		}
	};
};