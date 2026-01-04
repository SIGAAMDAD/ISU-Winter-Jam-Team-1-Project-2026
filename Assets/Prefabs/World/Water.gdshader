shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, repeat_enable, filter_nearest;

group_uniforms water;
uniform sampler2D waterNoise : repeat_enable, filter_nearest;
uniform sampler2D waterDistortionNoise : repeat_enable, filter_nearest;
uniform vec4 waterColor : source_color = vec4(0.117, 0.27, 0.58, 1);
uniform float colorCorection : hint_range(0.0, 1.0, 0.01) = 0.35;

group_uniforms water_waves;
uniform float distortionForce : hint_range(0.00, .1, 0.001) = .01;
uniform float WDBrightness : hint_range(0, 3, 0.05) = 1.5;
uniform float WDFreq : hint_range(0.2, .9, 0.05) = 0.6;
uniform float WDSize : hint_range(0.6, 1.2, 0.05) = .9;
uniform float WDSpeed : hint_range(1, 20, 0.05) = 4;
uniform vec2 tiling = vec2(1);
uniform vec2 offSetSpeed = vec2(.1);

group_uniforms BG_distortion;
uniform float backGroundDirX : hint_range(-0.1, 0.1) = 0.01;
uniform float backGroundDirY : hint_range(-0.1, 0.1) = 0.01;

// Wake effect uniforms
group_uniforms wake;
uniform vec2 wake_position = vec2(0.5, 0.5); // Position in UV space (0-1)
uniform float wake_radius : hint_range(0.0, 1.0) = 0.1;
uniform float wake_strength : hint_range(0.0, 0.5) = 0.05;
uniform float wake_speed : hint_range(0.0, 10.0) = 2.0;
uniform float wake_falloff : hint_range(0.1, 10.0) = 2.0;

void fragment() {
	vec4 color = vec4(waterColor.rgb , 1);
	
	// Calculate distance to wake position
	vec2 to_wake = UV - wake_position;
	float wake_dist = length(to_wake);
	
	// Create wake effect (ring waves)
	float wake_time = TIME * wake_speed;
	float wake_wave = sin(wake_dist * 30.0 - wake_time * 5.0) * exp(-wake_dist * 10.0 * wake_falloff);
	
	// Apply wake only within radius
	float wake_mask = smoothstep(wake_radius, wake_radius * 0.8, wake_dist);
	float wake_effect = wake_wave * wake_mask * wake_strength;
	
	// Normalize direction for radial waves
	vec2 wake_dir = normalize(to_wake + vec2(0.001)); // Small offset to avoid division by zero
	vec2 wake_distortion = wake_dir * wake_effect;
	
	vec2 noiseUV = UV * tiling + offSetSpeed * TIME;
	float noiseValue = texture(waterDistortionNoise, noiseUV).r;

	vec2 waterUV = UV * tiling;
	waterUV.x += offSetSpeed.x * TIME;
	waterUV.y += cos(TIME * min(1., offSetSpeed.y)) * 0.01;
	waterUV = waterUV + noiseValue * distortionForce * WDSpeed + wake_distortion;

	vec4 noiseColor = texture(waterNoise, waterUV);
	float intensity = smoothstep(WDFreq, WDSize, noiseColor.r);
	color.rgb += intensity * vec3(WDBrightness);
	
	vec2 backGroundUV = SCREEN_UV;
	backGroundUV.x += noiseValue * backGroundDirX + wake_distortion.x;
	backGroundUV.y += noiseValue * backGroundDirY + wake_distortion.y;

	color = mix(texture(SCREEN_TEXTURE, backGroundUV), color, 0.2);
	COLOR = mix(color, waterColor, colorCorection);
}