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

    emit(out_pos, cell_size, vec4(0.05, 0.05, 0.05, 1));

    float life = texelFetch(_sample, ivec2(int(cell.x), int(cell.y)), 0).x;
    float mod = 1 - (1 - life);

    cell_size = get_cell_size(mod);
    out_pos = get_grid_position(mod, cell, cell_size);

    emit(out_pos, cell_size, vec4(0.05, life, 0.05, 1));
}
