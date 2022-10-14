
namespace GPU_Of_Life;

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
