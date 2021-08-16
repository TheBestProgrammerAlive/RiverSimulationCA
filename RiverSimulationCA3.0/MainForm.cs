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
using System.Timers;

namespace RiverSimulationCA
{
    public partial class MainForm : Form
    {
        private int _columnWidth;
        private decimal _velocityRate;
        private int _stepCounter;
        private Pen _pen;
        private State _stateSelected;
        private Graphics _graphics;
        private CellularColumn[] _automata;
        private CellularColumn[] _temporaryAutomata;
        private Dictionary<State, SolidBrush> _dictionary;

        public MainForm()
        {
            InitializeComponent();
            InitializeVariables();
            CreateCells(_automata, State.Air);
            CreateCells(_temporaryAutomata, State.Air);
        }


        #region helpful methods

        private void InitializeVariables()
        {
            CellularColumn.MaximumWaterLevel = panelAutomata.Size.Height;
            _columnWidth = 20;
            _velocityRate = numericUpDownVelocitySetter.Value;
            _stepCounter = 0;
            _pen = new Pen(Color.Black);
            _stateSelected = State.Earth;
            _dictionary = new Dictionary<State, SolidBrush>();
            AddToDictionary();
            _automata = new CellularColumn[panelAutomata.Size.Width / _columnWidth];
            _temporaryAutomata = new CellularColumn[panelAutomata.Size.Width / _columnWidth];
            _graphics = panelAutomata.CreateGraphics();
        }

        private void AddToDictionary()
        {
            _dictionary.Add(State.Water, new SolidBrush(Color.CornflowerBlue));
            _dictionary.Add(State.Earth, new SolidBrush(Color.Sienna));
            _dictionary.Add(State.Air, new SolidBrush(Color.Azure));
        }

        private void CreateCells(CellularColumn[] automata, State state)
        {
            int x = 0;
            automata[0] = new CellularColumn(new Rectangle(new Point(x, 0),
                new Size(_columnWidth, panelAutomata.Size.Height)), state);
            automata[0].EarthRect = new Rectangle(new Point(x, panelAutomata.Size.Height),
                new Size(_columnWidth, 0));
            automata[0].WaterRect = new Rectangle(new Point(x, 0),
                new Size(_columnWidth, panelAutomata.Size.Height));
            x += _columnWidth;

            for (int i = 1; i < automata.Length - 1; i++)
            {
                automata[i] = new CellularColumn(new Rectangle(new Point(x, 0),
                    new Size(_columnWidth, panelAutomata.Size.Height)), state);
                automata[i].EarthRect = new Rectangle(
                    new Point(x, panelAutomata.Size.Height - (int) automata[i].EarthLevel),
                    new Size(_columnWidth, (int) automata[i].EarthLevel));
                automata[i].WaterRect = new Rectangle(
                    new Point(x,
                        panelAutomata.Size.Height - (int) automata[i].EarthLevel - (int) automata[i].CurrentWaterLevel),
                    new Size(_columnWidth, (int) automata[i].CurrentWaterLevel));
                x += _columnWidth;
            }

            automata[automata.Length - 1] = new CellularColumn(new Rectangle(new Point(x, 0),
                new Size(_columnWidth, panelAutomata.Size.Height)), state);
            automata[automata.Length - 1].EarthRect = new Rectangle(new Point(x, 0),
                new Size(_columnWidth, panelAutomata.Size.Height));
            automata[automata.Length - 1].WaterRect = new Rectangle(new Point(x, 0),
                new Size(_columnWidth, 0));
        }

        private void DrawAllCells(CellularColumn[] automata, Graphics graphics)
        {
            for (int i = 1; i < automata.Length - 1; i++)
            {
                graphics.DrawRectangle(_pen, automata[i].CellShape);
                graphics.DrawRectangle(_pen, automata[i].WaterRect);
                graphics.DrawRectangle(_pen, automata[i].EarthRect);
            }
        }

        private void FillAllCells(CellularColumn[] automata, Graphics graphics)
        {
            for (int i = 1; i < automata.Length - 1; i++)
            {
                FillCell(automata[i], graphics);
            }
        }

        private void FillCell(CellularColumn cell, Graphics graphics)
        {
            graphics.FillRectangle(_dictionary[State.Air], cell.CellShape);
            graphics.FillRectangle(_dictionary[State.Water], cell.WaterRect);
            graphics.FillRectangle(_dictionary[State.Earth], cell.EarthRect);
            graphics.DrawRectangle(_pen, cell.CellShape);
            graphics.DrawRectangle(_pen, cell.WaterRect);
            graphics.DrawRectangle(_pen, cell.EarthRect);
        }

        private void UpdatePanelAfterLoading(CellularColumn[] tempAutomata)
        {
            panelAutomata.CreateGraphics().Clear(Color.Azure);
            for (int i = 0; i < tempAutomata.Length; i++)
            {
                _automata[i].CellState = tempAutomata[i].CellState;
                FillCell(_automata[i], _graphics);
            }
        }

        private void Clear()
        {
            for (int i = 0; i < _automata.Length; i++)
            {
                _automata[i].CellState = State.Air;
            }

            FillAllCells(_automata, _graphics);
            DrawAllCells(_automata, _graphics);
            _stepCounter = 0;
            labelSteps.Text = $"Krok: {_stepCounter}";
        }

        #endregion

        #region button methods

        private void UpdateCellEarthState(CellularColumn cell, Graphics graphics, float earthLevel)
        {
            cell.EarthLevel = panelAutomata.Size.Height - earthLevel;
            cell.CurrentWaterLevel = 0;
            cell.EarthRect = new Rectangle(new Point(cell.CellShape.Left, (int) earthLevel),
                new Size(_columnWidth, (int) cell.EarthLevel));
            cell.WaterRect = new Rectangle(new Point(cell.CellShape.Left, (int) earthLevel),
                new Size(_columnWidth, (int) cell.CurrentWaterLevel));
        }

       
        private float CalculateYForFunction(int x, string functionName)
        {
            float maxY = CellularColumn.MaximumWaterLevel;
            int maxX = _automata.Length;
            switch (functionName)
            {
                case "linear":
                    return maxY - (maxY * x) / (maxX);
                case "parabola":
                    return (maxY -maxY * (float)Math.Pow(x,2)) / (maxX);
                case "sinus":
                    return 1;
                default:
                    throw new Exception("Podano złą nazwę funckji");
            }
            
        }
        private void buttonDrawLinear_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < _automata.Length - 1; i++)
            {
                UpdateCellEarthState(_automata[i], _graphics,CalculateYForFunction(i,"linear")
                    );
            }

            FillAllCells(_automata, _graphics);
        }

        private void buttonParabola_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < _automata.Length - 1; i++)
            {
                UpdateCellEarthState(_automata[i], _graphics,CalculateYForFunction(i,"parabola")
                );
            }

            FillAllCells(_automata, _graphics);
        }

        private void buttonSin_Click(object sender, EventArgs e)
        {
            for (int i = 1; i < _automata.Length - 1; i++)
            {
                UpdateCellEarthState(_automata[i], _graphics,CalculateYForFunction(i,"sinus")
                );
            }

            FillAllCells(_automata, _graphics);
        }
        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                numericUpDownVelocitySetter.Enabled = false;
                
                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                buttonOneStep.Enabled = false;
                backgroundWorker.RunWorkerAsync();
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            numericUpDownVelocitySetter.Enabled = true;
            buttonOneStep.Enabled = true;
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
                CellularColumn[] tempAutomata;
                using (StreamReader sr = new StreamReader(filePath))
                {
                    tempAutomata = JsonConvert.DeserializeObject<CellularColumn[]>((sr.ReadToEnd()));
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
                Thread.Sleep(500);
                labelSteps.Text = $"Krok: {_stepCounter}";
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
        private void UpdateCellWaterLevelState(CellularColumn cell, Graphics graphics, float waterLevel)
        {
            cell.CurrentWaterLevel = panelAutomata.Size.Height - cell.EarthLevel - waterLevel;
            cell.WaterRect = new Rectangle(new Point(cell.CellShape.Left, (int) waterLevel),
                new Size(_columnWidth, (int) cell.CurrentWaterLevel));
        }
        private void Flood()
        {
            _velocityRate = numericUpDownVelocitySetter.Value;
            _temporaryAutomata = _automata;
           

            //logika
            for (int i = 1; i < _automata.Length - 1; i++)
            {
                float waterLevel=0;
                if (_automata[i].CurrentLevel < _automata[i-1].CurrentLevel && 0 < _automata[i-1].CurrentWaterLevel)
                {
                    waterLevel = _automata[i - 1].CurrentWaterLevel - _automata[i].CurrentWaterLevel;
                }
                if (_automata[i].CurrentLevel < _automata[i+1].CurrentLevel && 0 < _automata[i+1].CurrentWaterLevel)
                {
                    
                }

               // UpdateCellWaterLevelState(_automata[i],_graphics,waterLevel);
               _automata[i].CurrentWaterLevel = CellularColumn.MaximumWaterLevel - _automata[i].EarthLevel - waterLevel;
               _automata[i].WaterRect = new Rectangle(new Point(_automata[i].CellShape.Left, (int) waterLevel),
                   new Size(_columnWidth, (int) _automata[i].CurrentWaterLevel));
             
            }
            FillAllCells(_automata, _graphics);
            DrawAllCells(_automata, _graphics);
        }

        #endregion

        #region automata events

        private void panelAutomata_Paint(object sender, PaintEventArgs e)
        {
            FillAllCells(_automata, e.Graphics);
            DrawAllCells(_automata, e.Graphics);
        }


        private void panelAutomata_MouseDown(object sender, MouseEventArgs e)
        {
            int cellColumn = e.X / _columnWidth;
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    //rozdziel
                    if (!(cellColumn == 0 || cellColumn == _automata.Length - 1))
                    {
                        UpdateCellEarthState(_automata[cellColumn], _graphics, e.Y);
                        FillCell(_automata[cellColumn], _graphics);
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
            int cellColumn = e.X / _columnWidth;
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    //rozdziel
                    if (!(cellColumn == 0 || cellColumn == _automata.Length - 1))
                    {
                        UpdateCellEarthState(_automata[cellColumn], _graphics, e.Y);
                        FillCell(_automata[cellColumn], _graphics);
                    }
                }
            }
            catch (Exception _)
            {
                // ignored
            }
        }

        #endregion
    }
}