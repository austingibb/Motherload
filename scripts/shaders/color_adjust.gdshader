shader_type canvas_item;

uniform float gray_divide = 0;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	COLOR.r = COLOR.r / gray_divide;
	COLOR.g = COLOR.g / gray_divide;
	COLOR.b = COLOR.b / gray_divide;
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
