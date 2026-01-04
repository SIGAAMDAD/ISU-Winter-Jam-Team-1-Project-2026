shader_type canvas_item;

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture;

// Random function
float random(vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898, 78.233))) * 43758.5453123);
}

// Smooth random for better lightning
float smooth_random(vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);
    
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));
    
    vec2 u = f * f * (3.0 - 2.0 * f);
    
    return mix(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
}

// Flicker effect for lightning
float flicker(vec2 uv, float time) {
    float flicker_value = 0.0;
    
    // Create multiple layers of random noise for natural lightning
    for (int i = 0; i < 3; i++) {
        float scale = float(i + 1) * 2.5;
        flicker_value += smooth_random(uv * scale + time * 0.5) * (1.0 / float(i + 1));
    }
    
    return flicker_value / 2.0;
}

void fragment() {
    vec2 uv = FRAGCOORD.xy / SCREEN_PIXEL_SIZE;
    
    // Base color (slight blue tint for atmosphere)
    vec4 base_color = texture(SCREEN_TEXTURE, SCREEN_UV);
    vec3 color = base_color.rgb;
    
    // Time parameters
    float time = TIME;
    
    // Random seed based on time (changes every 10 seconds)
    float time_segment = floor(time / 10.0);
    vec2 seed = vec2(time_segment, time_segment * 1.618);
    
    // Random lightning parameters
    float lightning_chance = random(seed + 0.1);
    float lightning_duration = 0.1 + random(seed + 0.2) * 0.3; // 0.1-0.4 seconds
    float lightning_intensity = 0.5 + random(seed + 0.3) * 0.5; // 0.5-1.0 intensity
    float lightning_timing = random(seed + 0.4) * 8.0; // Random delay 0-8 seconds
    
    // Calculate if lightning should occur
    float time_in_segment = mod(time, 10.0);
    float is_lightning_time = step(lightning_timing, time_in_segment) * 
                             step(time_in_segment, lightning_timing + lightning_duration);
    
    // Only apply lightning if random chance is high enough
    if (lightning_chance > 0.85 && is_lightning_time > 0.0) {
        // Calculate progress through lightning strike (0 to 1)
        float progress = (time_in_segment - lightning_timing) / lightning_duration;
        
        // Lightning intensity curve (quick flash, slow fade)
        float intensity_curve = 1.0 - smoothstep(0.0, 1.0, progress);
        intensity_curve *= sin(progress * 20.0) * 0.5 + 0.5; // Add flicker
        
        // Generate lightning pattern
        float lightning_pattern = flicker(uv * 2.0 + vec2(sin(time * 10.0), 0.0), time);
        
        // Apply lightning
        float lightning_brightness = lightning_pattern * intensity_curve * lightning_intensity;
        
        // Blue-white lightning color
        vec3 lightning_color = mix(vec3(0.9, 0.95, 1.0), vec3(0.6, 0.8, 1.0), lightning_pattern);
        
        // Add lightning to screen color
        color = mix(color, lightning_color, lightning_brightness * 0.6);
        color += lightning_color * lightning_brightness * 0.3;
    }
    
    // Subtle screen darkening for atmosphere (optional)
    color *= 0.95;
    
    COLOR = vec4(color, base_color.a);
}