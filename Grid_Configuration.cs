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

namespace GPU_Of_Life;

public class Grid_Configuration
{
    public int Width  { set; get; }
    public int Height { set; get; }
    public bool Is__Using_New_Seed__For_Each_Reset { get; set; }
    public int? Seed { get; set; }

    public string? Image__Path { get; set; }
    public string? RLE__Path { get; set; }

    public Shader.Invocation? Reset__Tool_Invocation { get; set; }

    public override string ToString()
        => $"grid_configuration(seed:{Seed} width:{Width} height:{Height})";
}
