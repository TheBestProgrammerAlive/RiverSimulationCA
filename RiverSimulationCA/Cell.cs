using System.Drawing;

namespace RiverSimulationCA
{
    public class Cell
    {
        public Cell(Rectangle rect , State state)
        {
            CellState = state;
            CellShape = rect;
        }

        public State CellState { get; set; }

        public Rectangle CellShape { get; private set; }
    }
}