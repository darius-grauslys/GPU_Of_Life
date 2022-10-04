
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Viewport
{
    public int x, y;
    public int width, height;

    public Viewport(){}
    public Viewport(Vector2i size)
    {
        width = size.X;
        height = size.Y;
    }

    public void Resize(Vector2i size)
    {
        width = size.X;
        height = size.Y;
    }

    public override string ToString()
        => $"Viewport(x:{x}, y:{y} - w:{width}, h:{height})";
}

public static class GLHelper
{
    private static readonly Stack<Viewport> Viewport_Stack =
        new Stack<Viewport>();

    public static Viewport Current => Viewport_Stack.Peek();

    public static void Initalize(Viewport v)
    {
        if (Viewport_Stack.Count != 0) return;

        Viewport_Stack.Push(v);
    }

    public static void Push_Viewport(int x, int y, int width, int height)
    {
        Viewport_Stack.Push(new Viewport() { x = x, y = y, width = width, height = height });
        Set_Viewport();
    }

    public static void Push_Viewport(Viewport v)
    {
        Viewport_Stack.Push(v);
        Set_Viewport();
    }

    public static void Pop_Viewport()
    {
        if (Viewport_Stack.Count == 1) return;

        Viewport_Stack.Pop();
        Set_Viewport();
    }

    internal static void Set_Viewport()
    {
        Viewport v = Viewport_Stack.Peek();
        GL.Viewport(v.x, v.y, v.width, v.height);
    }
}
