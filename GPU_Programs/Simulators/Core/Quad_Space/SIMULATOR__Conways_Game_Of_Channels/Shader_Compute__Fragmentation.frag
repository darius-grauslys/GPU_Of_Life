//*************************************************************************
//
//    Copyright (c) 2022 Darius Grauslys
//
//    Permission is hereby granted, free of charge, to any person obtaining
//    a copy of this software and associated documentation files (the
//    "Software"), to deal in the Software without restriction, including
//    without limitation the rights to use, copy, modify, merge, publish,
//    distribute, sublicense, and/or sell copies of the Software, and to
//    permit persons to whom the Software is furnished to do so, subject to
//    the following conditions:
//
//    The above copyright notice and this permission notice shall be
//    included in all copies or substantial portions of the Software.
//
//    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
//    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
//    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
//    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*************************************************************************/

// FRAGMENTATION SHADER -- Cellular Automata Computation

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
