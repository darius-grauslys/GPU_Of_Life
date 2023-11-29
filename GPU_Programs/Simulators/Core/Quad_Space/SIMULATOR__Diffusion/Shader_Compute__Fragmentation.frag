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
    vec3 life_sum = vec3(0,0,0);
    for(int i=-1;i<2;i++)
    {
        for(int j=-1;j<2;j++)
        {
            if (i==0 && j==0) continue;

            ivec2 neighbor = ivec2(i, j);
            life_sum += texelFetch(_sample, ivec2(point.x, point.y) + neighbor, 0).xyz;
        }
    }

    // float life = life_rule(life_sum);
    vec3 here = texelFetch(_sample, ivec2(point.x, point.y), 0).xyz;
    vec3 life = (life_sum / 8 + here) / 2;

    color = vec4(life, 1);
}
