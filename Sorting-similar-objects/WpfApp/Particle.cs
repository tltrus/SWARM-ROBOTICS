using System.Windows.Media;
using System.Windows;
using System.Globalization;

namespace WpfApp
{
    public enum ParticleType
    {
        Type1 = 0,
        Type2 = 1
    }

    public enum ParticleState
    {
        Down = 0,
        Up = 1
    }

    internal class Particle
    {
        public Vector2D pos;
        public ParticleType type;
        public ParticleState state;
        public int id;

        public Particle(int id, Vector2D pos, ParticleType type)
        {
            this.id = id;
            this.type = type;
            this.pos = pos;
            this.state = ParticleState.Down;
        }
        public Point GetPoint() => new Point(pos.X, pos.Y);
        public void Draw(DrawingContext dc)
        {
            switch (state)
            {
                case ParticleState.Down:
                    dc.DrawEllipse(Brushes.DodgerBlue, null, GetPoint(), 5, 5);
                    break;
                case ParticleState.Up:
                    dc.DrawEllipse(Brushes.DodgerBlue, null, GetPoint(), 7, 7);
                    break;
            }

            // Draw labeling
            //FormattedText formattedText = new FormattedText(id.ToString(), CultureInfo.GetCultureInfo("en-us"),
            //                                                FlowDirection.LeftToRight, new Typeface("Verdana"), 8, Brushes.DarkGray,
            //                                                VisualTreeHelper.GetDpi(MainWindow.visual).PixelsPerDip);
            //dc.DrawText(formattedText, new Point(pos.X + 5, pos.Y - 5 - 15));
        }
    }
}
