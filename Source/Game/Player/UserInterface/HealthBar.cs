using Godot;
using Nomad.Core.Events;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	HealthBar
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class HealthBar {
		private readonly ProgressBar _node;
		private readonly Label _currentHealth;
		private readonly Label _maxHealth;

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
			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );

			_node = node;
			_currentHealth = _node.GetNode<Label>( "%CurrentHealth" );
			_maxHealth = _node.GetNode<Label>( "%MaxHealth" );
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
				_node.Value = args.Value;
				_currentHealth.Text = args.Value.ToString();
			} else if ( args.StatId == PlayerStats.MAX_HEALTH ) {
				_node.MaxValue = args.Value;
				_maxHealth.Text = $"/{args.Value}";
			}
		}
	};
};
