#version 420 core

layout(location = 0) in vec2 point;

void main()
{
    gl_Position = vec4(point, 0, 1);
}
