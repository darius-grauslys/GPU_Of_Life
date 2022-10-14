
using OpenTK.Graphics.OpenGL;

namespace GPU_Of_Life;

public class Shader__Post_Processor :
Shader
{
    protected int SCREEN_RECT__VERTEX_BUFFER;
    protected int SCREEN_RECT__ELEMENT_BUFFER;
    protected int SCREEN_RECT__VAO;

    private readonly float[] SCREEN_RECT__VERTS =
    {
        // verts      // text coords
         1,  1, 0,     1,  1,
         1, -1, 0,     1,  0,
        -1, -1, 0,     0,  0,
        -1,  1, 0,     0,  1
    };

    private readonly uint[] SCREEN_RECT__ELEMENTS =
    {
        0, 1, 3,
        1, 2, 3
    };

    private int FBO;

    public Shader__Post_Processor()
    {
        Private_Initalize();
    }

    public Shader__Post_Processor
    (
        string source__vert,
        string source__frag,
        out bool error
    )
    : base(source__vert, source__frag, out error)
    {
        Private_Initalize();
    }

    private void Private_Initalize()
    {
        FBO = GL.GenFramebuffer();

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
    }

    public virtual void Process(Texture sample, Texture? target = null)
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            target?.TEXTURE_HANDLE ?? 0,
            0
        );

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture
        (
            TextureTarget.Texture2D,
            sample.TEXTURE_HANDLE
        );

        Use();
        Handle_Bind__Uniforms();

        GL.BindVertexArray(SCREEN_RECT__VAO);
        GL.DrawElements(PrimitiveType.Triangles, SCREEN_RECT__ELEMENTS.Length, DrawElementsType.UnsignedInt, 0);
    }

    protected virtual void Handle_Bind__Uniforms() { }
}
