/**************************************************************************
 *
 *    Copyright (c) 2022 Darius Grauslys
 *
 *    Permission is hereby granted, free of charge, to any person obtaining
 *    a copy of this software and associated documentation files (the
 *    "Software"), to deal in the Software without restriction, including
 *    without limitation the rights to use, copy, modify, merge, publish,
 *    distribute, sublicense, and/or sell copies of the Software, and to
 *    permit persons to whom the Software is furnished to do so, subject to
 *    the following conditions:
 *
 *    The above copyright notice and this permission notice shall be
 *    included in all copies or substantial portions of the Software.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 *    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 *    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 *    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 *    LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 *    OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 *    WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 **************************************************************************/

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

/// <summary>
///
/// </summary>
public static partial class GLHelper
{
    // It is helpful to pass this by reference, so it is a class.
    public class Viewport
    {
        private static readonly Stack<Viewport> Viewport_Stack =
            new Stack<Viewport>();

        public static Viewport Current => Viewport_Stack.Peek();

        public static void Initalize(Viewport v)
        {
            if (Viewport_Stack.Count != 0) return;

            Viewport_Stack.Push(v);
        }

        public static void Push(int x, int y, int width, int height)
        {
            Viewport_Stack.Push(new Viewport() { x = x, y = y, width = width, height = height });
            Set_Viewport();
        }

        public static void Push(Viewport v)
        {
            Viewport_Stack.Push(v);
            Set_Viewport();
        }

        public static void Pop()
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
}
