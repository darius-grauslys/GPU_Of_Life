#version 420

out vec4 output_color;

uniform float life;
uniform float test1;
uniform float test2;
uniform float test3;
uniform float test4;

void main()
{
    output_color = vec4(life,0,0,1);
    //output_color = vec4(1,0,1,1);
}
