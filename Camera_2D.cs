
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace GPU_Of_Life;

public class Camera_2D
{
    internal Matrix4 GRID__PROJECTION = Matrix4.Identity;
    private float GRID__PROJECTION__ZOOM__EXP = 1f;
    private float GRID__PROJECTION__ZOOM__LOG_INTERVAL = 2;
    private float GRID__PROJECTION__ZOOM = 1f;
    private float GRID__PROJECTION__WIDTH, GRID__PROJECTION__HEIGHT;

    internal Matrix4 GRID__TRANSLATION = Matrix4.Identity;
    private Vector2 GRID__TRANSLATION__POINT = new Vector2();
    private float GRID__TRANSLATION__X
        => GRID__TRANSLATION__POINT.X;
    private float GRID__TRANSLATION__Y
        => GRID__TRANSLATION__POINT.Y;

    public void Resize__Focal_Size(Vector2 size)
    {
        GRID__PROJECTION__WIDTH  = (size.X / (float)size.Y) + 1;
        GRID__PROJECTION__HEIGHT = (size.Y / (float)size.X) + 1;
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
        Matrix4 translation = Matrix4.CreateTranslation(GRID__TRANSLATION__X, GRID__TRANSLATION__Y, 0);

        mouse__last_frame = mouse;
        //Console.WriteLine($"MOUSE: {mouse}");
        //Console.WriteLine($"w_min: {w_min} w_max: {w_max} h_min: {h_min} h_max: {h_max}");
        //Console.WriteLine($"pos: {GRID__TRANSLATION} nPos: {nTranslation} delta: {delta}");
    }
}
