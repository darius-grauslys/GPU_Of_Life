#version 420
layout(points) in;
layout(triangle_strip, max_vertices = 4) out;

uniform float width;
uniform float height;

uniform sampler2D _sample;

out float life;

void main()
{
    vec2 cell = gl_in[0].gl_Position.xy;
    life = texelFetch(_sample, ivec2(int(cell.x), int(cell.y)), 0).x;
    //if (life == 0) return;

    float mod = 1 - (life - 1);
    //float mod = 0.7;

    float cell_w = 1/width  * mod / 2;
    float cell_h = 1/height * mod / 2;

    vec4 out_pos = vec4( cell.x * 2 / width, (cell.y) * 2 / height, 0, 1 ) + vec4(cell_w / mod - 1, cell_h / mod - 1,0,0);

    gl_Position = out_pos + vec4(-cell_w, -cell_h, 0, 0);
    //gl_Position = vec4(-0.5,-0.5,0,1);
    EmitVertex();

    gl_Position = out_pos + vec4( cell_w, -cell_h, 0, 0);
    //gl_Position = vec4(0.5,-0.5,0,1);
    EmitVertex();

    gl_Position = out_pos + vec4(-cell_w,  cell_h, 0, 0);
    //gl_Position = vec4(-0.5,0.5,0,1);
    EmitVertex();

    gl_Position = out_pos + vec4( cell_w,  cell_h, 0, 0);
    //gl_Position = vec4(0.5,0.5,0,1);
    EmitVertex();

    EndPrimitive();
}
