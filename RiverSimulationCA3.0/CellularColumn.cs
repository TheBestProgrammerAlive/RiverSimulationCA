using System;
using System.Drawing;

namespace RiverSimulationCA
{
    public class CellularColumn
    {
        static public float MaximumWaterLevel = 500;
        public float CurrentWaterLevel { get; set; }

        public float EarthLevel { get; set; }

        public float CurrentLevel 
        { 
            get
            {
                currentLevel = EarthLevel + CurrentWaterLevel;
                return currentLevel;
            } 
            private set
            {
                currentLevel = EarthLevel + CurrentWaterLevel;
                
            }
        } 

        public State CellState { get; set; }
        public Rectangle CellShape { get; private set; }
        private float currentLevel;
        public Rectangle EarthRect { get; set; }
        public Rectangle WaterRect { get; set; }

        public CellularColumn(Rectangle rect, State state)
        {
            CellState = state;
            CellShape = rect;
            EarthLevel = 250;
            CurrentWaterLevel = 0;
        }
        
    }
}