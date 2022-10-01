
using Gwen.Net;
using Gwen.Net.Control;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Gwen_UI : ControlBase
{
    private readonly MenuStrip _menu;
    private readonly ControlBase _simulation_space;

    private bool _toggle__run;

    public event Action<Vector2i>? Resized__Grid;
    public event Action<bool>? Toggle__Run;
    public event Action? Pulsed__Step;

    public event Action<int, int>? Render__Grid;

    public Gwen_UI
    (
        ControlBase parent
    )
    : base(parent)
    {
        Dock = Dock.Fill;
        _menu = new MenuStrip(this);
        _menu.Dock = Dock.Top;

        MenuItem menu_item__file = new MenuItem(_menu) { Text = "File" };
        MenuItem menu_item__run = new MenuItem(_menu) { Text = "Run" };
        menu_item__run.Clicked += Private_Handle__Run_Click;
        MenuItem menu_item__step = new MenuItem(_menu) { Text = "Step" };
        menu_item__step.Clicked += (s, e) => Pulsed__Step?.Invoke();

        _simulation_space = new Gwen.Net.Control.Border(this);
        _simulation_space.Dock = Dock.Fill;
        _simulation_space.Hide();
    }

    protected override void Render(Gwen.Net.Skin.SkinBase skin)
    {
        skin.Renderer.End();
        GLHelper.Push_Viewport
        (
            _simulation_space.ActualPosition.X,
            _simulation_space.ActualPosition.Y,
            _simulation_space.ActualSize.Width,
            _simulation_space.ActualSize.Height
        );

        Render__Grid?.Invoke(_simulation_space.ActualSize.Width, _simulation_space.ActualSize.Height);

        GLHelper.Pop_Viewport();
        skin.Renderer.Begin();
    }

    private void Private_Handle__Run_Click
    (
        ControlBase sender, 
        ClickedEventArgs arguments
    )
    {
        _toggle__run = !_toggle__run;
        MenuItem _sender = (sender as MenuItem)!;
        _sender.Text = 
            (_toggle__run)
            ? "Stop"
            : "Run"
            ;

        Toggle__Run?.Invoke(_toggle__run);
    }
}
