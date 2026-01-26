using System;
using Game.Player.Upgrades;
using Godot;
using Nomad.Core.Events;
using Nomad.Events;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	WeaponSlotContainer

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public sealed class WeaponSlotContainer : IDisposable {
		private readonly HarpoonSlotContainer[] _slotIcons;
		private HarpoonType _currentSlot;

		private readonly DisposableSubscription<PlayerHarpoonChangedEventArgs> _harpoonTypeChangedEvent;
		private readonly DisposableSubscription<HarpoonTypeUpgradeBoughtEventArgs> _harpoonBoughtEvent;

		/*
		===============
		WeaponSlotContainer
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="container"></param>
		/// <param name="eventFactory"></param>
		public WeaponSlotContainer( VBoxContainer container, IGameEventRegistryService eventFactory ) {
			var children = container.GetChildren();
			_slotIcons = new HarpoonSlotContainer[ children.Count ];
			for ( int i = 0; i < children.Count; i++ ) {
				if ( children[ i ] is HarpoonSlotContainer slot ) {
					_slotIcons[ i ] = slot;
					slot.Modulate = Colors.DarkGray;
					slot.Hide();
				}
			}

			OnWeaponSlotChanged( new PlayerHarpoonChangedEventArgs( HarpoonType.Default ) );

			_slotIcons[ (int)HarpoonType.Default ].Show();

			_harpoonTypeChangedEvent = new DisposableSubscription<PlayerHarpoonChangedEventArgs>(
				eventFactory.GetEvent<PlayerHarpoonChangedEventArgs>( nameof( PlayerAttackController ), nameof( PlayerAttackController.HarpoonChanged ) ),
				OnWeaponSlotChanged
			);
			_harpoonBoughtEvent = new DisposableSubscription<HarpoonTypeUpgradeBoughtEventArgs>(
				eventFactory.GetEvent<HarpoonTypeUpgradeBoughtEventArgs>( nameof( UpgradeManager ), nameof( UpgradeManager.HarpoonBought ) ),
				OnHarpoonBought
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
			_harpoonTypeChangedEvent.Dispose();
			_harpoonBoughtEvent.Dispose();
		}

		/*
		===============
		OnHarpoonBought
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnHarpoonBought( in HarpoonTypeUpgradeBoughtEventArgs args ) {
			_slotIcons[ (int)args.Type ].Show();
		}

		/*
		===============
		OnWeaponSlotChanged
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnWeaponSlotChanged( in PlayerHarpoonChangedEventArgs args ) {
			HarpoonSlotContainer oldSlot = _slotIcons[ (int)_currentSlot ];
			_currentSlot = args.Type;
			HarpoonSlotContainer newSlot = _slotIcons[ (int)_currentSlot ];

			oldSlot.Modulate = Colors.DarkGray;
			newSlot.Modulate = Colors.White;
		}
	};
};
