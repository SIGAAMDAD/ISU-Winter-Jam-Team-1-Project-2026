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

		public float Value { get; set; }

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

			SetDeferred( PropertyName.Modulate, Colors.White );
			if ( Value > 50.0f ) {
				SetDeferred( PropertyName.Modulate,Modulate = Colors.Yellow );
			}
			if ( Value > 100.0f ) {
				SetDeferred( PropertyName.Modulate,Modulate = Colors.Red );
			}

			SetDeferred( PropertyName.Visible, true );
			SetDeferred( PropertyName.Text, Value.ToString() );

			var hideTimer = new Timer() {
				WaitTime = 0.90f,
				OneShot = true
			};
			hideTimer.Connect( Timer.SignalName.Timeout, Callable.From( QueueFree ) );
			CallDeferred( MethodName.AddChild, hideTimer );
			hideTimer.CallDeferred( Timer.MethodName.Start );

			var fadeTween = CreateTween();
			fadeTween.CallDeferred( Tween.MethodName.TweenInterval, hideTimer.WaitTime * 0.75f );
			fadeTween.CallDeferred( Tween.MethodName.TweenProperty, this, ModulateNodePath, Colors.Transparent, hideTimer.WaitTime * 0.25f );
		}
	};
};