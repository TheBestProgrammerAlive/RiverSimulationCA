using System.Drawing;

namespace RiverSimulationCA
{
    public class CellularColumn
    {
        public static float MaximumWaterLevel;
        private float _currentLevel;

        public CellularColumn(Rectangle rect, State state)
        {
            CellState = state;
            CellShape = rect;
            EarthLevel = 250;
            CurrentWaterLevel = 0;
        }
        public float CurrentWaterLevel { get; set; }

        public float EarthLevel { get; set; }

        public float CurrentLevel
        { 
            get
            {
                _currentLevel = EarthLevel + CurrentWaterLevel;
                return _currentLevel;
            }
        }

        public State CellState { get; set; }
        public Rectangle CellShape { get; }
        public Rectangle EarthRect { get; set; }
        public Rectangle WaterRect { get; set; }
    }
}