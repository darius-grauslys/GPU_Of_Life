
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using YamlDotNet.Serialization;

namespace GPU_Of_Life;

public class Tool
{
    public class Configuration
    {
        public struct Uniform
        {
            public string Type { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
            public string? Min { get; set; }
            public string? Max { get; set; }
        }

        public string Name { get; set; } = "Tool";
        public bool Is__Requiring__Mouse_Position_History { get; set; }

        public List<Uniform> Uniforms { get; set; } =
            new List<Uniform>();
    }

    public class Uniform_Table
    {
        private readonly List<object> UNIFORMS =
            new List<object>();

        internal Uniform_Table() { }

        internal void Scan__Source(string source)
        {
            IEnumerable<string> uniforms = source.Split('\n').Where(s => s.IndexOf("uniform ") >= 0);

            foreach(string uniform_field in uniforms)
            {
                string[] tokens = 
                    uniform_field.Split(' ');

                string uniform_name = tokens[tokens.Length - 1].Split(';')[0];
                
                object? uniform = null;

                foreach(string token in tokens)
                {
                    if (uniform != null) break;
                    uniform = Private_Get__Uniform(uniform_name, token);
                }

                Private_Record__Uniform(uniform_name, uniform);
            }
        }

        public void Scan__Configuration(Configuration configuration)
        {
            foreach
            (
                Configuration.Uniform config_uniform in configuration.Uniforms
            )
            {
                object? uniform =
                    Private_Get__Uniform
                    (
                        config_uniform.Name,
                        config_uniform.Type,
                        config_uniform.Value,
                        config_uniform.Max,
                        config_uniform.Min
                    );

                Private_Record__Uniform(config_uniform.Name, uniform);
            }
        }

        private void Private_Record__Uniform
        (
            string uniform_name,
            object? uniform
        )
        {
            if (uniform == null) return;
            UNIFORMS.Add(uniform);
        }

        private object? Private_Get__Uniform
        (
            string field_name, 
            string uniform_type,
            string? value = null,
            string? max = null,
            string? min = null
        )
        {
            object uniform;

            switch(uniform_type)
            {
                default:
                    return null;
                case "int":
                    uniform =
                        (min == null || max == null)
                        ? new Shader.Uniform<int>(field_name, value != null ? int.Parse(value) : 0)
                        : Shader.Parse__Uniform__Clamped_Int(field_name, value ?? "0", min, max);
                        ;
                    break;
                case "uint":
                    uniform = 
                        (min == null || max == null)
                        ? new Shader.Uniform<uint>(field_name, value != null ? uint.Parse(value) : 0)
                        : Shader.Parse__Uniform__Clamped_Unsigned_Int(field_name, value ?? "0", min, max);
                        ;
                    break;
                case "float":
                    uniform = 
                        (min == null || max == null)
                        ? new Shader.Uniform<float>(field_name, value != null ? float.Parse(value) : 0)
                        : Shader.Parse__Uniform__Clamped_Float(field_name, value ?? "0", min, max)
                        ;
                    break;
                case "double":
                    uniform = 
                        (min == null || max == null)
                        ? new Shader.Uniform<double>(field_name, value != null ? double.Parse(value) : 0)
                        : Shader.Parse__Uniform__Clamped_Double(field_name, value ?? "0", min, max)
                        ;
                    break;
                case "vec2":
                    uniform = new Shader.Uniform<Vector2>(field_name, new Vector2());
                    break;
            }

            return uniform;
        }

        public Shader.Invocation Get__As_Invocation
        (
            Shader shader
        )
        {
            List<Shader.IUniform<int>> uniform1__int = new List<Shader.IUniform<int>>();
            List<Shader.IUniform<uint>> uniform1__uint = new List<Shader.IUniform<uint>>();
            List<Shader.IUniform<float>> uniform1__float = new List<Shader.IUniform<float>>();
            List<Shader.IUniform<double>> uniform1__double = new List<Shader.IUniform<double>>();
            List<Shader.IUniform<Vector2>> uniform2__vector2 = new List<Shader.IUniform<Vector2>>();

            Private_Add__Any_Uniforms_That_Is__To_List
            (
                UNIFORMS,
                uniform1__int
            );
            Private_Add__Any_Uniforms_That_Is__To_List
            (
                UNIFORMS,
                uniform1__uint
            );
            Private_Add__Any_Uniforms_That_Is__To_List
            (
                UNIFORMS,
                uniform1__float
            );
            Private_Add__Any_Uniforms_That_Is__To_List
            (
                UNIFORMS,
                uniform1__double
            );
            Private_Add__Any_Uniforms_That_Is__To_List
            (
                UNIFORMS,
                uniform2__vector2
            );
            
            return new Shader.Invocation
                (
                    shader,

                    uniform1__int,
                    uniform1__uint,
                    uniform1__float,
                    uniform1__double,
                    uniform2__vector2,
                    null
                );
        }

        private void Private_Add__Any_Uniforms_That_Is__To_List<T>
        (
            List<object> source_collection,
            List<Shader.IUniform<T>> target_list
        )
        where T : struct
        {
            foreach
            (
                Shader.IUniform<T> uniform 
                in 
                UNIFORMS
                    .Where(o => o is Shader.IUniform<T>)
                    .Cast<Shader.IUniform<T>>()
            )
                target_list.Add(uniform);
        }
    }

    internal readonly Uniform_Table UNIFORM__TABLE =
        new Uniform_Table();

    private Configuration Tool__Configuration { get; }
    public string Name
        => Tool__Configuration.Name;
    public bool Is__Requiring__Mouse_Position_History
        => Tool__Configuration.Is__Requiring__Mouse_Position_History;
    public Shader Tool__Shader { get; }

    private Tool
    (
        Configuration config,
        Shader shader,
        string source__vert,
        string source__frag,
        string? source__geom = null
    )
    {
        Tool__Configuration = config;
        Tool__Shader = shader;

        UNIFORM__TABLE
            .Scan__Configuration(config);
        UNIFORM__TABLE
            .Scan__Source(source__vert);
        UNIFORM__TABLE
            .Scan__Source(source__frag);
        if (source__geom != null)
            UNIFORM__TABLE
                .Scan__Source(source__geom);
    }

    public Shader.Invocation Get__Invocation()
    {
        return UNIFORM__TABLE.Get__As_Invocation(Tool__Shader);
    }

    public static Tool Load(string folder_path)
    {
        if (!Directory.Exists(folder_path)) 
            throw new ArgumentException("Folder path does not exist.");

        IDeserializer deserializer =
            new DeserializerBuilder()
            .Build();

        Configuration? config;

        using (TextReader text_reader = File.OpenText(Path.Combine(folder_path, "tool.yaml")))
        {
            config =
                deserializer.Deserialize<Configuration>(text_reader);
        }

        if (config == null)
            throw new IOException("Failed to load tool.yaml configuration.");

        string[] file__vertex_shader   =
            Directory.GetFiles(folder_path, "*.vert");
        string[] file__geometry_shader =
            Directory.GetFiles(folder_path, "*.geom");
        string[] file__fragment_shader =
            Directory.GetFiles(folder_path, "*.frag");

        if (file__vertex_shader.Length == 0 || file__fragment_shader.Length == 0)
            throw new IOException("Tool folder does not have a .vert or .frag file.");

        string source__vert = File.ReadAllText(file__vertex_shader[0]);
        string source__frag = File.ReadAllText(file__fragment_shader[0]);
        string? source__geom = 
            file__geometry_shader.Length > 0
            ? File.ReadAllText(file__geometry_shader[0])
            : null
            ;

        Shader.Builder shader_builder =
            new Shader.Builder();

        shader_builder
        .Begin()
        .Add__Shader(ShaderType.VertexShader, source__vert)
        .Add__Shader(ShaderType.FragmentShader, source__frag);
        if (source__geom != null)
            shader_builder
                .Add__Shader(ShaderType.GeometryShader, source__geom);

        Tool tool = 
            new Tool
            (
                config, 
                shader_builder.Link(),
                source__vert,
                source__frag,
                source__geom
            );

        return tool;
    }
}
