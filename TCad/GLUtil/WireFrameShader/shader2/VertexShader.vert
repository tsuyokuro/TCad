
# version 460 core

precision mediump float;
        
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 aNormal;
layout(location = 2) in vec3 aBarycentric;

out vec3 vNormal;
out vec3 vBarycentric;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    vNormal = normalize(aNormal);
    vBarycentric = aBarycentric;

    gl_Position = projectionMatrix * modelViewMatrix * vec4(aPos, 1.0);
}


/*
# version 120
void main(void)
{
    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
}
*/

/*
# version 460 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec3 incolor;
layout(location = 2) in vec3 barycentric;

out vec4 vertexColor;
out vec3 baryxyz;

uniform mat4 modelViewMatrix;
uniform mat4 projectionMatrix;

void main()
{
    gl_Position = projectionMatrix * modelViewMatrix * vec4(aPos, 1.0);
    vertexColor = vec4(incolor, 1.0);
    baryxyz = barycentric;
}
*/
