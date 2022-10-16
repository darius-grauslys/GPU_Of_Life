
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public partial class Shader
{
    public class Invocation
    {
        public readonly Shader Shader;
        public Vertex_Array_Object VAO;
        public int Primtive__Count;

        public Uniform__Vector2 Mouse_Position__Origin =
            new Uniform__Vector2("mouse_origin", new Vector2(-1));
        public Uniform__Vector2 Mouse_Position__Latest =
            new Uniform__Vector2("mouse_position", new Vector2(-1));

        public Dictionary<string, IUniform>? Uniform1__Int          { get; }
        public Dictionary<string, IUniform>? Uniform1__Unsigned_Int { get; }
        public Dictionary<string, IUniform>? Uniform1__Float        { get; }
        public Dictionary<string, IUniform>? Uniform1__Double       { get; }

        public Dictionary<string, IUniform>? Uniform2__Vector2      { get; }
        public Dictionary<string, IUniform>? Uniform2__Vector2i     { get; }

        public Dictionary<string, IUniform>? Uniform__Matrix4   { get; }

        public Invocation
        (
            Shader shader,

            Dictionary<string, IUniform>?
                uniform1__int = null,
            Dictionary<string, IUniform>?
                uniform1__uint = null,
            Dictionary<string, IUniform>?
                uniform1__float = null,
            Dictionary<string, IUniform>?
                uniform1__double = null,

            Dictionary<string, IUniform>?
                uniform2__vec2 = null,
            Dictionary<string, IUniform>?
                uniform2__ivec2 = null,

            Dictionary<string, IUniform>?
                uniform__mat4 = null
        )
        {

            Shader                 = shader;

            Uniform1__Int          = 
                (uniform1__int != null)
                ? new Dictionary<string, IUniform>(uniform1__int)
                : null
                ;
            Uniform1__Unsigned_Int = 
                (uniform1__uint != null)
                ? new Dictionary<string, IUniform>(uniform1__uint)
                : null
                ;
            Uniform1__Float        = 
                (uniform1__float != null)
                ? new Dictionary<string, IUniform>(uniform1__float)
                : null
                ;
            Uniform1__Double       = 
                (uniform1__double != null)
                ? new Dictionary<string, IUniform>(uniform1__double)
                : null
                ;
                                   
            Uniform2__Vector2      = 
                (uniform2__vec2 != null)
                ? new Dictionary<string, IUniform>(uniform2__vec2)
                : null
                ;
            Uniform2__Vector2i     = 
                (uniform2__ivec2 != null)
                ? new Dictionary<string, IUniform>(uniform2__ivec2)
                : null
                ;
                                   
            Uniform__Matrix4   = 
                (uniform__mat4 != null)
                ? new Dictionary<string, IUniform>(uniform__mat4)
                : null
                ;
        }

        public void Set__Uniform(IUniform uniform)
        {
            if (uniform is IUniform<int>)
                Private__Set__Uniform(Uniform1__Int, uniform);
            if (uniform is IUniform<uint>)
                Private__Set__Uniform(Uniform1__Unsigned_Int, uniform);
            if (uniform is IUniform<float>)
                Private__Set__Uniform(Uniform1__Float, uniform);
            if (uniform is IUniform<double>)
                Private__Set__Uniform(Uniform1__Double, uniform);

            if (uniform is IUniform<Vector2>)
                Private__Set__Uniform(Uniform2__Vector2, uniform);
            if (uniform is IUniform<Vector2i>)
                Private__Set__Uniform(Uniform2__Vector2i, uniform);

            if (uniform is IUniform<Matrix4>)
                Private__Set__Uniform(Uniform__Matrix4, uniform);
        }

        private void Private__Set__Uniform(Dictionary<string, Shader.IUniform>? dictionary, Shader.IUniform uniform)
        {
                if (dictionary?.Remove(uniform.Name) ?? false)
                    dictionary.Add(uniform.Name, uniform);
        }

        public Invocation Clone()
        {
            Invocation invocation_clone =
            new Invocation
            (
                Shader,

                Uniform1__Int,
                Uniform1__Unsigned_Int,
                Uniform1__Float,
                Uniform1__Double,
                                       
                Uniform2__Vector2,
                Uniform2__Vector2i,
                                       
                Uniform__Matrix4
            );

            invocation_clone.Mouse_Position__Origin =
                Mouse_Position__Origin;
            invocation_clone.Mouse_Position__Latest =
                Mouse_Position__Latest;

            return invocation_clone;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = 
                new System.Text.StringBuilder();

            sb.Append($"mouse: {Mouse_Position__Origin.Internal__Value} -- {Mouse_Position__Latest.Internal__Value}");
            sb.Append($" vao: {VAO.VAO_Handle}:{VAO.VBO_Handle}");

            return sb.ToString();
        }
    }

    public int PROGRAM_HANDLE { get; private set; }

    public Shader()
    {
        PROGRAM_HANDLE = GL.CreateProgram();
    }

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

    public virtual void Set__Uniform<T>(string uniform_name, T value)
    {
        Type uniform_type = typeof(T);
        int location = Get__Uniform(uniform_name);

        if      (uniform_type == typeof(int)    ) GL.Uniform1(location, (value as int?) ?? 0);
        else if (uniform_type == typeof(uint)   ) GL.Uniform1(location, (value as uint?) ?? 0);
        else if (uniform_type == typeof(float)  ) GL.Uniform1(location, (value as float?) ?? 0);
        else if (uniform_type == typeof(double) ) GL.Uniform1(location, (value as double?) ?? 0);
        else if (uniform_type == typeof(Vector2)) GL.Uniform2(location, (value as Vector2?) ?? new Vector2());

        else throw new ArgumentException($"Uniforms of type: {uniform_type} are not currently supported.");
    }

    public void Set__Uniform<T>(IUniform<T> uniform)
    where T : struct
    {
        Set__Uniform<T>(uniform.Name, uniform.Value);
    }

    public void Set__Uniform(IUniform uniform)
    {
        if      (uniform.GetType().IsAssignableTo(typeof(IUniform<int>))    )
            Set__Uniform((uniform as IUniform<int>)!);
        else if (uniform.GetType().IsAssignableTo(typeof(IUniform<uint>))   )
            Set__Uniform((uniform as IUniform<uint>)!);
        else if (uniform.GetType().IsAssignableTo(typeof(IUniform<float>))  )
            Set__Uniform((uniform as IUniform<float>)!);
        else if (uniform.GetType().IsAssignableTo(typeof(IUniform<double>)) )
            Set__Uniform((uniform as IUniform<double>)!);
        else if (uniform.GetType().IsAssignableTo(typeof(IUniform<Vector2>)))
            Set__Uniform((uniform as IUniform<Vector2>)!);

        else throw 
            new ArgumentException
            (
                $"Uniforms of type: {uniform.GetType()} are not currently supported."
            );
    }

    public class Builder :
    Builder<Shader>
    { }

    public class Builder<TShader>
    where TShader : Shader, new()
    {
        [AllowNull]
        private Shader shader;
        private List<int> handles = new List<int>();

        public Builder<TShader> Begin()
        {
            if (shader != null) return this;
            if (handles.Count != 0) return this;

            shader = new Shader(GL.CreateProgram());

            return this;
        }

        public Builder<TShader> Add__Shader(ShaderType shader_type, string source)
        {
            string error_message;
            bool error = false;
            Add__Shader(shader_type, source, ref error, out error_message);

            if (error)
                throw new AttachShaderException(error_message);

            return this;
        }

        public Builder<TShader> Add__Shader(ShaderType shader_type, string source, ref bool error)
            => Add__Shader(shader_type, source, ref error, out _);

        public Builder<TShader> Add__Shader(ShaderType shader_type, string source, ref bool error, out string error_message)
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

        public Builder<TShader> Add__Shader_From_File(ShaderType shader_type, string path)
        {
            string source = File.ReadAllText(path);

            return Add__Shader(shader_type, source);
        }

        public Builder<TShader> Add__Shader_From_File(ShaderType shader_type, string path, ref bool error)
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
