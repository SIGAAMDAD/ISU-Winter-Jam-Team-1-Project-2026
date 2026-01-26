using Nomad.Core.Events;
using Godot;
using System;
using Nomad.Events;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	MoneyCounter

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class MoneyCounter : IDisposable {
		private readonly Label _countLabel;
		private readonly DisposableSubscription<StatChangedEventArgs> _statChangedEvent;

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

			_statChangedEvent = new DisposableSubscription<StatChangedEventArgs>(
				eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) ),
				OnStatChanged
			);
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
			if ( args.StatId == PlayerStats.MONEY ) {
				_countLabel.SetDeferred( Label.PropertyName.Text, $"{args.Value}" );
			}
		}
	};
};
