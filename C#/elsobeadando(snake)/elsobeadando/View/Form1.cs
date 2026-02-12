using elsobeadando.Model.Events;
using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using static elsobeadando.Model.Directions;


namespace elsobeadando
{
    public partial class Form1 : Form
    {
        private Modell game = null!;
        private int cellSize = 25;
        private int elapsedSeconds = 0;
        private bool isPaused = false;
        private const int topOffset = 30;


        public Form1()
        {
            InitializeComponent();
            DoubleBuffered = true;
            KeyPreview = true;
            KeyDown += Form1_KeyDown;
            

            this.PreviewKeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down ||
                    e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
                {
                    e.IsInputKey = true;
                }
            };
            buttonPause.TabStop = false;


            StartGame();
        }

        private void StartGame()
        {
            
            LevelSelectForm selectForm = new LevelSelectForm();
            if (selectForm.ShowDialog() != DialogResult.OK || selectForm.SelectedLevelPath == null)
                return;


            string selectedLevel = selectForm.SelectedLevelPath;

            game = new Modell(10, 10);
            game.ScoreChanged += score =>
            {
                if (labelScore.InvokeRequired)
                    labelScore.BeginInvoke((Action)(() => labelScore.Text = $"Score: {score}"));
                else
                    labelScore.Text = $"Score: {score}";
            };
            game.SnakeMoved += OnSnakeMoved;
            game.GameEnded += (s, e) =>
            {
                if (this.InvokeRequired)
                    this.BeginInvoke((Action)(() => OnGameEnded(s, e)));
                else
                    OnGameEnded(s, e);
            };
            game.LoadLevel(selectedLevel);

            game.StartGameLoop();


            elapsedSeconds = 0;
            labelTime.Text = "Time: 0 s";
            timerTime.Start();

            this.ClientSize = new Size(game.GetWidth * cellSize + 16, game.GetHeight * cellSize + 39);
            this.Text = "Snake";

            this.ActiveControl = null;
            this.Focus();
        }


        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (game == null || isPaused)
                return;

            switch (e.KeyCode)
            {
                case Keys.Up:
                    game.ChangeDirection(Direction.Up);
                    break;
                case Keys.Down:
                    game.ChangeDirection(Direction.Down);
                    break;
                case Keys.Left:
                    game.ChangeDirection(Direction.Left);
                    break;
                case Keys.Right:
                    game.ChangeDirection(Direction.Right);
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            DrawGrid(g);
            DrawObstacles(g);

            Point foodPos = game.GetFoodPosition;
            g.FillEllipse(Brushes.Red, foodPos.X * cellSize, foodPos.Y * cellSize + topOffset, cellSize, cellSize);

            foreach (Point p in game.GetSnakeBody)
            {
                g.FillRectangle(Brushes.Green, p.X * cellSize, p.Y * cellSize + topOffset, cellSize, cellSize);
            }
        }

        private void DrawGrid(Graphics g)
        {
            using (Pen pen = new Pen(Color.LightGray))
            {
                for (int x = 0; x <= game.GetWidth; x++)
                    g.DrawLine(pen, x * cellSize, 0 + topOffset, x * cellSize, game.GetHeight * cellSize + topOffset);

                for (int y = 0; y <= game.GetHeight; y++)
                    g.DrawLine(pen, 0, y * cellSize + topOffset, game.GetWidth * cellSize, y * cellSize + topOffset);
            }
        }

        private void DrawObstacles(Graphics g)
        {
            foreach (Point o in game.Obstacles)
            {
                g.FillRectangle(Brushes.Gray, o.X * cellSize, o.Y * cellSize + topOffset , cellSize, cellSize);
            }
        }

        private void timerTime_Tick(object sender, EventArgs e)
        {
            elapsedSeconds++;
            labelTime.Text = $"Time: {elapsedSeconds} s";
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                game.Pause();       
                timerTime.Stop();   
                isPaused = true;
                buttonPause.Text = "Folytatás";
            }
            else
            {
                game.Resume();      
                timerTime.Start(); 
                isPaused = false;
                buttonPause.Text = "Szünet";
            }

            this.ActiveControl = null;
            this.Focus();
        }

        private void labelScore_Click(object sender, EventArgs e)
        {

        }
        private void OnSnakeMoved(object? sender, SnakeEventArgs e)
        {
            Invalidate(); 
            if (e.AteFood)
                labelScore.Text = $"Score: {e.Score}";
        }

        private void OnGameEnded(object? sender, SnakeEventArgs e)
        {
            timerTime.Stop();
            MessageBox.Show($"Game over! Score: {e.Score}", "Snake");
            StartGame();
        }

    }
}
