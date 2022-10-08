
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Shader
{
    public interface IUniform<T>
    where T : struct
    {
        public string Name { get; }
        public T Value { get; set; }
    }

    public struct Uniform<T> : IUniform<T>
    where T : struct
    {
        public string Name { get; }
        internal T Internal__Value;
        public T Value { get => Internal__Value; set => Internal__Value = value; }

        public Uniform(string name, T value)
        {
            Name = name;
            Internal__Value = value;
        }
    }

    public struct Uniform__Clamped<T> : IUniform<T>
    where T : struct
    {
        private readonly Func<T,T,int> Comparator;
        
        private T min, max;
        public T Min 
        {
            get => min; 
            set
            {
                int comparison =
                    Comparator.Invoke(value, max);
                min =
                    (comparison > 0)
                    ? max
                    : value
                    ;
            }
        }
        public T Max
        {
            get => max; 
            set
            {
                int comparison =
                    Comparator.Invoke(min, value);
                min =
                    (comparison > 0)
                    ? min
                    : value
                    ;
            }
        }

        private Uniform<T> Uniform;

        public string Name => Uniform.Name;
        public T Value
        {
            get => Uniform.Value;
            set
            {
                int comparison =
                    Comparator.Invoke(value, min);
                value =
                    (comparison < 0)
                    ? min
                    : value
                    ;
                comparison =
                    Comparator.Invoke(value, max);
                value =
                    (comparison > 0)
                    ? max
                    : value
                    ;
                Uniform.Value = value;
            }
        }

        public Uniform__Clamped
        (
            string name, 
            T value,
            T min, T max,
            Func<T,T,int> comparator
        )
        {
            Uniform = new Uniform<T>(name, value);
            if (comparator(min, max) > 0) min = max;
            this.min = min;
            this.max = max;
            Comparator = comparator;
            Value = value;
        }
    }

    public static Uniform__Clamped<int> Parse__Uniform__Clamped_Int
    (
        string name,
        string value,
        string min, string max
    )
    =>
        Uniform__Clamped_Int
        (
            name,
            int.Parse(value),
            int.Parse(min), int.Parse(max)
        );
    public static Uniform__Clamped<int> Uniform__Clamped_Int
    (
        string name,
        int value,
        int min, int max
    )
    => new Uniform__Clamped<int>
    (
        name, value, min, max,
        (value1, value2) => value1 - value2
    );

    public static Uniform__Clamped<uint> Parse__Uniform__Clamped_Unsigned_Int
    (
        string name,
        string value,
        string min, string max
    )
    =>
        Uniform__Clamped_Unsigned_Int
        (
            name,
            uint.Parse(value),
            uint.Parse(min), uint.Parse(max)
        );
    public static Uniform__Clamped<uint> Uniform__Clamped_Unsigned_Int
    (
        string name,
        uint value,
        uint min, uint max
    )
    => new Uniform__Clamped<uint>
    (
        name, value, min, max,
            (value1, value2) => 
                (value1 == value2)
                ? 0
                : (value1 < value2)
                    ? -1
                    : 1
    );

    public static Uniform__Clamped<float> Parse__Uniform__Clamped_Float
    (
        string name,
        string value,
        string min, string max
    )
    =>
        Uniform__Clamped_Float
        (
            name,
            float.Parse(value),
            float.Parse(min), float.Parse(max)
        );
    public static Uniform__Clamped<float> Uniform__Clamped_Float
    (
        string name,
        float value,
        float min, float max
    )
    => new Uniform__Clamped<float>
    (
        name, value, min, max,
            (value1, value2) => 
                (value1 == value2)
                ? 0
                : (value1 < value2)
                    ? -1
                    : 1
    );

    public static Uniform__Clamped<double> Parse__Uniform__Clamped_Double
    (
        string name,
        string value,
        string min, string max
    )
    =>
        Uniform__Clamped_Double
        (
            name,
            double.Parse(value),
            double.Parse(min), double.Parse(max)
        );
    public static Uniform__Clamped<double> Uniform__Clamped_Double
    (
        string name,
        double value,
        double min, double max
    )
    => new Uniform__Clamped<double>
    (
        name, value, min, max,
            (value1, value2) => 
                (value1 == value2)
                ? 0
                : (value1 < value2)
                    ? -1
                    : 1
    );

    public static Uniform__Clamped<Vector2> Uniform__Clamped_Vector2
    (
        string name,
        Vector2 value,
        Vector2 min, Vector2 max
    )
    => new Uniform__Clamped<Vector2>
    (
        name, value, min, max,
            (value1, value2) => 
                (value1 == value2)
                ? 0
                : (value1.X < value2.X || value1.Y < value2.Y)
                    ? -1
                    : 1
    );

    public class Invocation
    {
        public readonly Shader Shader;
        public int VAO__Handle;
        public int Primtive__Count;

        public Uniform<Vector2> Mouse_Position__Origin =
            new Uniform<Vector2>("mouse_origin", new Vector2(-1));
        public Uniform<Vector2> Mouse_Position__Latest =
            new Uniform<Vector2>("mouse_position", new Vector2(-1));

        public List<IUniform<int     >>? Uniform1__Int          { get; }
        public List<IUniform<uint    >>? Uniform1__Unsigned_Int { get; }
        public List<IUniform<float   >>? Uniform1__Float        { get; }
        public List<IUniform<double  >>? Uniform1__Double       { get; }

        public List<IUniform<Vector2 >>? Uniform2__Vector2      { get; }
        public List<IUniform<Vector2i>>? Uniform2__Vector2i     { get; }

        public List<IUniform<Matrix4 >>? UniformMat4__Matrix4   { get; }

        public Invocation
        (
            Shader shader,

            List<IUniform<int     >>?
                uniform1__int = null,
            List<IUniform<uint    >>?
                uniform1__uint = null,
            List<IUniform<float   >>?
                uniform1__float = null,
            List<IUniform<double  >>?
                uniform1__double = null,

            List<IUniform<Vector2 >>?
                uniform1__vec2 = null,
            List<IUniform<Vector2i>>?
                uniform1__ivec2 = null,

            List<IUniform<Matrix4 >>?
                uniform1__mat4 = null
        )
        {
            Shader                 = shader;

            Uniform1__Int          = uniform1__int?.ToList();
            Uniform1__Unsigned_Int = uniform1__uint?.ToList();
            Uniform1__Float        = uniform1__float?.ToList();
            Uniform1__Double       = uniform1__double?.ToList();
                                   
            Uniform2__Vector2      = uniform1__vec2?.ToList();
            Uniform2__Vector2i     = uniform1__ivec2?.ToList();
                                   
            UniformMat4__Matrix4   = uniform1__mat4?.ToList();
        }

        public Invocation Clone()
            => new Invocation
            (
                Shader,

                Uniform1__Int,
                Uniform1__Unsigned_Int,
                Uniform1__Float,
                Uniform1__Double,
                                       
                Uniform2__Vector2,
                Uniform2__Vector2i,
                                       
                UniformMat4__Matrix4
            );
    }

    public int PROGRAM_HANDLE { get; private set; }

    private Shader(int handle)
    {
        PROGRAM_HANDLE = handle;
    }

    public Shader
    (
        string vert,
        string frag,
        out bool error
    )
    {
        int handle_vert = GL.CreateShader(ShaderType.VertexShader);
        int handle_frag = GL.CreateShader(ShaderType.FragmentShader);

        GL.ShaderSource(handle_vert, vert);
        GL.ShaderSource(handle_frag, frag);
        GL.CompileShader(handle_vert);
        string err;
        Console.WriteLine("SHADER-VERT:\n{0}", err = GL.GetShaderInfoLog(handle_vert));
        error = err != string.Empty;
        GL.CompileShader(handle_frag);
        Console.WriteLine("SHADER-FRAG:\n{0}", err = GL.GetShaderInfoLog(handle_frag));
        error = error || err != string.Empty;
        PROGRAM_HANDLE = GL.CreateProgram();
        GL.AttachShader(PROGRAM_HANDLE, handle_vert);
        GL.AttachShader(PROGRAM_HANDLE, handle_frag);
        GL.LinkProgram(PROGRAM_HANDLE);
        GL.DeleteShader(handle_vert);
        GL.DeleteShader(handle_frag);
    }

    public void Use()
    {
        GL.UseProgram(PROGRAM_HANDLE);
    }

    public int Get__Uniform(string uniform_name)
        => GL.GetUniformLocation(PROGRAM_HANDLE, uniform_name);

    public void Set__Uniform<T>(Uniform<T> uniform)
    where T : struct
    {
        Type uniform_type = typeof(T);

        if      (uniform_type == typeof(int))
        {
            GL.Uniform1(PROGRAM_HANDLE, (uniform as Uniform<int>?)?.Value ?? 0);
        }
        else if (uniform_type == typeof(uint))
        {
            GL.Uniform1(PROGRAM_HANDLE, (uniform as Uniform<uint>?)?.Value ?? 0);
        }
        else if (uniform_type == typeof(float))
        {
            GL.Uniform1(PROGRAM_HANDLE, (uniform as Uniform<float>?)?.Value ?? 0);
        }
        else if (uniform_type == typeof(double))
        {
            GL.Uniform1(PROGRAM_HANDLE, (uniform as Uniform<double>?)?.Value ?? 0);
        }
        else if (uniform_type == typeof(Vector2))
        {
            GL.Uniform2(PROGRAM_HANDLE, (uniform as Uniform<Vector2>?)?.Value ?? new Vector2());
        }
        else
        {
            throw new ArgumentException($"Uniforms of type: {uniform_type} are not currently supported.");
        }
    }

    public class Builder
    {
        [AllowNull]
        private Shader shader;
        private List<int> handles = new List<int>();

        public Builder Begin()
        {
            if (shader != null) return this;
            if (handles.Count != 0) return this;

            shader = new Shader(GL.CreateProgram());

            return this;
        }

        public Builder Add__Shader(ShaderType shader_type, string source)
        {
            string error_message;
            bool error = false;
            Add__Shader(shader_type, source, ref error, out error_message);

            if (error)
                throw new AttachShaderException(error_message);

            return this;
        }

        public Builder Add__Shader(ShaderType shader_type, string source, ref bool error)
            => Add__Shader(shader_type, source, ref error, out _);

        public Builder Add__Shader(ShaderType shader_type, string source, ref bool error, out string error_message)
        {
            error_message = "";
            if (error) return this;

            int handle = GL.CreateShader(shader_type);
            GL.ShaderSource(handle, source);
            GL.CompileShader(handle);
            error_message = GL.GetShaderInfoLog(handle);
            error = error_message != string.Empty;

            if (error)
            {
                Console.WriteLine("SHADER[{0}]: \n{1}", shader_type, error_message);
                return this;
            }

            handles.Add(handle);
            GL.AttachShader(shader.PROGRAM_HANDLE, handle);
            return this;
        }

        public Builder Add__Shader_From_File(ShaderType shader_type, string path)
        {
            string source = File.ReadAllText(path);

            return Add__Shader(shader_type, source);
        }

        public Builder Add__Shader_From_File(ShaderType shader_type, string path, ref bool error)
        {
            if (error) return this;
            if (!File.Exists(path)) 
            {
                error = true; 
                Console.WriteLine($"Shader-Factory: Failed to load file for {shader_type}, {path} does not exist.");
                return this; 
            }

            string source;
            try
            {
                source = File.ReadAllText(path);
            }
            catch(Exception e)
            {
                error = true;
                Console.WriteLine($"Shader-Factory: IO Exception.\n{e}");
                return this;
            }

            Add__Shader(shader_type, source, ref error);

            return this;
        }

        public Shader Link()
        {
            bool error = false;
            Shader shader = Link(ref error);

            if (error)
                throw new LinkShaderException(GL.GetProgramInfoLog(shader.PROGRAM_HANDLE));

            return shader;
        }

        public Shader Link(ref bool error)
        {
            if (error) return new Shader(0);

            GL.LinkProgram(shader.PROGRAM_HANDLE);
            foreach(int handle in handles)
            {
                GL.DetachShader(shader.PROGRAM_HANDLE, handle);
                GL.DeleteShader(handle);
            }
            return shader;
        }

        public class AttachShaderException : Exception
        {
            public AttachShaderException(string shader_log)
            : base(shader_log) { }
        }

        public class LinkShaderException : Exception
        {
            public LinkShaderException(string shader_log)
            : base(shader_log) { }
        }
    }
}
