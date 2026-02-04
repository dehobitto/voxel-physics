#version 330

in vec3 fragPosition;
in vec2 fragTexCoord;
in vec3 fragNormal;
in mat3 TBN;
in vec3 tangentViewPos;
in vec3 tangentFragPos;

out vec4 finalColor;

// Текстуры
uniform sampler2D albedoMap;    
uniform sampler2D metalMap;     
uniform sampler2D normalMap;    
uniform sampler2D roughMap;     
uniform sampler2D aoMap;        
uniform sampler2D displacementMap; // Карта высот

// Параметры
uniform vec3 viewPos;
uniform vec3 ambientColor;
uniform float ambientIntensity;
uniform float displacementScale; // Сила эффекта (поставь 0.05 - 0.1)

struct Light {
    int enabled;
    int type;
    vec3 position;
    vec3 target;
    vec4 color;
    float intensity;
};

uniform Light lights[4];

const float PI = 3.14159265359;

// --- Parallax Mapping (Basic) ---
vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{ 
    // Читаем высоту. Если текстура инвертирована (черное - глубоко), используем .r
    // Если белое - глубоко, используем (1.0 - .r)
    float height =  texture(displacementMap, texCoords).r;    
    
    // Смещаем UV координаты
    // viewDir.xy / viewDir.z проецирует вектор взгляда на плоскость
    vec2 p = viewDir.xy / viewDir.z * (height * displacementScale);
    
    return texCoords - p;    
}

// PBR функции (оставляем как были, сократил для краткости)
float DistributionGGX(vec3 N, vec3 H, float roughness) {
    float a = roughness*roughness; float a2 = a*a; float NdotH = max(dot(N, H), 0.0); float NdotH2 = NdotH*NdotH;
    float nom   = a2; float denom = (NdotH2 * (a2 - 1.0) + 1.0); denom = PI * denom * denom; return nom / denom;
}
float GeometrySchlickGGX(float NdotV, float roughness) {
    float r = (roughness + 1.0); float k = (r*r) / 8.0; float num = NdotV; float denom = NdotV * (1.0 - k) + k; return num / denom;
}
float GeometrySmith(vec3 N, vec3 V, vec3 L, float roughness) {
    float NdotV = max(dot(N, V), 0.0); float NdotL = max(dot(N, L), 0.0);
    float ggx2 = GeometrySchlickGGX(NdotV, roughness); float ggx1 = GeometrySchlickGGX(NdotL, roughness); return ggx1 * ggx2;
}
vec3 fresnelSchlick(float cosTheta, vec3 F0) {
    return F0 + (1.0 - F0) * pow(clamp(1.0 - cosTheta, 0.0, 1.0), 5.0);
}

void main()
{
    // 1. Расчет Parallax UV
    vec3 viewDir = normalize(tangentViewPos - tangentFragPos);
    
    // Базовая защита от артефактов на крутых углах
    // Если смотрим слишком сбоку, уменьшаем эффект
    vec2 texCoords = fragTexCoord;
    if(viewDir.z > 0.0) { // Только если смотрим на лицевую сторону
         texCoords = ParallaxMapping(fragTexCoord, viewDir);
    }
    
    // Если UV вылетели за пределы (0,1), обрезаем или зацикливаем
    // Для камней обычно зацикливание не страшно, но может быть шов.
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0)
        discard; // Откидываем пиксели, которые "уехали" за край полигона (для теста)

    // 2. Сэмплинг текстур с НОВЫМИ координатами (texCoords)
    vec4 albedoSample = texture(albedoMap, texCoords);
    vec3 albedo = pow(albedoSample.rgb, vec3(2.2));
    
    // Normal Map тоже берем по смещенным координатам
    vec3 normalSample = texture(normalMap, texCoords).rgb;
    normalSample = normalSample * 2.0 - 1.0;
    vec3 N = normalize(TBN * normalSample);

    float metallic = texture(metalMap, texCoords).r;
    float roughness = texture(roughMap, texCoords).r;
    float ao = texture(aoMap, texCoords).r;

    // ... Дальше стандартный PBR (как в прошлом ответе) ...
    vec3 V = normalize(viewPos - fragPosition);
    vec3 F0 = vec3(0.04); 
    F0 = mix(F0, albedo, metallic);

    vec3 Lo = vec3(0.0);

    for(int i = 0; i < 4; ++i) 
    {
        if(lights[i].enabled == 0) continue;
        
        vec3 L = normalize(lights[i].position - fragPosition);
        vec3 H = normalize(V + L);
        float distance = length(lights[i].position - fragPosition);
        
        // Более мягкое затухание
        float attenuation = 1.0 / (1.0 + 0.1 * distance + 0.01 * distance * distance);
        vec3 radiance = lights[i].color.rgb * lights[i].intensity * attenuation;

        float NDF = DistributionGGX(N, H, roughness);
        float G   = GeometrySmith(N, V, L, roughness);
        vec3 F    = fresnelSchlick(max(dot(H, V), 0.0), F0);

        vec3 numerator    = NDF * G * F;
        float denominator = 4.0 * max(dot(N, V), 0.0) * max(dot(N, L), 0.0) + 0.0001;
        vec3 specular = numerator / denominator;

        vec3 kS = F;
        vec3 kD = vec3(1.0) - kS;
        kD *= 1.0 - metallic;

        float NdotL = max(dot(N, L), 0.0);
        Lo += (kD * albedo / PI + specular) * radiance * NdotL;
    }

    vec3 ambient = ambientColor * albedo * ao * ambientIntensity;
    vec3 color = ambient + Lo;

    color = color / (color + vec3(1.0));
    color = pow(color, vec3(1.0/2.2));

    finalColor = vec4(color, 1.0);
}