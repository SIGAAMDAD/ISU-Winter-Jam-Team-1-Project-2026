using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	StatsList

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class StatsList : IDisposable {
		private readonly ImmutableDictionary<InternString, StatValueContainer> _stats;

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
			_stats = new Dictionary<InternString, StatValueContainer> {
				[ PlayerStats.MAX_HEALTH ] = container.GetNode<StatValueContainer>( "MaxHealth" ),
				[ PlayerStats.HEALTH_REGEN ] = container.GetNode<StatValueContainer>( "HealthRegen" ),
				[ PlayerStats.ARMOR ] = container.GetNode<StatValueContainer>( "Armor" ),
				[ PlayerStats.SPEED ] = container.GetNode<StatValueContainer>( "Speed" ),
				[ PlayerStats.ATTACK_DAMAGE ] = container.GetNode<StatValueContainer>( "AttackDamage" ),
				[ PlayerStats.ATTACK_SPEED ] = container.GetNode<StatValueContainer>( "AttackSpeed" )
			}.ToImmutableDictionary();
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		/// s
		/// </summary>
		public void Dispose() {
			_stats.Clear();
		}
	};
};
