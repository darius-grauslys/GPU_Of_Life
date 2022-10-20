/**************************************************************************
 *
 *    Copyright (c) 2022 Darius Grauslys
 *
 *    Permission is hereby granted, free of charge, to any person obtaining
 *    a copy of this software and associated documentation files (the
 *    "Software"), to deal in the Software without restriction, including
 *    without limitation the rights to use, copy, modify, merge, publish,
 *    distribute, sublicense, and/or sell copies of the Software, and to
 *    permit persons to whom the Software is furnished to do so, subject to
 *    the following conditions:
 *
 *    The above copyright notice and this permission notice shall be
 *    included in all copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 *    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 *    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 *    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 *    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 *    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 *    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 **************************************************************************/

namespace GPU_Of_Life;

/// <summary>
/// Helpful passthrough shader. This is great for 
/// copying data from one texture to another.
/// Utilize Shader__Post_Processor.Process() to do so.
/// </summary>
public sealed class Shader__Passthrough : Shader__Post_Processor
{
    private const string source__vertex =
@"
#version 420 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPosition, 1);
    TexCoord = aTexCoord;
}
";

    private const string source__fragment =
@"
#version 420

out vec4 output_color;
in vec2 TexCoord;

uniform sampler2D _sampler;

void main()
{
    output_color = texture(_sampler, TexCoord);
}
";

    public Shader__Passthrough()
    : base (source__vertex, source__fragment, out _) { }
    public Shader__Passthrough(out bool error)
    : base (source__vertex, source__fragment, out error) { }
}
