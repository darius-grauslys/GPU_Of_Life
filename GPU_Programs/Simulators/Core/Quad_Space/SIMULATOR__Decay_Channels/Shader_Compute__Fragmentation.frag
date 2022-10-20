#version 420

out vec4 color;

in vec2 point;

uniform sampler2D _sample;

float life_rule(float life_sum)
{
    float life = life_sum;
    if (life_sum < 3 || life_sum > 4)
    {
        if (life_sum < 2 || life_sum > 5)
        {
            life = 0;
        }
        else
        {
            if (life_sum > 3) life_sum - 1;

            life = 1 - abs(3 - life_sum);
        }
    }
    return life;
}

void main()
{
    vec4 texel;

    float life_sum_red = 0;
    float life_sum_green = 0;
    float life_sum_blue = 0;
    for(int i=-1;i<2;i++)
    {
        for(int j=-1;j<2;j++)
        {
            if (i==0 && j==0) continue;

            ivec2 neighbor = ivec2(i, j);
            texel = texelFetch(_sample, ivec2(point.x, point.y) + neighbor, 0);

            life_sum_red += texel.x;
            life_sum_green += texel.y;
            life_sum_blue += texel.z;
        }
    }

    float life_red = life_rule(life_sum_red);
    float life_green = life_rule(life_sum_green);
    float life_blue = life_rule(life_sum_blue);

    color = vec4(life_red, life_green, life_blue, 1);
}
