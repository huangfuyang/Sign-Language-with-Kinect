
namespace EducationSystem.SignNumGame
{
    public class Tile
    {
        public int Value { get; set; }

        private Point _position;
        public Point Position
        {
            get { return _position; }
        }

        private Point _previousPosition;
        public Point PreviousPosition
        {
            get { return _previousPosition; }
        }

        public Tile MergeFrom { get; set; }


        public Tile(int x, int y, int value)
        {
            this.Value = value;
            this._position = new Point(x, y);
            this._previousPosition = new Point(x, y);
            this.MergeFrom = null;
        }

        public void UpdatePosition(Point newPosition)
        {
            _previousPosition.X = _position.X;
            _previousPosition.Y = _position.Y;
            _position.X = newPosition.X;
            _position.Y = newPosition.Y;
        }
    }
}
