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
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Vertex_Array_Object : IDisposable
{
    public struct Attribute
    {
        public readonly int NUMBER_OF_ELEMENTS;
        public readonly VertexAttribPointerType TYPE;
        public readonly int SIZE_IN_BYTES;
        public readonly int OFFSET_IN_BYTES;

        public Attribute
        (
            int number_of_elements,
            VertexAttribPointerType type,
            int size_in_bytes,
            int offset_in_bytes
        )
        {
            NUMBER_OF_ELEMENTS = number_of_elements;
            TYPE = type;
            SIZE_IN_BYTES = size_in_bytes;
            OFFSET_IN_BYTES = offset_in_bytes;
        }
    }

    public readonly int VAO_Handle;
    public readonly int VBO_Handle;

    public readonly int MAX__SIZE;

    public Vertex_Array_Object
    (
        int max_size__in_bytes,
        params Attribute[] attributes
    )
    {
        VAO_Handle = GL.GenVertexArray();

        MAX__SIZE = max_size__in_bytes;

        GL.BindVertexArray(VAO_Handle);
        VBO_Handle = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_Handle);
        GL.BufferData(BufferTarget.ArrayBuffer, MAX__SIZE, IntPtr.Zero, BufferUsageHint.StaticDraw);
        for(int i=0;i<attributes.Length;i++)
        {
            GL.VertexAttribPointer
            (
                i, 
                attributes[i].NUMBER_OF_ELEMENTS,
                attributes[i].TYPE,
                false,
                attributes[i].SIZE_IN_BYTES,
                attributes[i].OFFSET_IN_BYTES
            );
            GL.EnableVertexAttribArray(i);
        }
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

    public void Buffer__Data
    (
        Vector2[] data, 
        IntPtr? offset = null,
        int? length = null
    )
    {
        int _length = length ?? data.Length;
        int size = 
            (_length * Vector2.SizeInBytes < MAX__SIZE)
            ? _length * Vector2.SizeInBytes
            : MAX__SIZE
            ;

        GL.BindVertexArray(VAO_Handle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_Handle);
        GL.BufferSubData
        (
            BufferTarget.ArrayBuffer,
            offset ?? IntPtr.Zero,
            size,
            data
        );
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void Buffer__Data
    (
        float[] data, 
        IntPtr? offset = null,
        int? length = null
    )
    {
        int _length = length ?? data.Length;
        int size = 
            (_length * sizeof(float) < MAX__SIZE)
            ? _length * sizeof(float)
            : MAX__SIZE
            ;

        GL.BindVertexArray(VAO_Handle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO_Handle);
        GL.BufferSubData
        (
            BufferTarget.ArrayBuffer,
            offset ?? IntPtr.Zero,
            size,
            data
        );
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
}
