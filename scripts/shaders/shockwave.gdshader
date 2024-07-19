shader_type canvas_item;
uniform sampler2D shockwave_shape : filter_nearest;
uniform vec2 center;
uniform float gray_divide;

void vertex() {
	
}

vec4 get_neighbor_color(vec2 uv, vec2 offset, sampler2D source, int width, int height) {
	vec2 neighbor_uv;
	neighbor_uv.x = uv.x + offset.x/float(width);
	neighbor_uv.y = uv.y + offset.y/float(height);
	if(neighbor_uv.x < 0.0 || neighbor_uv.x > 1.0 || neighbor_uv.y < 0.0 || neighbor_uv.y > 1.0) {
		return vec4(0.0, 0.0, 0.0, 0.0);
	}
	return texture(source, neighbor_uv);
}

vec4 get_shockwave_color(vec2 uv, int width, int height) {
	int shockwave_width = textureSize(shockwave_shape, 0).x;
	int shockwave_height = textureSize(shockwave_shape, 0).y;
	
	uv.x = uv.x * float(width) / float(shockwave_width);
	uv.y = uv.y * float(height) / float(shockwave_height);
	return texture(shockwave_shape, uv);
}

void fragment() {
	int width = textureSize(TEXTURE, 0).x;
	int height = textureSize(TEXTURE, 0).y;

	vec2 uv_offset;
	uv_offset.x = UV.x - center.x/float(width);
	uv_offset.y = UV.y - center.y/float(height);

	if (get_shockwave_color(uv_offset, width, height).a > 0.0) {
		COLOR = get_neighbor_color(UV, vec2(-1.0, 0.0), TEXTURE, width, height);

		COLOR.r = COLOR.r / gray_divide;
		COLOR.g = COLOR.g / gray_divide;
		COLOR.b = COLOR.b / gray_divide;
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
