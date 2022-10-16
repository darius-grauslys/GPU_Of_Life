#version 420
layout(points) in;
layout(line_strip, max_vertices = 2) out;

uniform vec2 mouse_origin;
uniform vec2 mouse_position;

void emit(vec2 point)
{
    gl_Position = vec4(point, 0, 1);
    EmitVertex();
}

void main()
{
    emit(mouse_origin);

    emit(mouse_position);

    EndPrimitive();
}
