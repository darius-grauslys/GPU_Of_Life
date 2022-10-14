
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Shader__Composite_Line :
Shader
{
    private const string source__vert =
@"
#version 420 core
layout(location=0) in vec2 aPoint;

void main()
{
    gl_Position = vec4(aPoint, 0, 1);
    //gl_Position = vec4(1/(2 + gl_VertexID), 0, 0, 1);
}
";

    private const string source__frag =
@"
#version 420

out vec4 color;

uniform float blue;

void main()
{
    color = vec4(0,1,blue,1);
}
";

    public Uniform<float> Blue = new Uniform<float>("blue", 0);

    Vertex_Array_Object? line_vao;
    int line_vao__raw;
    int primitive_count;

    public void Set__VAO(Vertex_Array_Object vao, int primitive_count)
    {
        line_vao = vao;
        line_vao__raw = 0;
        this.primitive_count = primitive_count;
    }
    public void Buffer(Vector2[] data)
    {
        if (line_vao == null) return;
        line_vao.Buffer__Data(data);
    }
    public void Set__VAO(int vao, int primitive_count)
    {
        line_vao = null;
        line_vao__raw = vao;
        this.primitive_count = primitive_count;
    }

    public Shader__Composite_Line()
    : base(source__vert, source__frag, out _)
    {
    }

    public Shader__Composite_Line(Vertex_Array_Object line_vao, int primitive_count)
    : base(source__vert, source__frag, out _)
    {
        this.line_vao = line_vao;
        this.primitive_count = primitive_count;
    }

    public Shader__Composite_Line(int line_vao, int primitive_count)
    : base(source__vert, source__frag, out _)
    {
        this.line_vao__raw = line_vao;
        this.primitive_count = primitive_count;
    }

    public void Process(Vertex_Array_Object? vao = null)
    {
        Use();

        Set__Uniform(Blue);

        if (line_vao != null)
            line_vao.Bind();
        else if (line_vao__raw != 0)
            GL.BindVertexArray(line_vao__raw);
        else if (vao != null)
            vao.Bind();
        else
            return;

        GL.DrawArrays(PrimitiveType.LineStrip, 0, primitive_count);
    }
} 
