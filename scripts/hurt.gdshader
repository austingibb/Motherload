shader_type canvas_item;

uniform float strength = 0;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	COLOR.r = pow(pow(COLOR.r, 2)+pow(strength, 2), 0.5);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
