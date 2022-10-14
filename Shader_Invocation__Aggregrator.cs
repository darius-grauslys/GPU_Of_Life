
using OpenTK.Graphics.OpenGL;

namespace GPU_Of_Life;

public class Shader_Invocation__Aggregator
{
    private readonly int FRAMEBUFFER__AGGREGATION;

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
        Console.WriteLine($"viewport: {GLHelper.Current}");
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

        //Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Int);
        //Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Unsigned_Int);
        //Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Float);
        //Private_Bind__Uniforms(invocation.Shader, invocation.Uniform1__Double);
        //Private_Bind__Uniforms(invocation.Shader, invocation.Uniform2__Vector2);
        //Private_Bind__Uniforms(invocation.Shader, invocation.Uniform2__Vector2i);
        //Private_Bind__Uniforms(invocation.Shader, invocation.UniformMat4__Matrix4);

        Console.WriteLine($"vao: {invocation.VAO.VAO_Handle} - {invocation.Primtive__Count}");
        invocation.VAO.Bind();
        GL.DrawArrays(PrimitiveType.LineStrip, 0, invocation.Primtive__Count);
        //GL.Finish();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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
        {
            Console.WriteLine($"uniform: {uniform.Name} = {uniform.Internal__Value}");
            shader.Set__Uniform(uniform);
        }
    }
}
