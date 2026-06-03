using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class UiHelper
{
    public static void MakeRounded(Button btn, int radius)
    {
        GraphicsPath path = new GraphicsPath();

        path.AddArc(0, 0, radius, radius, 180, 90);
        path.AddArc(btn.Width - radius, 0, radius, radius, 270, 90);
        path.AddArc(btn.Width - radius, btn.Height - radius, radius, radius, 0, 90);
        path.AddArc(0, btn.Height - radius, radius, radius, 90, 90);

        path.CloseFigure();
        btn.Region = new Region(path);
    }

    public static void AddRightBorder(Control control, Color color, int thickness)
    {
        control.Paint += (sender, e) =>
        {
            using (Pen pen = new Pen(color, thickness))
            {
                e.Graphics.DrawLine(
                    pen,
                    control.Width - 1,
                    0,
                    control.Width - 1,
                    control.Height
                );
            }
        };

        control.Invalidate();
    }

    public static void AddHoverText(Button btn, string hover, string normal)
    {
        btn.MouseEnter += (s, e) => btn.Text = hover;
        btn.MouseLeave += (s, e) => btn.Text = normal;
    }
}