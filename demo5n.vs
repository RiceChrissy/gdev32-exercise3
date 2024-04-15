#version 330 core

layout (location = 0) in vec3 vertexPosition;
layout (location = 1) in vec3 vertexNormal;
layout (location = 2) in vec3 vertexTangent;
layout (location = 3) in vec2 vertexTexCoord;
uniform mat4 projectionTransform;
uniform mat4 viewTransform;
uniform mat4 modelTransform;
uniform vec3 lightPosition;
out vec3 shaderPosition;
out mat3 shaderTBN;
out vec2 shaderTexCoord;
out vec3 shaderLightPosition;

uniform vec3 spotLightDirection;
out vec3 shaderLightDirection;

///////////////////////////////////////////////////////////////////////////////
// added for shadow mapping
uniform mat4 lightTransform;
out vec4 shaderLightSpacePosition;

//New Variables for Exercise 3
uniform bool shadowsAreOn;
///////////////////////////////////////////////////////////////////////////////

void main()
{
    // combine the model and view transforms to get the camera space transform
    mat4 modelViewTransform = viewTransform * modelTransform;

    // compute the vertex's attributes in camera space
    shaderPosition = vec3(modelViewTransform * vec4(vertexPosition, 1.0f));
    shaderTexCoord = vertexTexCoord;

    // compute the normal transform as the transpose of the inverse of the camera transform,
    // then compute a TBN matrix using this transform
    mat3 normalTransform = mat3(transpose(inverse(modelViewTransform)));
    vec3 normal = normalize(normalTransform * vertexNormal);
    vec3 tangent = normalize(normalTransform * vertexTangent);
    vec3 bitangent = cross(normal, tangent);
    shaderTBN = mat3(tangent, bitangent, normal);

    // also compute the light position in camera space
    // (we want all lighting calculations to be done in camera space to avoid losing precision)
    shaderLightPosition = vec3(viewTransform * vec4(lightPosition, 1.0f));
    shaderLightDirection = vec3(normalTransform * spotLightDirection);

    // we still need OpenGL to compute the final vertex position in projection space
    // to correctly determine where the fragments of the triangle actually go on the screen
    gl_Position = projectionTransform * vec4(shaderPosition, 1.0f);

    ///////////////////////////////////////////////////////////////////////////
    // also compute this fragment position from the light's point of view
    // if(shadowsAreOn == true)
        shaderLightSpacePosition = lightTransform * modelTransform * vec4(vertexPosition, 1.0f);
    ///////////////////////////////////////////////////////////////////////////
}
