using Godot;
using Nomad.Core.Events;
using Nomad.Core.Util;

namespace Game.Player.UserInterface {
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

		private Label _value;

		private InternString _statId => new( _statName );

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

			var icon = GetNode<TextureRect>( "Icon" );
			icon.Texture = _icon;

			var eventFactory = GetNode<Systems.NomadBootstrapper>( "/root/NomadBootstrapper" ).ServiceLocator.GetService<IGameEventRegistryService>();
			var statChanged = eventFactory.GetEvent<StatChangedEventArgs>( nameof( PlayerStats.StatChanged ) );
			statChanged.Subscribe( this, OnUpdate );
		}
	};
};