using Godot;

namespace Game.Player {
	/*
	===================================================================================
	
	Player
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>
	
	public partial class Player : CharacterBody2D {
		private PlayerStats _stats;
		private PlayerController _controller;

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

			

			_stats = GetNode<PlayerStats>( "Stats" );
			_controller = new PlayerController( this, _stats );
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			base._Process( delta );

			_controller.Update( (float)delta );
		}
	};
};