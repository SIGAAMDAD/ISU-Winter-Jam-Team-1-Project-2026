using System;
using Godot;
using Nomad.Core.Events;
using Nomad.Events;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	HealthBar

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class HealthBar : IDisposable {
		private readonly ProgressBar _node;
		private readonly Label _currentHealth;
		private readonly Label _maxHealth;

		private readonly DisposableSubscription<StatChangedEventArgs> _statChangedEvent;

		/*
		===============
		HealthBar
		===============
		*/
		/// <summary>
		/// Creates a HealthBar.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="owner"></param>
		public HealthBar( ProgressBar node, IGameEventRegistryService eventFactory ) {
			_statChangedEvent = new DisposableSubscription<StatChangedEventArgs>(
				eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) ),
				OnStatChanged
			);

			_node = node;
			_currentHealth = _node.GetNode<Label>( "%CurrentHealth" );
			_maxHealth = _node.GetNode<Label>( "%MaxHealth" );
		}

		/*
		===============
		Dispose
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public void Dispose() {
			_statChangedEvent.Dispose();
		}

		/*
		===============
		OnStatChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnStatChanged( in StatChangedEventArgs args ) {
			if ( args.StatId == PlayerStats.HEALTH ) {
				_node.SetDeferred( ProgressBar.PropertyName.Value, args.Value );
				_currentHealth.SetDeferred( Label.PropertyName.Text, args.Value.ToString() );
			} else if ( args.StatId == PlayerStats.MAX_HEALTH ) {
				_node.SetDeferred( ProgressBar.PropertyName.MaxValue, args.Value );
				_maxHealth.SetDeferred( Label.PropertyName.Text, $"/{args.Value}" );
			}
		}
	};
};
