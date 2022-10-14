
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class History__Tool_Invocation
: History<Shader.Invocation, Texture>
{
    private readonly History__Mouse_Position HISTORY__MOUSE_POSITION;
    public Vertex_Array_Object Aggregation__Mouse_Positions(out int primitive_count)
    {
        primitive_count = HISTORY__MOUSE_POSITION.Quantity__Of__Records;
        bool error = true;
        return HISTORY__MOUSE_POSITION.Aggregate__Epochs(ref error);
    }
    private readonly Vertex_Array_Object TOOL__VAO__NO_MOUSE_HISTORY;

    private readonly Shader__Passthrough SHADER__PASSTHROUGH;

    private int GRID__WIDTH, GRID__HEIGHT;
    private Shader_Invocation__Aggregator EPOCH__AGGREGATOR = 
        new Shader_Invocation__Aggregator();

    private Texture TOOL__BASE_AGGREGATION;
    private Texture TOOL__BASE_PASSTHROUGH;
    internal Texture[] TOOL__EPOCH_TEXTURES;
    public Texture Aggregation__Latest
        => TOOL__EPOCH_TEXTURES[Index__Current];

    internal new Shader.Invocation Preparing__Value
        => base.Preparing__Value;

    public bool Is__Requiring_Mouse_Position_History { get; set; }

    public History__Tool_Invocation
    (
        Texture base_aggregation,
        int history__tool__epoch_size, 
        int history__tool__epoch_count,
        int history__mouse__epoch_size,
        int history__mouse__epoch_count
    ) 
    : base
    (
        history__tool__epoch_size, 
        history__tool__epoch_count
    )
    { 
        SHADER__PASSTHROUGH = new Shader__Passthrough();

        TOOL__BASE_AGGREGATION = base_aggregation;
        TOOL__BASE_PASSTHROUGH =
            new Texture
            (
                base_aggregation.Width,
                base_aggregation.Height
            );
        TOOL__EPOCH_TEXTURES = new Texture[history__tool__epoch_count];

        TOOL__VAO__NO_MOUSE_HISTORY =
            new Vertex_Array_Object(2 * sizeof(float));
        TOOL__VAO__NO_MOUSE_HISTORY
            .Buffer__Data(new float[] {0,0});

        HISTORY__MOUSE_POSITION =
            new History__Mouse_Position(history__mouse__epoch_size, history__tool__epoch_count);

        TOOL__EPOCH_TEXTURES[0] = new Texture(base_aggregation.Width, base_aggregation.Height);
    }

    public void Buffer__Mouse_Position(Vector2 mouse_position)
    {
        if (!Is__Preparing__Value) return;
        if (!Is__Requiring_Mouse_Position_History) return;
        bool finishing = HISTORY__MOUSE_POSITION.Is__Current_Epoch_Finished;
        Console.WriteLine($"epoch: {HISTORY__MOUSE_POSITION.Epoch__Currently_Indexed.EPOCH__HISTORY_INDEX} / {EPOCH__SIZE}");

        HISTORY__MOUSE_POSITION.Append(mouse_position);

        if (finishing)
        {
            Console.WriteLine(" >>> FINISH - Mouse history overflow");
            bool error = false;
            Shader.Invocation clone = Preparing__Value.Clone();
            Vertex_Array_Object aggregate_vao =
                HISTORY__MOUSE_POSITION.Aggregate__Epochs(ref error);
            Preparing__Value.VAO = aggregate_vao;
            Finish();
            Prepare(clone);
        }
    }

    public override void Finish()
    {
        bool error = false;
        Vertex_Array_Object aggregate_vao =
            HISTORY__MOUSE_POSITION.Aggregate__Epochs(ref error);
        Preparing__Value.Primtive__Count = HISTORY__MOUSE_POSITION.Quantity__Of__Records;
        Preparing__Value.VAO = aggregate_vao;
        HISTORY__MOUSE_POSITION.Clear();
        base.Finish();
    }

    public virtual void Resize(int width, int height, ref bool error)
    {
        //TODO: impl, resize textures per epoch
    }

    public override Texture Aggregate__Epochs(ref bool error)
    {
        if (error) return null;

        GLHelper.Push_Viewport(0,0,TOOL__BASE_AGGREGATION.Width,TOOL__BASE_AGGREGATION.Height);

        if (Is__In_Need_Of__Update)
            SHADER__PASSTHROUGH.Process(TOOL__BASE_AGGREGATION, TOOL__BASE_PASSTHROUGH);
        else 
            SHADER__PASSTHROUGH.Process(TOOL__EPOCH_TEXTURES[Index__Current], TOOL__BASE_PASSTHROUGH);

        Texture base_aggregation = TOOL__BASE_PASSTHROUGH;

        Console.WriteLine(GLHelper.Current);
        for(int i=0;i<Quantity__Of__Epochs_Generated;i++)
        {
            int epoch_index = Get__Index_From__Oldest_Epoch(i);

            Epoch epoch = EPOCHS[epoch_index];

            if (!epoch.Is__In_Need_Of__Update) continue;
            Console.WriteLine("update epoch");
            
            SHADER__PASSTHROUGH.Process(base_aggregation, TOOL__EPOCH_TEXTURES[epoch_index]);

            foreach(Shader.Invocation invocation in epoch.Active__Values)
            {
                EPOCH__AGGREGATOR
                    .Aggregate__Invocation
                    (
                        invocation,
                        TOOL__EPOCH_TEXTURES[epoch_index],
                        ref error
                    );

                if (error) return null;
            }
            
            epoch.Is__In_Need_Of__Update = false;

            base_aggregation = TOOL__EPOCH_TEXTURES[epoch_index];
        }

        if (Is__Preparing__Value)
        {
            int primitive_count;
            Vertex_Array_Object vao_aggregate =
                Aggregation__Mouse_Positions(out primitive_count);
            Preparing__Value.VAO = vao_aggregate;
            Preparing__Value.Primtive__Count = primitive_count;
            EPOCH__AGGREGATOR
                .Aggregate__Invocation
                (
                    Preparing__Value,
                    base_aggregation,
                    ref error
                );
        }

        GLHelper.Pop_Viewport();

        return base_aggregation;
        //return TOOL__EPOCH_TEXTURES[Index__Current];
    }

    protected override void Handle_New__Epoch()
    {
        TOOL__EPOCH_TEXTURES[Index__Current] = 
            new Texture
            (
                TOOL__BASE_AGGREGATION.Width,
                TOOL__BASE_AGGREGATION.Height
            );
    }
}
