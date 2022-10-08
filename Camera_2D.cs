
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GPU_Of_Life;

public class Camera_2D
{
    private Vector2 FOCAL__SIZE;
    private float FOCAL__ASPECT;
    private float FOCAL__WIDTH
        => FOCAL__SIZE.X;
    private float FOCAL__HEIGHT
        => FOCAL__SIZE.Y;

    internal Matrix4 GRID__PROJECTION = Matrix4.Identity;
    public float GRID__PROJECTION__ZOOM__EXP = 1f;
    public float GRID__PROJECTION__ZOOM__LOG_INTERVAL = 2;
    public float GRID__PROJECTION__ZOOM = 1f;
    public float GRID__PROJECTION__WIDTH, GRID__PROJECTION__HEIGHT;

    internal Matrix4 GRID__TRANSLATION = Matrix4.Identity;
    internal Vector2 GRID__TRANSLATION__POINT = new Vector2();
    private float GRID__TRANSLATION__X
        => GRID__TRANSLATION__POINT.X;
    private float GRID__TRANSLATION__Y
        => GRID__TRANSLATION__POINT.Y;

    public void Resize__Focal_Size(Vector2 ortho_size, Vector2 size)
    {
        FOCAL__ASPECT = size.X / size.Y;
        FOCAL__SIZE = ortho_size;

        GRID__PROJECTION__WIDTH  = (ortho_size.X / (float)ortho_size.Y) + 1;
        GRID__PROJECTION__HEIGHT = (ortho_size.Y / (float)ortho_size.X) + 1;
        GRID__PROJECTION = 
            Matrix4.CreateOrthographic
            (
                GRID__PROJECTION__WIDTH * GRID__PROJECTION__ZOOM, 
                GRID__PROJECTION__HEIGHT * GRID__PROJECTION__ZOOM, 
                0, 1
            );
    }

    public void Process__Scroll(MouseWheelEventArgs e)
    {
        float delta = ((e.OffsetY + 1) - GRID__PROJECTION__ZOOM__EXP) / 4;
        float dsign = Math.Sign(e.OffsetY);

        GRID__PROJECTION__ZOOM__EXP = GRID__PROJECTION__ZOOM__EXP +  delta * delta * dsign;
        GRID__PROJECTION__ZOOM = 
            (float)Math.Pow
            (
                GRID__PROJECTION__ZOOM__LOG_INTERVAL,
                GRID__PROJECTION__ZOOM__EXP
            );
        GRID__PROJECTION__ZOOM -= 1;

        GRID__PROJECTION = 
            Matrix4.CreateOrthographic
            (
                GRID__PROJECTION__WIDTH * GRID__PROJECTION__ZOOM, 
                GRID__PROJECTION__HEIGHT * GRID__PROJECTION__ZOOM, 
                0, 1
            );

        //Console.WriteLine(GRID__PROJECTION__ZOOM);
        //Console.WriteLine($"delta:{e.Offset} norm:{normalize_delta}");
        //Console.WriteLine($"zoom: {GRID__PROJECTION__ZOOM} zoom__exp: {GRID__PROJECTION__ZOOM__EXP} delta: {delta}");
    }

    Vector2? mouse__last_frame;
    public void Process__Update(MouseState mouse_state, Vector2 size, Vector2 mouse_position, FrameEventArgs args)
    {
        if (!mouse_state.IsButtonDown(MouseButton.Right))
        {
            mouse__last_frame = null;
            return;
        }
        Vector2 mouse = mouse_position - size / 2;
        if (mouse__last_frame == null)
        {
            mouse__last_frame = mouse;
            return;
        }

        Vector2 delta = (mouse - (Vector2)mouse__last_frame);
        delta.Y *= -1;
        if (delta.X == 0 && delta.Y == 0) return;
        delta.Normalize();
        delta *= 
            GRID__PROJECTION__ZOOM__EXP 
            * 
            (float)args.Time 
            * 
            new Vector2
            (
                (size.X < size.Y)
                ? size.Y / (float)size.X
                : 1,
                (size.Y < size.X)
                ? size.X / (float)size.Y
                : 1
            );
        Vector2 nTranslation =
            GRID__TRANSLATION__POINT
            +
            delta;

        float w_min = GRID__PROJECTION__WIDTH / -2;
        float w_max = GRID__PROJECTION__WIDTH / 2;
        float h_min = GRID__PROJECTION__HEIGHT / -2;
        float h_max = GRID__PROJECTION__HEIGHT / 2;

        GRID__TRANSLATION__POINT =
            new Vector2
            (
                (float)Math.Max(w_min, Math.Min(w_max, nTranslation.X)),
                (float)Math.Max(h_min, Math.Min(h_max, nTranslation.Y))
            );
        GRID__TRANSLATION = Matrix4.CreateTranslation(GRID__TRANSLATION__X, GRID__TRANSLATION__Y, 0);

        mouse__last_frame = mouse;
        //Console.WriteLine($"MOUSE: {mouse}");
        //Console.WriteLine($"w_min: {w_min} w_max: {w_max} h_min: {h_min} h_max: {h_max}");
        //Console.WriteLine($"pos: {GRID__TRANSLATION} nPos: {nTranslation} delta: {delta}");
    }

    public Vector4 Get__Mouse_To_World(Vector2 mouse_position)
    {
        Matrix4 projection_inverted =
            (GRID__PROJECTION).Inverted();

        float test =
            (float)Math.Pow
            (
                GRID__PROJECTION__ZOOM__LOG_INTERVAL,
                GRID__PROJECTION__ZOOM__EXP - 4
            ) + 1;

        Vector4 v4_mouse_pos = 
            new Vector4
            (
                mouse_position.X / FOCAL__WIDTH, 
                (mouse_position.Y) / FOCAL__HEIGHT,
                0, 0.5f
            ) * 2;
        v4_mouse_pos += new Vector4(-1,-1,0,0);
        //v4_mouse_pos.X *= FOCAL__ASPECT;
        //v4_mouse_pos.Y /= FOCAL__ASPECT;

        v4_mouse_pos = (projection_inverted * v4_mouse_pos);
        v4_mouse_pos = (Matrix4.Transpose(GRID__TRANSLATION).Inverted()) * v4_mouse_pos;

        return v4_mouse_pos;
    }
}
