using Godot;
using System;

namespace Game.Player {
	public sealed partial class PlayerWaterWake : Node {
		private ShaderMaterial _waterMaterial;
		private float _updateInterval = 0.016f;

		private float _timeSinceLastUpdate = 0.0f;

		private readonly Player _owner;

		/*
		===============
		PlayerWaterWake
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		public PlayerWaterWake( Player owner ) {
			_owner = owner;

			Vector2 screenCenter = _owner.GetViewport().GetVisibleRect().Size / 2.0f;
			Vector2 normalizedPos = _owner.GlobalPosition / screenCenter;

			_waterMaterial = ResourceLoader.Load<ShaderMaterial>( "res://Assets/Prefabs/World/Water.tres" );
			_waterMaterial.SetShaderParameter( "boat_position", normalizedPos );
		}

		/*
		===============
		Update
		===============
		*/
		/// <summary>
		/// 
		/// </summary>
		/// <param name="delta"></param>
		public void Update( float delta ) {
			_timeSinceLastUpdate += delta;

			if ( _timeSinceLastUpdate >= _updateInterval ) {
				_timeSinceLastUpdate = 0.0f;

				Vector2 viewportSize = _owner.GetViewport().GetVisibleRect().Size;
				Vector2 normalizedPos = _owner.GlobalPosition / viewportSize;

				Vector2 velocity = _owner.Velocity;
				_waterMaterial.SetShaderParameter( "boat_position", normalizedPos );
				_waterMaterial.SetShaderParameter( "boat_velocity", velocity );

				float speed = velocity.Length();
				_waterMaterial.SetShaderParameter( "stream_strength", Math.Min( speed * 10.0f, 0.5f ) );
			}
		}
	};
};