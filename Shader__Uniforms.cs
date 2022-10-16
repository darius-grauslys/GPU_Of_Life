
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public static class Uniform__Utility
{
    public static string As__String<T>(this Shader.IUniform<T> uniform)
    where T : struct
    {
        return $"uniform_{typeof(T)}({uniform.Name}, {uniform.Value})";
    }
}


public partial class Shader
{
    public interface IUniform
    {
        public string Name { get; }

        public static IUniform From<T>(string name, int value)
        {
            if (typeof(T) == typeof(int))
            {
                return new Uniform__Int(name, value);
            }
            if (typeof(T) == typeof(uint))
            {
                return new Uniform__Unsigned_Int(name, (uint)value);
            }
            if (typeof(T) == typeof(float))
            {
                return new Uniform__Float(name, (float)value);
            }
            if (typeof(T) == typeof(double))
            {
                return new Uniform__Double(name, (double)value);
            }

            throw new ArgumentException($"Type: {typeof(T)} is not valid when casting {value.GetType()} field to a Uniform1.");
        }

        public static IUniform From<T>(string name, uint value)
        {
            if (typeof(T) == typeof(int))
            {
                return new Uniform__Int(name, (int)value);
            }
            if (typeof(T) == typeof(uint))
            {
                return new Uniform__Unsigned_Int(name, value);
            }
            if (typeof(T) == typeof(float))
            {
                return new Uniform__Float(name, (float)value);
            }
            if (typeof(T) == typeof(double))
            {
                return new Uniform__Double(name, (double)value);
            }

            throw new ArgumentException($"Type: {typeof(T)} is not valid when casting {value.GetType()} field to a Uniform1.");
        }

        public static IUniform From<T>(string name, float value)
        {
            if (typeof(T) == typeof(int))
            {
                return new Uniform__Int(name, (int)value);
            }
            if (typeof(T) == typeof(uint))
            {
                return new Uniform__Unsigned_Int(name, (uint)value);
            }
            if (typeof(T) == typeof(float))
            {
                return new Uniform__Float(name, value);
            }
            if (typeof(T) == typeof(double))
            {
                return new Uniform__Double(name, (double)value);
            }

            throw new ArgumentException($"Type: {typeof(T)} is not valid when casting {value.GetType()} field to a Uniform1.");
        }

        public static IUniform From<T>(string name, double value)
        {
            if (typeof(T) == typeof(int))
            {
                return new Uniform__Int(name, (int)value);
            }
            if (typeof(T) == typeof(uint))
            {
                return new Uniform__Unsigned_Int(name, (uint)value);
            }
            if (typeof(T) == typeof(float))
            {
                return new Uniform__Float(name, (float)value);
            }
            if (typeof(T) == typeof(double))
            {
                return new Uniform__Double(name, value);
            }

            throw new ArgumentException($"Type: {typeof(T)} is not valid when casting {value.GetType()} field to a Uniform1.");
        }
    }

    public interface IUniform<T> : IUniform
    where T : struct
    {
        public T Value { get; set; }
    }

    public struct Uniform__Int : IUniform<int>
    {
        public string Name { get; }
        internal int Internal__Value;
        public int Value { get => Internal__Value; set => Internal__Value = value; }

        public Uniform__Int(string name, int value)
        {
            Name = name;
            Internal__Value = value;
        }
    }

    public struct Uniform__Unsigned_Int : IUniform<uint>
    {
        public string Name { get; }
        internal uint Internal__Value;
        public uint Value { get => Internal__Value; set => Internal__Value = value; }

        public Uniform__Unsigned_Int(string name, uint value)
        {
            Name = name;
            Internal__Value = value;
        }
    }

    public struct Uniform__Float : IUniform<float>
    {
        public string Name { get; }
        internal float Internal__Value;
        public float Value { get => Internal__Value; set => Internal__Value = value; }

        public Uniform__Float(string name, float value)
        {
            Name = name;
            Internal__Value = value;
        }
    }

    public struct Uniform__Double : IUniform<double>
    {
        public string Name { get; }
        internal double Internal__Value;
        public double Value { get => Internal__Value; set => Internal__Value = value; }

        public Uniform__Double(string name, double value)
        {
            Name = name;
            Internal__Value = value;
        }
    }

    public struct Uniform__Vector2 : IUniform<Vector2>
    {
        public string Name { get; }
        internal Vector2 Internal__Value;
        public Vector2 Value { get => Internal__Value; set => Internal__Value = value; }

        public Uniform__Vector2(string name, Vector2 value)
        {
            Name = name;
            Internal__Value = value;
        }
    }

    public interface IUniform__Clamped<T> : IUniform<T>
    where T : struct
    {
        public T Max { get; set; }
        public T Min { get; set; }
    }

    public struct Uniform__Int__Clamped : IUniform__Clamped<int>
    {
        public string Name { get; }
        internal int Internal__Value;
        
        internal int Internal__Min = int.MinValue, Internal__Max = int.MaxValue;

        public int Max 
        {
            get => Internal__Max;
            set 
            {
                Internal__Max =
                    Math.Max(value, Internal__Min);
                Internal__Value =
                    Math.Min(Internal__Value, Internal__Max);
            }
        }
        public int Min
        {
            get => Internal__Min;
            set 
            {
                Internal__Min =
                    Math.Min(value, Internal__Max);
                Internal__Value =
                    Math.Max(Internal__Value, Internal__Min);
            }
        }

        public int Value 
        {
            get => Internal__Value; 
            set => Internal__Value = Math.Max(Internal__Min, Math.Min(Internal__Max, value)); 
        }

        public Uniform__Int__Clamped
        (
            string name, 
            int value,
            int min,
            int max
        )
        {
            Name = name;

            Internal__Value = 0;
            Internal__Min = min;
            Max = max;
            Value = value;
        }

        public static explicit operator Uniform__Int(Uniform__Int__Clamped uniform_int_clamped)
            => new Uniform__Int(uniform_int_clamped.Name, uniform_int_clamped.Value);
    }

    public struct Uniform__Unsigned_Int__Clamped : IUniform__Clamped<uint>
    {
        public string Name { get; }
        internal uint Internal__Value;
        
        internal uint Internal__Min = uint.MinValue, Internal__Max = uint.MaxValue;

        public uint Max 
        {
            get => Internal__Max;
            set 
            {
                Internal__Max =
                    Math.Max(value, Internal__Min);
                Internal__Value =
                    Math.Min(Internal__Value, Internal__Max);
            }
        }
        public uint Min
        {
            get => Internal__Min;
            set 
            {
                Internal__Min =
                    Math.Min(value, Internal__Max);
                Internal__Value =
                    Math.Max(Internal__Value, Internal__Min);
            }
        }

        public uint Value 
        {
            get => Internal__Value; 
            set => Internal__Value = (uint)Math.Max(Internal__Min, Math.Min(Internal__Max, value)); 
        }

        public Uniform__Unsigned_Int__Clamped 
        (
            string name, 
            uint value,
            uint min,
            uint max
        )
        {
            Name = name;

            Internal__Value = 0;
            Internal__Min = min;
            Max = max;
            Value = value;
        }

        public static explicit operator 
        Uniform__Unsigned_Int
        (
            Uniform__Unsigned_Int__Clamped uniform_unsigned_int_clamped
        )
            => new Uniform__Unsigned_Int
            (
                uniform_unsigned_int_clamped.Name, 
                uniform_unsigned_int_clamped.Value
            );
    }

    public struct Uniform__Float__Clamped : IUniform__Clamped<float>
    {
        public string Name { get; }
        internal float Internal__Value;
        
        internal float Internal__Min = float.MinValue, Internal__Max = float.MaxValue;

        public float Max 
        {
            get => Internal__Max;
            set 
            {
                Internal__Max =
                    Math.Max(value, Internal__Min);
                Internal__Value =
                    Math.Min(Internal__Value, Internal__Max);
            }
        }
        public float Min
        {
            get => Internal__Min;
            set 
            {
                Internal__Min =
                    Math.Min(value, Internal__Max);
                Internal__Value =
                    Math.Max(Internal__Value, Internal__Min);
            }
        }

        public float Value 
        {
            get => Internal__Value; 
            set => Internal__Value = (float)Math.Max(Internal__Min, Math.Min(Internal__Max, value)); 
        }

        public Uniform__Float__Clamped 
        (
            string name, 
            float value,
            float min,
            float max
        )
        {
            Name = name;

            Internal__Value = 0;
            Internal__Min = min;
            Max = max;
            Value = value;
        }

        public static explicit operator 
        Uniform__Float
        (
            Uniform__Float__Clamped uniform_float_clamped
        )
            => new Uniform__Float
            (
                uniform_float_clamped.Name, 
                uniform_float_clamped.Value
            );
    }

    public struct Uniform__Double__Clamped : IUniform__Clamped<double>
    {
        public string Name { get; }
        internal double Internal__Value;
        
        internal double Internal__Min = double.MinValue, Internal__Max = double.MaxValue;

        public double Max 
        {
            get => Internal__Max;
            set 
            {
                Internal__Max =
                    Math.Max(value, Internal__Min);
                Internal__Value =
                    Math.Min(Internal__Value, Internal__Max);
            }
        }
        public double Min
        {
            get => Internal__Min;
            set 
            {
                Internal__Min =
                    Math.Min(value, Internal__Max);
                Internal__Value =
                    Math.Max(Internal__Value, Internal__Min);
            }
        }


        public double Value 
        {
            get => Internal__Value; 
            set => Internal__Value = (double)Math.Max(Internal__Min, Math.Min(Internal__Max, value)); 
        }

        public Uniform__Double__Clamped 
        (
            string name, 
            double value,
            double min,
            double max
        )
        {
            Name = name;

            Internal__Min = float.MinValue;
            Internal__Max = float.MaxValue;
            Internal__Value = 0;
            Internal__Min = min;
            Max = max;
            Value = value;
        }

        public static explicit operator 
        Uniform__Double
        (
            Uniform__Double__Clamped uniform_double_clamped
        )
            => new Uniform__Double
            (
                uniform_double_clamped.Name, 
                uniform_double_clamped.Value
            );
    }

    public struct Uniform__Vector2__Clamped : IUniform__Clamped<Vector2>
    {
        public string Name { get; }
        internal Vector2 Internal__Value;
        
        internal Vector2 
            Internal__Min = 
                new Vector2(float.MinValue, float.MinValue), 
            Internal__Max = 
                new Vector2(float.MaxValue, float.MaxValue);

        public Vector2 Max 
        {
            get => Internal__Max;
            set 
            {
                value.X = (value.X >= Internal__Min.X) ? value.X : Internal__Min.X;
                value.Y = (value.Y >= Internal__Min.Y) ? value.Y : Internal__Min.Y;
                Internal__Max = value;
                Internal__Value.X = (Internal__Max.X >= Internal__Value.X) ? Internal__Value.X : Internal__Max.X;
                Internal__Value.Y = (Internal__Max.Y >= Internal__Value.Y) ? Internal__Value.Y : Internal__Max.Y;
            }
        }
        public Vector2 Min
        {
            get => Internal__Min;
            set 
            {
                value.X = (value.X >= Internal__Max.X) ? value.X : Internal__Max.X;
                value.Y = (value.Y >= Internal__Max.Y) ? value.Y : Internal__Max.Y;
                Internal__Min = value;
                Internal__Value.X = (Internal__Min.X <= Internal__Value.X) ? Internal__Value.X : Internal__Min.X;
                Internal__Value.Y = (Internal__Min.Y <= Internal__Value.Y) ? Internal__Value.Y : Internal__Min.Y;
            }
        }

        public Vector2 Value 
        {
            get => Internal__Value; 
            set
            {
                value.X = (Internal__Max.X >= value.X) ? value.X : Internal__Max.X;
                value.Y = (Internal__Max.Y >= value.Y) ? value.Y : Internal__Max.Y;
                value.X = (Internal__Min.X <= value.X) ? value.X : Internal__Min.X;
                value.Y = (Internal__Min.Y <= value.Y) ? value.Y : Internal__Min.Y;
                Internal__Value = value;
            }
        }

        public Uniform__Vector2__Clamped 
        (
            string name, 
            Vector2 value,
            Vector2 min,
            Vector2 max
        )
        {
            Name = name;

            Internal__Value = new Vector2();
            Internal__Min = min;
            Max = max;
            Value = value;
        }

        public static explicit operator 
        Uniform__Vector2
        (
            Uniform__Vector2__Clamped uniform_Vector2_clamped
        )
            => new Uniform__Vector2
            (
                uniform_Vector2_clamped.Name, 
                uniform_Vector2_clamped.Value
            );
    }
}
