#version 420

out vec4 output_color;

uniform float life;

void main()
{
    output_color = vec4(life,0,0,1);
}
