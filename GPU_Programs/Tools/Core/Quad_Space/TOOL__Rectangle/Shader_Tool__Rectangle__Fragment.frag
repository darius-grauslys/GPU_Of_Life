#version 420

out vec4 output_color;

uniform float red;
uniform float green;
uniform float blue;
uniform float alpha;

void main()
{
    output_color = vec4(red,green,blue,alpha);
}
