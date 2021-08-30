using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace RiverSimulationCA
{
    public partial class MainForm : Form
    {
        private CellularColumn[] _automata;
        private int _columnWidth;
        private Dictionary<State, SolidBrush> _dictionary;
        private Graphics _graphics;
        private Pen _pen;
        private State _stateSelected;
        private int _stepCounter;
        private CellularColumn[] _temporaryAutomata;
        private decimal _velocityRate;
        private float _maxLevel;

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
            _maxLevel = panelAutomata.Size.Height;
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
            var x = 0;

            automata[0] = new CellularColumn(new Rectangle(new Point(x, 0),
                new Size(_columnWidth, panelAutomata.Size.Height)), state);
            automata[0].CurrentWaterLevel = _maxLevel;
            automata[0].EarthLevel = 0;
            automata[0].EarthRect = new Rectangle(new Point(x, panelAutomata.Size.Height),
                new Size(_columnWidth, 0));
            automata[0].WaterRect = new Rectangle(new Point(x, 0),
                new Size(_columnWidth, panelAutomata.Size.Height));
            x += _columnWidth;

            for (var i = 1; i < automata.Length - 1; i++)
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
            automata[automata.Length - 1].EarthLevel = _maxLevel;
            automata[automata.Length - 1].EarthRect = new Rectangle(new Point(x, 0),
                new Size(_columnWidth, panelAutomata.Size.Height));
            automata[automata.Length - 1].WaterRect = new Rectangle(new Point(x, 0),
                new Size(_columnWidth, 0));
        }

        private void DrawAllCells(CellularColumn[] automata, Graphics graphics)
        {
            for (var i = 1; i < automata.Length - 1; i++)
            {
                graphics.DrawRectangle(_pen, automata[i].CellShape);
                graphics.DrawRectangle(_pen, automata[i].WaterRect);
                graphics.DrawRectangle(_pen, automata[i].EarthRect);
            }
        }

        private void FillAllCells(CellularColumn[] automata, Graphics graphics)
        {
            for (var i = 1; i < automata.Length - 1; i++) FillCell(automata[i], graphics);
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
            panelAutomata.CreateGraphics().Clear(Color.FromArgb(153,180,209));
            for (var i = 1; i < tempAutomata.Length - 1; i++)
            {
                _automata[i].CellState = tempAutomata[i].CellState;
                _automata[i].CurrentWaterLevel = tempAutomata[i].CurrentWaterLevel;
                _automata[i].WaterRect = tempAutomata[i].WaterRect;
                _automata[i].EarthLevel = tempAutomata[i].EarthLevel;
                _automata[i].EarthRect = tempAutomata[i].EarthRect;
                FillCell(_automata[i], _graphics);
            }
        }

        private void ClearAndResetCounter()
        {
            for (var i = 0; i < _automata.Length; i++) _automata[i].CellState = State.Air;

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
            var maxY = CellularColumn.MaximumWaterLevel;
            var maxX = _automata.Length;
            switch (functionName)
            {
                case "linear":
                    return maxY - maxY * x / maxX;
                case "linear reversed":
                    return maxY -  maxY * (maxX - x) / maxX;
                case "parabola":
                    return  maxY - (maxY/(x));
                case "sinus":
                    return maxY - 100*(float)Math.Sin(x)-200;
                default:
                    throw new Exception("Podano złą nazwę funckji");
            }
        }

        private void buttonDrawLinear_Click(object sender, EventArgs e)
        {
            for (var i = 1; i < _automata.Length - 1; i++)
                UpdateCellEarthState(_automata[i], _graphics,
                    CalculateYForFunction(i, "linear"));
            ClearAndResetCounter();
            FillAllCells(_automata, _graphics);
        }

        private void buttonParabola_Click(object sender, EventArgs e)
        { 
            bool overHalf = false;
            int j = 2;
            for (var i = 1; i < _automata.Length - 1; i++)
            {
                if (j==1)
                {
                    j++;
                }
               
                UpdateCellEarthState(_automata[i], _graphics, CalculateYForFunction(j, "parabola")
                );
                if (j>=(_automata.Length) / 2)
                {
                    overHalf = true;
                }

                if (overHalf)
                {
                    j--;
                }
                else
                {
                    j++;
                }
              
            }
            ClearAndResetCounter();
            FillAllCells(_automata, _graphics);
        }
        private void buttonSin_Click(object sender, EventArgs e)
        {
            for (var i = 1; i < _automata.Length - 1; i++)
                UpdateCellEarthState(_automata[i], _graphics, CalculateYForFunction(i, "sinus")
                );
            ClearAndResetCounter();
            FillAllCells(_automata, _graphics);
        }
        private void buttonDrawLinearReversed_Click(object sender, EventArgs e)
        {
            for (var i = 1; i < _automata.Length - 1; i++)
                UpdateCellEarthState(_automata[i], _graphics, CalculateYForFunction(i, "linear reversed")
                );
            ClearAndResetCounter();
            FillAllCells(_automata, _graphics);
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            
                numericUpDownVelocitySetter.Enabled = false;
                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                buttonOneStep.Enabled = false;
                backgroundWorker.RunWorkerAsync();
            
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

        private void SimulateOneStep()
        {
            _stepCounter++;
            labelSteps.Text = $"Krok: {_stepCounter}";
            Flood();
        }
        
        #endregion

        #region menustrip methods

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = saveFileDialog.FileName;
                using (var sw = new StreamWriter(filePath))
                {
                    sw.Write(JsonConvert.SerializeObject(_automata));
                }
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var filePath = openFileDialog.FileName;
                CellularColumn[] tempAutomata;
                using (var sr = new StreamReader(filePath))
                {
                    tempAutomata = JsonConvert.DeserializeObject<CellularColumn[]>(sr.ReadToEnd());
                }

                UpdatePanelAfterLoading(tempAutomata);
            }
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
                Flood();
                if (backgroundWorker.CancellationPending)
                    break;
            }
        }

        private void GiveWater( CellularColumn from, CellularColumn to)
        {
            float waterFlown = (from.CurrentWaterLevel - to.CurrentWaterLevel) *
                               (float) _velocityRate;
            to.CurrentWaterLevel += waterFlown;
            from.CurrentWaterLevel -= waterFlown;
        }
        private void Flood()
        {
            _velocityRate = numericUpDownVelocitySetter.Value;
            _temporaryAutomata = _automata;
            float setWaterLevel = _maxLevel;
            _automata[0].CurrentWaterLevel = setWaterLevel;
            for (var i = _automata.Length - 2; i > 0; i--)
            {
                float waterFlown = 0;
                if (_automata[i].CurrentLevel <  _automata[0].CurrentLevel)
                {
                    if (_automata[i].CurrentLevel < _automata[i - 1].CurrentLevel )
                {
                   GiveWater(  _automata[i - 1],  _automata[i]);
                    // waterFlown = (_automata[i - 1].CurrentWaterLevel - _automata[i].CurrentWaterLevel) *
                    //              (float) _velocityRate;
                    // _automata[i].CurrentWaterLevel += waterFlown;
                    // _automata[i - 1].CurrentWaterLevel -= waterFlown;
                    // if (_automata[i].CurrentWaterLevel> setWaterLevel)
                    // {
                    //     _automata[i].CurrentWaterLevel -= setWaterLevel - _automata[i].CurrentWaterLevel;
                    //     _automata[i+1].CurrentWaterLevel += setWaterLevel - _automata[i].CurrentWaterLevel;
                    // }
                }
                
                
                //
                // if (_automata[i].EarthLevel > _automata[i+1].CurrentLevel && _automata[i].CurrentWaterLevel > 0)
                // {
                //     _automata[i + 1].CurrentWaterLevel +=  _automata[i].CurrentWaterLevel-1;
                //     _automata[i].CurrentWaterLevel = 1;
                // }
                //
                //
                // if (_automata[i].CurrentLevel < _automata[i + 1].CurrentLevel && _automata[i].CurrentLevel > _automata[i + 1].EarthLevel)
                // {
                //     waterFlown = (_automata[i + 1].CurrentWaterLevel - _automata[i].CurrentWaterLevel) *
                //                  (float) _velocityRate;
                //     _automata[i].CurrentWaterLevel += waterFlown;
                //     _automata[i + 1].CurrentWaterLevel -= waterFlown;
                // }
                //
                //
                // if (_automata[i].CurrentLevel < _automata[i + 1].CurrentLevel &&
                //     _automata[i].CurrentWaterLevel < _automata[i + 1].CurrentWaterLevel)
                // {
                //
                //     waterFlown = (_automata[i + 1].CurrentWaterLevel - _automata[i].CurrentWaterLevel) *
                //                  (float) _velocityRate;
                //     _automata[i].CurrentWaterLevel += waterFlown;
                //     _automata[i + 1].CurrentWaterLevel -= waterFlown;
                //     if (_automata[i].CurrentWaterLevel > setWaterLevel)
                //     {
                //         _automata[i].CurrentWaterLevel -= setWaterLevel - _automata[i].CurrentWaterLevel;
                //         _automata[i + 1].CurrentWaterLevel += setWaterLevel - _automata[i].CurrentWaterLevel;
                //     }
                // }
                }
                // if (_automata[0].CurrentWaterLevel < setWaterLevel)
                //     {
                //         _automata[0].CurrentWaterLevel += 10;
                //     }
                
                
                _automata[i].WaterRect = new Rectangle(new Point(_automata[i].CellShape.Left, (int) (_maxLevel-_automata[i].CurrentLevel)),
                    new Size(_columnWidth, (int) _automata[i].CurrentWaterLevel));
                FillCell(_automata[i], _graphics);
                
            }
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
            var cellColumn = e.X / _columnWidth;
            try
            {
                if (e.Button == MouseButtons.Left)
                    //rozdziel
                    if (!(cellColumn == 0 || cellColumn == _automata.Length - 1))
                    {
                        UpdateCellEarthState(_automata[cellColumn], _graphics, e.Y);
                        FillCell(_automata[cellColumn], _graphics);
                    }
            }
            catch (Exception _)
            {
                // ignored
            }
        }

        private void panelAutomata_MouseMove(object sender, MouseEventArgs e)
        {
            var cellColumn = e.X / _columnWidth;
            try
            {
                if (e.Button == MouseButtons.Left)
                    //rozdziel
                    if (!(cellColumn == 0 || cellColumn == _automata.Length - 1))
                    {
                        UpdateCellEarthState(_automata[cellColumn], _graphics, e.Y);
                        FillCell(_automata[cellColumn], _graphics);
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