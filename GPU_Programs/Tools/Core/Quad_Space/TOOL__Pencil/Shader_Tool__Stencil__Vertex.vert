#version 420 core
layout(location=0) in vec2 aPosition;

void main()
{
    vec2 position = aPosition;
    //if (position.x == -1 && position.y == 1)
    //{
    //    position = vec2(-0.5,0.5);
    //}
    //if (position.x == 1)
    //{
    //    position = vec2(0.5,0.5);
    //}
    gl_Position = vec4(position, 0, 1);
}
