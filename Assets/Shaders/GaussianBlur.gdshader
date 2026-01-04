shader_type canvas_item;

uniform int samples = 12;
uniform float blur_amount = 1.0;
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

vec4 gaussianBlur(sampler2D source, vec2 uv) {
	vec4 color = texture(source, uv);
	float amount = blur_amount;
	amount *= .015;
	
	for (int i = 1; i < samples; i++) {
		float offset_x = sin(float(i) * 3.14 / amount) * amount - amount;
		float offset_y = cos(float(i) * 3.14 / amount) * amount + amount;

		color += texture(source, uv + vec2(offset_x, offset_y));
	}

	color = color / float(samples);

	return color;
}

void fragment() {
	COLOR = gaussianBlur(SCREEN_TEXTURE, SCREEN_UV);
}
