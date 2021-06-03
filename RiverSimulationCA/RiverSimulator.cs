using System;
using System.Threading;
using System.Windows.Forms;

namespace RiverSimulationCA
{
    public class RiverSimulator
    {
        public Cell[,] Automata { get; set; }
        public int StepCounter { get; set; }

        private bool _isAutomataRunning;

        private Label _labelSteps;
        //private int _currentRow, _currentColumn;

       
        
        //sąsiedztwo
        //
        //wchodzi woda z lewej albo prawej strony albo z obu?
        //realizacja wzoru z wiki
        //woda musi się ciągle wlewać
        //na razie liniowo
        public RiverSimulator(ref Cell[,] automata,Label labelSteps)
        {
            _isAutomataRunning = true;
            Automata = automata;
            _labelSteps = labelSteps;
        }
        public void Simulate()
        {
          
            while (true)
            {
                
                StepCounter++;
                //FloodFromLeft();
                
            }
        }

 

        public void OneStep()
        {
            StepCounter++;
          
           // FloodFromLeft();
        }
        private void FloodFromLeft()
        {
            for (int j = 0; j < Automata.GetLength(1); j++)
            {
                for (int i = 0; i < Automata.GetLength(0); i++)
                {
                    switch (Automata[i,j].CellState)
                    {
                        case State.Air:
                           ;
                            break;
                       
                        case State.Water:
                            ;
                            break;
                        case State.Earth:
                            break;
                        default:
                            throw new Exception("wrong state specified");
                            break;
                    }
                }
            }
        }

    
    }
}