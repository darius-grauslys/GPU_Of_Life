
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

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
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FRAMEBUFFER__AGGREGATION);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            target.TEXTURE_HANDLE,
            0
        );

        if (invocation.Is__Using_Blending)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendEquation(invocation.Blend__Mode);
            GL.BlendFunc(invocation.Blend__Factor_Source, invocation.Blend__Factor_Destination);
        }

        invocation.Shader.Use();
        invocation.Set__Uniform(invocation.Mouse_Position__Latest);
        invocation.Set__Uniform(invocation.Mouse_Position__Origin);

        Private_Bind__Uniforms<int>     (invocation.Shader, invocation.Uniform1__Int);
        Private_Bind__Uniforms<uint>    (invocation.Shader, invocation.Uniform1__Unsigned_Int);
        Private_Bind__Uniforms<float>   (invocation.Shader, invocation.Uniform1__Float);
        Private_Bind__Uniforms<double>  (invocation.Shader, invocation.Uniform1__Double);
        Private_Bind__Uniforms<Vector2> (invocation.Shader, invocation.Uniform2__Vector2);
        Private_Bind__Uniforms<Vector2i>(invocation.Shader, invocation.Uniform2__Vector2i);
        Private_Bind__Uniforms<Matrix4> (invocation.Shader, invocation.Uniform__Matrix4);

        invocation.VAO.Bind();
        //GL.DrawArrays(PrimitiveType.LineStrip, 0, invocation.Primtive__Count);
        GL.DrawArrays(PrimitiveType.Points, 0, invocation.Primtive__Count);
        //GL.Finish();

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        if (invocation.Is__Using_Blending)
        {
            GL.Disable(EnableCap.Blend);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
        }
    }

    private void Private_Bind__Uniforms<T>
    (
        Shader shader,
        Dictionary<string, Shader.IUniform>? uniforms
    )
    where T   : struct
    {
        if (uniforms == null) return;

        foreach(Shader.IUniform<T> uniform in uniforms.Values)
        {
            shader.Set__Uniform(uniform);
        }
    }
}
