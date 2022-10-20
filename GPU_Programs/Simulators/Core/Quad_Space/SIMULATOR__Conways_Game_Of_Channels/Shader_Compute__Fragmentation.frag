#version 420

out vec4 color;

in vec2 point;

uniform sampler2D _sample;

float life_rule(float life_sum, float this_life)
{
    float life = this_life;
    if (life_sum < 2 || life_sum > 3)
        life = 0;
    else if (life_sum > 2)
        life = 1;
    return life;
}

void main()
{
    float life_sum_red = 0;
    float life_sum_green = 0;
    float life_sum_blue = 0;

    float this_life_red = 0;
    float this_life_green = 0;
    float this_life_blue = 0;

    vec4 texel;

    for(int i=-1;i<2;i++)
    {
        for(int j=-1;j<2;j++)
        {
            if (i==0 && j==0)
            {
                texel           = texelFetch(_sample, ivec2(point.x, point.y), 0);
                this_life_red   = texel.x;
                this_life_green = texel.y;
                this_life_blue  = texel.z;
                continue;
            }

            ivec2 neighbor  = ivec2(i, j);
            texel           = texelFetch(_sample, ivec2(point.x, point.y) + neighbor, 0);
            life_sum_red   += texel.x;
            life_sum_green += texel.y;
            life_sum_blue  += texel.z;
        }
    }

    float life_red   = life_rule(life_sum_red, this_life_red);
    float life_green = life_rule(life_sum_green, this_life_green);
    float life_blue  = life_rule(life_sum_blue, this_life_blue);

    color = vec4(life_red, life_green, life_blue, 1);
}
