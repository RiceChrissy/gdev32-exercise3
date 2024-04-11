#version 330 core

layout (location = 0) in vec3 vertexPosition;
uniform mat4 lightTransform;
uniform mat4 modelTransform;

void main()
{
    gl_Position = lightTransform * modelTransform * vec4(vertexPosition, 1.0f);
}
