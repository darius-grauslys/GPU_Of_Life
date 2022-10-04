#version 420

in vec4 geoColor;

out vec4 color;

void main()
{
    color = geoColor;
    //color = vec4(0,1,0,1);
}
