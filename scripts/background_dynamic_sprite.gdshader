shader_type canvas_item;

//uniform sampler2D transparency_mask_frames: filter_nearest;
uniform sampler2D top_cover: filter_nearest;
uniform bool show_top_cover;

void vertex() {
	// Called for every vertex the material is visible on.
}

vec4 apply_cover_texture(bool show, sampler2D cover_texture, vec2 uv_param, vec4 color) {
	if(show) {
		vec4 cover_pixel = texture(cover_texture, uv_param);
		if(cover_pixel.a > 0.0) {
			return vec4(cover_pixel.r, cover_pixel.g, cover_pixel.b, cover_pixel.a);
		}
	}
	return color;
}

void fragment() {
	COLOR = apply_cover_texture(show_top_cover, top_cover, UV, COLOR);
}
