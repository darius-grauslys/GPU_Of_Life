//*************************************************************************
//
//    Copyright (c) 2022 Darius Grauslys
//
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*************************************************************************/

// GEOMETRY SHADER -- Cellular Automata Tool

#version 420
layout(points) in;
layout(line_strip, max_vertices = 11) out;

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

    for(int i=0;i<5;i++)
    {
        emit(get_hexagon_point(i, delta, mouse_origin));
        emit(get_hexagon_point(4 - i, inner_hexagon_size, mouse_origin + delta_midpoint));
    }
    emit(get_hexagon_point(0, delta, mouse_origin));

    EndPrimitive();
}
