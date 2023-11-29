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

// VERTEX SHADER -- Cellular Automata Computation

#version 420 core
layout(location = 0) in vec2 aPoint;

uniform float width;
uniform float height;

out vec2 point;

void main()
{
    gl_Position = vec4((aPoint.x + 0.5) * 2 / width, (aPoint.y + 0.5) * 2 / height, 0, 1) - vec4(1, 1, 0, 0);
    //gl_Position = vec4(aPoint.x / width, (aPoint.y + 1) / height, 0, 1) - vec4(1, 1, 0, 0);
    //gl_Position = vec4(0,0,0,1);
    point = aPoint;
}
