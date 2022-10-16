
namespace GPU_Of_Life;

public class Grid_Configuration
{
    public int Width  { set; get; }
    public int Height { set; get; }
    public bool Is__Using_New_Seed__For_Each_Reset { get; set; }
    public int? Seed { get; set; }

    public string? Image__Path { get; set; }

    public override string ToString()
        => $"grid_configuration(seed:{Seed} width:{Width} height:{Height})";
}
