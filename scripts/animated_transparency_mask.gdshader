shader_type canvas_item;

uniform sampler2D transparency_mask_frames: filter_nearest;
uniform uint num_frames;
uniform int frame_index;
uniform bool flipH;
uniform bool flipV;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	if (frame_index > -1) {
		vec2 new_UV = UV;
		float x_mod = UV.x;
		if (flipH) {
			x_mod = 1.0 - x_mod;
		}
		float y_mod = UV.y;
		if (flipV) {
			y_mod = 1.0 - y_mod;
		}
		new_UV.x = (x_mod / float(num_frames)) + (float(frame_index) * 1.0/float(num_frames));
		new_UV.y = y_mod;
		vec4 mask_pixel = texture(transparency_mask_frames, new_UV);
		COLOR.a = 1.0 - mask_pixel.a;
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
