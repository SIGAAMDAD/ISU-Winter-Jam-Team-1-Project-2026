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
		private PlayerAnimator _animator;

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
			_animator = new PlayerAnimator( this );
		}

		/*
		===============
		_Process
		===============
		*/
		public override void _Process( double delta ) {
			base._Process( delta );

			float _delta = (float)delta;
			_controller.Update( _delta, out bool inputWasActive );
			_animator.Update( _delta, inputWasActive );
		}
	};
};