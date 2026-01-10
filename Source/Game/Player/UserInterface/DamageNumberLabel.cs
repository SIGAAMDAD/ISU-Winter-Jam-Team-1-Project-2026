using Godot;

namespace Game.Player.UserInterface {
	/*
	===================================================================================
	
	DamageNumberLabel
	
	===================================================================================
	*/
	/// <summary>
	/// 
	/// </summary>

	public sealed partial class DamageNumberLabel : Label {
		private static readonly NodePath @ModulateNodePath = "modulate";

		private readonly Timer _hideTimer = new Timer() {
			WaitTime = 0.90f,
			OneShot = true
		};

		public float Value { get; set; }

		/*
		===============
		Show
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		/// <param name="position"></param>
		public void Show( float value, Vector2 position ) {
			Modulate = Colors.White;
			if ( value > 50.0f ) {
				Modulate = Colors.Yellow;
			}
			if ( value > 100.0f ) {
				Modulate = Colors.Red;
			}

			Text = value.ToString();
			GlobalPosition = position;

			_hideTimer.Start();

			var fadeTween = CreateTween();
			fadeTween.CallDeferred( Tween.MethodName.TweenInterval, _hideTimer.WaitTime * 0.75f );
			fadeTween.CallDeferred( Tween.MethodName.TweenProperty, this, ModulateNodePath, Colors.Transparent, _hideTimer.WaitTime * 0.25f );

			Show();
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

			_hideTimer.Connect( Timer.SignalName.Timeout, Callable.From( Hide ) );
			AddChild( _hideTimer );

			Visible = false;
		}
	};
};