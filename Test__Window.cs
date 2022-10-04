
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GPU_Of_Life;

public class Test__Window : GameWindow
{
    protected readonly int SCREEN_RECT__VERTEX_BUFFER;
    protected readonly int SCREEN_RECT__ELEMENT_BUFFER;
    protected readonly int SCREEN_RECT__VAO;

    protected readonly float[] SCREEN_RECT__VERTS =
    {
        // verts      // text coords
         1,  1, 0,     1,  1,
         1, -1, 0,     1,  0,
        -1, -1, 0,     0,  0,
        -1,  1, 0,     0,  1
    };

    protected readonly uint[] SCREEN_RECT__ELEMENTS =
    {
        0, 1, 3,
        1, 2, 3
    };

    protected readonly Shader SHADER__DEFAULT;

    public Test__Window()
        : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        KeyDown += Private_Handle__Input;

        SCREEN_RECT__VERTEX_BUFFER  = GL.GenBuffer();
        SCREEN_RECT__ELEMENT_BUFFER = GL.GenBuffer(); 
        SCREEN_RECT__VAO            = GL.GenVertexArray();

        GL.BindVertexArray(SCREEN_RECT__VAO);

        GL.BindBuffer(BufferTarget.ArrayBuffer, SCREEN_RECT__VERTEX_BUFFER);
        GL.BufferData(BufferTarget.ArrayBuffer, SCREEN_RECT__VERTS.Length * sizeof(float), SCREEN_RECT__VERTS, BufferUsageHint.StaticDraw);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, SCREEN_RECT__ELEMENT_BUFFER);
        GL.BufferData(BufferTarget.ElementArrayBuffer, SCREEN_RECT__ELEMENTS.Length * sizeof(uint), SCREEN_RECT__ELEMENTS, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
        GL.EnableVertexAttribArray(1);

        string source__vert = @"
#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

uniform mat4 projection;
uniform mat4 translation;

void main()
{
    gl_Position = projection * translation * vec4(aPosition, 1);
    TexCoord = aTexCoord;
}
";
        string source__frag = @"
#version 330

out vec4 output_color;

in vec2 TexCoord;

uniform sampler2D sample;

void main()
{
    output_color = texture(sample, TexCoord);
}
";

        bool err = false;
        SHADER__DEFAULT =
            new Shader.Factory()
            .Begin()
            .Add__Shader(ShaderType.VertexShader, source__vert, ref err)
            .Add__Shader(ShaderType.FragmentShader, source__frag, ref err)
            .Link()
            ;

        if (err) Close();
    }

    protected void RENDER__DEFAULT
    (
        int? texture = null,
        Matrix4? nullable_projection = null,
        Matrix4? nullable_translation = null
    )
    {
        Matrix4 projection  = nullable_projection  ?? Matrix4.Identity;
        Matrix4 translation = nullable_translation ?? Matrix4.Identity; 

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        if (texture != null)
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, (int)texture);
        }
        SHADER__DEFAULT.Use();
        GL.UniformMatrix4(SHADER__DEFAULT.Get__Uniform("projection"), false, ref projection);
        GL.UniformMatrix4(SHADER__DEFAULT.Get__Uniform("translation"), false, ref translation);
        GL.BindVertexArray(SCREEN_RECT__VAO);
        GL.DrawElements(PrimitiveType.Triangles, SCREEN_RECT__ELEMENTS.Length, DrawElementsType.UnsignedInt, 0);
    }

    protected internal virtual void Handle__Arguments(string[] args)
    {

    }

    protected internal virtual void Handle__Reset()
    {

    }

    private void Private_Handle__Input(KeyboardKeyEventArgs e)
    {
        if (e.Key == Keys.Escape)
            Close();
        if (e.Key == Keys.Space)
            Handle__Reset();
    }
}
