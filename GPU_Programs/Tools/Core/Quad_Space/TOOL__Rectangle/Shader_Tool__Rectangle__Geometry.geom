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
