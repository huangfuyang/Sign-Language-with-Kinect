
namespace EducationSystem.SignNumGame
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public Point Add(Point other)
        {
            return new Point(this.X + other.X, this.Y + other.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Point))
            {
                return false;
            }
            else
            {
                Point point = obj as Point;
                return (this.X == point.X) && (this.Y == point.Y);
            }
        }
    }
}
