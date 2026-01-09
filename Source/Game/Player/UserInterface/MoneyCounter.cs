using Nomad.Core.Events;
using Godot;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	MoneyCounter
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public sealed class MoneyCounter {
		private readonly Label _countLabel;

		/*
		===============
		MoneyCounter
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="countLabel"></param>
		/// <param name="eventFactory"></param>
		public MoneyCounter( Label countLabel, IGameEventRegistryService eventFactory ) {
			_countLabel = countLabel;

			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnStatChanged );
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
			if ( args.StatId == PlayerStats.MONEY ) {
				_countLabel.SetDeferred( Label.PropertyName.Text, $"{args.Value}" );
			}
		}
	};
};