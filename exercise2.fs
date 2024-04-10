#version 330 core

in vec3 shaderPosition;
in mat3 shaderTBN;
in vec2 shaderTexCoord;
in vec3 shaderLightPosition;
uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
out vec4 fragmentColor;

//New Variables for Exercise
in vec3 shaderLightDirection;
uniform float spotLightRadius;
uniform float deg;                  //phi
uniform float spotLightOuterRadius;
uniform float outerDeg;             //gamma
uniform bool attenuationIsOn;

void main()
{
    // define some constant properties for the light
    // (you should really be passing these parameters into the shader as uniform vars instead)
    vec3 lightColor = vec3(1.0f, 1.0f, 1.0f);  // diffuse
    float ambientIntensity = 0.15f;            // ambient
    float specularIntensity = 0.5f;            // specular (better implementation: look this up from a specular map!)
    float specularPower = 32.0f;               // specular exponent

    // look up the normal from the normal map, then reorient it with the current model transform via the TBN matrix
    vec3 textureNormal = vec3(texture(normalMap, shaderTexCoord));
    textureNormal = normalize(textureNormal * 2.0f - 1.0f);  // convert range from [0, 1] to [-1, 1]
    vec3 normalDir = normalize(shaderTBN * textureNormal);

    // calculate ambient
    vec3 lightAmbient = lightColor * ambientIntensity;

    // calculate diffuse
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

    // Check if the fragment is within the spotlight cone
    if (intensity > 0)
    {
        // Calculate diffuse
        vec3 lightDiffuse = max(dot(normalDir, -lightDir), 0.0f) * lightColor;

        // Calculate specular
        vec3 viewDir = normalize(-shaderPosition);
        vec3 reflectDir = reflect(-lightDir, normalDir);
        vec3 lightSpecular = pow(max(dot(reflectDir, viewDir)* intensity, 0), specularPower) * lightColor * intensity;

        // Compute final fragment color
        if (attenuationIsOn == true)
            fragmentColor = vec4((lightAmbient + lightDiffuse + lightSpecular) * attenuation, 1.0f) * texture(diffuseMap, shaderTexCoord);
        else
            fragmentColor = vec4((lightAmbient + lightDiffuse + lightSpecular), 1.0f) * texture(diffuseMap, shaderTexCoord);
    }
    
    else {
        // Discard if fragment is outside the spotlight cone. This was used to make the spotlight more noticeable
        discard;
    }
}
