#version 330 core

in vec3 shaderPosition;
in mat3 shaderTBN;
in vec2 shaderTexCoord;
in vec3 shaderLightPosition;
uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
// uniform sampler2D planeMap;
out vec4 fragmentColor;

//Spotlight Variables
in vec3 shaderLightDirection;
uniform float spotLightRadius;
uniform float deg;                  //phi
uniform float spotLightOuterRadius;
uniform float outerDeg;             //gamma
uniform bool attenuationIsOn;

///////////////////////////////////////////////////////////////////////////////
// Shadow Mapping for Exercise 3
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
    // define some constant properties for the light
    // (you should really be passing these parameters into the shader as uniform vars instead)
    vec3 lightColor = vec3(3.0f, 3.0f, 3.0f);  // diffuse
    float ambientIntensity = 0.15f;            // ambient
    float specularIntensity = 0.5f;            // specular (better implementation: look this up from a specular map!)
    float specularPower = 32.0f;               // specular exponent

    // look up the normal from the normal map, then reorient it with the current model transform via the TBN matrix
    vec3 textureNormal = vec3(texture(normalMap, shaderTexCoord));
    textureNormal = normalize(textureNormal * 2.0f - 1.0f);  // convert range from [0, 1] to [-1, 1]
    vec3 normalDir = normalize(shaderTBN * textureNormal);

    // calculate ambient
    vec3 lightAmbient = lightColor * ambientIntensity;

    // calculate the direction from the light to the fragment
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
    vec3 lightSpecular = pow(max(dot(reflectDir, viewDir)* intensity, 0), specularPower) * lightColor * specularIntensity * intensity;

    ///////////////////////////////////////////////////////////////////////////
    // zero-out the diffuse and specular components if the fragment is in shadow
    /*Chris' Code*/
    if (inShadow())
        lightDiffuse = lightAmbient = lightSpecular = vec3(0.0f, 0.0f, 0.0f);

    ///////////////////////////////////////////////////////////////////////////

    // Check if the fragment is within the spotlight cone
    if (intensity > 0)
    {
        // Compute final fragment color
        if (attenuationIsOn == true)
            fragmentColor = vec4((lightAmbient + lightDiffuse + lightSpecular) * attenuation, 1.0f) * texture(diffuseMap, shaderTexCoord);
        else
            fragmentColor = vec4((lightAmbient + lightDiffuse + lightSpecular), 1.0f) * texture(diffuseMap, shaderTexCoord);
    }
    
    else 
    {
        // zeroing-out diffuse, ambient, and specular to make the spotlight more noticeable
        //exclude ambient so it's not pitch black
        lightDiffuse = lightAmbient = lightSpecular = vec3(0.0f, 0.0f, 0.0f);
        fragmentColor = vec4((lightAmbient + lightDiffuse + lightSpecular), 1.0f) * texture(diffuseMap, shaderTexCoord);
    }
}
