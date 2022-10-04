#version 420

out vec4 color;

uniform float cell_w;
uniform float cell_h;

uniform sampler2D _sample;

void main()
{
    float life_sum
    for(float i=-1;i<2;i++)
    {
        for(float j=-1;j<2;j++)
        {
            if (i==0 && j==0) continue;

            vec2 neighbor = vec2(cell_w * i, cell_h * j);
            life_sum += texture(_sample, gl_FragCoord.xy + neighbor);
        }
    }

    life = life_rule(life_sum);

    color = vec4(life, 0, 0, 1);
}

float life_rule(float life_sum)
{
    float life;
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
}
