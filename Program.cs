
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.OpenTk;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using KeyModifiers = OpenTK.Windowing.GraphicsLibraryFramework.KeyModifiers;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using StbImageSharp;
using System.Drawing;
using System.Drawing.Imaging;

namespace GPU_Of_Life;

public class Program : Test__Window //GameWindow 
{
    private Viewport BASE__VIEWPORT;

    private readonly IGwenGui _gwen_gui;    
    [AllowNull]
    private Gwen_UI UI;

    private Shader SHADER__COMPUTE;
    [AllowNull]
    private Shader SHADER__DRAW;

    [AllowNull]
    private History__Tool_Invocation HISTORY__TOOL_INVOCATION;
    private readonly Tool__Repository TOOL__REPOSITORY =
        new Tool__Repository();

    private int VAO__CELL_POINTS;
    private int VBO__CELL_POINTS;

    private Grid_Configuration GRID__CONFIGURATION;
    private int GRID__WIDTH = 10, GRID__HEIGHT = 10;
    private int CELL__COUNT => GRID__WIDTH * GRID__HEIGHT;
    private int GRID__FRAMEBUFFER__COMPUTE;
    private int GRID__INDEX_COMPUTE = 0;

    private Camera_2D GRID__CAMERA = new Camera_2D();

    [AllowNull]
    private Texture GRID__TEXTURE__BASE;
    [AllowNull]
    private Texture GRID__TEXTURE0;
    [AllowNull]
    private Texture GRID__TEXTURE1;

    [AllowNull]
    private Texture GRID__TEXTURE__READ;
    [AllowNull]
    private Texture GRID__TEXTURE__WRITE;

    private bool GRID__IS_ACTIVE   = false;
    private bool GRID__IS_STEPPING = false;
    private bool GRID__IS_INITIAL  = true;
    private double GRID__DELAY__MS = 0;
    private const double GRID__DELAY__SEC_INTERVAL = 0.5;

    private Vector2i GRID__POSITION;
    private Vector2i GRID__SIZE;

    private double TIME__ELAPSED;

    private bool Is__UI_Busy  = false;

    public Program() 
    //: base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        GRID__FRAMEBUFFER__COMPUTE = GL.GenFramebuffer();

        BASE__VIEWPORT = new Viewport(Size);
        GLHelper.Push_Viewport(BASE__VIEWPORT);

        _gwen_gui =
            GwenGuiFactory
            .CreateFromGame
            (
                this,
                GwenGuiSettings
                .Default
                .From
                (
                    settings =>
                    settings.SkinFile =
                    new System.IO.FileInfo("DefaultSkin2.png")
                )
            );

        string? err = Private_Load__Grid_Compute_Shader();

        if (err != null) { Close(); return; }

        err = Private_Load__Grid_Shader();

        if (err != null) { Close(); return; }

        Private_Establish__Grid();
    }

    protected override void OnKeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch(e.Key)
        {
            case Keys.F1:
                Private_Reset__Grid();
                break;
            case Keys.F5:
                GRID__IS_ACTIVE = !GRID__IS_ACTIVE;
                break;
            case Keys.F6:
                GRID__IS_STEPPING = true;
                break;
            case Keys.U:
                if (e.Modifiers == KeyModifiers.Control || e.Control)
                    HISTORY__TOOL_INVOCATION.Undo();
                    //Private_Undo__Tool();
                break;
            case Keys.R:
                if (e.Modifiers == KeyModifiers.Control || e.Control)
                    HISTORY__TOOL_INVOCATION.Redo();
                    //Private_Redo__Tool();
                break;
        }
    }

    protected override void OnLoad()
    {
        _gwen_gui.Load();
        _gwen_gui.Resize(Size);
        UI = new Gwen_UI(_gwen_gui.Root);
        _gwen_gui.Render();

        GRID__CAMERA.Resize__Focal_Size(new Vector2(UI.GRID__WIDTH, UI.GRID__HEIGHT), Size);

        TOOL__REPOSITORY.Load__Tool(Path.Combine(Directory.GetCurrentDirectory(), "GPU_Programs/Tools/Core/Quad_Space/TOOL__Pencil/"));
        TOOL__REPOSITORY.Load__Tool(Path.Combine(Directory.GetCurrentDirectory(), "GPU_Programs/Tools/Core/Quad_Space/TOOL__Rectangle/"));
        TOOL__REPOSITORY.Load__Tool(Path.Combine(Directory.GetCurrentDirectory(), "GPU_Programs/Tools/Core/Quad_Space/TOOL__Line/"));
        TOOL__REPOSITORY.Load__Tool(Path.Combine(Directory.GetCurrentDirectory(), "GPU_Programs/Tools/Core/Quad_Space/TOOL__Full_Rectangle/"));
        TOOL__REPOSITORY.Load__Tool(Path.Combine(Directory.GetCurrentDirectory(), "GPU_Programs/Tools/Core/Quad_Space/TOOL__Star/"));
        //TOOL__REPOSITORY.Load__Tool(Path.Combine(Directory.GetCurrentDirectory(), "GPU_Programs/Tools/Core/Quad_Space/TOOL__Full_Star/"));

        foreach(Tool tool in TOOL__REPOSITORY.TOOLS)
            UI.Load__Tool(tool);

        UI.Status__UI_Busy += status => Is__UI_Busy = status;

        UI.Updated__Tool_Selection +=
            (tool_name) => 
            {
                TOOL__REPOSITORY.Set__Active_Tool(tool_name);
                HISTORY__TOOL_INVOCATION.Is__Requiring_Mouse_Position_History =
                    TOOL__REPOSITORY.TOOL__ACTIVE!.Is__Requiring__Mouse_Position_History;
                UI.Select__Tool(TOOL__REPOSITORY.TOOL__ACTIVE?.Get__Invocation());
            };

        UI.Loaded__Grid_Shader +=  p => Private_Load__Grid_Shader(p);
        UI.Loaded__Grid_Compute_Shader +=  p => Private_Load__Grid_Compute_Shader(p);
        UI.Loaded__Tool   +=  p => Private_Load__Tool(p);

        UI.Invoked__Load  +=  c => Private_Establish__Grid(c);
        UI.Invoked__New   +=  c => Private_Establish__Grid(c);
        UI.Invoked__Save  +=  s => Private_Save__Grid(s);
        UI.Invoked__Reset += () => Private_Reset__Grid();
        UI.Toggle__Run    +=  b => GRID__IS_ACTIVE = b;
        UI.Pulsed__Step   += () => GRID__IS_STEPPING = true;

        UI.Render__Grid += Private_Render__Simulation_Space;

        UI.Updated__Compute_Speed += 
            (speed_level) => 
            { 
                GRID__DELAY__MS = GRID__DELAY__SEC_INTERVAL * speed_level + 0.1; 
                GRID__DELAY__MS *= GRID__DELAY__MS; 
            };

        UI.Updated__Tool_Uniform +=
            (uniform) =>
            {
                TOOL__REPOSITORY?
                    .TOOL__ACTIVE__CONFIGURATION?
                    .Set__Uniform(uniform);
            };

        //UI.Updated__Stencil_Value +=
        //    (stencil_value) => TOOL__STENCIL__VALUE = stencil_value;
    }

    protected override void OnResize(OpenTK.Windowing.Common.ResizeEventArgs e)
    {
        base.OnResize(e);
        _gwen_gui.Resize(Size);
        _gwen_gui.Root.DoLayout();
        BASE__VIEWPORT.Resize(Size);
        GRID__CAMERA.Resize__Focal_Size(new Vector2(UI.GRID__WIDTH, UI.GRID__HEIGHT), Size);
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        TIME__ELAPSED += args.Time;
        GL.Clear(ClearBufferMask.ColorBufferBit);
        //gwen gui makes InvalidEnum error.
        //ruled out my render override causing it.
        _gwen_gui.Render();
        SwapBuffers();
    }

    Vector2? mouse__last_frame;
    protected override void OnUpdateFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        GRID__CAMERA.Process__Update
        (
            MouseState,
            Size,
            MousePosition,
            args
        );
        Private_Process__Tool();
    }

    protected override void OnMouseWheel(OpenTK.Windowing.Common.MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        GRID__CAMERA.Process__Scroll(e);
    }

    private string? Private_Load__Tool(string path)
    {
        try
        {
            Tool tool = TOOL__REPOSITORY.Load__Tool(path);
            UI.Load__Tool(tool);
        }
        catch (Exception e)
        {
            return e.Message;
        }
        return null;
    }

    public const string SHADER__DRAW__FILE_NAME_VERTEX =
        "Shader_Draw__Vertex.vert";
    public const string SHADER__DRAW__FILE_NAME_GEOMETRY =
        "Shader_Draw__Geometry.geom";
    public const string SHADER__DRAW__FILE_NAME_FRAGMENT =
        "Shader_Draw__Fragmentation.frag";
    private string? Private_Load__Grid_Shader(string? path = null)
    {
        string path_vert =
            (path != null)
            ? Path.Combine(path, SHADER__DRAW__FILE_NAME_VERTEX)
            : Path.Combine(Directory.GetCurrentDirectory(), SHADER__DRAW__FILE_NAME_VERTEX)
            ;
        string path_geom =
            (path != null)
            ? Path.Combine(path, SHADER__DRAW__FILE_NAME_GEOMETRY)
            : Path.Combine(Directory.GetCurrentDirectory(), SHADER__DRAW__FILE_NAME_GEOMETRY)
            ;
        string path_frag =
            (path != null)
            ? Path.Combine(path, SHADER__DRAW__FILE_NAME_FRAGMENT)
            : Path.Combine(Directory.GetCurrentDirectory(), SHADER__DRAW__FILE_NAME_FRAGMENT)
            ;
        bool err = false;
        Shader shader =
            new Shader.Builder()
            .Begin()
            .Add__Shader_From_File(ShaderType.VertexShader, path_vert, ref err)
            .Add__Shader_From_File(ShaderType.GeometryShader, path_geom, ref err)
            .Add__Shader_From_File(ShaderType.FragmentShader, path_frag, ref err)
            .Link()
            ;

        if (!err) SHADER__DRAW = shader;
        return (err) ? "An error has occured." : null;
    }

    public const string SHADER__COMPUTE__FILE_NAME_VERTEX =
        "Shader_Compute__Vertex.vert";
    public const string SHADER__COMPUTE__FILE_NAME_FRAGMENT =
        "Shader_Compute__Fragmentation.frag";
    private string? Private_Load__Grid_Compute_Shader(string? path = null)
    {
        string path_vert =
            (path != null)
            ? Path.Combine(path, SHADER__COMPUTE__FILE_NAME_VERTEX)
            : Path.Combine(Directory.GetCurrentDirectory(), SHADER__COMPUTE__FILE_NAME_VERTEX)
            ;
        string path_frag =
            (path != null)
            ? Path.Combine(path, SHADER__COMPUTE__FILE_NAME_FRAGMENT)
            : Path.Combine(Directory.GetCurrentDirectory(), SHADER__COMPUTE__FILE_NAME_FRAGMENT)
            ;
        bool err = false;
        Shader shader =
            new Shader.Builder()
            .Begin()
            .Add__Shader_From_File(ShaderType.VertexShader, path_vert, ref err)
            .Add__Shader_From_File(ShaderType.FragmentShader, path_frag, ref err)
            .Link()
            ;

        if (!err) SHADER__COMPUTE = shader;
        return (err) ? "An error has occured." : null;
    }

    Vector2 mouse_previous = new Vector2(-1);
    Vector2 mouse_origin   = new Vector2(-1);

    private void Private_Process__Tool()
    {
        if (Is__UI_Busy) return;
        if (TOOL__REPOSITORY.TOOL__ACTIVE == null) return;
        if (MousePosition.X < UI.GRID__X || MousePosition.X > UI.GRID__X + UI.GRID__WIDTH)  return;
        if (MousePosition.Y < UI.GRID__Y || MousePosition.Y > UI.GRID__Y + UI.GRID__HEIGHT) return;

        if (!MouseState.IsButtonDown(MouseButton.Left))
        {
            if (HISTORY__TOOL_INVOCATION.Is__Preparing__Value)
            {
                HISTORY__TOOL_INVOCATION.Finish();
            }
            mouse_origin = new Vector2(-1);
            mouse_previous = new Vector2(-1);
            return; 
        }

        Vector2 mouse_position =
            new Vector2
            (
                MousePosition.X - UI.GRID__X,
                Size.Y - (MousePosition.Y + UI.GRID__Y)
            );

        if (mouse_previous == mouse_position) return;
        mouse_previous = mouse_position;

        Vector4 tool_position = GRID__CAMERA.Get__Mouse_To_World(mouse_position);
        if (mouse_origin.X == -1) mouse_origin = tool_position.Xy;

        if(TOOL__REPOSITORY.TOOL__ACTIVE.Is__Requiring__Mouse_Position_History)
        {
            HISTORY__TOOL_INVOCATION.Buffer__Mouse_Position(tool_position.Xy);
            if (mouse_origin.X == -1) // do it again
                HISTORY__TOOL_INVOCATION.Buffer__Mouse_Position(tool_position.Xy);
        }

        if (!HISTORY__TOOL_INVOCATION.Is__Preparing__Value)
        {
            Shader.Invocation invocation =
                TOOL__REPOSITORY.TOOL__ACTIVE__CONFIGURATION!.Clone()!;

            invocation.Mouse_Position__Origin.Internal__Value = mouse_origin;
            HISTORY__TOOL_INVOCATION.Prepare(invocation);
        }
        HISTORY__TOOL_INVOCATION.Preparing__Value!
            .Mouse_Position__Latest.Internal__Value =
            tool_position.Xy;
        HISTORY__TOOL_INVOCATION.Preparing__Value!
            .Mouse_Position__Origin.Internal__Value =
            mouse_origin;
    }

    private void Private_Establish__Grid
    (
        Grid_Configuration? grid_configuration = null
    )
    {
        GRID__IS_ACTIVE = false;
        GRID__IS_STEPPING = false;
        GRID__IS_INITIAL = true;
        GRID__WIDTH  = grid_configuration?.Width  ?? 50;
        GRID__HEIGHT = grid_configuration?.Height ?? 50;

        Texture.Direct__Pixel_Initalizer base_pixel_initalizer;
        GRID__CONFIGURATION = grid_configuration ?? new Grid_Configuration();

        if (grid_configuration?.Image__Path != null)
        {
            byte[] byte_array;
            ColorComponents color;
            using (FileStream image_stream = File.Open(grid_configuration.Image__Path, FileMode.Open))
            {
                StbImage.stbi_set_flip_vertically_on_load(1);
                ImageResult image =
                    ImageResult.FromStream(image_stream);

                GRID__WIDTH = image.Width;
                GRID__HEIGHT = image.Height;
                byte_array =
                    image.Data;
                color = image.Comp;
            }

            PixelInternalFormat internal_format;
            PixelFormat format;

            int channel_count;

            switch(color)
            {
                default:
                case ColorComponents.Grey:
                    channel_count = 1;
                    internal_format = PixelInternalFormat.Luminance;
                    format = PixelFormat.Luminance;
                    break;
                case ColorComponents.GreyAlpha:
                    channel_count = 2;
                    internal_format = PixelInternalFormat.LuminanceAlpha;
                    format = PixelFormat.LuminanceAlpha;
                    break;
                case ColorComponents.RedGreenBlue:
                    channel_count = 3;
                    internal_format = PixelInternalFormat.Rgb;
                    format = PixelFormat.Rgb;
                    break;
                case ColorComponents.RedGreenBlueAlpha:
                    channel_count = 4;
                    internal_format = PixelInternalFormat.Rgba;
                    format = PixelFormat.Rgba;
                    break;
            }

            base_pixel_initalizer =
                new Texture.Direct__Pixel_Initalizer
                (
                    channel_count,
                    internal_format,
                    format,
                    PixelType.UnsignedByte,
                    byte_buffer: byte_array
                );
        }
        else
        {
            base_pixel_initalizer =
                new Texture.Direct__Pixel_Initalizer
                (
                    4,
                    PixelInternalFormat.Rgba,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    seed: GRID__CONFIGURATION.Seed
                );
        }

        UI?.Set__Seed(GRID__CONFIGURATION.Seed);

        if (VAO__CELL_POINTS != 0)
        {
            GL.DeleteVertexArray(VAO__CELL_POINTS);
            GL.DeleteBuffer(VBO__CELL_POINTS);
            if (GRID__TEXTURE0 != null)
                GL.DeleteTexture(GRID__TEXTURE0.TEXTURE_HANDLE);
            if (GRID__TEXTURE1 != null)
                GL.DeleteTexture(GRID__TEXTURE1.TEXTURE_HANDLE);
        }

        VAO__CELL_POINTS = GL.GenVertexArray();
        GL.BindVertexArray(VAO__CELL_POINTS);
        VBO__CELL_POINTS = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO__CELL_POINTS);
        float[] points = new float[GRID__WIDTH * GRID__HEIGHT * 2];
        for(int i=0;i<points.Length;i+=2)
        {
            points[i  ] = (i/2) % GRID__WIDTH;
            points[i+1] = (i/2) / GRID__WIDTH;
        }
        GL.BufferData
        (
            BufferTarget.ArrayBuffer,
            points.Length * sizeof(float),
            points,
            BufferUsageHint.StaticDraw
        );
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
        GL.EnableVertexAttribArray(0);
        GL.BindVertexArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

        GRID__TEXTURE__BASE =
            new Texture
            (
                GRID__WIDTH, GRID__HEIGHT,
                base_pixel_initalizer
            );

        HISTORY__TOOL_INVOCATION = 
            new History__Tool_Invocation
            (
                GRID__TEXTURE__BASE, 
                100, 10,
                100, 10
            );
        if (TOOL__REPOSITORY.TOOL__ACTIVE?.Is__Requiring__Mouse_Position_History ?? false)
            HISTORY__TOOL_INVOCATION.Is__Requiring_Mouse_Position_History = true;

        GRID__TEXTURE0 =
            new Texture
            (
                GRID__WIDTH, GRID__HEIGHT,
                new Texture.Direct__Pixel_Initalizer
                (
                    base_pixel_initalizer.Channel_Count,
                    base_pixel_initalizer.Internal_Format,
                    base_pixel_initalizer.Pixel_Format,
                    PixelType.UnsignedByte,
                    byte_buffer: base_pixel_initalizer.Buffer__Bytes
                )
            );

        GRID__TEXTURE1 =
            new Texture
            (
                GRID__WIDTH, GRID__HEIGHT,
                base_pixel_initalizer.Channel_Count,
                base_pixel_initalizer.Internal_Format,
                base_pixel_initalizer.Pixel_Format
            );

        GRID__TEXTURE__READ  = GRID__TEXTURE0;
        GRID__TEXTURE__WRITE = GRID__TEXTURE1;

        Private_Reset__Grid_Swap();
    }

    private void Private_Save__Grid(string path)
    {
        Bitmap bmp = new Bitmap(GRID__WIDTH, GRID__HEIGHT);
        BitmapData data = 
            bmp.LockBits
            (
                new Rectangle(0,0,GRID__WIDTH,GRID__HEIGHT),
                System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb
            );

        GL.BindTexture(TextureTarget.Texture2D, GRID__TEXTURE__BASE.TEXTURE_HANDLE);
        GL.GetTexImage
        (
            TextureTarget.Texture2D,
            0,
            PixelFormat.Bgra,
            PixelType.UnsignedByte,
            data.Scan0
        );
        GL.BindTexture(TextureTarget.Texture2D, 0);

        bmp.UnlockBits(data);
        bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
        bmp.Save(path, ImageFormat.Png);
    }

    private void Private_Reset__Grid_Swap()
    {
        GRID__INDEX_COMPUTE--;

        Private_Swap__Grids();
    }

    private void Private_Reset__Grid()
    {
        GRID__IS_INITIAL = true;
        if (GRID__CONFIGURATION.Is__Using_New_Seed__For_Each_Reset)
        {
            GRID__TEXTURE__BASE.Pixel_Buffer_Initalizer.Seed = new Random().Next();
            GRID__TEXTURE__BASE.Reinitalize__Texture();

            HISTORY__TOOL_INVOCATION.Rebase(GRID__TEXTURE__BASE);
        }
        UI.Set__Seed(GRID__TEXTURE__BASE.Pixel_Buffer_Initalizer.Seed);
        
        //GL.BindTexture(TextureTarget.Texture2D, GRID__TEXTURE__READ.TEXTURE_HANDLE);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GRID__FRAMEBUFFER__COMPUTE);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            GRID__TEXTURE__BASE.TEXTURE_HANDLE,
            0
        );
        GL.CopyTextureSubImage2D
        (
            GRID__TEXTURE__READ.TEXTURE_HANDLE, 
            0,
            0, 0,
            0, 0,
            GRID__WIDTH, GRID__HEIGHT
        );

        Private_Reset__Grid_Swap();
        //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void Private_Render__Simulation_Space
    (
        int viewport_width,
        int viewport_height
    )
    {
        Private_Render__Tool();

        if (!GRID__IS_ACTIVE && !GRID__IS_STEPPING) goto render_grid;
        if (GRID__IS_STEPPING) GRID__IS_STEPPING = false;
        else if (TIME__ELAPSED < GRID__DELAY__MS) goto render_grid;

        GRID__IS_INITIAL = false;
        TIME__ELAPSED = 0;
        
        Private_Compute__Grid();

render_grid:
        Private_Render__Grid();
    }

    private void Private_Render__Tool()
    {
        bool error = false;
        if (HISTORY__TOOL_INVOCATION.Is__In_Need_Of__Update || HISTORY__TOOL_INVOCATION.Is__Preparing__Value)
        {
            GRID__TEXTURE__BASE =
                HISTORY__TOOL_INVOCATION.Aggregate__Epochs(ref error);
            if (GRID__IS_INITIAL) Private_Reset__Grid_Swap();
        }
    }

    private void Private_Compute__Grid()
    {
        GLHelper.Push_Viewport(0,0,GRID__WIDTH,GRID__HEIGHT);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GRID__FRAMEBUFFER__COMPUTE);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, GRID__TEXTURE__READ.TEXTURE_HANDLE);
        SHADER__COMPUTE.Use();
        GL.Uniform1(SHADER__COMPUTE.Get__Uniform("width"), (float)GRID__WIDTH);
        GL.Uniform1(SHADER__COMPUTE.Get__Uniform("height"), (float)GRID__HEIGHT);
        GL.BindVertexArray(VAO__CELL_POINTS);
        GL.DrawArrays(PrimitiveType.Points, 0, CELL__COUNT);
        GLHelper.Pop_Viewport();

        Private_Swap__Grids();
    }

    int test__step = 0;
    private void Private_Render__Grid()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture
        (
            TextureTarget.Texture2D, 
            (GRID__IS_INITIAL) 
            ? GRID__TEXTURE__BASE.TEXTURE_HANDLE
            : (GRID__IS_ACTIVE)
                ? GRID__TEXTURE__WRITE.TEXTURE_HANDLE
                : GRID__TEXTURE__READ.TEXTURE_HANDLE
        );
        SHADER__DRAW.Use();
        GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)GRID__WIDTH);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)GRID__HEIGHT);
        GL.UniformMatrix4(SHADER__DRAW.Get__Uniform("projection"), false, ref GRID__CAMERA.GRID__PROJECTION);
        GL.UniformMatrix4(SHADER__DRAW.Get__Uniform("translation"), false, ref GRID__CAMERA.GRID__TRANSLATION);
        GL.BindVertexArray(VAO__CELL_POINTS);
        GL.DrawArrays(PrimitiveType.Points, 0, CELL__COUNT);
    }

    private void Private_Swap__Grids()
    {
        GRID__INDEX_COMPUTE = (GRID__INDEX_COMPUTE + 1) % 2;
        switch(GRID__INDEX_COMPUTE)
        {
            case 0:
                GRID__TEXTURE__READ = GRID__TEXTURE0;
                GRID__TEXTURE__WRITE = GRID__TEXTURE1;
                break;
            case 1:
                GRID__TEXTURE__READ = GRID__TEXTURE1;
                GRID__TEXTURE__WRITE = GRID__TEXTURE0;
                break;
        }

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GRID__FRAMEBUFFER__COMPUTE);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            GRID__TEXTURE__WRITE.TEXTURE_HANDLE,
            0
        );

        if (GRID__IS_INITIAL)
            GRID__TEXTURE__READ = GRID__TEXTURE__BASE;
    }

    public static void Main(string[] args)
    {
        new Program().Run();
    }

    public static void Dump_Error(string? msg = null, bool print = true)
    {
        if (msg != null)
            Console.WriteLine(msg);
        ErrorCode err;
        do
        {
            err = GL.GetError();
            if (err != ErrorCode.NoError && print)
                Console.WriteLine(err);
        } while(err != ErrorCode.NoError);
    }
}
