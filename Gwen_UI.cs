
using Gwen.Net;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;

namespace GPU_Of_Life;

public class Gwen_UI : ControlBase
{
    private readonly MenuStrip _menu;
    private readonly ControlBase _simulation_space;

    private readonly Label _seed;
    public void Set__Seed(int? seed) => _seed.Text = $"Seed: {seed}";

    private bool _toggle__run;

    public event Action<Grid_Configuration>? Invoked__New;
    public event Action? Invoked__Reset;
    public event Action<bool>? Toggle__Run;
    public event Action? Pulsed__Step;

    public event Action<int, int>? Render__Grid;

    public event Action<float>? Updated__Compute_Speed;

    public Gwen_UI
    (
        ControlBase parent
    )
    : base(parent)
    {
        Dock = Dock.Fill;

        DockLayout dock_layout = new DockLayout(this);

        _menu = new MenuStrip(dock_layout);
        _menu.Dock = Dock.Top;

        // file
        // - new
        // - load
        // - save
        // - save as
        MenuItem menu_item__file = new MenuItem(_menu) { Text = "File" };
        {
            menu_item__file.Menu.AddItem("New").Clicked += (s,e) => Private_Dialog__New_Configuration();
            menu_item__file.Menu.AddItem("Load").Clicked += (s,e) => { };
            menu_item__file.Menu.AddItem("Save").Clicked += (s,e) => { };
            menu_item__file.Menu.AddItem("Save As").Clicked += (s,e) => { };
        }

        // edit
        // - resize 
        // - shift
        // - set life rule
        // - set compute shader
        // - set render shader

        MenuItem menu_item__edit = new MenuItem(_menu) { Text = "Edit" };
        {
            menu_item__edit.Menu.AddItem("Resize").Clicked += (s,e) => { };
            menu_item__edit.Menu.AddItem("Shift").Clicked += (s,e) => { };
            menu_item__edit.Menu.AddItem("Configure").Clicked += (s,e) => { };
        }

        // tool
        // - load tools
        // - open tool editor
        MenuItem menu_item__simulation = new MenuItem(_menu) { Text = "Simulation" };
        {
            MenuItem menu_item__reset = menu_item__simulation.Menu.AddItem("Reset\t(F1)");
            menu_item__reset.Clicked += (s, e) => Invoked__Reset?.Invoke();
            MenuItem menu_item__run = menu_item__simulation.Menu.AddItem("Run\t(F5)");
            menu_item__run.Clicked += Private_Handle__Run_Click;
            MenuItem menu_item__step = menu_item__simulation.Menu.AddItem("Step\t(F6)");
            menu_item__step.Clicked += (s, e) => Pulsed__Step?.Invoke();
        }

        _simulation_space = new Gwen.Net.Control.Border(dock_layout);
        _simulation_space.Dock = Dock.Fill;
        _simulation_space.Hide();

        StatusBar bar = new StatusBar(dock_layout);
        bar.Dock = Dock.Bottom;

        HorizontalSlider simulation_speed = new HorizontalSlider(bar);
        simulation_speed.ValueChanged += (s,e) => Updated__Compute_Speed?.Invoke(simulation_speed.Value);
        simulation_speed.NotchCount = 10;
        simulation_speed.SnapToNotches = true;
        simulation_speed.Width = 100;
        new Label(bar) { Text = "Simulation Speed: " };

        _seed = new Label(bar) { Dock = Dock.Left };
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

    private void Private_Dialog__New_Configuration()
    {
        Grid_Configuration grid_configuration = new Grid_Configuration();

        Window dialog = new Window(this);
        dialog.Title = "New Grid";
        dialog.Width = 300;
        dialog.Height = 300;
        dialog.Position = new Point(ActualSize.Width/2 - dialog.Width/2, ActualSize.Height/2 - dialog.Height/2);

        DockLayout layout = new DockLayout(dialog);

        new Label(layout) { Text = "Width: ", Dock = Dock.Top };
        NumericUpDown width  = new NumericUpDown(layout);
        width.Size = new Size(100, 25);
        width.Min = 1; width.Max = 1000;
        width.Value = grid_configuration.Width = 100;
        width.Dock = Dock.Top;
        width.ValueChanged += 
            (s,e) => grid_configuration.Width = (int)width.Value;
        new Label(layout) { Text = "Height : ", Dock = Dock.Top };
        NumericUpDown height = new NumericUpDown(layout);
        height.Size = new Size(100, 25);
        height.Min = 1; height.Max = 1000;
        height.Value = grid_configuration.Height = 100;
        height.Dock = Dock.Top;
        height.ValueChanged += 
            (s,e) => grid_configuration.Height = (int)height.Value;

        LabeledCheckBox is__random_or_direct = new LabeledCheckBox(layout);
        is__random_or_direct.Dock = Dock.Top;
        is__random_or_direct.Text = "Is Random on Reset";

        LabeledCheckBox is__new_seed_each_time = new LabeledCheckBox(layout);
        is__new_seed_each_time.Dock = Dock.Top;
        is__new_seed_each_time.Text = "Use a new seed each time";
        is__new_seed_each_time.IsHidden = true;
        
        NumericUpDown seed = new NumericUpDown_AsInt(dialog);
        seed.Size = new Size(150, 25);
        seed.Dock = Dock.Top;
        seed.Max = float.MaxValue;
        seed.Value = new Random().Next();
        seed.IsHidden = true;

        void set_seed()
        {
            grid_configuration.Seed = 
                (seed.IsHidden || seed.IsDisabled)
                ? null
                :(int)seed.Value
                ;
        }

        seed.ValueChanged += 
            (s,e) => set_seed();

        is__random_or_direct.CheckChanged += 
            (s,e) => 
            {
                seed.IsHidden = !is__random_or_direct.IsChecked;
                is__new_seed_each_time.IsHidden = !is__random_or_direct.IsChecked;
                set_seed();
            };
        is__new_seed_each_time.CheckChanged +=
            (s,e) =>
            {
                grid_configuration.Is__Using_New_Seed__For_Each_Reset =
                    is__new_seed_each_time.IsChecked;
                seed.IsDisabled = is__new_seed_each_time.IsChecked;
                set_seed();
            };

        StatusBar selection = new StatusBar(layout);
        selection.Dock = Dock.Bottom;

        new Button(selection) 
            { Text = "Okay", Dock = Dock.Right }
            .Clicked += (s,e) => {dialog.Close(); Invoked__New?.Invoke(grid_configuration); };
        new Button(selection) 
            { Text = "Cancel", Dock = Dock.Right }
            .Clicked += (s,e) => dialog.Close();

        is__random_or_direct.IsChecked = true;
        dialog.Show();
    }

    private class NumericUpDown_AsInt : NumericUpDown
    {
        public NumericUpDown_AsInt(ControlBase parent)
            : base(parent)
        {}

        public override float Value
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                Text = ((int)value).ToString();
            }
        }
    }
}
