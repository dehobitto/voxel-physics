#version 330

// Атрибуты
in vec3 vertexPosition;
in vec2 vertexTexCoord;
in vec3 vertexNormal;
in vec4 vertexTangent;

// Матрицы
uniform mat4 mvp;
uniform mat4 matModel;
uniform mat4 matView;
uniform mat4 matProjection;
uniform vec3 viewPos; // Позиция камеры в мире

// Выходные данные
out vec3 fragPosition;
out vec2 fragTexCoord;
out vec3 fragNormal;
out mat3 TBN;

// Данные для Parallax (View Direction в пространстве касательных)
out vec3 tangentViewPos;
out vec3 tangentFragPos;

void main()
{
    // Обычные координаты без смещения (чтобы кубы не разваливались)
    gl_Position = mvp * vec4(vertexPosition, 1.0);
    
    fragPosition = vec3(matModel * vec4(vertexPosition, 1.0));
    fragTexCoord = vertexTexCoord;
    
    // Расчет нормалей
    fragNormal = normalize(vec3(matModel * vec4(vertexNormal, 0.0)));
    
    // Создаем TBN матрицу
    vec3 T = normalize(vec3(matModel * vertexTangent));
    vec3 N = normalize(vec3(matModel * vec4(vertexNormal, 0.0)));
    // Ортогонализация (Gram-Schmidt)
    T = normalize(T - dot(T, N) * N);
    vec3 B = cross(N, T) * vertexTangent.w;
    
    TBN = mat3(T, B, N);
    
    // ВАЖНО: Транспонируем TBN, чтобы получить обратную матрицу
    // Это нужно, чтобы перевести вектора ИЗ мирового пространства В пространство текстуры
    mat3 TBN_transposed = transpose(TBN);
    
    tangentViewPos = TBN_transposed * viewPos;
    tangentFragPos = TBN_transposed * fragPosition;
}