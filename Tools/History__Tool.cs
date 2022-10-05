
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Tool_Invocation
{
    public readonly Shader Tool__Shader;
    public int VAO__Handle;
    public int Primtive__Count;

    public Uniform<Vector2> Mouse_Position__Origin =
        new Uniform<Vector2>("mouse_origin", new Vector2(-1));
    public Uniform<Vector2> Mouse_Position__Latest =
        new Uniform<Vector2>("mouse_position", new Vector2(-1));

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

public class History__Tool_Invocation
: History<Tool_Invocation, Texture>
{
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
    {
    }

    public override Texture Aggregate__Epochs()
    {
        throw new NotImplementedException();
    }
}
