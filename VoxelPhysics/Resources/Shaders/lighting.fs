#version 330 core

// Input vertex attributes (from vertex shader)
in vec3 fragPosition;
in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 fragNormal;

// Input uniform values
uniform sampler2D texture0;
uniform vec4 colDiffuse;

// Output fragment color
out vec4 finalColor;

// Sun/directional light
uniform vec3 sunDirection;
uniform vec4 sunColor;
uniform float sunIntensity;

// Ambient light
uniform vec4 ambientColor;
uniform float ambientIntensity;

// View position for specular
uniform vec3 viewPos;

// Time for day/night cycle (0-1, 0.5 = noon)
uniform float timeOfDay;

void main()
{
    // Sample atlas and tint with vertex color (voxel tint + lighting tint)
    vec4 texColor = texture(texture0, fragTexCoord);
    vec4 baseColor = texColor * fragColor * colDiffuse;
    
    // Normal in world space
    vec3 normal = normalize(fragNormal);
    
    // === DIRECTIONAL SUN LIGHT ===
    vec3 sunDir = normalize(sunDirection);
    float NdotL = max(dot(normal, sunDir), 0.0);
    
    // Soft shadow falloff (Minecraft-style - never fully dark)
    float diffuse = NdotL * 0.5 + 0.5; // Remap 0-1 to 0.5-1.0
    
    // Sun contribution
    vec3 sunContrib = sunColor.rgb * sunIntensity * diffuse;
    
    // === AMBIENT LIGHT ===
    // Sky ambient - faces pointing up get more sky light
    float skyFactor = normal.y * 0.5 + 0.5; // 0 for down-facing, 1 for up-facing
    vec3 ambientContrib = ambientColor.rgb * ambientIntensity * (0.6 + skyFactor * 0.4);
    
    // === SIMPLE AMBIENT OCCLUSION ===
    // Darken faces pointing down slightly (simulates ground bounce being darker)
    float ao = clamp(normal.y * 0.3 + 0.7, 0.5, 1.0);
    
    // === COMBINE LIGHTING ===
    vec3 lighting = (sunContrib + ambientContrib) * ao;
    
    // Apply lighting to base color
    vec3 litColor = baseColor.rgb * lighting;
    
    // Slight saturation boost for more vibrant colors
    float luminance = dot(litColor, vec3(0.299, 0.587, 0.114));
    litColor = mix(vec3(luminance), litColor, 1.15);
    
    // Gamma correction for better color perception
    litColor = pow(litColor, vec3(1.0 / 2.0));
    
    // Output
    finalColor = vec4(clamp(litColor, 0.0, 1.0), baseColor.a);
}