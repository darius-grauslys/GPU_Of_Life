
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class History__Tool_Invocation
: History<Shader.Invocation, Texture>
{
    private readonly History__Mouse_Position HISTORY__MOUSE_POSITION;
    public Vertex_Array_Object Aggregation__Tool_Positions(out int primitive_count)
    {
        if (Is__Requiring_Mouse_Position_History)
        {
            primitive_count = HISTORY__MOUSE_POSITION.Quantity__Of__Records;
            bool error = true;
            return HISTORY__MOUSE_POSITION.Aggregate__Epochs(ref error);
        }

        primitive_count = 1;
        return TOOL__VAO__NO_MOUSE_HISTORY;
    }
    private readonly Vertex_Array_Object TOOL__VAO__NO_MOUSE_HISTORY;

    private readonly Shader__Passthrough SHADER__PASSTHROUGH;

    private int GRID__WIDTH, GRID__HEIGHT;
    private Shader_Invocation__Aggregator EPOCH__AGGREGATOR = 
        new Shader_Invocation__Aggregator();

    internal bool Is__Genesis_Rolling { get; private set; }
    internal Texture EPOCH__TEXTURE__GENESIS;
    internal Texture EPOCH__TEXTURE__OVERLAY;
    internal Texture[] EPOCH__TEXTURES;
    public Texture Aggregation__Latest
        => EPOCH__TEXTURES[Index__Current];

    internal new Shader.Invocation Preparing__Value
        => base.Preparing__Value;

    public bool Is__Requiring_Mouse_Position_History { get; set; }
    private readonly int EPOCH__WIDTH, EPOCH__HEIGHT;
    private readonly int EPOCH__CHANNEL_COUNT;
    private readonly PixelInternalFormat EPOCH__PIXEL_INTERNAL_FORMAT;
    private readonly PixelFormat EPOCH__PIXEL_FORMAT;

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

        EPOCH__TEXTURES = new Texture[history__tool__epoch_count];

        TOOL__VAO__NO_MOUSE_HISTORY =
            new Vertex_Array_Object
            (
                2 * sizeof(float),
                new Vertex_Array_Object.Attribute(2, VertexAttribPointerType.Float, sizeof(float) * 2, 0)
            );
        TOOL__VAO__NO_MOUSE_HISTORY
            .Buffer__Data(new float[] {0,0});

        HISTORY__MOUSE_POSITION =
            new History__Mouse_Position(history__mouse__epoch_size, history__tool__epoch_count);

        EPOCH__WIDTH  = base_aggregation.Width;
        EPOCH__HEIGHT = base_aggregation.Height;
        EPOCH__CHANNEL_COUNT = base_aggregation.Pixel_Buffer_Initalizer.Channel_Count;
        EPOCH__PIXEL_INTERNAL_FORMAT = base_aggregation.Pixel_Buffer_Initalizer.Internal_Format;
        EPOCH__PIXEL_FORMAT = base_aggregation.Pixel_Buffer_Initalizer.Pixel_Format;

        EPOCH__TEXTURE__GENESIS =
            new Texture
            (
                base_aggregation.Width, base_aggregation.Height,
                base_aggregation.Pixel_Buffer_Initalizer
            );

        EPOCH__TEXTURES[0] = 
            new Texture
            (
                base_aggregation.Width, base_aggregation.Height,
                base_aggregation.Pixel_Buffer_Initalizer.Channel_Count,
                base_aggregation.Pixel_Buffer_Initalizer.Internal_Format,
                base_aggregation.Pixel_Buffer_Initalizer.Pixel_Format
            );

        EPOCH__TEXTURE__OVERLAY = 
            new Texture
            (
                base_aggregation.Width, base_aggregation.Height,
                base_aggregation.Pixel_Buffer_Initalizer.Channel_Count,
                base_aggregation.Pixel_Buffer_Initalizer.Internal_Format,
                base_aggregation.Pixel_Buffer_Initalizer.Pixel_Format
            );

        SHADER__PASSTHROUGH.Process(base_aggregation, EPOCH__TEXTURE__GENESIS);
        SHADER__PASSTHROUGH.Process(base_aggregation, EPOCH__TEXTURES[0]);
    }

    public void Buffer__Mouse_Position(Vector2 mouse_position)
    {
        if (!Is__Preparing__Value) return;
        if (!Is__Requiring_Mouse_Position_History) return;
        bool finishing = HISTORY__MOUSE_POSITION.Is__Current_Epoch_Finished;

        HISTORY__MOUSE_POSITION.Append(mouse_position);

        if (finishing)
        {
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
        int primitive_count;
        Vertex_Array_Object invocation_vao =
            Aggregation__Tool_Positions(out primitive_count);
        Preparing__Value.Primtive__Count = primitive_count;
        Preparing__Value.VAO = invocation_vao;
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

        GLHelper.Push_Viewport(0,0,EPOCH__TEXTURES[0].Width,EPOCH__TEXTURES[0].Height);

        for(int i=0;i<Quantity__Of__Epochs_Generated;i++)
        {
            int epoch_index = Get__Index_From__Oldest_Epoch(i);
            int previous_epoch = Get__Index_Offset_From__Oldest_Epoch(i, -1);

            Epoch epoch = EPOCHS[epoch_index];

            if (!epoch.Is__In_Need_Of__Update) continue;
            
            SHADER__PASSTHROUGH.Process
            (
                (i == 0)
                ? EPOCH__TEXTURE__GENESIS
                : EPOCH__TEXTURES[previous_epoch], 
                EPOCH__TEXTURES[epoch_index]
            );

            foreach(Shader.Invocation invocation in epoch.Active__Values)
            {
                EPOCH__AGGREGATOR
                    .Aggregate__Invocation
                    (
                        invocation,
                        EPOCH__TEXTURES[epoch_index],
                        ref error
                    );

                if (error) return null;
            }
            
            epoch.Is__In_Need_Of__Update = false;
        }

        if (Is__Preparing__Value)
        {
            SHADER__PASSTHROUGH.Process
            (
                EPOCH__TEXTURES[Index__Current],
                EPOCH__TEXTURE__OVERLAY
            );

            int primitive_count;
            Vertex_Array_Object? vao_aggregate =
                Aggregation__Tool_Positions(out primitive_count);
            Preparing__Value.VAO = vao_aggregate;
            Preparing__Value.Primtive__Count = primitive_count;
            EPOCH__AGGREGATOR
                .Aggregate__Invocation
                (
                    Preparing__Value,
                    EPOCH__TEXTURE__OVERLAY,
                    ref error
                );

            GLHelper.Pop_Viewport();
            return EPOCH__TEXTURE__OVERLAY;
        }

        GLHelper.Pop_Viewport();

        return EPOCH__TEXTURES[Index__Current];
    }

    protected override void Handle_New__Epoch()
    {
        Is__Genesis_Rolling =
            Is__Genesis_Rolling
            ||
            Index__Current == 0
            ;

        EPOCH__TEXTURES[Index__Current] = 
            new Texture
            (
                EPOCH__WIDTH,
                EPOCH__HEIGHT,
                EPOCH__CHANNEL_COUNT,
                EPOCH__PIXEL_INTERNAL_FORMAT,
                EPOCH__PIXEL_FORMAT
            );
        
        if (Is__Genesis_Rolling)
            EPOCH__TEXTURE__GENESIS =
                EPOCH__TEXTURES[Get__Index_From__Oldest_Epoch(0)];
    }
}
