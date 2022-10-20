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
    //float r = 0,g = 0,b = 0;
    //if (r == max(r, max(g,b)))
    //    r = color.x;
    //else if (g == max(r, max(g,b)))
    //    g = color.y;
    //else
    //    b = color.z;
    //geoColor = vec4(r,g,b,1);
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

    vec4 texel = texelFetch(_sample, ivec2(int(cell.x), int(cell.y)), 0);
    float life = max(texel.x, max(texel.y, texel.z));
    float mod = 1 - (0.9 - (life * 0.9));

    emit(out_pos, cell_size, vec4(0.05, 0.05, 0.05, 1));
    if (life == 0) return;

    cell_size = get_cell_size(mod);
    out_pos = get_grid_position(mod, cell, cell_size);

    emit(out_pos, cell_size, texel);
}