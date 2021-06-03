using System;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace RiverSimulationCA
{
    public partial class MainForm : Form
    {
        private int _stepCounter;
        private Pen _pen;
        private State _stateSelected;
        private Graphics _graphics;
        private Cell[,] _automata;
        private Cell[,] _temporaryAutomata;
        private int _cellWidthAndHeight;
        private Dictionary<State, SolidBrush> _dictionary;
        public MainForm()
        {
            InitializeComponent();
            InitializeVariables();
            CreateCells(_automata,State.Air);
            CreateCells(_temporaryAutomata,State.Air);
            
           
        }
       

        #region helpful methods

        private void InitializeVariables()
        {
            _stepCounter = 0;
            _cellWidthAndHeight = 15;
            _pen = new Pen(Color.Black);
            _stateSelected = State.Earth;
            _dictionary = new Dictionary<State, SolidBrush>();
            AddToDictionary();
            _automata = new Cell[panelAutomata.Size.Height/_cellWidthAndHeight, panelAutomata.Size.Width/_cellWidthAndHeight];
            _temporaryAutomata = new Cell[panelAutomata.Size.Height/_cellWidthAndHeight, panelAutomata.Size.Width/_cellWidthAndHeight];
            _graphics = panelAutomata.CreateGraphics();
        }

        private void AddToDictionary()
        {
            _dictionary.Add(State.Water,new SolidBrush(Color.CornflowerBlue));
            _dictionary.Add(State.Earth,new SolidBrush(Color.Sienna));
            _dictionary.Add(State.Air,new SolidBrush(Color.Azure));
        }

        private void CreateCells(Cell[,] automata, State state)
        {
            int x = 0;
            int y = 0;
            for (int i = 0; i < automata.GetLength(0); i++)
            {
             
                for (int j = 0; j < automata.GetLength(1); j++)
                {
                    automata[i, j] = new Cell(new Rectangle(new Point(x,y),new Size(_cellWidthAndHeight,_cellWidthAndHeight)), state);
                    
                    x += _cellWidthAndHeight;
                }
                x = 0;
                y += _cellWidthAndHeight;
            }
        }
        private void DrawAllCells(Cell[,] automata,Graphics graphics)
        {
        
            
            for (int i = 1; i < automata.GetLength(0)-1; i++)
            {
                for (int j = 1; j < automata.GetLength(1)-1; j++)
                {
                    graphics.DrawRectangle(_pen,automata[i, j].CellShape);
                }
            }
            
        }
        private void FillAllCells(Cell[,] automata,Graphics graphics)
        {
            for (int i = 1; i < automata.GetLength(0)-1; i++)
            {
                for (int j = 1; j < automata.GetLength(1)-1; j++)
                {
                    FillCell(automata[i, j], graphics);
                 
                }
                
            }
        }

        private void FillCell(Cell cell,Graphics graphics)
        {
            graphics.FillRectangle(_dictionary[cell.CellState],cell.CellShape);
            graphics.DrawRectangle(_pen,cell.CellShape);
        }

        private void UpdatePanelAfterLoading(Cell[,] tempAutomata)
        {
            
            panelAutomata.CreateGraphics().Clear(Color.Azure);
            for (int i = 0; i < tempAutomata.GetLength(0); i++)
            {
                for (int j = 0; j < tempAutomata.GetLength(1); j++)
                {
                    
                        _automata[i, j].CellState = tempAutomata[i, j].CellState;
                        FillCell(_automata[i,j],_graphics);
                }
            }
        }
        private void Clear()
        {
            for (int i = 0; i < _automata.GetLength(0); i++)
            {
                for (int j = 0; j < _automata.GetLength(1); j++)
                {
                    _automata[i, j].CellState = State.Air;
                }
             
              
            }
            FillAllCells(_automata,_graphics);
            DrawAllCells(_automata,_graphics);
            _stepCounter = 0;
            labelSteps.Text = $"Krok: {_stepCounter}";
        }
        #endregion
        
        #region button methods
    
   
        private void buttonEarth_Click(object sender, EventArgs e)
        {
            _stateSelected = State.Earth;
        }

        private void buttonAir_Click(object sender, EventArgs e)
        {
            _stateSelected = State.Air;
        }

        private void buttonWater_Click(object sender, EventArgs e)
        {
            _stateSelected = State.Water;
        }
        private void buttonStart_Click(object sender, EventArgs e)
        {
            
            if (!backgroundWorker.IsBusy)
            {
                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                backgroundWorker.RunWorkerAsync();  
            }

        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            
            buttonStart.Enabled = true;
            buttonStop.Enabled = false;        
            backgroundWorker.CancelAsync();
         
    
        }

        private void buttonOneStep_Click(object sender, EventArgs e)
        {
            SimulateOneStep();
           
        }

      

        #endregion
        
        #region menustrip methods

        private void zapiszToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                using (StreamWriter sw = new StreamWriter(filePath))
                {
                    sw.Write(JsonConvert.SerializeObject(_automata));
                
                }
            }
           
        }

        private void wczytajToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                Cell[,] tempAutomata;
                using (StreamReader sr = new StreamReader(filePath))
                {  
                    tempAutomata = JsonConvert.DeserializeObject<Cell[,]>((sr.ReadToEnd()));
                }
                UpdatePanelAfterLoading(tempAutomata);
               

            }
           
        }  
        private void zmieńWszystkoWPowietrzeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clear();
        }

        #endregion

        #region backgroundworker and simulation methods
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Simulate();
        }
        
        private void Simulate()
        {
            while (true)
            {
                _stepCounter++;
                labelSteps.Text = $"Krok: {_stepCounter}";
               Thread.Sleep(500);
                Flood();
                if (backgroundWorker.CancellationPending)
                    break;


            }
        }
        private void SimulateOneStep()
        {
          
            _stepCounter++;
            labelSteps.Text = $"Krok: {_stepCounter}";
            Flood();
        }

        private void Flood()
        {
            //edges
            //up
            //  for (int i  = 0; i <  _temporaryAutomata.GetLength(1); i++)
            //  {
            //      
            //  }
            //  //down
            //  for (int i = _temporaryAutomata.GetLength(0); i <  _temporaryAutomata.GetLength(1); i++)
            //  {
            //      
            //  }
            //  //left
            //  for (int i = 0; i < UPPER; i++)
            //  {
            //      
            //  }
            // //right
            // for (int i = 0; i < UPPER; i++)
            // {
            //     
            // }

            //inside
            _temporaryAutomata = (Cell[,])_automata.Clone();
            for (int j = _temporaryAutomata.GetLength(1) - 2; j >= 1; j--)
            {
                for (int i = _temporaryAutomata.GetLength(0) - 2; i >= 1; i--)
                {
                    switch (_temporaryAutomata[i, j].CellState)
                    {
                        case State.Air:
                            break;

                        case State.Water:
                            CheckWaterNeighbourhood(_temporaryAutomata, i, j);
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

        private void CheckWaterNeighbourhood(Cell[,] automata, int i, int j)
        {
            //down
            if ( automata[i + 1, j].CellState==State.Air)
            {
                automata[i + 1, j].CellState = State.Water;
                FillCell(automata[i+1,j],_graphics);
                return;

            }
            //down-left down-right
            //-->air
            // if ( automata[i + 1, j - 1].CellState==State.Air)
            // {
            //     automata[i + 1, j - 1].CellState = State.Water;
            //     FillCell(automata[i + 1, j - 1],_graphics);
            //     return;
            // }
            //
            // if (automata[i + 1, j + 1].CellState==State.Air)
            // {
            //     automata[i + 1, j + 1].CellState = State.Water;
            //     FillCell(automata[i + 1, j + 1],_graphics);
            //     return;
            // }
            //left right
            if (automata[i, j - 1].CellState==State.Air)
            {
                automata[i, j - 1].CellState = State.Water;
                FillCell(automata[i, j - 1],_graphics);
                return;
            }
            if (automata[i, j + 1].CellState==State.Air)
            {
                automata[i, j + 1].CellState = State.Water;
                FillCell(_automata[i, j + 1],_graphics);
                return;
            }
            //all upper
            // if (automata[i - 1, j - 1].CellState==State.Air)
            // {
            //     automata[i - 1, j - 1].CellState = State.Water;
            //     FillCell(_automata[i - 1, j - 1],_graphics);
            //     return;
            // }
            if (automata[i - 1, j].CellState==State.Air)
            {
                automata[i - 1, j].CellState = State.Water;
                FillCell(_automata[i - 1, j],_graphics);
                return;
            }
            // if (automata[i - 1, j + 1].CellState==State.Air)
            // {
            //     automata[i - 1, j + 1].CellState = State.Water;
            //     FillCell(_automata[i-1,j+1],_graphics);
            // }
            //test
         

           

        }

        #endregion

        #region automata events

        private void panelAutomata_Paint(object sender, PaintEventArgs e)
        {
          
            FillAllCells(_automata,e.Graphics);
            DrawAllCells(_automata,e.Graphics);
        }
        private void panelAutomata_MouseDown(object sender, MouseEventArgs e)
        {
            
            int cellRow=e.Y / _cellWidthAndHeight;
            int cellColumn=e.X / _cellWidthAndHeight;
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    _automata[cellRow,cellColumn].CellState=_stateSelected;
                    FillCell(_automata[cellRow,cellColumn], panelAutomata.CreateGraphics());
                }
                if (e.Button == MouseButtons.Middle)
                {
                    for (int i = cellRow; i < _automata.GetLength(0); i++)
                    {
                        _automata[i,cellColumn].CellState=_stateSelected;
                        FillCell(_automata[i,cellColumn], panelAutomata.CreateGraphics());
                    }
                }

                if (e.Button == MouseButtons.Right)
                {
                    if (_stateSelected != State.Earth)
                    {
                        for (int i = cellRow; i < _automata.GetLength(0); i++)
                        {
                            if ( _automata[i,cellColumn].CellState==State.Earth)
                            {
                                break;
                            }
                            _automata[i,cellColumn].CellState=_stateSelected;
                            FillCell(_automata[i,cellColumn], panelAutomata.CreateGraphics());
                        }
                    }
                    else
                    {
                        for (int i = cellRow; i < _automata.GetLength(0); i++)
                        {
                            _automata[i,cellColumn].CellState=_stateSelected;
                            FillCell(_automata[i,cellColumn], panelAutomata.CreateGraphics());
                        }  
                    }
                  
                }
            }
            catch (Exception _)
            {
                // ignored
            }
            
        }
        private void panelAutomata_MouseMove(object sender, MouseEventArgs e)
        {
            int cellRow=e.Y / _cellWidthAndHeight;
            int cellColumn=e.X / _cellWidthAndHeight;
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    _automata[cellRow,cellColumn].CellState=_stateSelected;
                    FillCell(_automata[cellRow,cellColumn], panelAutomata.CreateGraphics());
                }
                if (e.Button == MouseButtons.Middle)
                {
                    for (int i = cellRow; i < _automata.GetLength(0); i++)
                    {
                        _automata[i,cellColumn].CellState=_stateSelected;
                        FillCell(_automata[i,cellColumn], panelAutomata.CreateGraphics());
                    }
                }

                if (e.Button == MouseButtons.Right)
                {
                    if (_stateSelected != State.Earth)
                    {
                        for (int i = cellRow; i < _automata.GetLength(0); i++)
                        {
                            if ( _automata[i,cellColumn].CellState==State.Earth)
                            {
                                break;
                            }
                            _automata[i,cellColumn].CellState=_stateSelected;
                            FillCell(_automata[i,cellColumn], panelAutomata.CreateGraphics());
                        }
                    }
                    else
                    {
                        for (int i = cellRow; i < _automata.GetLength(0); i++)
                        {
                            _automata[i,cellColumn].CellState=_stateSelected;
                            FillCell(_automata[i,cellColumn], panelAutomata.CreateGraphics());
                        }  
                    }
                  
                }
            }
            catch (Exception exception)
            {
                // ignored
            }
        }
        #endregion
        

       
  

   
    }
    
}