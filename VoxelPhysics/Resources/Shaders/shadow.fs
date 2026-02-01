#version 330

in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;
in vec4 shadowPos;

uniform sampler2D texture0;  // Stone texture
uniform sampler2D shadowMap; // Depth map
uniform vec3 lightDir;       // Direction of the sun

out vec4 finalColor;

float CalculateShadow(vec4 sp)
{
    // Convert shadow position to coordinates [0, 1]
    vec3 projCoords = sp.xyz / sp.w;
    projCoords = projCoords * 0.5 + 0.5;

    if (projCoords.z > 1.0) return 0.0;

    float currentDepth = projCoords.z;
    
    // Percentage Closer Filtering (PCF) - makes edges softer
    float shadow = 0.0;
    vec2 texelSize = 1.0 / textureSize(shadowMap, 0);
    for(int x = -1; x <= 1; ++x) {
        for(int y = -1; y <= 1; ++y) {
            float pcfDepth = texture(shadowMap, projCoords.xy + vec2(x, y) * texelSize).r;
            shadow += currentDepth - 0.005 > pcfDepth ? 1.0 : 0.0;
        }
    }
    return shadow / 9.0;
}

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord) * fragColor;
    
    // Simple Diffuse Lighting
    float dotProduct = max(dot(fragNormal, normalize(-lightDir)), 0.0);
    float lightIntensity = 0.3 + dotProduct; // 0.3 is ambient light
    
    float shadow = CalculateShadow(shadowPos);
    
    // Apply shadow to lighting
    float lightFactor = lightIntensity * (1.0 - (shadow * 0.6));
    
    finalColor = vec4(texelColor.rgb * lightFactor, texelColor.a);
}