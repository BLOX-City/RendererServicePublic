#version 330 core
layout (location = 0) in vec3 position;
layout (location = 3) in vec2 texCoord;
layout (location = 2) in vec3 normalData;
layout (location = 4) in vec3 vertexColor;
uniform mat4 projection;
uniform mat4 view;
out vec3 normal;
out vec3 objPosition;
out vec3 perVertexLighting;
out vec2 texCoordData;
out vec3 vertColor;

uniform vec3 objPos;

vec3 createPointLight(vec3 normal, vec3 position) {
    float dist        = distance(position, objPosition);
    float attenuation = 20.0 / ( 1.0 + 0.1 * dist + 0.01 * dist * dist);
    vec3 lightDir = normalize(position);
    return vec3(dot(lightDir, normal)) * attenuation;
}

void main()
{
    normal = normalData;
    vec3 normalP = normalize(normal);
    vec3 lights = vec3(0, 0, 0);
    texCoordData = texCoord;
    vertColor = vertexColor;
    perVertexLighting = createPointLight(normalP, vec3(10, 10, 10));
        
    gl_Position = projection * view * vec4(position + objPos, 1.0);
    objPosition = position + objPos;    
}