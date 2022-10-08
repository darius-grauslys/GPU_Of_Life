
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Vertex_Array_Object : IDisposable
{
    public readonly int VAO_Handle;
    public readonly int VBO_Handle;

    public Vertex_Array_Object()
    {
        VAO_Handle = GL.GenVertexArray();

        GL.BindVertexArray(VAO_Handle);
        VBO_Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_Handle);
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteVertexArray(VAO_Handle);
        GL.DeleteBuffer(VBO_Handle);
    }

    public void Bind()
    {
        GL.BindVertexArray(VAO_Handle);
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void BufferData
    (
        Vector2[] data, 
        BufferUsageHint usage_hint = BufferUsageHint.StaticDraw
    )
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_Handle);
        GL.BufferData
        (
            BufferTarget.ArrayBuffer,
            data.Length * Vector2.SizeInBytes,
            data,
            usage_hint
        );
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void BufferData
    (
        float[] data, 
        BufferUsageHint usage_hint = BufferUsageHint.StaticDraw
    )
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_Handle);
        GL.BufferData
        (
            BufferTarget.ArrayBuffer,
            data.Length * sizeof(float),
            data,
            usage_hint
        );
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
}
