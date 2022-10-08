
using OpenTK.Graphics.OpenGL;

namespace GPU_Of_Life;

public class Shader_Invocation__Aggregator
{
    private int FRAMEBUFFER__AGGREGATION;

    public Shader_Invocation__Aggregator()
    {
        FRAMEBUFFER__AGGREGATION = GL.GenFramebuffer();
    }

    public void Aggregate__Invocation
    (
        Shader.Invocation invocation, 
        Texture target, 
        ref bool error
    )
    {
        GLHelper.Push_Viewport(0,0,target.Width,target.Height);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FRAMEBUFFER__AGGREGATION);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            target.TEXTURE_HANDLE,
            0
        );

        invocation.Shader.Use();

        Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Int);
        Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Unsigned_Int);
        Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Float);
        Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Double);
        Private_Bind__Uniforms(invocation.Shader, invocation.Uniform2__Vector2);
        Private_Bind__Uniforms(invocation.Shader, invocation.Uniform2__Vector2i);
        Private_Bind__Uniforms(invocation.Shader, invocation.UniformMat4__Matrix4);

        GL.BindVertexArray(invocation.VAO__Handle);
        GL.DrawArrays(PrimitiveType.Points, 0, invocation.Primtive__Count);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GLHelper.Pop_Viewport();
    }

    private void Private_Bind__Uniforms<T>
    (
        Shader shader,
        IEnumerable<Shader.IUniform<T>>? uniforms
    )
    where T   : struct
    {
        if (uniforms == null) return;

        foreach(Shader.Uniform<T> uniform in uniforms)
            shader.Set__Uniform(uniform);
    }
}
