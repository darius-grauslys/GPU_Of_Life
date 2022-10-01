///*
// * Notes:
// *
// * You must bind the corresponding VBO shortly after
// * creating and binding the VAO. Look into this?
// * */
//
//using System.Diagnostics.CodeAnalysis;
//using OpenTK.Graphics.OpenGL;
//using OpenTK.Mathematics;
//
//namespace OpenTK_Test;
//
//public class Test__Geometry__Game_Of_Life : Test__Window
//{
//    private int FRAMEBUFFER__COMPUTE;
//
//    [AllowNull]
//    private Texture TEXTURE_0;
//    [AllowNull]
//    private Texture TEXTURE_1;
//
//    private int INDEX__ACTIVE_READ;
//    [AllowNull]
//    private Texture TEXTURE_READ;
//    [AllowNull]
//    private Texture TEXTURE_WRITE;
//
//    private readonly Shader SHADER__COMPUTE;
//    private int VAO__CELL_POINTS;
//    private int CELL__COUNT
//        => Width * Height;
//    private int Width = 10, Height = 10, Seed = -1;
//
//    [AllowNull]
//    private readonly Shader SHADER__DRAW;
//
//    public Test__Geometry__Game_Of_Life()
//    {
//        string source__compute_vert = @"
//#version 420 core
//layout(location = 0) in vec2 aPosition;
//
//uniform float width;
//uniform float height;
//
//void main()
//{
//    gl_Position = vec4(aPosition, 0, 1);
//    return;
//    //gl_Position = vec4(aPosition.x / width, aPosition.y / height, 0, 1) - vec4(1, 1, 0, 0);
//    //gl_Position = vec4((aPosition.x - width/2) / width, (aPosition.y - height/2) / height, 0, 1) - vec4(0.4, 1, 0, 0);
//    
//    
//    vec2 offset = aPosition ;//- vec2(width, height/2);
//    gl_Position = vec4(offset.x / width / 6.4, offset.y / height / 3.599, 0, 1) - vec4(1, 1, 0, 0);
//    //gl_Position = vec4(offset.x / width, offset.y / height, 0, 1) - vec4(0.5, 0.5, 0, 0);
//    return;
//
//
//
//
//    if (aPosition.x == 49)
//        gl_Position = vec4(0,0,0,1);
//    else
//        gl_Position = vec4(-100, 0, 0, 1);
//    //gl_Position = vec4(aPosition, 0, 1);
//    //gl_Position = vec4(aPosition, 0, 1);
//    //gl_Position = vec4(aPosition.x / width, aPosition.y / height, 0, 1);
//}
//";
//        string source__compute_geom = @"
//#version 420
//layout(points) in;
//layout(triangle_strip, max_vertices = 4) out;
//
//uniform sampler2D _sample;
//
//out float life;
//
//uniform float width;
//uniform float height;
//
//void main()
//{
//    vec2 cell = gl_in[0].gl_Position.xy;
//    vec2 cell_sample = vec2((cell.x + 0.5) / width, (cell.y + 0.5) / height);
//    cell = vec2(cell.x  * 2 / width, cell.y * 2 / height);
//
//    float cell_w = 1/width;
//    float cell_h = 1/height;
//
//    float life_sum = 0;
//    int iwidth = int(width);
//    int iheight = int(height);
//    vec2 p_in = gl_in[0].gl_Position.xy;
//    ivec2 sample_pos = ivec2(int(p_in.x), int(p_in.y));
//    for(int i=-1;i<2;i++)
//    {
//        if (i + sample_pos.x > iwidth || i+sample_pos.x < 0) continue;
//        for(int j=-1;j<2;j++)
//        {
//            if (j + sample_pos.y > iheight || j+sample_pos.y < 0) continue;
//
//            if(i==0 && j==0) continue;
//
//            life_sum += texelFetch(_sample, sample_pos + ivec2(i,j), 0).x;
//        }
//    }
//
//    life = life_sum;
//    if (life_sum < 3 || life_sum > 4)
//    {
//        if (life_sum < 2 || life_sum > 5)
//        {
//            life = 0;
//        }
//        else
//        {
//            if (life_sum > 3) life_sum - 1;
//
//            life = 1 - abs(3 - life_sum);
//        }
//    }
//
//    vec2 cell_offset = vec2(-1 + cell_w, -1 + cell_h);
//    vec4 pos = vec4(cell + cell_offset, 0, 1);
//
//    gl_Position = pos + vec4(-cell_w, -cell_h, 0, 0);
//    //gl_Position = vec4(-0.5,0.5,0,1);
//    EmitVertex();
//
//    gl_Position = pos + vec4( cell_w, -cell_h, 0, 0);
//    //gl_Position = vec4(0.5,0.5,0,1);
//    EmitVertex();
//
//    gl_Position = pos + vec4(-cell_w,  cell_h, 0, 0);
//    //gl_Position = vec4(-0.5,-0.5,0,1);
//    EmitVertex();
//
//    gl_Position = pos + vec4( cell_w,  cell_h, 0, 0);
//    //gl_Position = vec4(0.5,-0.5,0,1);
//    EmitVertex();
//
//    EndPrimitive();
//}
//";
//        string source__compute_frag = @"
//#version 420 
//
//out vec4 output_color;
//
//in float life;
//
//void main()
//{
//    output_color = vec4(life, 0, 0, 1);
//}
//";
//        bool err = false;
//        SHADER__COMPUTE =
//            new Shader.Factory()
//            .Begin()
//            .Add__Shader(ShaderType.VertexShader, source__compute_vert, ref err)
//            .Add__Shader(ShaderType.GeometryShader, source__compute_geom, ref err)
//            .Add__Shader(ShaderType.FragmentShader, source__compute_frag, ref err)
//            .Link()
//            ;
//
//        if (err) { Close(); return; }
//
//        string source__draw_vert = @"
//#version 420 core
//layout(location = 0) in vec2 cell;
//
//void main()
//{
//    gl_Position = vec4(cell, 0, 1);
//}
//";
//        string source__draw_geom = @"
//#version 420
//layout(points) in;
//layout(triangle_strip, max_vertices = 4) out;
//
//out float life;
//
//uniform sampler2D _sample;
//
//uniform float width;
//uniform float height;
//
//void main()
//{
//    vec2 cell = gl_in[0].gl_Position.xy;
//    vec2 cell_sample = vec2((cell.x + 0.5) / width, (cell.y + 0.5) / height);
//    cell = vec2(cell.x  * 2 / width, cell.y * 2 / height);
//    vec4 vec_life = texture(_sample, cell_sample);
//
//    life = max(0, (vec_life.x - 0.5)) / 0.5;
//    life = life * life;
//
//    float mod = 1 - (1 - life);
//    if (mod <= 0) return;
//
//    float cell_w = 1/width * mod;
//    float cell_h = 1/height * mod;
//
//    vec2 cell_offset = vec2(-1 + cell_w / mod, -1 + cell_h / mod);
//    vec4 pos = vec4(cell + cell_offset, 0, 1);
//    //pos = vec4(cell, 0, 1);
//
//    gl_Position = pos + vec4(-cell_w, -cell_h, 0, 0);
//    //gl_Position = vec4(-0.5,0.5,0,1);
//    EmitVertex();
//
//    gl_Position = pos + vec4( cell_w, -cell_h, 0, 0);
//    //gl_Position = vec4(0.5,0.5,0,1);
//    EmitVertex();
//
//    gl_Position = pos + vec4(-cell_w,  cell_h, 0, 0);
//    //gl_Position = vec4(-0.5,-0.5,0,1);
//    EmitVertex();
//
//    gl_Position = pos + vec4( cell_w,  cell_h, 0, 0);
//    //gl_Position = vec4(0.5,-0.5,0,1);
//    EmitVertex();
//
//    EndPrimitive();
//}
//";
//        string source__draw_frag = @"
//#version 420
//
//out vec4 output_color;
//
//in float life;
//
//void main()
//{
//    output_color = vec4(0, life, 0, 1);
//}
//";
//
//        err = false;
//        SHADER__DRAW =
//            new Shader.Factory()
//            .Begin()
//            .Add__Shader(ShaderType.VertexShader, source__draw_vert, ref err)
//            .Add__Shader(ShaderType.GeometryShader, source__draw_geom, ref err)
//            .Add__Shader(ShaderType.FragmentShader, source__draw_frag, ref err)
//            .Link()
//            ;
//
//        if (err) Close();
//    }
//
//    protected internal override void Handle__Arguments(string[] args)
//    {
//        Args_Parser pargs = new Args_Parser(args);
//
//        pargs.Try(1, ref Width, " as width");
//        pargs.Try(2, ref Height, " as height");
//        pargs.Try(3, ref Seed, " as seed");
//
//        // Had to set size along with viewport to
//        // fix scaling issues.
//        if (Size.X < Width || Size.Y < Height)
//        {
//            Size = new Vector2i
//                (
//                    (Size.X < Width) ? Width : Size.X,
//                    (Size.Y < Height) ? Height : Size.Y
//                );
//            GL.Viewport(0, 0, Size.X, Size.Y);
//        }
//
//        FRAMEBUFFER__COMPUTE = GL.GenFramebuffer();
//
//        Handle__Reset();
//    }
//
//    protected internal override void Handle__Reset()
//    {
//        INDEX__ACTIVE_READ = 0;
//
//        float[] cell_points = new float[Width * Height * 2];
//
//        for(int i=0;i<cell_points.Length;i+=2)
//        {
//            cell_points[i  ] = (i/2) % Width;
//            cell_points[i+1] = (i/2) / Width;
//        }
//
//        VAO__CELL_POINTS = GL.GenVertexArray();
//        GL.BindVertexArray(VAO__CELL_POINTS);
//        int buffer = GL.GenBuffer();
//        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
//        GL.BufferData
//        (
//            BufferTarget.ArrayBuffer,
//            cell_points.Length * sizeof(float),
//            cell_points,
//            BufferUsageHint.StaticDraw
//        );
//        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
//        GL.EnableVertexAttribArray(0);
//        GL.BindVertexArray(0);
//
//        byte[] bytes = new byte[Width * Height * 4];
//
//        Random random = 
//            (Seed != -1)
//            ? new Random(Seed)
//            : new Random()
//            ;
//        if (Seed == -1)
//        {
//            int seed = random.Next();
//            Console.WriteLine($"Seed: {seed}");
//            random = new Random(seed);
//        }
//
//        random.NextBytes(bytes);
//
//        // invalid value
//        // for some reason I cannot use Luminance or Alpha...
//        
//        TEXTURE_0 = new Texture
//        (
//            Width, 
//            Height,
//            new Texture.Direct__Pixel_Initalizer
//            (
//                4,
//                PixelInternalFormat.Rgba,
//                PixelFormat.Rgba,
//                PixelType.UnsignedByte,
//                byte_buffer: bytes
//            )
//        );
//
//        TEXTURE_1 = new Texture(Width, Height
//                ,
//            new Texture.Direct__Pixel_Initalizer
//            (
//                4,
//                PixelInternalFormat.Rgba,
//                PixelFormat.Rgba,
//                PixelType.UnsignedByte,
//                byte_buffer: bytes
//            )
//                );
//        
//        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FRAMEBUFFER__COMPUTE);
//        Private_Swap__Color_Attachments();
//        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
//    }
//
//    private void Private_Swap__Color_Attachments()
//    {
//        // swap compute framebuffer color attachment
//        
//        INDEX__ACTIVE_READ = (INDEX__ACTIVE_READ + 1) % 2;
//        switch (INDEX__ACTIVE_READ)
//        {
//            default:
//            case 0:
//                TEXTURE_READ  = TEXTURE_0;
//                TEXTURE_WRITE = TEXTURE_1;
//                break;
//            case 1:
//                TEXTURE_READ  = TEXTURE_1;
//                TEXTURE_WRITE = TEXTURE_0;
//                break;
//        }
//
//        GL.FramebufferTexture2D
//        (
//            FramebufferTarget.Framebuffer, 
//            FramebufferAttachment.ColorAttachment0,
//            TextureTarget.Texture2D,
//            TEXTURE_WRITE.TEXTURE_HANDLE,
//            0
//        );
//    }
//    
//    bool flop = false;
//    bool next = false;
//    protected override void OnKeyDown(OpenTK.Windowing.Common.KeyboardKeyEventArgs e)
//    {
//        base.OnKeyDown(e);
//        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.T) flop = !flop;
//        if (e.Key == OpenTK.Windowing.GraphicsLibraryFramework.Keys.N) next = !next;
//    }
//
//    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs args)
//    {
//        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FRAMEBUFFER__COMPUTE);
//        GL.ActiveTexture(TextureUnit.Texture0);
//        GL.BindTexture(TextureTarget.Texture2D, TEXTURE_READ.TEXTURE_HANDLE);
//        SHADER__COMPUTE.Use();
//        //GL.Uniform1(SHADER__COMPUTE.Get__Uniform("width"), (float)(Width * 12.75));
//        //GL.Uniform1(SHADER__COMPUTE.Get__Uniform("height"), (float)(Height * 7.272425));
//        GL.Uniform1(SHADER__COMPUTE.Get__Uniform("width"), (float)(Size.X < Width ? Width : Size.X));
//        GL.Uniform1(SHADER__COMPUTE.Get__Uniform("height"), (float)(Size.Y < Height ? Height : Size.Y));
//        //GL.Uniform1(SHADER__COMPUTE.Get__Uniform("width"), (float)(Width));
//        //GL.Uniform1(SHADER__COMPUTE.Get__Uniform("height"), (float)(Height));
//        GL.BindVertexArray(VAO__CELL_POINTS);
//        GL.DrawArrays(PrimitiveType.Points, 0, CELL__COUNT);
//
//        //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
//        //GL.Clear(ClearBufferMask.ColorBufferBit);
//        //GL.ActiveTexture(TextureUnit.Texture0);
//        //if (flop)
//        //GL.BindTexture(TextureTarget.Texture2D, TEXTURE_READ.TEXTURE_HANDLE);
//        //else
//        //GL.BindTexture(TextureTarget.Texture2D, TEXTURE_WRITE.TEXTURE_HANDLE);
//        //SHADER__DRAW.Use();
//        //GL.Uniform1(SHADER__COMPUTE.Get__Uniform("width"), (float)(Size.X < Width ? Width : Size.X));
//        //GL.Uniform1(SHADER__COMPUTE.Get__Uniform("height"), (float)(Size.Y < Height ? Height : Size.Y));
//        //GL.BindVertexArray(VAO__CELL_POINTS);
//        //GL.DrawArrays(PrimitiveType.Points, 0, CELL__COUNT);
//        //SwapBuffers();
//        //if (next)
//        //{
//        //    GL.BindFramebuffer(FramebufferTarget.Framebuffer, FRAMEBUFFER__COMPUTE);
//        //    Private_Swap__Color_Attachments();
//        //    next = false;
//        //}
//        //return;
//        
//
//        GL.MemoryBarrier(MemoryBarrierFlags.FramebufferBarrierBit);
//
//        // invalidoperation
//        Private_Swap__Color_Attachments();
//        //Task.Delay(1000).Wait();
//
//        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
//        GL.Clear(ClearBufferMask.ColorBufferBit);
//        GL.ActiveTexture(TextureUnit.Texture0);
//        GL.BindTexture(TextureTarget.Texture2D, TEXTURE_WRITE.TEXTURE_HANDLE);
//        SHADER__DRAW.Use();
//        GL.Uniform1(SHADER__DRAW.Get__Uniform("width"), (float)Width);
//        GL.Uniform1(SHADER__DRAW.Get__Uniform("height"), (float)Height);
//        GL.BindVertexArray(VAO__CELL_POINTS);
//        GL.DrawArrays(PrimitiveType.Points, 0, CELL__COUNT);
//        SwapBuffers();
//        ErrorCode err;
//        do
//        {
//            err = GL.GetError();
//            if (err != ErrorCode.NoError)
//                Console.WriteLine(err);
//        } while(err != ErrorCode.NoError);
//    }
//}
