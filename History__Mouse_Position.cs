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

using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class History__Mouse_Position :
History<Vector2, Vertex_Array_Object>
{
    [AllowNull]
    private Vertex_Array_Object MOUSE_POSITION__VAO;

    public History__Mouse_Position
    (
        int history_epoch_size, 
        int history_epoch_count
    ) 
    : base
    (
        history_epoch_size,
        history_epoch_count
    )
    {
        Private_Establish__VAO();
    }

    public override Vertex_Array_Object Aggregate__Epochs(ref bool error)
    {
        if (Quantity__Of__Epochs_Generated == 0) return MOUSE_POSITION__VAO;

        for(int i=0;i<Quantity__Of__Epochs_Generated;i++)
        {
            int epoch_index =
                Get__Index_From__Oldest_Epoch(i);

            MOUSE_POSITION__VAO
            .Buffer__Data
            (
                EPOCHS[epoch_index].EPOCH__VALUES,
                new IntPtr(Vector2.SizeInBytes * EPOCH__SIZE * i),
                Quantity__Of__Records
            );
        }

        return MOUSE_POSITION__VAO;
    }

    public override void Clear()
    {
        base.Clear();
        Private_Establish__VAO();
    }

    private void Private_Establish__VAO()
    {
        MOUSE_POSITION__VAO =
            new Vertex_Array_Object
            (
                EPOCH__SIZE * EPOCH__COUNT * Vector2.SizeInBytes,
                new Vertex_Array_Object.Attribute(2, VertexAttribPointerType.Float, 2 * sizeof(float), 0)
            );
    }
}
