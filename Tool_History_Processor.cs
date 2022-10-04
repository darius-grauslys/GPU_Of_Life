
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Tool_History_Processor
{
    private int FRAMEBUFFER__PROCESS;

    public Tool_History_Processor()
    {
        FRAMEBUFFER__PROCESS = GL.GenFramebuffer();
    }

    public void Process__Tool_History
    (
        Texture initial_grid,
        Tool_History history
    )
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FRAMEBUFFER__PROCESS);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            initial_grid.TEXTURE_HANDLE,
            0
        );

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void Private_Process__Epoch
    (
        Tool_History.Invocation_Epoch epoch,
        ref bool error
    )
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, epoch.EPOCH__TEXTURE.TEXTURE_HANDLE);
        foreach(Tool_History.Tool_Invocation invocation in epoch.Invocations)
        {
            invocation.Tool__Shader.Use();

            if (invocation.Uniform1__Int != null)
                Private_Bind_Uniforms__Int
                (
                    invocation.Tool__Shader,
                    invocation.Uniform1__Int,
                    ref error
                );
            if (invocation.Uniform1__Unsigned_Int != null)
                Private_Bind_Uniforms__Unsigned_Int
                (
                    invocation.Tool__Shader,
                    invocation.Uniform1__Unsigned_Int,
                    ref error
                );
            if (invocation.Uniform1__Float != null)
                Private_Bind_Uniforms__Float
                (
                    invocation.Tool__Shader,
                    invocation.Uniform1__Float,
                    ref error
                );
            if (invocation.Uniform1__Double != null)
                Private_Bind_Uniforms__Double
                (
                    invocation.Tool__Shader,
                    invocation.Uniform1__Double,
                    ref error
                );

            if (invocation.Uniform2__Vector2 != null)
                Private_Bind_Uniforms__Vector2
                (
                    invocation.Tool__Shader,
                    invocation.Uniform2__Vector2,
                    ref error
                );
            if (invocation.Uniform2__Vector2i != null)
                Private_Bind_Uniforms__Vector2i
                (
                    invocation.Tool__Shader,
                    invocation.Uniform2__Vector2i,
                    ref error
                );

            if (invocation.UniformMat4__Matrix4 != null)
                Private_Bind_Uniforms__Matrix4
                (
                    invocation.Tool__Shader,
                    invocation.UniformMat4__Matrix4,
                    ref error
                );

            GL.BindVertexArray(invocation.VAO__Handle);
            GL.DrawArrays(PrimitiveType.Points, 0, invocation.Primtive__Count);
        }
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    private void Private_Bind_Uniforms__Int
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<int>> uniforms,
        ref bool error
    )
    {
        if (error) return;

        void callback(int location, ref int value) => GL.Uniform1(location, value);
        Private_Iterate<int>
        (
            tool__shader, uniforms, ref error,
            callback
        );
    }

    private void Private_Bind_Uniforms__Unsigned_Int
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<uint>> uniforms,
        ref bool error
    )
    {
        if (error) return;

        void callback(int location, ref uint value) => GL.Uniform1(location, value);
        Private_Iterate<uint>
        (
            tool__shader, uniforms, ref error,
            callback
        );
    }

    private void Private_Bind_Uniforms__Float
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<float>> uniforms,
        ref bool error
    )
    {
        if (error) return;

        void callback(int location, ref float value) => GL.Uniform1(location, value);
        Private_Iterate<float>
        (
            tool__shader, uniforms, ref error,
            callback
        );
    }

    private void Private_Bind_Uniforms__Double
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<double>> uniforms,
        ref bool error
    )
    {
        if (error) return;

        void callback(int location, ref double value) => GL.Uniform1(location, value);
        Private_Iterate<double>
        (
            tool__shader, uniforms, ref error,
            callback
        );
    }

    private void Private_Bind_Uniforms__Vector2
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<Vector2>> uniforms,
        ref bool error
    )
    {
        if (error) return;

        void callback(int location, ref Vector2 value) => GL.Uniform2(location, value);
        Private_Iterate<Vector2>
        (
            tool__shader, uniforms, ref error,
            callback
        );
    }

    private void Private_Bind_Uniforms__Vector2i
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<Vector2i>> uniforms,
        ref bool error
    )
    {
        if (error) return;

        void callback(int location, ref Vector2i value) => GL.Uniform2(location, value);
        Private_Iterate<Vector2i>
        (
            tool__shader, uniforms, ref error,
            callback
        );
    }

    private void Private_Bind_Uniforms__Matrix4
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<Matrix4>> uniforms,
        ref bool error
    )
    {
        if (error) return;

        void callback(int location, ref Matrix4 value) => GL.UniformMatrix4(location, false, ref value);
        Private_Iterate<Matrix4>
        (
            tool__shader, uniforms, ref error,
            callback
        );
    }

    private delegate void Callback__Found_Uniform_Location<T>(int location, ref T value)
    where T : struct;

    private void Private_Iterate<T>
    (
        Shader tool__shader,
        IEnumerable<Tool_History.Tool_Invocation.Uniform<T>> 
            uniforms,
        ref bool error,
        Callback__Found_Uniform_Location<T>
            callback__got_location
    )
    where T : struct
    {
        int location;
        foreach(Tool_History.Tool_Invocation.Uniform<T> pair in uniforms)
        {
            location = tool__shader.Get__Uniform(pair.Uniform__Name);
            if (error |= location < 0 && error)
                return;

            callback__got_location(location, ref pair.Uniform__Value);
        }
    }
}
