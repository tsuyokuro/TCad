# version 460 core

precision mediump float;

in vec3 vNormal;
        
out vec4 FragColor;

uniform vec3 uLightDir;

uniform vec4 uObjColor;

const vec3 ambientLight = vec3(0.2, 0.2, 0.2);
        
void main()
{
    vec3 normal = normalize(vNormal);
    vec3 lightDir = normalize(uLightDir);

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * vec3(uObjColor);

    vec3 ambient = uObjColor.rgb * ambientLight;

    vec3 finalColor = diffuse + ambient;

    FragColor = vec4(finalColor, 1.0);
}

/*
# version 460 core

precision mediump float;

in vec3 vNormal;
        
out vec4 FragColor;

uniform vec3 uLightDir;

uniform vec4 uObjColor;

const vec3 ambientLight = vec3(0.2, 0.2, 0.2);
        
void main()
{
    vec3 normal = normalize(vNormal);
    vec3 lightDir = normalize(vec3(0.0, 2.0, 1.0));
    vec3 lightDir2 = normalize(vec3(0.0, -2.0, -1.0));

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * vec3(uObjColor);

    float diff2 = max(dot(normal, lightDir2), 0.0);
    vec3 diffuse2 = diff2 * vec3(uObjColor);

    vec3 ambient = uObjColor.rgb * ambientLight;

    vec3 finalColor = diffuse + diffuse2 + ambient;

    FragColor = vec4(finalColor, 1.0);
}
*/


/*
# version 120
void main (void)
{
    gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}
*/

/*
# version 460 core

out vec4 FragColor;
in vec4 vertexColor;

in vec3 baryxyz;

const float lineWidth = 1.0;

const vec3 lineColor = vec3(1.0, 1.0, 1.0);

float edgeFactor() {
    vec3 d = fwidth( baryxyz );
    vec3 f = step( d * lineWidth, baryxyz );
    return min( min( f.x, f.y ), f.z );
}

void main()
{
    FragColor.rgb = mix(
    lineColor,
    vertexColor.xyz,
    edgeFactor()
    );
}

*/

