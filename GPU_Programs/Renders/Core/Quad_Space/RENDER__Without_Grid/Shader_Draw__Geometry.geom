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

// GEOMETRY SHADER -- Cellular Automata Grid Renderer

#version 420
layout(points) in;
layout(triangle_strip, max_vertices = 8) out;

uniform float width;
uniform float height;

uniform mat4 projection;
uniform mat4 translation;

uniform sampler2D _sample;

out vec4 geoColor;

vec4 get_grid_position(float mod, vec2 cell, vec2 cell_size)
{
    vec4 out_off = vec4(- 1 + 1/width, - 1 + 1/height, 0, 0 );
    vec4 out_pos = vec4( cell.x * 2 / width, (cell.y) * 2 / height, 0, 1 ) + out_off;

    return out_pos;
}

vec2 get_cell_size(float mod)
{
    float cell_w = (1/width)  * mod / 2;
    float cell_h = (1/height) * mod / 2;

    return vec2(cell_w, cell_h);
}

void emit(vec4 out_pos, vec2 cell_size, vec4 color)
{
    geoColor = color;

    gl_Position = out_pos + vec4(-cell_size.x, -cell_size.y, 0, 0);
    gl_Position = projection * translation * gl_Position;
    EmitVertex();

    gl_Position = out_pos + vec4( cell_size.x, -cell_size.y, 0, 0);
    gl_Position = projection * translation * gl_Position;
    EmitVertex();

    gl_Position = out_pos + vec4(-cell_size.x,  cell_size.y, 0, 0);
    gl_Position = projection * translation * gl_Position;
    EmitVertex();

    gl_Position = out_pos + vec4( cell_size.x,  cell_size.y, 0, 0);
    gl_Position = projection * translation * gl_Position;
    EmitVertex();

    EndPrimitive();
}

void main()
{
    vec2 cell = gl_in[0].gl_Position.xy;

    vec2 cell_size = get_cell_size(1.9);
    vec4 out_pos = get_grid_position(1, cell, cell_size);

    float life = texelFetch(_sample, ivec2(int(cell.x), int(cell.y)), 0).x;
    if (life == 0) return;
    float mod = 1 - (0.9 - (life * 0.9));

    cell_size = get_cell_size(mod);
    out_pos = get_grid_position(mod, cell, cell_size);

    emit(out_pos, cell_size, vec4(life, 0.05, 0.05, 1));
}
