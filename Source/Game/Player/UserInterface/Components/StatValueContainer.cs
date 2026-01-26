using System;
using Game.Player.Upgrades;
using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;
using Nomad.Events;

namespace Game.Player.UserInterface.Components {
	/*
	===================================================================================

	StatValueContainer

	===================================================================================
	*/
	/// <summary>
	///
	/// </summary>

	public partial class StatValueContainer : HBoxContainer {
		[Export]
		private Texture2D _icon;
		[Export]
		private string _statName;
		[Export]
		private UpgradeType _statType;

		private Label _value;

		private DisposableSubscription<StatChangedEventArgs> _statChangedEvent;
		private InternString _statId;

		/*
		===============
		OnUpdate
		===============
		*/
		/// <summary>
		///
		/// </summary>
		/// <param name="args"></param>
		private void OnUpdate( in StatChangedEventArgs args ) {
			if ( _statId == args.StatId ) {
				_value.Text = $"{args.Value}";
			}
		}

		/*
		===============
		_Ready
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _Ready() {
			base._Ready();

			_value = GetNode<Label>( "ValueLabel" );
			_statId = new( Enum.GetName( _statType.GetType(), _statType ) );

			var icon = GetNode<TextureRect>( "Icon" );
			icon.Texture = _icon;

			var eventFactory = GetNode<Systems.NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			_statChangedEvent = new DisposableSubscription<StatChangedEventArgs>(
				eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats ), nameof( PlayerStats.StatChanged ) ),
				OnUpdate
			);
		}

		/*
		===============
		_ExitTree
		===============
		*/
		/// <summary>
		///
		/// </summary>
		public override void _ExitTree() {
			base._ExitTree();

			_statChangedEvent.Dispose();
		}
	};
};
