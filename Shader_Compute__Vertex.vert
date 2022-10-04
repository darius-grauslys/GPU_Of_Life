#version 420 core
layout(location = 0) in vec2 aPoint;

uniform float width;
uniform float height;

out vec2 point;

void main()
{
    gl_Position = vec4(aPoint.x * 2 / width, (aPoint.y + 1) * 2 / height, 0, 1) - vec4(1, 1, 0, 0);
    //gl_Position = vec4(aPoint.x / width, (aPoint.y + 1) / height, 0, 1) - vec4(1, 1, 0, 0);
    //gl_Position = vec4(0,0,0,1);
    point = aPoint;
}
