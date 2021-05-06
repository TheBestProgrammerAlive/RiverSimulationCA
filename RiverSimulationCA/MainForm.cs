using System;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;

namespace RiverSimulationCA
{
    public partial class MainForm : Form
    {
        private Pen _pen;
        private State _stateSelected;
        private Graphics _graphics;
        private Cell[,] _automata;
        private int _cellWidthAndHeight;
        private Dictionary<State, SolidBrush> _dictionary;
        public MainForm()
        {
            InitializeComponent();
            InitializeVariables();
            CreateCells(_automata,State.Air);
           
        }
        private void panelAutomata_Paint(object sender, PaintEventArgs e)
        {
      
            FillAllCells(_automata,e.Graphics);
            DrawAllCells(_automata,e.Graphics);
        }

        #region helpful methods

        private void InitializeVariables()
        {
            _pen = new Pen(Color.Black);
            _stateSelected = State.Earth;
            _dictionary = new Dictionary<State, SolidBrush>();
            AddToDictionary();
            _cellWidthAndHeight = 15;
            _automata = new Cell[panelAutomata.Size.Height/_cellWidthAndHeight, panelAutomata.Size.Width/_cellWidthAndHeight];
            Debug.WriteLine($"{panelAutomata.Size.Height/_cellWidthAndHeight}  { panelAutomata.Size.Width/_cellWidthAndHeight}");
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
        
            
            for (int i = 0; i < automata.GetLength(0); i++)
            {
                for (int j = 0; j < automata.GetLength(1); j++)
                {
                    graphics.DrawRectangle(_pen,automata[i, j].CellShape);
                }
            }
            
        }
        private void FillAllCells(Cell[,] automata,Graphics graphics)
        {
            for (int i = 0; i < automata.GetLength(0); i++)
            {
                for (int j = 0; j < automata.GetLength(1); j++)
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
                        FillCell(_automata[i,j],panelAutomata.CreateGraphics());
                }
            }
        }
        #endregion
        #region button methods
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
            catch (Exception exception)
            {
                // ignored
            }
            
        }

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
        

        #endregion

        #region dialogs methods

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
            Cell[,] tempAutomata = new Cell[_automata.GetLength(0), _automata.GetLength(1)];
            //nie działa, dostaje sie do messageboxów ale potem nie da sie nic kliknąć i sie nie wczytuje
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                using (StreamReader sr = new StreamReader(filePath))
                {  
                    tempAutomata = JsonConvert.DeserializeObject<Cell[,]>((sr.ReadToEnd()));
                }
                UpdatePanelAfterLoading(tempAutomata);
               

            }
           
        }

        #endregion
      

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"{_automata[0, 0].CellState}");
        }
    }
    
}