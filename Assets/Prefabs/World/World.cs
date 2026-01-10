using Game.Player.UserInterface;
using Godot;

namespace Prefabs {
	/*
	===================================================================================
	
	World
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class World : Node2D {
		private AudioStreamPlayer _ambience;
		private readonly DamageNumberFactory _damageNumberFactory = new DamageNumberFactory();

		/*
		===============
		OnAudioStreamFinished
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		private void OnAudioStreamFinished() {
			_ambience.Play();
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

			_ambience = GetNode<AudioStreamPlayer>( "WorldAmbience" );
			_ambience.Connect( AudioStreamPlayer.SignalName.Finished, Callable.From( OnAudioStreamFinished ) );

			GetTree().Root.AddChild( _damageNumberFactory );
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

			GetTree().Root.RemoveChild( _damageNumberFactory );
		}
	};
};
