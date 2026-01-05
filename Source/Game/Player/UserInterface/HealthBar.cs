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
			if ( args.StatId == PlayerStats.HEALTH_ID ) {
				_node.Value = args.Value;
			}
		}
	};
};
