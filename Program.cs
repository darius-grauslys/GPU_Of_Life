
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.OpenTk;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;
using KeyModifiers = OpenTK.Windowing.GraphicsLibraryFramework.KeyModifiers;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace GPU_Of_Life;

public class Program : Test__Window //GameWindow 
{
    private Viewport BASE__VIEWPORT;

    private readonly IGwenGui _gwen_gui;    
    [AllowNull]
    private Gwen_UI UI;

    private readonly Shader SHADER__COMPUTE;
    [AllowNull]
    private readonly Shader SHADER__DRAW;

    [AllowNull]
    private readonly History__Mouse_Position HISTORY__MOUSE_POSITION;
    private readonly History__Tool_Invocation HISTORY__TOOL_INVOCATION;
    private readonly Tool__Repository TOOL__REPOSITORY =
        new Tool__Repository();

    private readonly Shader SHADER__TOOL__STENCIL;
    private byte TOOL__STENCIL__VALUE = 255;
    private int history__length = 100;
    private int history__current_index = 0;
    private int TOOL__FRAMEBUFFER;

    private int stencil_vao;
    private int stencil_vbo;

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

    public Program() 
    //: base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        GRID__FRAMEBUFFER__COMPUTE = GL.GenFramebuffer();
        TOOL__FRAMEBUFFER = GL.GenFramebuffer();

        stencil_vao = GL.GenVertexArray();
        GL.BindVertexArray(stencil_vao);
        stencil_vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, stencil_vbo);
        Private_Clear__Stencil_History();
        GL.BindVertexArray(0);

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

        bool err = false;
        SHADER__COMPUTE =
            new Shader.Builder()
            .Begin()
            .Add__Shader_From_File(ShaderType.VertexShader, "Shader_Compute__Vertex.vert", ref err)
            .Add__Shader_From_File(ShaderType.FragmentShader, "Shader_Compute__Fragmentation.frag", ref err)
            .Link()
            ;

        if (err) { Close(); return; }

        err = false;

        SHADER__DRAW =
            new Shader.Builder()
            .Begin()
            .Add__Shader_From_File(ShaderType.VertexShader, "Shader_Draw__Vertex.vert", ref err)
            .Add__Shader_From_File(ShaderType.GeometryShader, "Shader_Draw__Geometry.geom", ref err)
            .Add__Shader_From_File(ShaderType.FragmentShader, "Shader_Draw__Fragmentation.frag", ref err)
            .Link()
            ;

        if (err) { Close(); return; }

        SHADER__TOOL__STENCIL =
            new Shader.Builder()
            .Begin()
            .Add__Shader_From_File(ShaderType.VertexShader, "Shader_Tool__Stencil__Vertex.vert", ref err)
            .Add__Shader_From_File(ShaderType.FragmentShader, "Shader_Tool__Stencil__Fragment.frag", ref err)
            .Link()
            ;

        if (err) { Close(); return; }

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
                if (e.Modifiers == KeyModifiers.Control)
                    history_mouse.Undo();
                    //Private_Undo__Tool();
                break;
            case Keys.R:
                if (e.Modifiers == KeyModifiers.Control)
                    history_mouse.Redo();
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
        foreach(Tool tool in TOOL__REPOSITORY.TOOLS)
            UI.Load__Tool(tool);

        UI.Updated__Tool_Selection +=
            (tool_name) => UI.Select__Tool(TOOL__REPOSITORY.RECORDED__TOOLS[tool_name].Get__Invocation());

        UI.Invoked__New   +=  c => Private_Establish__Grid(c);
        UI.Invoked__Reset += () => Private_Reset__Grid();
        UI.Toggle__Run    +=  b => GRID__IS_ACTIVE = b;
        UI.Pulsed__Step   += () => GRID__IS_STEPPING = true;

        UI.Render__Grid += Private_Render__Grid;

        UI.Updated__Compute_Speed += 
            (speed_level) => 
            { 
                GRID__DELAY__MS = GRID__DELAY__SEC_INTERVAL * speed_level + 0.1; 
                GRID__DELAY__MS *= GRID__DELAY__MS; 
            };

        UI.Updated__Stencil_Value +=
            (stencil_value) => TOOL__STENCIL__VALUE = stencil_value;
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
    }

    protected override void OnMouseWheel(OpenTK.Windowing.Common.MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        GRID__CAMERA.Process__Scroll(e);
    }

    protected override void OnMouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        Private_Process__Tool();
    }

    Vector2 mouse_previous = new Vector2(-1);
    protected override void OnMouseMove(OpenTK.Windowing.Common.MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        Private_Process__Tool();
    }

    private History__Mouse_Position history_mouse;
    private void Private_Process__Tool()
    {
        if (MousePosition.X < UI.GRID__X || MousePosition.X > UI.GRID__X + UI.GRID__WIDTH)  return;
        if (MousePosition.Y < UI.GRID__Y || MousePosition.Y > UI.GRID__Y + UI.GRID__HEIGHT) return;
        if (!MouseState.IsButtonDown(MouseButton.Left)) { Private_Clear__Stencil_History(); return; }
        if (mouse_previous == MousePosition) return;
        mouse_previous = MousePosition;

        Vector2 mouse_position =
            new Vector2
            (
                MousePosition.X - UI.GRID__X,
                Size.Y - (MousePosition.Y + UI.GRID__Y)
            );
        Vector4 tool_position = GRID__CAMERA.Get__Mouse_To_World(mouse_position);

        if (history_mouse == null)
            history_mouse = new History__Mouse_Position(100, 10);
        history_mouse.Append(tool_position.Xy);

        if (history__current_index >= history__length)
            Private_Clear__Stencil_History();

        if (history__current_index == 0)
        {
            Private_Buffer__Point(ref tool_position);
            history__current_index++;
        }
        Private_Buffer__Point(ref tool_position);
        history__current_index++;
    }

    private void Private_Buffer__Point(ref Vector4 tool_position)
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, stencil_vbo);
        GL.BufferSubData
        (
            BufferTarget.ArrayBuffer, 
            new IntPtr(history__current_index * 2 * sizeof(float)),
            sizeof(float) * 2,
            new float[] { tool_position.X, tool_position.Y }
        );
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    private void Private_Clear__Stencil_History()
    {
        history__current_index = 0;
        GL.BindVertexArray(stencil_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, stencil_vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, history__length * 2 * sizeof(float), IntPtr.Zero, BufferUsageHint.StreamDraw);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, sizeof(float) * 2, 0);
        GL.EnableVertexAttribArray(0);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    private void Private_Establish__Grid(Grid_Configuration? grid_configuration = null)
    {
        GRID__IS_ACTIVE = false;
        GRID__IS_STEPPING = false;
        GRID__IS_INITIAL = true;
        GRID__WIDTH  = grid_configuration?.Width  ?? 50;
        GRID__HEIGHT = grid_configuration?.Height ?? 50;

        GRID__CONFIGURATION = grid_configuration ?? new Grid_Configuration();
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

        Texture.Direct__Pixel_Initalizer base_pixel_initalizer =
                new Texture.Direct__Pixel_Initalizer
                (
                    4,
                    PixelInternalFormat.Rgba,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    seed: grid_configuration?.Seed
                );

        GRID__TEXTURE__BASE =
            new Texture
            (
                GRID__WIDTH, GRID__HEIGHT,
                base_pixel_initalizer
            );

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, TOOL__FRAMEBUFFER);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            GRID__TEXTURE__BASE.TEXTURE_HANDLE,
            0
        );

        GRID__TEXTURE0 =
            new Texture
            (
                GRID__WIDTH, GRID__HEIGHT,
                new Texture.Direct__Pixel_Initalizer
                (
                    4,
                    PixelInternalFormat.Rgba,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    byte_buffer: base_pixel_initalizer.Buffer__Bytes
                )
            );

        GRID__TEXTURE1 =
            new Texture(GRID__WIDTH, GRID__HEIGHT);

        GRID__TEXTURE__READ  = GRID__TEXTURE0;
        GRID__TEXTURE__WRITE = GRID__TEXTURE1;

        Private_Reset__Grid_Swap();
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

    private void Private_Render__Grid
    (
        int viewport_width,
        int viewport_height
    )
    {
        GLHelper.Push_Viewport(0,0,GRID__WIDTH,GRID__HEIGHT);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, TOOL__FRAMEBUFFER);
        SHADER__TOOL__STENCIL.Use();
        GL.Uniform1(SHADER__TOOL__STENCIL.Get__Uniform("life"), TOOL__STENCIL__VALUE / 255f);
        GL.BindVertexArray(stencil_vao);
        GL.DrawArrays(PrimitiveType.LineStrip, 0, history__current_index);
        GLHelper.Pop_Viewport();

        //if (GRID__IS_ACTIVE)
        //    Console.WriteLine("ACTIVE");
        //if (GRID__IS_STEPPING)
        //{
        //    GRID__IS_STEPPING = false;
        //    Console.WriteLine("STEPPING");
        //}
        //return;
        //if (!GRID__IS_ACTIVE && !GRID__IS_STEPPING) return;
        if (!GRID__IS_ACTIVE && !GRID__IS_STEPPING) goto render_grid;
        if (GRID__IS_STEPPING) GRID__IS_STEPPING = false;
        else if (TIME__ELAPSED < GRID__DELAY__MS) goto render_grid;
        GRID__IS_INITIAL = false;
        TIME__ELAPSED = 0;
        
        GLHelper.Push_Viewport(0,0,GRID__WIDTH,GRID__HEIGHT);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GRID__FRAMEBUFFER__COMPUTE);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, GRID__TEXTURE__READ.TEXTURE_HANDLE);
        SHADER__COMPUTE.Use();
        GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)GRID__WIDTH);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)GRID__HEIGHT);
        GL.BindVertexArray(VAO__CELL_POINTS);
        GL.DrawArrays(PrimitiveType.Points, 0, CELL__COUNT);
        GLHelper.Pop_Viewport();

        Private_Swap__Grids();

render_grid:
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
        //GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)viewport_width);
        //GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)viewport_height);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)GRID__WIDTH);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)GRID__HEIGHT);
        GL.UniformMatrix4(SHADER__DRAW.Get__Uniform("projection"), false, ref GRID__CAMERA.GRID__PROJECTION);
        //Matrix4 translation = Matrix4.Identity;
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

        //TODO: Console.WriteLine("might need to viewport this too");
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

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, TOOL__FRAMEBUFFER);
        GL.FramebufferTexture2D
        (
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            GRID__TEXTURE__READ.TEXTURE_HANDLE,
            0
        );

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
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
