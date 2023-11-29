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
using LibHistory;

namespace GPU_Of_Life;

public class History__Tool_Invocation
: Epoch_History<Shader.Invocation>
{
    private readonly History__Mouse_Position HISTORY__MOUSE_POSITION;
    public Vertex_Array_Object Aggregation__Tool_Positions(out int primitive_count)
    {
        if (Is__Requiring_Mouse_Position_History)
        {
            primitive_count = HISTORY__MOUSE_POSITION.Quantity_Of__Valid_Records;
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

    internal Texture EPOCH__TEXTURE__GENESIS;
    internal Texture EPOCH__TEXTURE__OVERLAY;
    internal Texture EPOCH__TEXTURE__OUTPUT;
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
            new History__Mouse_Position(history__mouse__epoch_size, history__mouse__epoch_count);

        EPOCH__WIDTH  = base_aggregation.Width;
        EPOCH__HEIGHT = base_aggregation.Height;
        EPOCH__CHANNEL_COUNT = base_aggregation.Pixel_Buffer_Initalizer.Channel_Count;
        EPOCH__PIXEL_INTERNAL_FORMAT = base_aggregation.Pixel_Buffer_Initalizer.Internal_Format;
        EPOCH__PIXEL_FORMAT = base_aggregation.Pixel_Buffer_Initalizer.Pixel_Format;

        // EPOCH__TEXTURE__GENESIS =
        //     new Texture
        //     (
        //         base_aggregation.Width, base_aggregation.Height,
        //         base_aggregation.Pixel_Buffer_Initalizer
        //     );
        EPOCH__TEXTURE__GENESIS =
            new Texture
            (
                base_aggregation.Width, base_aggregation.Height,
                base_aggregation.Pixel_Buffer_Initalizer.Channel_Count,
                base_aggregation.Pixel_Buffer_Initalizer.Internal_Format,
                base_aggregation.Pixel_Buffer_Initalizer.Pixel_Format
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

        EPOCH__TEXTURE__OUTPUT =
            new Texture
            (
                base_aggregation.Width, base_aggregation.Height,
                base_aggregation.Pixel_Buffer_Initalizer.Channel_Count,
                base_aggregation.Pixel_Buffer_Initalizer.Internal_Format,
                base_aggregation.Pixel_Buffer_Initalizer.Pixel_Format
            );

        GLHelper.Viewport.Push(0,0,EPOCH__TEXTURE__GENESIS.Width, EPOCH__TEXTURE__GENESIS.Height);
        SHADER__PASSTHROUGH.Process(base_aggregation, EPOCH__TEXTURE__GENESIS);
        SHADER__PASSTHROUGH.Process(base_aggregation, EPOCH__TEXTURES[0]);
        GLHelper.Viewport.Pop();
    }

    public void Rebase(Texture base_aggregation)
    {
        Clear();
        GLHelper.Viewport.Push(0,0,EPOCH__TEXTURE__GENESIS.Width, EPOCH__TEXTURE__GENESIS.Height);
        SHADER__PASSTHROUGH.Process(base_aggregation, EPOCH__TEXTURE__GENESIS);
        // SHADER__PASSTHROUGH.Process(base_aggregation, EPOCH__TEXTURES[0]);
        GLHelper.Viewport.Pop();
    }

    public void Buffer__Mouse_Position(Vector2 mouse_position)
    {
        if (!Is_Preparing__Value) return;
        if (!Is__Requiring_Mouse_Position_History) return;
        if (HISTORY__MOUSE_POSITION.Is_Needing__Consolidation)
        {
            bool error = false;
            Shader.Invocation clone = Preparing__Value.Clone();
            Vertex_Array_Object aggregate_vao =
                HISTORY__MOUSE_POSITION.Aggregate__Epochs(ref error);
            Preparing__Value.VAO = aggregate_vao;
            Finish();
            Prepare(clone);
            HISTORY__MOUSE_POSITION.Clear();
        }

        HISTORY__MOUSE_POSITION.Append(mouse_position);
    }

    public override void Finish()
    {
        if (Is_Needing__Consolidation)
            Get__Consolidation();
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

    public Texture Aggregate__Epochs(ref bool error)
    {
        //TODO: handle null cases here properly.
        if (error) return null;

        GLHelper.Viewport.Push(0,0,EPOCH__TEXTURES[0].Width,EPOCH__TEXTURES[0].Height);
        
        int epoch_index = -1;
        int previous_epoch = -1;
        for(int i=0;i<Quantity_Of__Epochs;i++)
        {
            previous_epoch = epoch_index;
            epoch_index = Get__Index_From__Oldest_Epoch(i);
            IEpoch<Shader.Invocation> epoch = EPOCHS[epoch_index];

            if (epoch == null) continue;
            if (!epoch.Is__With_New_Record_Changes) continue;
            
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
        }
        Verify__Record_Changes();

        if (Is_Preparing__Value)
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

            SHADER__PASSTHROUGH.Process
            (
                EPOCH__TEXTURE__OVERLAY,
                EPOCH__TEXTURE__OUTPUT
            );
            GLHelper.Viewport.Pop();
            // return EPOCH__TEXTURE__OVERLAY;
            return EPOCH__TEXTURE__OUTPUT;
        }

        SHADER__PASSTHROUGH.Process
        (
            EPOCH__TEXTURES[Index__Current],
            EPOCH__TEXTURE__OUTPUT
        );
        GLHelper.Viewport.Pop();

        // return EPOCH__TEXTURES[Index__Current];
        return EPOCH__TEXTURE__OUTPUT;
    }

    public override IEpoch<Shader.Invocation> Get__Consolidation() 
    {
        // EPOCH__TEXTURE__GENESIS =
        //     EPOCH__TEXTURES[Index__Next_Epoch];
        // SHADER__PASSTHROUGH.Process(EPOCH__TEXTURES[Index__Next_Epoch], EPOCH__TEXTURE__GENESIS);

        GLHelper.Viewport.Push(0,0,EPOCH__TEXTURES[0].Width,EPOCH__TEXTURES[0].Height);
        EPOCH__TEXTURE__GENESIS =
            EPOCH__TEXTURES[Index__Next_Epoch];
        SHADER__PASSTHROUGH.Process(EPOCH__TEXTURES[Index__Next_Epoch], EPOCH__TEXTURE__GENESIS);
        // foreach(Shader.Invocation invocation in Epoch__Following__Currently_Indexed!.Active__Values)
        // {
        //     bool error = false;
        //     EPOCH__AGGREGATOR
        //         .Aggregate__Invocation
        //         (
        //             invocation,
        //             EPOCH__TEXTURE__GENESIS,
        //             ref error
        //         );
        // }
        GLHelper.Viewport.Pop();

        return base.Get__Consolidation();
    }

    protected override IEpoch<Shader.Invocation> Handle_New__Epoch()
    {
        EPOCH__TEXTURES[Index__Next_Epoch] = 
            new Texture
            (
                EPOCH__WIDTH,
                EPOCH__HEIGHT,
                EPOCH__CHANNEL_COUNT,
                EPOCH__PIXEL_INTERNAL_FORMAT,
                EPOCH__PIXEL_FORMAT
            );

        return new Epoch<Shader.Invocation>(Max_Quantity_Of__Records_Per_Epoch);
    }
}
