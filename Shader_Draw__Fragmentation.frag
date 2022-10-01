#version 420

out vec4 color;

in float life;

void main()
{
    color = vec4(0, life, 0, 1);
}
