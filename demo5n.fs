#version 330 core

in vec3 shaderPosition;
in mat3 shaderTBN;  //Equivalent to demo 8's shaderNormal
in vec2 shaderTexCoord;
in vec3 shaderLightPosition;
uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
out vec4 fragmentColor;

//New Variables for Spotlight
in vec3 shaderLightDirection;
uniform float spotLightRadius;
uniform float deg;                  //phi
uniform float spotLightOuterRadius;
uniform float outerDeg;             //gamma
uniform bool attenuationIsOn;

///////////////////////////////////////////////////////////////////////////////
// added for shadow mapping
in vec4 shaderLightSpacePosition;
uniform sampler2D shadowMap;

bool inShadow()
{
    // perform perspective division and rescale to the [0, 1] range to get the coordinates into the depth texture
    vec3 position = shaderLightSpacePosition.xyz / shaderLightSpacePosition.w;
    position = position * 0.5f + 0.5f;

    // if the position is outside the light-space frustum, do NOT put the
    // fragment in shadow, to prevent the scene from becoming dark "by default"
    // (note that if you have a spot light, you might want to do the opposite --
    // that is, everything outside the spot light's cone SHOULD be dark by default)
    if (position.x < 0.0f || position.x > 1.0f
        || position.y < 0.0f || position.y > 1.0f
        || position.z < 0.0f || position.z > 1.0f)
    {
        return false;
    }

    // access the shadow map at this position
    float shadowMapZ = texture(shadowMap, position.xy).r;

    // add a bias to prevent shadow acne
    float bias = 0.0005f;
    shadowMapZ += bias;

    // if the depth stored in the texture is less than the current fragment's depth, we are in shadow
    return shadowMapZ < position.z;
}
///////////////////////////////////////////////////////////////////////////////


void main()
{
    // Define spotlight properties
    vec3 lightColor = vec3(1.0f, 1.0f, 1.0f);
    float ambientIntensity = 0.15f;
    float specularPower = 32.0f;
    //Below is from demo8
    float specularIntensity = 0.5f;            // specular (better implementation: look this up from a specular map!)

    // Look up the normal from the normal map and reorient it with the TBN matrix
    vec3 textureNormal = vec3(texture(normalMap, shaderTexCoord));
    textureNormal = normalize(textureNormal * 2.0f - 1.0f);
    vec3 normalDir = normalize(shaderTBN * textureNormal);

    // Calculate ambient
    vec3 lightAmbient = lightColor * ambientIntensity;

    // Calculate the direction from the light to the fragment
    vec3 lightDir = normalize(shaderLightPosition - shaderPosition);

    // Calculate the angle between the light direction and the fragment's direction
    //Source: https://learnopengl.com/Lighting/Light-casters
    float theta     = dot(lightDir, normalize(-shaderLightDirection));
    float epsilon   = spotLightRadius - spotLightOuterRadius;
    float intensity = clamp((theta - spotLightOuterRadius) / epsilon, 0.0, 1.0);

    //Calculate Attenuation
    float distance    = length(shaderLightPosition - shaderPosition);
    float attenuation = 1.0 / (1.0f + (0.045f * distance) + (0.0075f * distance * distance));
                            //constant  +   linear  +   quadratic


    // Calculate diffuse
    vec3 lightDiffuse = max(dot(normalDir, -lightDir), 0.0f) * lightColor;

    // Calculate specular
    vec3 viewDir = normalize(-shaderPosition);
    vec3 reflectDir = reflect(-lightDir, normalDir);
    vec3 lightSpecular = pow(max(dot(reflectDir, viewDir)* intensity, 0), specularPower) * lightColor * intensity;
    // Check if the fragment is within the spotlight cone
    if (intensity > 0)
    {
        if (inShadow())
            lightDiffuse = lightSpecular = vec3(0.0f, 0.0f, 0.0f);

        // Compute final fragment color
        if (attenuationIsOn == true)
            fragmentColor = vec4((lightAmbient + lightDiffuse + lightSpecular) * attenuation, 1.0f) * texture(diffuseMap, shaderTexCoord);
        else
            fragmentColor = vec4((lightAmbient + lightDiffuse + lightSpecular), 1.0f) * texture(diffuseMap, shaderTexCoord);
    }
    
    else {
        // Discard if fragment is outside the spotlight cone. This was used to make the spotlight more noticeable
        // discard;
        // lightDiffuse = lightSpecular = vec3(0.0f, 0.0f, 0.0f);
    }
}

