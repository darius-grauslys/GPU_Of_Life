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

using OpenTK.Graphics.OpenGL;

namespace GPU_Of_Life;

/// <summary>
/// A shader which takes a Texture2D as one of it's uniforms.
/// Process will bind a framebuffer, a screen rect, and sample
/// from the given Texture2D sample to the given Texture2D target.
/// If the target is null, it will render to the screen.
/// </summary>
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
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, target != null ? FBO : 0);
        if (target != null)
            GL.FramebufferTexture2D
            (
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                target.TEXTURE_HANDLE,
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
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    protected virtual void Handle_Bind__Uniforms() { }
}
