#version 420
layout(points) in;
layout(line_strip, max_vertices = 5) out;

uniform vec2 mouse_origin;
uniform vec2 mouse_position;

void emit(vec2 point)
{
    gl_Position = vec4(point, 0, 1);
    EmitVertex();
}

void main()
{
    vec2 delta = mouse_position - mouse_origin;

    emit(mouse_origin);

    emit(mouse_origin + vec2(delta.x, 0));

    emit(mouse_origin + delta);

    emit(mouse_origin + vec2(0, delta.y));

    emit(mouse_origin);

    EndPrimitive();
}
