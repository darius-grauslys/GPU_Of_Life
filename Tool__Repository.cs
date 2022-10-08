
namespace GPU_Of_Life;

public class Tool__Repository
{
    private readonly Dictionary<string, Tool> TOOLS =
        new Dictionary<string, Tool>();

    private Tool? TOOL__ACTIVE;

    public Shader.Invocation? Get__Active_Tool__Invocation()
    {
        return TOOL__ACTIVE?.Get__Invocation();
    }

    public void Set__Active_Tool(string tool_name)
    {
        if (TOOLS.ContainsKey(tool_name))
            TOOL__ACTIVE = TOOLS[tool_name];
    }

    public void Load__Tool(string path)
    {
        Tool tool = Tool.Load(path);

        TOOLS.Add(tool.Name, tool);
    }
}
