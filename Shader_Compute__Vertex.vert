#version 420 core

layout(location = 0) in vec2 point;

uniform float width;
uniform float height;

out float cell_w;
out float cell_h;

void main()
{
    cell_w = 1/width;
    cell_h = 1/height;
    gl_Position = vec4(point.x * cell_w, point.y * cell_h, 0, 1);
}
