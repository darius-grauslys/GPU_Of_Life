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

public class Tool__Repository
{
    internal readonly Dictionary<string, Tool> RECORDED__TOOLS =
        new Dictionary<string, Tool>();

    public IEnumerable<Tool> TOOLS
        => RECORDED__TOOLS.Values;

    public Tool? TOOL__ACTIVE { get; private set; }
    public Shader.Invocation? TOOL__ACTIVE__CONFIGURATION { get; private set; }

    public void Set__Active_Tool(string tool_name)
    {
        if (RECORDED__TOOLS.ContainsKey(tool_name))
        {
            TOOL__ACTIVE = RECORDED__TOOLS[tool_name];
            TOOL__ACTIVE__CONFIGURATION =
                TOOL__ACTIVE.Get__Invocation();
        }
    }

    public Tool? Get__Tool(string tool_name)
        =>
        (RECORDED__TOOLS.ContainsKey(tool_name))
        ? RECORDED__TOOLS[tool_name]
        : null
        ;

    public Tool Load__Tool(string path)
    {
        Tool tool = Tool.Load(path);

        RECORDED__TOOLS.Add(tool.Name, tool);

        return tool;
    }
}
