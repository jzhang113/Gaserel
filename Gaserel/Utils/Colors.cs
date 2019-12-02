using System.Drawing;

namespace Utils
{
    public static class Colors
    {
        public static Color Blend(this in Color c1, in Color c2, double alpha)
        {
            return Color.FromArgb(
                (int)(c1.R + (c2.R - c1.R) * alpha),
                (int)(c1.G + (c2.G - c1.G) * alpha),
                (int)(c1.B + (c2.B - c1.B) * alpha));
        }
    }
}
