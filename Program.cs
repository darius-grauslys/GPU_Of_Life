
using System.Diagnostics.CodeAnalysis;
using Gwen.Net.OpenTk;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace GPU_Of_Life;

public class Program : GameWindow
{
    private Viewport BASE__VIEWPORT;

    private readonly IGwenGui _gwen_gui;    
    private readonly Shader SHADER__COMPUTE;
    [AllowNull]
    private readonly Shader SHADER__DRAW;

    private int VAO__CELL_POINTS;
    private int VBO__CELL_POINTS;

    private int GRID__INDEX_COMPUTE = 0;

    private int GRID__FRAMEBUFFER__COMPUTE;

    private int GRID__WIDTH = 10, GRID__HEIGHT = 10;
    private int CELL__COUNT
        => GRID__WIDTH * GRID__HEIGHT;

    [AllowNull]
    private Texture GRID__TEXTURE0;
    [AllowNull]
    private Texture GRID__TEXTURE1;

    [AllowNull]
    private Texture GRID__TEXTURE__READ;
    [AllowNull]
    private Texture GRID__TEXTURE__WRITE;

    private bool GRID__IS_ACTIVE = false;
    private bool GRID__IS_STEPPING = false;
    private int  GRID__DELAY = 0;

    private Vector2i GRID__POSITION;
    private Vector2i GRID__SIZE;

    public Program() 
    : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
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
            new Shader.Factory()
            .Begin()
            .Add__Shader_From_File(ShaderType.VertexShader, "Shader_Compute__Vertex.vert", ref err)
            .Add__Shader_From_File(ShaderType.FragmentShader, "Shader_Compute__Fragmentation.frag", ref err)
            .Link()
            ;

        if (err) { Close(); return; }

        err = false;

        SHADER__DRAW =
            new Shader.Factory()
            .Begin()
            .Add__Shader_From_File(ShaderType.VertexShader, "Shader_Draw__Vertex.vert", ref err)
            .Add__Shader_From_File(ShaderType.GeometryShader, "Shader_Draw__Geometry.geom", ref err)
            .Add__Shader_From_File(ShaderType.FragmentShader, "Shader_Draw__Fragmentation.frag", ref err)
            .Link()
            ;

        if (err) { Close(); return; }

        Private_Establish__Grid(GRID__WIDTH, GRID__HEIGHT, new Random());
    }

    private void Private_Establish__Grid
    (
        int width, 
        int height,
        Random? randomizer = null
    )
    {
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
        float[] points = new float[width * height * 2];
        for(int i=0;i<points.Length;i+=2)
        {
            points[i  ] = (i/2) % width;
            points[i+1] = (i/2) / width;
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

        GRID__TEXTURE0 =
            new Texture
            (
                width, height,
                new Texture.Direct__Pixel_Initalizer
                (
                    4,
                    PixelInternalFormat.Rgba,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    randomizer: randomizer
                )
            );

        GRID__TEXTURE1 =
            new Texture(width, height);

        GRID__TEXTURE__READ  = GRID__TEXTURE0;
        GRID__TEXTURE__WRITE = GRID__TEXTURE1;
    }

    protected override void OnKeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.Escape)
            Close();
    }

    protected override void OnLoad()
    {
        _gwen_gui.Load();
        _gwen_gui.Resize(Size);
        Gwen_UI ui = new Gwen_UI(_gwen_gui.Root);

        ui.Resized__Grid += s => 
        {
            GRID__WIDTH  = s.X;
            GRID__HEIGHT = s.Y;
        };

        ui.Toggle__Run  +=  b => GRID__IS_ACTIVE = b;
        ui.Pulsed__Step += () => GRID__IS_STEPPING = true;

        ui.Render__Grid += Private_Render__Grid;
    }

    protected override void OnResize(OpenTK.Windowing.Common.ResizeEventArgs e)
    {
        base.OnResize(e);
        _gwen_gui.Resize(Size);
        BASE__VIEWPORT.Resize(Size);

        //TODO: reconstruct framebuffer
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        //gwen gui makes InvalidEnum error.
        //ruled out my render override causing it.
        _gwen_gui.Render();
        //Dump_Error("err:");
        SwapBuffers();
    }

    private void Private_Render__Grid
    (
        int viewport_width,
        int viewport_height
    )
    {
        //if (GRID__IS_ACTIVE)
        //    Console.WriteLine("ACTIVE");
        //if (GRID__IS_STEPPING)
        //{
        //    GRID__IS_STEPPING = false;
        //    Console.WriteLine("STEPPING");
        //}
        //return;
        if (!GRID__IS_ACTIVE && !GRID__IS_STEPPING) goto render_grid;
        if (GRID__IS_STEPPING) GRID__IS_STEPPING = false;
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, GRID__FRAMEBUFFER__COMPUTE);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, GRID__TEXTURE__READ.TEXTURE_HANDLE);
        SHADER__COMPUTE.Use();
        GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)GRID__WIDTH);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)GRID__HEIGHT);
        GL.BindVertexArray(VAO__CELL_POINTS);
        GL.DrawArrays(PrimitiveType.Points, 0, CELL__COUNT);

        Task.Delay(GRID__DELAY).Wait();
        Private_Swap__Grids();

render_grid:
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture
        (
            TextureTarget.Texture2D, 
            (GRID__IS_ACTIVE) 
            ? GRID__TEXTURE__WRITE.TEXTURE_HANDLE
            : GRID__TEXTURE__READ.TEXTURE_HANDLE
        );
        SHADER__DRAW.Use();
        Console.WriteLine("w: {0} h: {1}", viewport_width, viewport_height);
        //GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)viewport_width);
        //GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)viewport_height);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)GRID__WIDTH);
        GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)GRID__HEIGHT);
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
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    public static void Main(string[] args)
    {
        new Program().Run();
    }

    public static void Dump_Error(string? msg = null)
    {
        if (msg != null)
            Console.WriteLine(msg);
        ErrorCode err;
        do
        {
            err = GL.GetError();
            if (err != ErrorCode.NoError)
                Console.WriteLine(err);
        } while(err != ErrorCode.NoError);
    }
}
