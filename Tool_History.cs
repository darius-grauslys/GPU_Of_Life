
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Tool_History
{
    public class Tool_Invocation
    {
        internal bool Is__State__Undo { get; set; }
        internal bool Is__State__Dead_Undo { get; set; }

        public readonly Shader Tool__Shader;
        public int VAO__Handle;
        public int Primtive__Count;

        public class Uniform<T>
        where T : struct
        {
            public readonly string Uniform__Name;
            internal T Uniform__Value;

            public Uniform(string name, T value)
            {
                Uniform__Name = name;
                Uniform__Value = value;
            }
        }

        public IEnumerable<Uniform<int     >>? Uniform1__Int          { get; }
        public IEnumerable<Uniform<uint    >>? Uniform1__Unsigned_Int { get; }
        public IEnumerable<Uniform<float   >>? Uniform1__Float        { get; }
        public IEnumerable<Uniform<double  >>? Uniform1__Double       { get; }

        public IEnumerable<Uniform<Vector2 >>? Uniform2__Vector2      { get; }
        public IEnumerable<Uniform<Vector2i>>? Uniform2__Vector2i     { get; }

        public IEnumerable<Uniform<Matrix4 >>? UniformMat4__Matrix4   { get; }

        public Tool_Invocation
        (
            Shader tool__shader,

            IEnumerable<Uniform<int     >>?
                uniform1__int = null,                   
            IEnumerable<Uniform<uint    >>?
                uniform1__uint = null,                  
            IEnumerable<Uniform<float   >>?
                uniform1__float = null,                 
            IEnumerable<Uniform<double  >>?
                uniform1__double = null,                

            IEnumerable<Uniform<Vector2 >>?
                uniform1__vec2 = null,                  
            IEnumerable<Uniform<Vector2i>>?
                uniform1__ivec2 = null,                 

            IEnumerable<Uniform<Matrix4 >>?
                uniform1__mat4 = null
        )
        {
            Tool__Shader           = tool__shader;

            Uniform1__Int          = uniform1__int;
            Uniform1__Unsigned_Int = uniform1__uint;
            Uniform1__Float        = uniform1__float;
            Uniform1__Double       = uniform1__double;
                                   
            Uniform2__Vector2      = uniform1__vec2;
            Uniform2__Vector2i     = uniform1__ivec2;
                                   
            UniformMat4__Matrix4   = uniform1__mat4;
        }
    }

    protected internal class Invocation_Epoch
    {
        public Texture EPOCH__TEXTURE;

        public readonly Tool_Invocation[] EPOCH__HISTORY;
        public int EPOCH__HISTORY_INDEX { get; internal set; }
        public bool Is__Epoch_Finished
            => EPOCH__HISTORY_INDEX == EPOCH__HISTORY.Length || Is__Epoch_Empty;
        public bool Is__Epoch_Empty
            => EPOCH__HISTORY_INDEX <= 0;
        public bool Is__Epoch_Undone
            => EPOCH__HISTORY[0]?.Is__State__Undo ?? true;

        public IEnumerable<Tool_Invocation> Invocations
        {
            get 
            {
                for(int i=0;i<EPOCH__HISTORY_INDEX;i++)
                {
                    if (EPOCH__HISTORY[EPOCH__HISTORY_INDEX]?.Is__State__Undo ?? true) 
                        continue;
                    yield return EPOCH__HISTORY[EPOCH__HISTORY_INDEX];
                }
            }
        }

        public Invocation_Epoch(int epoch_size, int texture_width, int texture_height)
        {
            EPOCH__HISTORY = new Tool_Invocation[epoch_size];
            EPOCH__TEXTURE =
                new Texture(texture_width, texture_height);
        }

        public void Resize(int width, int height, ref bool error)
            => EPOCH__TEXTURE.Resize__Texture(width, height, ref error);

        public bool Append(Tool_Invocation tool_invocation)
        {
            if (Is__Epoch_Finished) return false;

            EPOCH__HISTORY[++EPOCH__HISTORY_INDEX] = tool_invocation;
            if (!Is__Epoch_Finished && EPOCH__HISTORY[EPOCH__HISTORY_INDEX + 1].Is__State__Undo)
                EPOCH__HISTORY[EPOCH__HISTORY_INDEX + 1].Is__State__Dead_Undo = true;

            return true;
        }

        public bool Undo()
        {
            if (Is__Epoch_Empty) return false;

            EPOCH__HISTORY[--EPOCH__HISTORY_INDEX].Is__State__Undo = true;

            return true;
        }

        public bool Redo()
        {
            if (Is__Epoch_Finished) return false;
            if (EPOCH__HISTORY[EPOCH__HISTORY_INDEX] == null) return false;
            if (EPOCH__HISTORY[EPOCH__HISTORY_INDEX + 1].Is__State__Dead_Undo) return false;

            EPOCH__HISTORY[EPOCH__HISTORY_INDEX++].Is__State__Undo = false;

            return true;
        }
    }

    private readonly int EPOCH__SIZE;
    private int GRID__WIDTH;
    private int GRID__HEIGHT;

    private readonly Invocation_Epoch[] TOOL_INVOCATION__HISTORY;
    private int TOOL_INVOCATION__HISTORY_INDEX = -1;

    private int Index__Next
        => 
        (TOOL_INVOCATION__HISTORY_INDEX + 1) 
        % 
        TOOL_INVOCATION__HISTORY.Length
        ;
    private int Index__Previous
        => 
        (
            (TOOL_INVOCATION__HISTORY_INDEX + 1)
            %
            TOOL_INVOCATION__HISTORY.Length
            +
            TOOL_INVOCATION__HISTORY.Length
        )
        %
        TOOL_INVOCATION__HISTORY.Length
        ;

    private Invocation_Epoch Currently_Indexed__Epoch
        => TOOL_INVOCATION__HISTORY[TOOL_INVOCATION__HISTORY_INDEX];
    private bool Is__Previous_Epoch_Existant
        => TOOL_INVOCATION__HISTORY[Index__Previous] != null;

    protected Invocation_Epoch Current__Epoch
    {
        get 
        {
            if (Currently_Indexed__Epoch.Is__Epoch_Undone && Is__Previous_Epoch_Existant)
            {
                Private_Move__Epoch_Index(is__progressing_forwards_or_backwards: false);
                return Currently_Indexed__Epoch;
            }
            if (Currently_Indexed__Epoch.Is__Epoch_Finished)
                Private_Move__Epoch_Index
                (
                    is__progressing_forwards_or_backwards: true, 
                    is__creating_new_epoch: true
                );

            return Currently_Indexed__Epoch;
        }
    }

    public Tool_History
    (
        int history_epoch_size, 
        int history_epoch_count
    )
    {
        EPOCH__SIZE = history_epoch_size;
        TOOL_INVOCATION__HISTORY =
            new Invocation_Epoch[history_epoch_count];
    }

    public virtual void Resize
    (
        int width, int height, 
        ref bool error
    )
    {
        //TODO: Corner case.
        //      what if for length N epoch array
        //      from 1->M st M < N, an epoch fails
        //      to resize? Issue: mismatching resize
        //      among epochs.
        for(int i=0;i<TOOL_INVOCATION__HISTORY.Length && !error;i++)
            TOOL_INVOCATION__HISTORY[i]?.Resize(width, height, ref error);
        if (!error)
        {
            GRID__WIDTH = width;
            GRID__HEIGHT = height;
        }
    }

    public virtual void Append(Tool_Invocation tool_invocation)
    {
        Current__Epoch.Append(tool_invocation);
    }

    public virtual void Undo()
    {
        Current__Epoch.Undo();
    }

    public virtual void Redo()
    {
        Current__Epoch.Redo();
    }

    private void Private_Move__Epoch_Index
    (
        bool is__progressing_forwards_or_backwards,
        bool is__creating_new_epoch = false
    )
    {
        TOOL_INVOCATION__HISTORY_INDEX =
            (is__progressing_forwards_or_backwards)
            ? Index__Next
            : Index__Previous
            ;

        if (is__creating_new_epoch)
            TOOL_INVOCATION__HISTORY[TOOL_INVOCATION__HISTORY_INDEX] =
                new Invocation_Epoch(EPOCH__SIZE, GRID__WIDTH, GRID__HEIGHT);
    }
}
