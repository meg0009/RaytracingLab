#version 430
/*uniform float aspect;
uniform vec3 campos;*/
in vec3 vPosition; // Входные переменные vPosition - позиция вектора

//out vec3 origin, direction;
out vec3 glPosition;

void main(){
	gl_Position = vec4(vPosition, 1.0);
	/*direction = normalize(vec3(vPosition.x * aspect, vPosition.y, -1.0));
	origin = campos;*/
	glPosition = vPosition;
}
