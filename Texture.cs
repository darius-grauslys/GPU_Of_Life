
using System.Diagnostics.CodeAnalysis;
using OpenTK.Graphics.OpenGL;

namespace GPU_Of_Life;

public class Texture : IDisposable
{
    private static int MAX_SIZE = -1;

    public readonly int TEXTURE_HANDLE;
    private int _width = 10, _height = 10;
    private int? _seed = null;
    public int Width
    {
        get => _width;
        set 
        {
            value = Math.Max(1, Math.Min(MAX_SIZE, value));
            Resize__Texture
            (
                _width, _height,
                value, _height
            );
            _width = value;
        }
    }
    public int Height
    {
        get => _height;
        set
        {
            value = Math.Max(1, Math.Min(MAX_SIZE, value));
            Resize__Texture
            (
                _width, _height,
                _width, value
            );
            _height = value;
        }
    }

    public Pixel_Initalizer Pixel_Buffer_Initalizer { get; }

    public Texture
    (
        int width, int height,
        int channel_count,
        PixelInternalFormat? internal_format = null,
        PixelFormat? pixel_format = null
    )
    : this
    (
        width, height,
        new Direct__Pixel_Initalizer
        (
            channel_count,
            internal_format ?? PixelInternalFormat.Luminance,
            pixel_format ?? PixelFormat.Luminance,
            PixelType.UnsignedByte
        )
    )
    { }
    
    public Texture
    (
        int width, int height,
        Pixel_Initalizer? pixel_initalizer = null
    ) 
    {
        if (MAX_SIZE < 0)
        {
            MAX_SIZE = GL.GetInteger(GetPName.MaxTextureSize);
            if (MAX_SIZE < 0)
                throw new Exception("OpenGL Context is invalid.");
        }

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

    public void Resize__Texture
    (
        int old_width, 
        int old_height,
        int new_width,
        int new_height,
        bool is__throwing_exception = true
    )
    {
        bool drop = false;
        Resize__Texture
        (
            old_width, old_height,
            new_width, new_height,
            ref drop
        );
        if (drop && is__throwing_exception)
            throw new Exception("Texture resize failed.");
    }
    
    public void Resize__Texture
    (
        int new_width,
        int new_height,
        ref bool error
    )
    {
        Resize__Texture
        (
            _width,    _height,
            new_width, new_height,
            ref error
        );
    }

    public void Resize__Texture
    (
        int old_width, 
        int old_height,
        int new_width,
        int new_height,
        ref bool error
    )
    {
        GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);
        Pixel_Buffer_Initalizer
            .Resize
            (
                old_width, old_height,
                new_width, new_height,
                ref error
            );
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Dispose()
    {
        GL.DeleteTexture(TEXTURE_HANDLE);
    }

    public abstract class Pixel_Initalizer
    {
        [AllowNull] //dep-injected
        protected internal Texture Texture_Reference { get; internal set; }
        protected int TEXTURE_HANDLE => Texture_Reference.TEXTURE_HANDLE;

        private int? seed;
        public int? Seed { get => seed; set => Randomizer = value != null ? new Random((int)(seed = value)!) : null; }
        protected Random? Randomizer { get; private set; }
        
        public readonly int Channel_Count;
        public readonly PixelInternalFormat Internal_Format;
        public readonly PixelFormat Pixel_Format;
        public readonly PixelType Pixel_Type;

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
            Randomizer = 
                (seed != null)
                ? new Random((int)seed)
                : null
                ;
            Seed = seed;
        }

        public virtual void Resize
        (
            int old_width, int old_height, 
            int new_width, int new_height,
            ref bool error
        )
        {
            if (error) return;
            Reinitalize__Pixels(new_width, new_height);
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

        public Byte[]? Buffer__Bytes
            => byte_buffer?.ToArray();

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
            int? seed = null
        ) 
        : base
        (
            channel_count, 
            internal_format, 
            pixel_format, 
            pixel_type,
            seed
        )
        {
            this.byte_buffer = byte_buffer;
            this.int_buffer = int_buffer;
            this.float_buffer = float_buffer;
        }

        public override void Resize
        (
            int old_width, int old_height, 
            int new_width, int new_height,
            ref bool error
        )
        {
            if (error) return;
            GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);

            byte   fill__byte  () =>   (byte?)Randomizer?.Next()       ?? (byte)0;
            int    fill__int   () =>    (int?)Randomizer?.Next()       ?? 0;
            float  fill__float () =>  (float?)Randomizer?.NextDouble() ?? 0.0f;

            if (byte_buffer != null)
            {
                byte_buffer = Private_Resize
                (
                    byte_buffer,
                    old_width, old_height,
                    new_width, new_height,
                    ref error,
                    (x,y) => fill__byte()
                );
                goto unbind_texture;
            }

            if (int_buffer != null)
            {
                int_buffer = Private_Resize
                (
                    int_buffer,
                    old_width, old_height,
                    new_width, new_height,
                    ref error,
                    (x,y) => fill__int()
                );
                goto unbind_texture;
            }

            if (float_buffer != null)
            {
                float_buffer = Private_Resize
                (
                    float_buffer,
                    old_width, old_height,
                    new_width, new_height,
                    ref error,
                    (x,y) => fill__float()
                );
                goto unbind_texture;
            }

unbind_texture:
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        private void Private_Reinitalize__Pixels(int width, int height, bool randomize)
        {
            GL.BindTexture(TextureTarget.Texture2D, TEXTURE_HANDLE);
            if (Randomizer != null)
            {
                byte_buffer = new byte[width * height * Channel_Count];
                Randomizer.NextBytes(byte_buffer);
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

        public override void Reinitalize__Pixels(int width, int height)
            => Private_Reinitalize__Pixels(width, height, true);

        private T[] Private_Resize<T>
        (
            T[] source, 
            int old_width, int old_height, 
            int new_width, int new_height,
            ref bool error,
            Func<int,int,T>? func__filler = null
        )
        where T : struct
        {
            if (error) return new T[0];
            error |= old_width  < 0 || new_width  < 0;
            error |= old_height < 0 || new_height < 0;
            if (error) return new T[0];

            int min_width  = Math.Min(old_width, new_width);
            int min_height = Math.Min(old_height, new_height);

            T[] resized = new T[new_width * new_height * Channel_Count];

            for(int y=0;y<min_height;y++)
                for(int x=0;x<min_width;x++)
                    resized[x + y * old_width] = source[x + y * old_width];

            if (func__filler == null)
                return resized;

            bool fill_width, fill_height;
            if (fill_width = old_width < new_width)
                Private_Fill__Range<T>
                (
                    resized,
                    new_width,
                    old_width, 0,
                    new_width, min_height,
                    func__filler
                );
            if (fill_height = old_height < new_height)
                Private_Fill__Range<T>
                (
                    resized,
                    min_width,
                    0, old_height,
                    old_width, new_height,
                    func__filler
                );
            if (fill_width && fill_height)
                Private_Fill__Range<T>
                (
                    resized,
                    min_width,
                    old_width, old_height,
                    new_width, new_height,
                    func__filler
                );

            return resized;
        }

        private void Private_Fill__Range<T>
        (
            T[] source, 
            int width,
            int min_x, int min_y, 
            int max_x, int max_y,
            Func<int, int, T> func__filler
        )
        where T : struct
        {
            for(int y=min_y;y<max_y;y++)
                for(int x=min_x;x<max_x;x++)
                    source[x + y * width] = func__filler(x,y);
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
            seed ?? new Random().Next()
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
                pixels[i] = Randomizer!.NextDouble();

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
            seed ?? new Random().Next()
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

            Randomizer!.NextBytes(pixels);

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
