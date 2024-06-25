shader_type canvas_item;

//uniform sampler2D transparency_mask_frames: filter_nearest;
uniform sampler2D top_cover: filter_nearest;
uniform sampler2D bottom_cover: filter_nearest;
uniform sampler2D left_cover: filter_nearest;
uniform sampler2D right_cover: filter_nearest;
uniform sampler2D concave_connector: filter_nearest;
uniform sampler2D convex_mask: filter_nearest;
uniform sampler2D dig_down: filter_nearest;
uniform sampler2D dig_side: filter_nearest;
uniform vec4 corner_states;
// dig orientation is true if the dig animation is horizontal, false if it is vertical
// flip_dig_animation variables are used to flip the dig animation horizontally or vertically
uniform bool dig_orientation;
uniform bool show_top_cover;
uniform bool show_bottom_cover;
uniform bool show_left_cover;
uniform bool show_right_cover;
uniform bool flip_dig_animation_h;
uniform bool flip_dig_animation_v;
uniform bool debug_animation;
uniform int dig_offset;

void vertex() {
	// Called for every vertex t   he material is visible on.
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

vec4 apply_transparancy_mask(sampler2D mask_texture, vec2 uv_param, vec4 color) {
	vec4 mask_pixel = texture(mask_texture, uv_param);
	if(mask_pixel.a > 0.0) {
		if(debug_animation) {
			return mask_pixel;
		} else {
			return vec4(0, 0, 0, 0);
		}
	}
	return color;
}

vec4 set_corner(int corner_index, float corner_val, vec2 uv_param, vec4 color) {
	if(corner_index % 2 == 1) {
		uv_param.x = 1.0 - uv_param.x;
	}

	if(corner_index >= 2) {
		uv_param.y = 1.0 - uv_param.y;
	}

	if(corner_val > 0.9) {
		vec4 convex_mask_pixel = texture(convex_mask, uv_param);
		if(convex_mask_pixel.a > 0.0) {
			return vec4(0, 0, 0, 0);
		}
	}

	if(corner_val < -0.9) {
		vec4 concave_connector_pixel = texture(concave_connector, uv_param);
		if(concave_connector_pixel.a > 0.0) {
			return vec4(concave_connector_pixel.r, concave_connector_pixel.g, concave_connector_pixel.b, concave_connector_pixel.a);
		}
	}

	return color;
}

void fragment() {
	COLOR = apply_cover_texture(show_top_cover, top_cover, UV, COLOR);
	COLOR = apply_cover_texture(show_bottom_cover, bottom_cover, UV, COLOR);
	COLOR = apply_cover_texture(show_left_cover, left_cover, UV, COLOR);
	COLOR = apply_cover_texture(show_right_cover, right_cover, UV, COLOR);
	
	COLOR = set_corner(0, corner_states.x, UV, COLOR);
	COLOR = set_corner(1, corner_states.y, UV, COLOR);
	COLOR = set_corner(2, corner_states.z, UV, COLOR);
	COLOR = set_corner(3, corner_states.w, UV, COLOR);

	if (dig_orientation) {
		int width = textureSize(dig_side, 0).x;

		vec2 UV_offset = UV;
		if (flip_dig_animation_h) {
			UV_offset.x = 1.0 - UV_offset.x;
			UV_offset.x = UV_offset.x - float(dig_offset)/float(width);
		} else {
			UV_offset.x = UV_offset.x - float(dig_offset)/float(width);
		}

		COLOR = apply_transparancy_mask(dig_side, UV_offset, COLOR);
	} else {
		int height = textureSize(dig_down, 0).y;

		vec2 UV_offset = UV;
		if (flip_dig_animation_v) {
			UV_offset.y = 1.0 - UV_offset.y;
			UV_offset.y = UV_offset.y + float(dig_offset)/float(height);
		} else {
			UV_offset.y = UV_offset.y + float(dig_offset)/float(height);
		}

		COLOR = apply_transparancy_mask(dig_down, UV_offset, COLOR);
	}
	//if (frame_index > -1) {
		//vec2 new_UV = UV;
		//float x_mod = UV.x;
		//if (flipH) {
			//x_mod = 1.0 - x_mod;
		//}
		//float y_mod = UV.y;
		//if (flipV) {
			//y_mod = 1.0 - y_mod;
		//}
		//new_UV.x = (x_mod / float(num_frames)) + (float(frame_index) * 1.0/float(num_frames));
		//new_UV.y = y_mod;
		//vec4 mask_pixel = texture(transparency_mask_frames, new_UV);
		//COLOR.a = 1.0 - mask_pixel.a;
	//}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
