#version 420
layout(points) in;
layout(triangle_strip, max_vertices = 17) out;

uniform vec2 mouse_origin;
uniform vec2 mouse_position;

uniform float arm_length;

const vec2[] hexagon = vec2[](vec2(0,0.66666), vec2(0.5,1), vec2(1,0.66666), vec2(0.66666,0), vec2(0.33333,0));

vec2 get_hexagon_point(int index, vec2 size, vec2 offset)
{
    vec2 point = hexagon[index];
    point = vec2(point.x * size.x, point.y * size.y);
    return point + offset;
}

void emit(vec2 point)
{
    gl_Position = vec4(point, 0, 1);
    EmitVertex();
}

void main()
{
    vec2 delta = mouse_position - mouse_origin;
    vec2 inner_hexagon_size = delta * min(1, max(0.001, arm_length));
    inner_hexagon_size.y *= -1;
    vec2 delta_midpoint = delta / 2 - (inner_hexagon_size / 2);

//1
    vec2 point;
    point = get_hexagon_point(1,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//2
    point = get_hexagon_point(4,delta,mouse_origin);
    emit(point);

//3
    point = get_hexagon_point(0,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//4
    emit(delta_midpoint + mouse_origin);

//5
    point = get_hexagon_point(0,delta,mouse_origin);
    emit(point);

//6
    point = get_hexagon_point(4,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//7
    point = get_hexagon_point(3,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//8
    point = get_hexagon_point(1,delta,mouse_origin);
    emit(point);

//9
    emit(delta_midpoint + mouse_origin);

//10 (7)
    point = get_hexagon_point(3,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//11
    point = get_hexagon_point(2,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//12
    point = get_hexagon_point(2,delta,mouse_origin);
    emit(point);

    //EndPrimitive(); return;

//13
    emit(delta_midpoint + mouse_origin);

//14 (11)
    point = get_hexagon_point(2,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//15 (1)
    point = get_hexagon_point(1,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

//16
    point = get_hexagon_point(3,delta,mouse_origin);
    emit(point);

//17 (14)
    point = get_hexagon_point(2,inner_hexagon_size,delta_midpoint + mouse_origin);
    emit(point);

    EndPrimitive();
}
