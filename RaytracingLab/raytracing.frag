#version 430

in vec3 glPosition;
out vec4 FragColor;

#define EPSILON 0.001
#define BIG 1000000.0
const int DIFFUSE_REFLECTION = 1;
const int MIRROR_REFLECTION = 2;
const int REFRACTION = 3;

struct SCamera{
	vec3 Position;
	vec3 View;
	vec3 Up;
	vec3 Side;
	vec2 Scale;
};

struct SRay{
	vec3 Origin;
	vec3 Direction;
};

struct SSphere{
	vec3 Center;
	float Radius;
	int MaterialIdx;
};

struct STriangle{
	vec3 v1;
	vec3 v2;
	vec3 v3;
	int MaterialIdx;
};

struct SCube{
	vec3 Center;
	float Side;
	int MaterialIdx;
	float ax;
	float ay;
	float az;
};

struct SIntersection{
	float Time;
	vec3 Point;
	vec3 Normal;
	vec3 Color;
	vec4 LightCoeffs;
	float ReflectionCoef;
	float RefractionCoef;
	int MaterialType;
};

struct SLight{
	vec3 Position;
};

struct SMaterial{
	vec3 Color;
	vec4 LightCoeffs;
	float ReflectionCoef;
	float RefractionCoef;
	int MaterialType;
};

struct STracingRay{
	SRay ray;
	float contribution;
	int depth;
};

const int countCubes = 2;
const int countSpheres = 2;
const int countTriangles = 12;
const int countMaterials = 7;
const int sizeStack = 10;
int countRay = 0;

SCube cubes[countCubes];
STriangle triangles[countTriangles];
SSphere spheres[countSpheres];
SCamera uCamera;
SLight uLight;
SMaterial materials[countMaterials];
STracingRay stackRays[sizeStack];

bool pushRay(STracingRay ray){
	if(countRay < sizeStack){
		stackRays[countRay++] = ray;
		return true;
	}
	return false;
}

STracingRay popRay(){
	return stackRays[--countRay];
}

bool isEmpty(){
	if(countRay == 0){
		return true;
	}
	return false;
}

void initializeDefaultLightMaterials(out SLight light, out SMaterial materials[countMaterials]){
	light.Position = vec3(4, 4, -7.5);

	vec4 lightCoeffs = vec4(0.4, 0.9, 0.2, 2.0);
	materials[0].Color = vec3(0, 1, 0);
	materials[0].LightCoeffs = lightCoeffs;
	materials[0].ReflectionCoef = 0.5;
	materials[0].RefractionCoef = 1.0;
	materials[0].MaterialType = DIFFUSE_REFLECTION;

	materials[1].Color = vec3(1, 0, 0);
	materials[1].LightCoeffs = lightCoeffs;
	materials[1].ReflectionCoef = 0.5;
	materials[1].RefractionCoef = 1.0;
	materials[1].MaterialType = DIFFUSE_REFLECTION;

	materials[2].Color = vec3(0, 0, 1);
	materials[2].LightCoeffs = lightCoeffs;
	materials[2].ReflectionCoef = 0.5;
	materials[2].RefractionCoef = 1.0;
	materials[2].MaterialType = DIFFUSE_REFLECTION;

	materials[3].Color = vec3(1, 1, 0);
	materials[3].LightCoeffs = lightCoeffs;
	materials[3].ReflectionCoef = 0.5;
	materials[3].RefractionCoef = 1.0;
	materials[3].MaterialType = MIRROR_REFLECTION;

	materials[4].Color = vec3(0, 1, 1);
	materials[4].LightCoeffs = lightCoeffs;
	materials[4].ReflectionCoef = 0.5;
	materials[4].RefractionCoef = 1.0;
	materials[4].MaterialType = DIFFUSE_REFLECTION;

	materials[5].Color = vec3(1, 0, 1);
	materials[5].LightCoeffs = lightCoeffs;
	materials[5].ReflectionCoef = 1.0;
	materials[5].RefractionCoef = 1.0;
	materials[5].MaterialType = MIRROR_REFLECTION;

	materials[6].Color = vec3(0.5, 0.5, 1);
	materials[6].LightCoeffs = lightCoeffs;
	materials[6].ReflectionCoef = 1.0;
	materials[6].RefractionCoef = 1.5;
	materials[6].MaterialType = REFRACTION;
}

void initializeDefaultScene(out STriangle triangles[countTriangles], out SSphere spheres[countSpheres]){
	// Triangles
	// left wall
	triangles[0].v1 = vec3(-5, -5, -8);
	triangles[0].v2 = vec3(-5, -5, 8);
	triangles[0].v3 = vec3(-5, 5, -8);
	triangles[0].MaterialIdx = 0;

	triangles[1].v1 = vec3(-5, -5, 8);
	triangles[1].v2 = vec3(-5, 5, 8);
	triangles[1].v3 = vec3(-5, 5, -8);
	triangles[1].MaterialIdx = 0;

	// back wall
	triangles[2].v1 = vec3(-5, -5, 8);
	triangles[2].v2 = vec3(5, -5, 8);
	triangles[2].v3 = vec3(-5, 5, 8);
	triangles[2].MaterialIdx = 1;

	triangles[3].v1 = vec3(5, -5, 8);
	triangles[3].v2 = vec3(5, 5, 8);
	triangles[3].v3 = vec3(-5, 5, 8);
	triangles[3].MaterialIdx = 1;

	// right wall
	triangles[4].v1 = vec3(5, -5, -8);
	triangles[4].v2 = vec3(5, -5, 8);
	triangles[4].v3 = vec3(5, 5, -8);
	triangles[4].MaterialIdx = 2;

	triangles[5].v1 = vec3(5, -5, 8);
	triangles[5].v2 = vec3(5, 5, 8);
	triangles[5].v3 = vec3(5, 5, -8);
	triangles[5].MaterialIdx = 2;

	// bottom wall
	triangles[6].v1 = vec3(-5, -5, -8);
	triangles[6].v2 = vec3(5, -5, -8);
	triangles[6].v3 = vec3(-5, -5, 8);
	triangles[6].MaterialIdx = 3;

	triangles[7].v1 = vec3(5, -5, -8);
	triangles[7].v2 = vec3(5, -5, 8);
	triangles[7].v3 = vec3(-5, -5, 8);
	triangles[7].MaterialIdx = 3;

	// top wall
	triangles[8].v1 = vec3(-5, 5, -8);
	triangles[8].v2 = vec3(5, 5, -8);
	triangles[8].v3 = vec3(-5, 5, 8);
	triangles[8].MaterialIdx = 3;

	triangles[9].v1 = vec3(5, 5, -8);
	triangles[9].v2 = vec3(5, 5, 8);
	triangles[9].v3 = vec3(-5, 5, 8);
	triangles[9].MaterialIdx = 3;

	// front wall
	triangles[10].v1 = vec3(-5, -5, -8);
	triangles[10].v2 = vec3(5, -5, -8);
	triangles[10].v3 = vec3(-5, 5, -8);
	triangles[10].MaterialIdx = 1;

	triangles[11].v1 = vec3(5, -5, -8);
	triangles[11].v2 = vec3(5, 5, -8);
	triangles[11].v3 = vec3(-5, 5, -8);
	triangles[11].MaterialIdx = 1;

	// Spheres
	spheres[0].Center = vec3(-1, -1, -2);
	spheres[0].Radius = 2.0;
	spheres[0].MaterialIdx = 5;

	spheres[1].Center = vec3(2, 1, 2);
	spheres[1].Radius = 1.0;
	spheres[1].MaterialIdx = 6;

	// Cubes
	cubes[0].Center = vec3(2, -4, -1);
	cubes[0].Side = 2.0;
	cubes[0].MaterialIdx = 4;
	cubes[0].ax = 0;
	cubes[0].ay = 1;
	cubes[0].az = 0;

	cubes[1].Center = vec3(-1, -4.5, -1.8);
	cubes[1].Side = 1;
	cubes[1].MaterialIdx = 4;
	cubes[1].ax = 0;
	cubes[1].ay = 0;
	cubes[1].az = 1.5;
}

SRay GenerateRay(SCamera uCamera){
	vec2 coords = glPosition.xy * uCamera.Scale;
	vec3 direction = uCamera.View + uCamera.Side * coords.x + uCamera.Up * coords.y;
	return SRay(uCamera.Position, normalize(direction));
}

void initializeDefaultCamera(){
	uCamera.Position = vec3(0, 0, -7.5);
	uCamera.View = vec3(0, 0, 1);
	uCamera.Up = vec3(0, 1, 0);
	uCamera.Side = vec3(1, 0, 0);
	uCamera.Scale = vec2(1);
}

bool IntersectTriangle(SRay ray, vec3 v1, vec3 v2, vec3 v3, out float time);

bool IntersectCube(SCube cube, SRay ray, float start, float final, out float time){
	time = -1;
	mat3 rotX = mat3(
		1, 0, 0,
		0, cos(cube.ax), -sin(cube.ax),
		0, sin(cube.ax), cos(cube.ax)
	);
	mat3 rotY = mat3(
		cos(cube.ay), 0, sin(cube.ay),
		0, 1, 0,
		-sin(cube.ay), 0, cos(cube.ay)
	);
	mat3 rotZ = mat3(
		cos(cube.az), -sin(cube.az), 0,
		sin(cube.az), cos(cube.az), 0,
		0, 0, 1
	);
	vec3 a[8];
	vec3 tmp = vec3(-cube.Side / 2.0, -cube.Side / 2.0, -cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[0] = tmp + cube.Center;

	tmp = vec3(cube.Side / 2.0, -cube.Side / 2.0, -cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[1] = tmp + cube.Center;

	tmp = vec3(cube.Side / 2.0, cube.Side / 2.0, -cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[2] = tmp + cube.Center;

	tmp = vec3(-cube.Side / 2.0, cube.Side / 2.0, -cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[3] = tmp + cube.Center;

	tmp = vec3(-cube.Side / 2.0, -cube.Side / 2.0, cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[4] = tmp + cube.Center;

	tmp = vec3(cube.Side / 2.0, -cube.Side / 2.0, cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[5] = tmp + cube.Center;

	tmp = vec3(cube.Side / 2.0, cube.Side / 2.0, cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[6] = tmp + cube.Center;

	tmp = vec3(-cube.Side / 2.0, cube.Side / 2.0, cube.Side / 2.0);
	tmp = rotX * tmp;
	tmp = rotY * tmp;
	tmp = rotZ * tmp;
	a[7] = tmp + cube.Center;

	STriangle tris[12];
	// front
	tris[0].v1 = a[0];
	tris[0].v2 = a[1];
	tris[0].v3 = a[3];

	tris[1].v1 = a[1];
	tris[1].v2 = a[2];
	tris[1].v3 = a[3];
	//back
	tris[2].v1 = a[4];
	tris[2].v2 = a[5];
	tris[2].v3 = a[7];

	tris[3].v1 = a[5];
	tris[3].v2 = a[6];
	tris[3].v3 = a[7];
	//left
	tris[4].v1 = a[0];
	tris[4].v2 = a[4];
	tris[4].v3 = a[3];

	tris[5].v1 = a[4];
	tris[5].v2 = a[7];
	tris[5].v3 = a[3];
	//right
	tris[6].v1 = a[1];
	tris[6].v2 = a[5];
	tris[6].v3 = a[2];

	tris[7].v1 = a[5];
	tris[7].v2 = a[6];
	tris[7].v3 = a[2];
	//bottom
	tris[8].v1 = a[0];
	tris[8].v2 = a[1];
	tris[8].v3 = a[4];

	tris[9].v1 = a[1];
	tris[9].v2 = a[5];
	tris[9].v3 = a[4];
	//top
	tris[10].v1 = a[3];
	tris[10].v2 = a[2];
	tris[10].v3 = a[7];

	tris[11].v1 = a[2];
	tris[11].v2 = a[6];
	tris[11].v3 = a[7];

	float t[12];
	bool f[12];
	for(int i = 0; i < 12; i++){
		f[i] = IntersectTriangle(ray, tris[i].v1, tris[i].v2, tris[i].v3, t[i]);
	}
	float tmax = t[0];
	int imax = 0;
	for(int i = 1; i < 12; i++){
		if(!f[imax] && f[i]){
			tmax = t[i];
			imax = i;
		}
		else if(f[imax] && f[i]){
			if(tmax > t[i]){
				tmax = t[i];
				imax = i;
			}
		}
	}
	if(f[imax]){
		time = t[imax];
		return true;
	}
	return false;
}

bool IntersectSphere(SSphere sphere, SRay ray, float start, float final, out float time){
	ray.Origin -= sphere.Center;
	float A = dot(ray.Direction, ray.Direction);
	float B = dot(ray.Direction, ray.Origin);
	float C = dot(ray.Origin, ray.Origin) - sphere.Radius * sphere.Radius;
	float D = B * B - A * C;
	if(D > 0.0){
		D = sqrt(D);
		float t1 = (-B - D) / A;
		float t2 = (-B + D) / A;
		if(t1 < 0 && t2 < 0){
			return false;
		}
		if(min(t1, t2) < 0){
			time = max(t1, t2);
			return true;
		}
		time = min(t1, t2);
		return true;
	}
	return false;
}

bool IntersectTriangle(SRay ray, vec3 v1, vec3 v2, vec3 v3, out float time){
	time = -1;
	vec3 A = v2 - v1;
	vec3 B = v3 - v1;
	vec3 N = cross(A, B);
	float NdotRayDiraction = dot(N, ray.Direction);
	if(abs(NdotRayDiraction) < EPSILON){
		return false;
	}
	float d = dot(N, v1);
	float t = -(dot(N, ray.Origin) - d) / NdotRayDiraction;
	if(t < 0){
		return false;
	}
	vec3 P = ray.Origin + t * ray.Direction;
	vec3 C;
	vec3 edge1 = v2 - v1;
	vec3 VP1 = P - v1;
	C = cross(edge1, VP1);
	if(dot(N, C) < 0){
		return false;
	}
	vec3 edge2 = v3 - v2;
	vec3 VP2 = P - v2;
	C = cross(edge2, VP2);
	if(dot(N, C) < 0){
		return false;
	}
	vec3 edge3 = v1 - v3;
	vec3 VP3 = P - v3;
	C = cross(edge3, VP3);
	if(dot(N, C) < 0){
		return false;
	}
	time = t;
	return true;
}

bool Raytrace(SRay ray,	float start, float final, inout SIntersection intersect){
	bool result = false;
	float test = start;
	intersect.Time = final;
	// calculate spheres
	for(int i = 0; i < countSpheres; i++){
		SSphere sphere = spheres[i];
		if(IntersectSphere(sphere, ray, start, final, test) && test < intersect.Time){
			intersect.Time = test;
			intersect.Point = ray.Origin + ray.Direction * test;
			intersect.Normal = normalize(intersect.Point - sphere.Center);
			intersect.Color = materials[sphere.MaterialIdx].Color;
			intersect.LightCoeffs = materials[sphere.MaterialIdx].LightCoeffs;
			intersect.ReflectionCoef = materials[sphere.MaterialIdx].ReflectionCoef;
			intersect.RefractionCoef = materials[sphere.MaterialIdx].RefractionCoef;
			intersect.MaterialType = materials[sphere.MaterialIdx].MaterialType;
			result = true;
		}
	}
	// calculate triangles
	//intersect.Time = final;
	for(int i = 0; i < countTriangles; i++){
		STriangle triangle = triangles[i];
		if(IntersectTriangle(ray, triangle.v1, triangle.v2, triangle.v3, test) && test < intersect.Time){
			intersect.Time = test;
			intersect.Point = ray.Origin + ray.Direction * test;
			intersect.Normal = normalize(cross(triangle.v1 - triangle.v2, triangle.v3 - triangle.v2));
			intersect.Color = materials[triangle.MaterialIdx].Color;
			intersect.LightCoeffs = materials[triangle.MaterialIdx].LightCoeffs;
			intersect.ReflectionCoef = materials[triangle.MaterialIdx].ReflectionCoef;
			intersect.RefractionCoef = materials[triangle.MaterialIdx].RefractionCoef;
			intersect.MaterialType = materials[triangle.MaterialIdx].MaterialType;
			result = true;
		}
	}
	//calculate cubes
	for(int i = 0; i < countCubes; i++){
		SCube cube = cubes[i];
		if(IntersectCube(cube, ray, start, final, test) && test < intersect.Time){
			intersect.Time = test;
			intersect.Point = ray.Origin + ray.Direction * test;
			//посмотреть
			intersect.Normal = normalize(intersect.Point - cube.Center);
			intersect.Color = materials[cube.MaterialIdx].Color;
			intersect.LightCoeffs = materials[cube.MaterialIdx].LightCoeffs;
			intersect.ReflectionCoef = materials[cube.MaterialIdx].ReflectionCoef;
			intersect.RefractionCoef = materials[cube.MaterialIdx].RefractionCoef;
			intersect.MaterialType = materials[cube.MaterialIdx].MaterialType;
			result = true;
		}
	}
	return result;
}

vec3 Phong(SIntersection intersect, SLight currLight, float shadow){
	vec3 light = normalize(currLight.Position - intersect.Point);
	float diffuse = max(dot(light, intersect.Normal), 0.0);
	vec3 view = normalize(uCamera.Position - intersect.Point);
	vec3 reflected = reflect(-view, intersect.Normal);
	float specular = pow(max(dot(reflected, light), 0.0), intersect.LightCoeffs.w);
	return intersect.LightCoeffs.x * intersect.Color +
		intersect.LightCoeffs.y * diffuse * intersect.Color * shadow +
		intersect.LightCoeffs.z * specular;
}

float Shadow(SLight currLight, SIntersection intersect){
	float shadowing = 1.0;
	vec3 direction = normalize(currLight.Position - intersect.Point);
	float distanceLight = distance(currLight.Position, intersect.Point);
	SRay shadowRay = SRay(intersect.Point + direction * EPSILON, direction);
	SIntersection shadowIntersect;
	shadowIntersect.Time = BIG;
	if(Raytrace(shadowRay, 0, distanceLight, shadowIntersect)){
		shadowing = 0.0;
	}
	return shadowing;
}

void main() {
	float start = 0;
	float final = BIG;

	initializeDefaultCamera();
	SRay ray = GenerateRay(uCamera);
	vec3 resultColor = vec3(0, 0, 0);
	initializeDefaultScene(triangles, spheres);
	initializeDefaultLightMaterials(uLight, materials);
	STracingRay trRay = STracingRay(ray, 1, 0);
	pushRay(trRay);
	while(!isEmpty()){
		STracingRay trRay = popRay();
		ray = trRay.ray;
		SIntersection intersect;
		intersect.Time = BIG;
		start = 0;
		final = BIG;
		if(Raytrace(ray, start, final, intersect)){
			switch(intersect.MaterialType){
				case DIFFUSE_REFLECTION:{
					float shadowing = Shadow(uLight, intersect);
					resultColor += trRay.contribution * Phong(intersect, uLight, shadowing);
					break;
				}
				case MIRROR_REFLECTION:{
					if(intersect.ReflectionCoef < 1){
						float contribution = trRay.contribution * (1 - intersect.ReflectionCoef);
						float shadowing = Shadow(uLight, intersect);
						resultColor += contribution * Phong(intersect, uLight, shadowing);
						break;
					}
					vec3 reflectDirection = reflect(ray.Direction, intersect.Normal);
					float contribution = trRay.contribution * intersect.ReflectionCoef;
					STracingRay reflectRay = STracingRay(
						SRay(intersect.Point + reflectDirection * EPSILON, reflectDirection),
						contribution, trRay.depth + 1
						);
					pushRay(reflectRay);
					break;
				}
				case REFRACTION:{
					float n;
					float Normal;
					if(dot(ray.Direction, intersect.Normal) < 0){
						n = 1.0 / intersect.RefractionCoef;
						Normal = 1;
					}
					else{
						n = intersect.RefractionCoef;
						Normal = -1;
					}
					vec3 refractDiraction = normalize(refract(ray.Direction, intersect.Normal * Normal, n));
					STracingRay refractRay = STracingRay(
						SRay(intersect.Point + refractDiraction * EPSILON, refractDiraction),
						trRay.contribution, trRay.depth + 1
						);
					pushRay(refractRay);
					break;
				}
			}
		}
	}
	FragColor = vec4(resultColor, 1);
}
