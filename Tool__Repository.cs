
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

    public Tool Load__Tool(string path)
    {
        Tool tool = Tool.Load(path);

        RECORDED__TOOLS.Add(tool.Name, tool);

        return tool;
    }
}
