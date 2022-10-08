
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class History__Tool_Invocation
: History<Shader.Invocation, Texture>
{
    private int GRID__WIDTH, GRID__HEIGHT;
    private Shader_Invocation__Aggregator EPOCH__AGGREGATOR = 
        new Shader_Invocation__Aggregator();

    public History__Tool_Invocation
    (
        int history_epoch_size, 
        int history_epoch_count
    ) 
    : base
    (
        history_epoch_size, 
        history_epoch_count
    )
    { }

    public virtual void Resize(int width, int height, ref bool error)
    {
        //TODO: impl, resize textures per epoch
    }

    public override void Aggregate__Epochs(ref Texture base_aggregation, ref bool error)
    {
        if (error) return;

        //TODO: only get updated epochs
        //and in addition to this, make a
        //passthrough shader to copy contents
        //over between epochs.
        IEnumerable<Epoch> epochs = Get__Epochs(false, true);
        IEnumerator<Epoch> enumerator_epochs = epochs.GetEnumerator();

        if (!enumerator_epochs.MoveNext()) return;

        while(enumerator_epochs.MoveNext())
        {
            Epoch epoch = enumerator_epochs.Current;

            foreach(Shader.Invocation invocation in epoch.Active__Values)
            {
                EPOCH__AGGREGATOR
                    .Aggregate__Invocation
                    (
                        invocation,
                        base_aggregation,
                        ref error
                    );

                if (error) return;
            }
        }
    }
}
