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
		}
	};
};
