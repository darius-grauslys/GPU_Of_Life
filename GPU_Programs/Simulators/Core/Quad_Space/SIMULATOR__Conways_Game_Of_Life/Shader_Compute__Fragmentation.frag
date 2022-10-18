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

float get_max_component(vec4 sampling)
{
    return max(sampling.x, max(sampling.y, sampling.z));
}

void main()
{
    float life_sum = 0;
    float this_life = 0;
    for(int i=-1;i<2;i++)
    {
        for(int j=-1;j<2;j++)
        {
            if (i==0 && j==0)
            {
                this_life = get_max_component(texelFetch(_sample, ivec2(point.x, point.y), 0));
                continue;
            }

            ivec2 neighbor = ivec2(i, j);
            life_sum += get_max_component(texelFetch(_sample, ivec2(point.x, point.y) + neighbor, 0));
        }
    }

    float life = life_rule(life_sum, this_life);

    color = vec4(life, life, life, 1);
}
