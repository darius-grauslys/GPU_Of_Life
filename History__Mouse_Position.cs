
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class History__Mouse_Position :
History<Vector2, Vertex_Array_Object[]>
{
    private Vertex_Array_Object[] VAOs;

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
        VAOs = new Vertex_Array_Object[history_epoch_count];
        VAOs[0] = new Vertex_Array_Object();
    }

    public override void Aggregate__Epochs(ref Vertex_Array_Object[] aggregation, ref bool error)
    {
        aggregation = new Vertex_Array_Object[Quantity__Of__Epochs_Generated];
        if (Quantity__Of__Epochs_Generated == 0) return;

        for(int i=0;i<EPOCH__COUNT;i++)
        {
            int epoch_index =
                Get__Index_From__Oldest_Epoch(i);

            VAOs[i]
            .BufferData
            (
                EPOCHS[epoch_index].EPOCH__VALUES
            );

            aggregation[i] = VAOs[i];
        }
    }

//    public override void Append(Vector2 value)
//    {
//        Console.WriteLine($"Appendding: {value}");
//        base.Append(value);
//        Console.WriteLine("Append finished.");
//    }

    protected override void Handle_New__Epoch()
    {
//        Console.WriteLine($"EPOCH GENERATED, total quantity: {Quantity__Of__Epochs_Generated}");
        if (VAOs[Index__Current] == null)
        {
            VAOs[Index__Current] = new Vertex_Array_Object();
        }
    }
}
