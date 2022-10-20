
using Gwen.Net;
using Gwen.Net.CommonDialog;
using Gwen.Net.Control;
using Gwen.Net.Control.Layout;
using OpenTK.Mathematics;

namespace GPU_Of_Life;

public class Gwen_UI : ControlBase
{
    private readonly MenuStrip _menu;
    private readonly ControlBase _simulation_space;

    public int GRID__X
        => _simulation_space.ActualPosition.X;
    public int GRID__Y
        => _simulation_space.ActualPosition.Y;
    public int GRID__WIDTH
        => _simulation_space.ActualWidth;
    public int GRID__HEIGHT
        => _simulation_space.ActualHeight;

    private readonly Label _seed;
    public void Set__Seed(int? seed) => _seed.Text = $"Seed: {seed}";

    private bool _toggle__run;

    public event Action<bool>? Status__UI_Busy;

    public event Action<Grid_Configuration>? Invoked__New;
    public event Action<Grid_Configuration>? Invoked__Load;
    public event Action<string>? Invoked__Save;

    public event Action? Invoked__Reset;
    public event Action<bool>? Toggle__Run;
    public event Action? Pulsed__Step;

    public event Action<int, int>? Render__Grid;

    public event Action<float>? Updated__Compute_Speed;

    public event Action<string>? Updated__Tool_Selection;
    public event Action<Shader.IUniform>? Updated__Tool_Uniform;

    public event Func<string, Shader.Invocation?>? Request__Tool_Invocation;

    public event Func<string, string?>? Loaded__Tool;
    public event Func<string, string?>? Loaded__Grid_Shader;
    public event Func<string, string?>? Loaded__Grid_Compute_Shader;

    private UI__Tool_Invocation_Field Tool__Fields;
    private ControlBase Tool__Selection;
    private readonly List<string> Tool__Loaded_Tools = new List<string>();

    public Gwen_UI
    (
        ControlBase parent
    )
    : base(parent)
    {
        Dock = Dock.Fill;

        DockBase dock_layout = new DockBase(this);

        _menu = new MenuStrip(this);
        _menu.Dock = Dock.Top;

        // file
        // - new
        // - load
        // - save
        // - save as
        MenuItem menu_item__file = new MenuItem(_menu) { Text = "File" };
        {
            menu_item__file.Menu.AddItem("New").Clicked += (s,e) => Private_Dialog__New_Configuration();
            menu_item__file.Menu.AddItem("Load").Clicked += (s,e) => Private_Dialog__Load();
            menu_item__file.Menu.AddItem("Save").Clicked += (s,e) => Private_Dialog__Save();
            menu_item__file.Menu.AddItem("Save As").Clicked += (s,e) => Private_Dialog__Save();
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
            menu_item__edit.Menu.AddItem("Load Tool").Clicked += 
                (s,e) => Private_Dialog__Load_Shader
                (
                    "Select Tool Directory",
                    path => Loaded__Tool?.Invoke(path)
                );
            menu_item__edit.Menu.AddItem("Load Grid Shader").Clicked +=
                (s,e) => Private_Dialog__Load_Shader
                (
                    "Select Grid Shader Directory",
                    path => Loaded__Grid_Shader?.Invoke(path)
                );
            menu_item__edit.Menu.AddItem("Load Compute Shader").Clicked +=
                (s,e) => Private_Dialog__Load_Shader
                (
                    "Select Grid Compute Shader Directory",
                    path => Loaded__Grid_Compute_Shader?.Invoke(path)
                );
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

        StatusBar bar = new StatusBar(this);
        bar.Dock = Dock.Bottom;

        GridLayout Tool__Panel = new GridLayout(this)
        {
            Dock = Dock.Fill
        };
        Tool__Panel.SetColumnWidths(1);
        Tool__Panel.SetRowHeights(0.25f, 0.75f);
        Border b = new Border(Tool__Panel) { Dock = Dock.Fill, BorderType = BorderType.ListBox };

        Tool__Selection = new FlowLayout(b) { Dock = Dock.Fill };

        Tool__Fields = new UI__Tool_Invocation_Field(Tool__Panel) 
        { 
            Dock = Dock.Fill
        };

        dock_layout.LeftDock.TabControl.AddPage("Tools", Tool__Panel);

        _simulation_space = new Gwen.Net.Control.Border(dock_layout);
        _simulation_space.Dock = Dock.Fill;
        _simulation_space.Hide();
        //_simulation_space.Margin = new Margin(0, _menu.ActualSize.Height, 0, bar.ActualSize.Height);
        _simulation_space.Margin = new Margin(0, 27, 0, 27);

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

    private void Private_Dialog__Load()
    {
        OpenFileDialog dialog =
            Gwen.Net.Xml.Component.Create<OpenFileDialog>(this);
        Status__UI_Busy?.Invoke(true);

        dialog.Title = "Load Image";
        dialog.InitialFolder = Directory.GetCurrentDirectory();

        dialog.Callback +=
            file_path => 
            {
                Status__UI_Busy?.Invoke(false);
                if (file_path == null)
                    return;

                Grid_Configuration grid_configuration =
                    new Grid_Configuration();

                if (file_path.Contains(".rle"))
                    grid_configuration.RLE__Path = file_path;
                else
                    grid_configuration.Image__Path = file_path;
                Invoked__Load?.Invoke(grid_configuration);
            };
    }

    private void Private_Dialog__Save()
    {
        SaveFileDialog dialog =
            Gwen.Net.Xml.Component.Create<SaveFileDialog>(this);
        Status__UI_Busy?.Invoke(true);

        dialog.Title = "Save Image";
        dialog.InitialFolder = Directory.GetCurrentDirectory();

        dialog.Callback +=
            file_path => 
            {
                Status__UI_Busy?.Invoke(false);
                if (file_path == null)
                    return;

                Invoked__Save?.Invoke(file_path);
            };
    }

    private void Private_Dialog__Load_Shader
    (
        string message,
        Func<string, string?> callback
    )
    {
        FolderBrowserDialog dialog =
            Gwen.Net.Xml.Component.Create<FolderBrowserDialog>(this);
        Status__UI_Busy?.Invoke(true);

        dialog.Title = message;
        dialog.InitialFolder = Directory.GetCurrentDirectory();

        dialog.Callback +=
            file_path => 
            {
                Status__UI_Busy?.Invoke(false);
                if (file_path == null)
                    return;

                string? status = callback.Invoke(file_path);
                if (status != null)
                    new MessageBox(this, status, "Failed to load.", MessageBoxButtons.OK).Show();
            };
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

        LabeledCheckBox is__with_initial_invocation = new LabeledCheckBox(layout);
        is__with_initial_invocation.Dock = Dock.Top;
        is__with_initial_invocation.Text = "Execute tool on initalization";
        is__with_initial_invocation.IsHidden = true;

        ComboBox combo_box__select_tool = new ComboBox(layout);
        combo_box__select_tool.Size = new Size(150, 25);
        combo_box__select_tool.Dock = Dock.Top;
        combo_box__select_tool.AddItem("None");
        combo_box__select_tool.IsHidden = true;
        foreach(string tool_name in Tool__Loaded_Tools)
            combo_box__select_tool.AddItem(tool_name);

        UI__Tool_Invocation_Field invocation_field = new UI__Tool_Invocation_Field(layout);
        invocation_field.Size = new Size(Util.Ignore, Util.Ignore);
        invocation_field.Dock = Dock.Fill;
        invocation_field.IsHidden = true;

        is__with_initial_invocation.CheckChanged += 
            (s,e) =>
            {
                if (!is__with_initial_invocation.IsChecked)
                    combo_box__select_tool.SelectedIndex = 0;
                combo_box__select_tool.IsHidden = !is__with_initial_invocation.IsChecked;
                invocation_field.IsHidden = !is__with_initial_invocation.IsChecked;
            };

        combo_box__select_tool.ItemSelected +=
            (s,e) =>
            {
                Shader.Invocation? invocation =
                    (combo_box__select_tool.SelectedItem.Text != "None")
                    ? Request__Tool_Invocation?.Invoke(combo_box__select_tool.SelectedItem.Text)
                    : null
                    ;

                grid_configuration.Reset__Tool_Invocation =
                    invocation;

                if (invocation == null) 
                {
                    invocation_field.Set__Invocation(null);
                    invocation_field.Hide();
                    return;
                }

                invocation_field.Set__Invocation
                (
                    grid_configuration.Reset__Tool_Invocation, 
                    is__specifying_mouse_positions: true, 
                    is__cloning_argument: false
                );
                invocation_field.Show();
            };

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
                is__with_initial_invocation.IsHidden = !is__new_seed_each_time.IsChecked;
                seed.IsDisabled = is__new_seed_each_time.IsChecked;
                set_seed();
            };

        StatusBar selection = new StatusBar(layout);
        selection.Dock = Dock.Bottom;

        new Button(selection) 
            { Text = "Okay", Dock = Dock.Right }
            .Clicked += (s,e) => { dialog.Close(); Invoked__New?.Invoke(grid_configuration); };
        new Button(selection) 
            { Text = "Cancel", Dock = Dock.Right }
            .Clicked += (s,e) => dialog.Close();

        Status__UI_Busy?.Invoke(true);
        dialog.Show();
        dialog.Closed +=
            (s,e) => Status__UI_Busy?.Invoke(false);
    }

    public void Load__Tool(Tool ui_tool)
    {
        Tool__Loaded_Tools.Add(ui_tool.Name);
        new Button(Tool__Selection) 
        {
            Text = ui_tool.Name[0].ToString(),
            Size = new Size(32,32),
            Margin = new Margin(1)
        }
            .Clicked += (s,e) => Updated__Tool_Selection?.Invoke(ui_tool.Name);
    }

    public void Select__Tool
    (
        Shader.Invocation? tool_invocation
    )
    {
        Tool__Fields
            .Set__Invocation
            (
                tool_invocation,
                uniform => Updated__Tool_Uniform?.Invoke(uniform),
                is__specifying_mouse_positions: false
            );
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

    private class UI__Tool_Invocation_Field : TreeControl
    {
        public Shader.Invocation? Configured__Invocation { get; set; }

        public UI__Tool_Invocation_Field
        (
            ControlBase parent
        )
        : base(parent) {}

        public void Set__Invocation
        (
            Shader.Invocation? invocation,
            Action<Shader.IUniform>? callback__uniform_updated = null,
            bool is__specifying_mouse_positions = false,
            bool is__cloning_argument = true
        )
        {
            RemoveAllNodes();
            if (invocation == null) return;
            if (callback__uniform_updated == null)
                Configured__Invocation = 
                    (is__cloning_argument)
                    ? invocation.Clone()
                    : invocation
                    ;

            if (is__specifying_mouse_positions)
            {
                Private_Display__Uniform2_Field
                (
                    invocation.Mouse_Position__Origin,
                    uniform => Private_Handle_Set__Uniform(uniform, callback__uniform_updated)
                );
                Private_Display__Uniform2_Field
                (
                    invocation.Mouse_Position__Latest,
                    uniform => Private_Handle_Set__Uniform(uniform, callback__uniform_updated)
                );
            }

            Private_Load__Uniforms1<int>(invocation.Uniform1__Int, callback__uniform_updated);
            Private_Load__Uniforms1<uint>(invocation.Uniform1__Unsigned_Int, callback__uniform_updated);
            Private_Load__Uniforms1<float>(invocation.Uniform1__Float, callback__uniform_updated);
            Private_Load__Uniforms1<double>(invocation.Uniform1__Double, callback__uniform_updated);
            Private_Load__Uniforms2<Vector2>(invocation.Uniform2__Vector2, callback__uniform_updated);
            Private_Load__Uniforms2<Vector2i>(invocation.Uniform2__Vector2i, callback__uniform_updated);
        }

        private void Private_Handle_Set__Uniform
        (
            Shader.IUniform? uniform,
            Action<Shader.IUniform>? callback__uniform_updated = null
        )
        {
            if (uniform == null)
                return;
            if (callback__uniform_updated == null)
            {
                Configured__Invocation?.Set__Uniform(uniform);
                return;
            }
            callback__uniform_updated?.Invoke(uniform);
        }

        private void Private_Load__Uniforms1<T>
        (
            Dictionary<string, Shader.IUniform>? uniforms,
            Action<Shader.IUniform>? callback__uniform_updated = null
        )
        where T : struct
        {
            if (uniforms == null) return;

            foreach(Shader.IUniform<T> u_float in uniforms.Values)
            {
                Private_Display__Uniform1_Field
                (
                    u_float,
                    uniform => Private_Handle_Set__Uniform(uniform, callback__uniform_updated)
                );
            }
        }

        private void Private_Load__Uniforms2<T>
        (
            Dictionary<string, Shader.IUniform>? uniforms,
            Action<Shader.IUniform>? callback__uniform_updated = null
        )
        where T : struct
        {
            if (uniforms == null) return;

            foreach(Shader.IUniform<T> u_float in uniforms.Values)
            {
                Private_Display__Uniform2_Field
                (
                    u_float,
                    uniform => Private_Handle_Set__Uniform(uniform, callback__uniform_updated)
                );
            }
        }

        private NumericUpDown Private_Get__Numeric
        (
            string uniform_name,
            bool is__float_or_int, 
            float value, 
            float? min = null, 
            float? max = null
        )
        {
            TreeNode field = AddNode(uniform_name);
            field.ExpandAll();
            NumericUpDown numeric =
                is__float_or_int
                ? new NumericUpDown_AsInt(field)
                : new NumericUpDown(field)
                ;
            numeric.Size = new Size(Util.Ignore, 25);
            numeric.MinimumSize = new Size(100, 25);
            numeric.Dock = Dock.Bottom;

            numeric.Name = uniform_name;

            numeric.Min =
                min
                ??
                numeric.Min
                ;

            numeric.Max =
                max
                ??
                numeric.Max
                ;
            
            numeric.Value = value;

            return numeric;
        }

        private void Private_Display__Uniform1_Field<T>
        (
            Shader.IUniform<T> uniform,
            Action<Shader.IUniform?> callback__value_updated
        )
        where T : struct
        {
            bool is__uint = 
                typeof(T) == typeof(uint);
            bool is__float_or_int =
                typeof(T) == typeof(int) || is__uint;
            float? min = null, max = null;

            if (is__uint)
                min = 0;

            if (uniform is Shader.IUniform__Clamped<T>)
            {
                Shader.IUniform__Clamped<T> clamped_uniform =
                    (Shader.IUniform__Clamped<T>)uniform;

                float min__as_float =
                    float.Parse(clamped_uniform.Min.ToString()!);
                float max__as_float =
                    float.Parse(clamped_uniform.Max.ToString()!);

                min =
                    min__as_float;

                max =
                    max__as_float;
            }

            float value = float.Parse(uniform.Value.ToString()!);

            NumericUpDown numeric =
                Private_Get__Numeric
                (
                    uniform.Name,
                    is__float_or_int,
                    value,
                    min,
                    max
                );
            
            numeric.ValueChanged +=
                (s,e) =>
                {
                    callback__value_updated?
                    .Invoke
                    (
                        Shader.IUniform
                        .From<T>(numeric.Name, numeric.Value)
                    );
                };

            //trigger the callback -- remove this later
            numeric.Value = numeric.Value;
        }

        private void Private_Display__Uniform2_Field<T>
        (
            Shader.IUniform<T> uniform,
            Action<Shader.IUniform?> callback__value_updated
        )
        where T : struct
        {
            Vector2? tuple =
                uniform.Value as Vector2?;

            if (tuple == null)
                throw new ArgumentException($"Tool is configured incorrectly. Cannot specify {uniform.Name} as a Uniform2 - it is a {uniform.Value.GetType()}");

            Vector2? min = null, max = null;

            if (uniform is Shader.IUniform__Clamped<T>)
            {
                Shader.IUniform__Clamped<T> clamped_uniform =
                    (Shader.IUniform__Clamped<T>)uniform;

                min =
                    clamped_uniform.Min as Vector2?;
                max =
                    clamped_uniform.Max as Vector2?;
            }

            bool is__float_or_int =
                typeof(T) != typeof(OpenTK.Mathematics.Vector2i);

            NumericUpDown numeric__x =
                Private_Get__Numeric
                (
                    uniform.Name + "_x  ",
                    is__float_or_int,
                    tuple.Value.X,
                    min?.X,
                    max?.X
                );

            NumericUpDown numeric__y =
                Private_Get__Numeric
                (
                    uniform.Name + "_y  ",
                    is__float_or_int,
                    tuple.Value.Y,
                    min?.Y,
                    max?.Y
                );
            
            numeric__x.ValueChanged +=
                (s,e) =>
                {
                    callback__value_updated?
                    .Invoke
                    (
                        Shader.IUniform
                        .From<T>(uniform.Name, numeric__x.Value, numeric__y.Value)
                    );
                };
            numeric__y.ValueChanged +=
                (s,e) =>
                {
                    callback__value_updated?
                    .Invoke
                    (
                        Shader.IUniform
                        .From<T>(uniform.Name, numeric__x.Value, numeric__y.Value)
                    );
                };

            //trigger the callback -- remove this later
            numeric__x.Value = numeric__x.Value;
        }
    }
}
