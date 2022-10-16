
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
        private readonly Dictionary<string, Shader.IUniform> UNIFORMS =
            new Dictionary<string, Shader.IUniform>();

        internal Uniform_Table() { }

        internal void Scan__Source(string source)
        {
            IEnumerable<string> uniforms = source.Split('\n').Where(s => s.IndexOf("uniform ") >= 0);

            foreach(string uniform_field in uniforms)
            {
                string[] tokens = 
                    uniform_field.Split(' ');

                string uniform_name = tokens[tokens.Length - 1].Split(';')[0];
                
                Shader.IUniform? uniform = null;

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
                Shader.IUniform? uniform =
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
            Shader.IUniform? uniform
        )
        {
            if (uniform == null) return;
            if (!UNIFORMS.ContainsKey(uniform_name))
                UNIFORMS.Add(uniform_name, uniform);
        }

        private Shader.IUniform? Private_Get__Uniform
        (
            string field_name, 
            string uniform_type,
            string? value = null,
            string? max = null,
            string? min = null
        )
        {
            Shader.IUniform uniform;

            switch(uniform_type)
            {
                default:
                    return null;
                case "int":
                    int parsed_value__int =
                        (value != null)
                        ? int.Parse(value)
                        : 0
                        ;
                    int parsed_max__int =
                        (max != null)
                        ? int.Parse(max)
                        : 0
                        ;
                    int parsed_min__int =
                        (min != null)
                        ? int.Parse(min)
                        : 0
                        ;
                    uniform =
                        (min == null && max == null)
                        ? new Shader.Uniform__Int(field_name, parsed_value__int)
                        : new Shader.Uniform__Int__Clamped(field_name, parsed_value__int, parsed_min__int, parsed_max__int);
                        ;
                    break;
                case "uint":
                    uint parsed_value__uint =
                        (value != null)
                        ? uint.Parse(value)
                        : 0
                        ;
                    uint parsed_max__uint =
                        (max != null)
                        ? uint.Parse(max)
                        : 0
                        ;
                    uint parsed_min__uint =
                        (min != null)
                        ? uint.Parse(min)
                        : 0
                        ;
                    uniform =
                        (min == null && max == null)
                        ? new Shader.Uniform__Unsigned_Int(field_name, parsed_value__uint)
                        : new Shader.Uniform__Unsigned_Int__Clamped(field_name, parsed_value__uint, parsed_min__uint, parsed_max__uint);
                        ;
                    break;
                case "float":
                    float parsed_value__float =
                        (value != null)
                        ? float.Parse(value)
                        : 0
                        ;
                    float parsed_max__float =
                        (max != null)
                        ? float.Parse(max)
                        : 0
                        ;
                    float parsed_min__float =
                        (min != null)
                        ? float.Parse(min)
                        : 0
                        ;
                    uniform =
                        (min == null && max == null)
                        ? new Shader.Uniform__Float(field_name, parsed_value__float)
                        : new Shader.Uniform__Float__Clamped(field_name, parsed_value__float, parsed_min__float, parsed_max__float);
                        ;
                    break;
                case "double":
                    double parsed_value__double =
                        (value != null)
                        ? double.Parse(value)
                        : 0
                        ;
                    double parsed_max__double =
                        (max != null)
                        ? double.Parse(max)
                        : 0
                        ;
                    double parsed_min__double =
                        (min != null)
                        ? double.Parse(min)
                        : 0
                        ;
                    uniform =
                        (min == null && max == null)
                        ? new Shader.Uniform__Double(field_name, parsed_value__double)
                        : new Shader.Uniform__Double__Clamped(field_name, parsed_value__double, parsed_min__double, parsed_max__double);
                        ;
                    break;
                case "vec2":
                    uniform = new Shader.Uniform__Vector2(field_name, new Vector2());
                    break;
            }

            return uniform;
        }

        public Shader.Invocation Get__As_Invocation
        (
            Shader shader
        )
        {
            Dictionary<string, Shader.IUniform> uniform1__int = new Dictionary<string, Shader.IUniform>();
            Dictionary<string, Shader.IUniform> uniform1__uint = new Dictionary<string, Shader.IUniform>();
            Dictionary<string, Shader.IUniform> uniform1__float = new Dictionary<string, Shader.IUniform>();
            Dictionary<string, Shader.IUniform> uniform1__double = new Dictionary<string, Shader.IUniform>();
            Dictionary<string, Shader.IUniform> uniform2__vector2 = new Dictionary<string, Shader.IUniform>();

            Private_Add__To_Dictionary__Any_Uniforms_That_Is<int>
            (
                UNIFORMS,
                uniform1__int
            );
            Private_Add__To_Dictionary__Any_Uniforms_That_Is<uint>
            (
                UNIFORMS,
                uniform1__uint
            );
            Private_Add__To_Dictionary__Any_Uniforms_That_Is<float>
            (
                UNIFORMS,
                uniform1__float
            );
            Private_Add__To_Dictionary__Any_Uniforms_That_Is<double>
            (
                UNIFORMS,
                uniform1__double
            );
            Private_Add__To_Dictionary__Any_Uniforms_That_Is<Vector2>
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

        private void Private_Add__To_Dictionary__Any_Uniforms_That_Is<T>
        (
            Dictionary<string, Shader.IUniform> source_collection,
            Dictionary<string, Shader.IUniform> target_dict
        )
        where T : struct
        {
            foreach
            (
                Shader.IUniform<T> uniform 
                in 
                source_collection
                    .Values
                    .Where(o => o is Shader.IUniform<T>)
                    .Cast<Shader.IUniform<T>>()
            )
                target_dict.Add(uniform.Name, uniform);
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
