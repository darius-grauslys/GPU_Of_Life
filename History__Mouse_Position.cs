
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

            for(int j=0;j<Quantity__Of__Records;j++)
                Console.WriteLine(EPOCHS[epoch_index].EPOCH__VALUES[j]);

            MOUSE_POSITION__VAO
            .Buffer__Data
            (
                EPOCHS[epoch_index].EPOCH__VALUES,
                // \/ this might've been the 0.75 week bug. Missing "* i"...
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
