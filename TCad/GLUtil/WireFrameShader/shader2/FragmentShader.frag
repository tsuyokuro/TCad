# version 460 core

precision mediump float;

in vec3 vNormal;

in vec3 vBarycentric;

out vec4 FragColor;

uniform vec3 uLightDir;

uniform vec4 uObjColor;


const float lineWidth = 1.0;

const vec3 lineColor = vec3(1.0, 1.0, 1.0);

float edgeFactor() {
    vec3 d = fwidth( vBarycentric );
    vec3 f = step( d * lineWidth, vBarycentric );
    return min( min( f.x, f.y ), f.z );
}

void main()
{
    vec3 normal = normalize(vNormal);
    //vec3 lightDir = normalize(-uLightDir);
    vec3 lightDir = normalize(vec3(0.0, 2.0, 1.0));

    float diff = max(dot(normal, lightDir), 0.0);
    vec3 diffuse = diff * vec3(uObjColor);

    vec3 finalColor = diffuse;

    FragColor.rgb = mix(
        lineColor,
        finalColor.xyz,
        edgeFactor()
    );
}


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

