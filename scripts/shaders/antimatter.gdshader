shader_type canvas_item;
uniform vec2 center;

void vertex() {
	
}

vec4 average_colors_2(vec4 colors[2]) {
	vec4 result;
	float r = 0.0;
	float g = 0.0;
	float b = 0.0;
	for(int i = 0; i < colors.length(); i++) {
		r += pow(colors[i].r, 2);
		g += pow(colors[i].g, 2);
		b += pow(colors[i].b, 2);
	}
	result = vec4(sqrt(r/float(colors.length())), sqrt(g/float(colors.length())), sqrt(b/float(colors.length())), 1.0);
	return result;
}

vec4 average_colors_4(vec4 colors[4]) {
	vec4 result;
	float r = 0.0;
	float g = 0.0;
	float b = 0.0;
	for(int i = 0; i < colors.length(); i++) {
		r += pow(colors[i].r, 2);
		g += pow(colors[i].g, 2);
		b += pow(colors[i].b, 2);
	}
	result = vec4(sqrt(r/float(colors.length())), sqrt(g/float(colors.length())), sqrt(b/float(colors.length())), 1.0);
	return result;
}

float floatsign(int x) {
	if (x > 0) {
		return 1.0;
	} else if (x < 0) {
		return -1.0;
	} else {
		return 0.0;
	}
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

vec4 blend_corner(vec2 uv, ivec2 pixel_offset, sampler2D source, int width, int height) {
	vec4 colors[2];
	colors[0] = texture(source, uv);
	colors[1] = get_neighbor_color(uv, vec2(-1.0 * floatsign(pixel_offset.x), -1.0 * floatsign(pixel_offset.y)), source, width, height);
	return average_colors_2(colors);
}

vec4 stretch_side(vec2 uv, ivec2 pixel_offset, sampler2D source, int width, int height) {
	if (pixel_offset.x == 0) {
		return get_neighbor_color(uv, vec2(0.0, -1.0 * floatsign(pixel_offset.y)), source, width, height);
	} else if (pixel_offset.y == 0) {
		return get_neighbor_color(uv, vec2(-1.0 * floatsign(pixel_offset.x), 0.0), source, width, height);
	}

	if (abs(pixel_offset.x) == 1) {
		vec4 colors[2];
		colors[0] = get_neighbor_color(uv, vec2(0.0, -1.0 * floatsign(pixel_offset.y)), source, width, height);
		colors[1] = get_neighbor_color(uv, vec2(-1.0 * floatsign(pixel_offset.x), -1.0 * floatsign(pixel_offset.y)), source, width, height);
		return average_colors_2(colors);
	} else if (abs(pixel_offset.y) == 1) {
		vec4 colors[2];
		colors[0] = get_neighbor_color(uv, vec2(-1.0 * floatsign(pixel_offset.x), 0.0), source, width, height);
		colors[1] = get_neighbor_color(uv, vec2(-1.0 * floatsign(pixel_offset.x), -1.0 * floatsign(pixel_offset.y)), source, width, height);
		return average_colors_2(colors);
	}

	return texture(source, uv);
}

vec4 blend_side(vec2 uv, ivec2 pixel_offset, sampler2D source, int width, int height) {
	vec4 colors[2];
	colors[0] = texture(source, uv);
	if (abs(pixel_offset.x) == 3) {
		colors[1] = get_neighbor_color(uv, vec2(-1.0 * floatsign(pixel_offset.x), 0.0), source, width, height);
	} else {
		colors[1] = get_neighbor_color(uv, vec2(0.0, -1.0 * floatsign(pixel_offset.y)), source, width, height);
	}
	return average_colors_2(colors);
}

ivec2 get_pixel_offset_from_center(vec2 uv, int width, int height) {
	ivec2 offset;
	offset.x = int(round(uv.x * float(width) - center.x - 0.5f));
	offset.y = int(round(uv.y * float(height) - center.y - 0.5f));
	return offset;
}

bool xor(bool a, bool b) {
	return (a || b) && !(a && b);
}

void fragment() {
	int width = textureSize(TEXTURE, 0).x;
	int height = textureSize(TEXTURE, 0).y;

	ivec2 pixel_offset = get_pixel_offset_from_center(UV, width, height);
	int distance = int(max(abs(pixel_offset.x), abs(pixel_offset.y)));
	if (distance == 0) {
		COLOR = vec4(1.0, 0.0, 0.0, 1.0);
	}
	else if (distance == 1) {
		COLOR = vec4(0.0, 0.0, 0.0, 1.0);
	} else if (distance == 2) {
		if (xor(abs(pixel_offset.x) == 2, abs(pixel_offset.y) == 2))
		{
			COLOR = stretch_side(UV, pixel_offset, TEXTURE, width, height);
		} else {
			COLOR =	blend_corner(UV, pixel_offset, TEXTURE, width, height);
		}
	} else if (distance == 3 && xor(abs(pixel_offset.x) == 3, abs(pixel_offset.y) == 3)) {
		COLOR = blend_side(UV, pixel_offset, TEXTURE, width, height);
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
