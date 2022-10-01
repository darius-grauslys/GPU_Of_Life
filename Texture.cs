
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;

namespace GPU_Of_Life;

public class Texture
{
    public readonly int TEXTURE_HANDLE;
    private int _width = 10, _height = 10;
    private int? _seed = null;
    public int Width
    {
        get => _width;
        set 
        {
            _width = value;
            Reinitalize__Texture();
        }
    }
    public int Height
    {
        get => _height;
        set
        {
            _height = value;
            Reinitalize__Texture();
        }
    }

    public Pixel_Initalizer Pixel_Buffer_Initalizer { get; }
    
    public Texture
    (
        int width, int height,
        Pixel_Initalizer? pixel_initalizer = null
    ) 
    {
        _width = width;
        _height = height;
        TEXTURE_HANDLE = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

        Pixel_Buffer_Initalizer = 
            pixel_initalizer
            ??
            Direct__Pixel_Initalizer
            .Default
            ;

        Pixel_Buffer_Initalizer.Texture_Reference = this;
        Reinitalize__Texture();
    }

    public void Reinitalize__Texture()
    {
        GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);
        Pixel_Buffer_Initalizer.Reinitalize__Pixels(Width, Height);
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public abstract class Pixel_Initalizer
    {
        [AllowNull] //dep-injected
        protected internal Texture Texture_Reference { get; internal set; }
        protected int TEXTURE_HANDLE => Texture_Reference.TEXTURE_HANDLE;

        protected readonly Random Random;
        
        protected readonly int Channel_Count;
        protected readonly PixelInternalFormat Internal_Format;
        protected readonly PixelFormat Pixel_Format;
        protected readonly PixelType Pixel_Type;

        public Pixel_Initalizer
        (
            int channel_count,
            PixelInternalFormat internal_format,
            PixelFormat pixel_format,
            PixelType pixel_type,
            int? seed = null
        )
        {
            Channel_Count = channel_count;
            Internal_Format = internal_format;
            Pixel_Format = pixel_format;
            Pixel_Type = pixel_type;
            Random = 
                (seed != null)
                ? new Random((int)seed)
                : new Random()
                ;
        }

        public abstract void Reinitalize__Pixels
        (
            int width,
            int height
        );
    }

    public class Direct__Pixel_Initalizer :
    Pixel_Initalizer
    {
        public static Direct__Pixel_Initalizer Default
            =>
            new Direct__Pixel_Initalizer
            (
                4,
                PixelInternalFormat.Rgba,
                PixelFormat.Rgba,
                PixelType.UnsignedByte
            );

        private byte[]? byte_buffer;
        private int[]?  int_buffer;
        private float[]? float_buffer;

        private Random? randomizer;

        public void Set__Byte_Buffer
        (
            byte[]? byte_buffer
        )
        {
            this.byte_buffer = byte_buffer;
        }
        
        public Direct__Pixel_Initalizer
        (
            int channel_count, 
            PixelInternalFormat internal_format, 
            PixelFormat pixel_format, 
            PixelType pixel_type,
            byte[]? byte_buffer  = null,
            int[]? int_buffer   = null,
            float[]? float_buffer = null,
            Random? randomizer = null
        ) 
        : base
        (
            channel_count, 
            internal_format, 
            pixel_format, 
            pixel_type
        )
        {
            this.byte_buffer = byte_buffer;
            this.int_buffer = int_buffer;
            this.float_buffer = float_buffer;
            this.randomizer = randomizer;
        }

        public override void Reinitalize__Pixels(int width, int height)
        {
            GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);
            if (byte_buffer != null)
            {
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    Internal_Format,
                    width, height,
                    0,
                    Pixel_Format,
                    Pixel_Type,
                    byte_buffer
                );
                goto unbind_texture;
            }

            if (int_buffer != null)
            {
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    Internal_Format,
                    width, height,
                    0,
                    Pixel_Format,
                    Pixel_Type,
                    int_buffer
                );
                goto unbind_texture;
            }

            if (float_buffer != null)
            {
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    Internal_Format,
                    width, height,
                    0,
                    Pixel_Format,
                    Pixel_Type,
                    int_buffer
                );
                goto unbind_texture;
            }

            if (randomizer != null)
            {
                byte_buffer = new byte[width * height * Channel_Count];
                randomizer.NextBytes(byte_buffer);
                GL.TexImage2D
                (
                    TextureTarget.Texture2D,
                    0,
                    Internal_Format,
                    width, height,
                    0,
                    Pixel_Format,
                    Pixel_Type,
                    byte_buffer
                );
                goto unbind_texture;
            }

            GL.TexImage2D
            (
                TextureTarget.Texture2D,
                0,
                Internal_Format,
                width, height,
                0,
                Pixel_Format,
                Pixel_Type,
                IntPtr.Zero
            );
            
            unbind_texture:
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public class Normalized__Pixel_Initalizer :
    Pixel_Initalizer
    {
        public static Normalized__Pixel_Initalizer Default
            =>
            new Normalized__Pixel_Initalizer
            (
                4,
                PixelInternalFormat.Rgba,
                PixelFormat.Rgba
            );

        public Normalized__Pixel_Initalizer
        (
            int channel_count,
            PixelInternalFormat internal_format,
            PixelFormat pixel_format,
            int? seed = null
        ) 
        : base 
        (
            channel_count,
            internal_format,
            pixel_format,
            PixelType.UnsignedByte, //PixelType.Float,
            seed
        ) { }

        public override void Reinitalize__Pixels
        (
            int width,
            int height
        )
        {
            int double_count = width * height * Channel_Count;
            double[] pixels = new double[double_count];

            for(int i=0;i<double_count;i++)
                pixels[i] = Random.NextDouble();

            GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);

            GL.TexImage2D
            (
                TextureTarget.Texture2D,
                0,
                Internal_Format,
                width, height,
                0,
                Pixel_Format,
                Pixel_Type,
                pixels
            );

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }

    public class Unsigned_Byte__Pixel_Initalizer :
    Pixel_Initalizer
    {
        public static Unsigned_Byte__Pixel_Initalizer Default
            =>
            new Unsigned_Byte__Pixel_Initalizer
            (
                4,
                PixelInternalFormat.Rgba,
                PixelFormat.Rgba
            );

        public Unsigned_Byte__Pixel_Initalizer
        (
            int channel_count, 
            PixelInternalFormat internal_format, 
            PixelFormat pixel_format, 
            int? seed = null
        ) 
        : base
        (
            channel_count, 
            internal_format, 
            pixel_format, 
            PixelType.UnsignedByte,
            seed
        )
        {
        }

        public override void Reinitalize__Pixels
        (
            int width, 
            int height
        )
        {
            GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);
            byte[] pixels = new byte[width * height * Channel_Count];

            Random.NextBytes(pixels);

            GL.TexImage2D
            (
                TextureTarget.Texture2D,
                0,
                Internal_Format,
                width, height,
                0,
                Pixel_Format,
                Pixel_Type,
                pixels
            );
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
