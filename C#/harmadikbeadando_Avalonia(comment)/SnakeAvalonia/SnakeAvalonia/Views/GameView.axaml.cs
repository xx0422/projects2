using Avalonia;
using Avalonia.Controls;
using Avalonia.Input; // Ez kell a Pointer eseményekhez
using Avalonia.Interactivity;
using Snake.Core.ViewModels; // Hogy elérjük a ViewModel-t
using System;

namespace SnakeAvalonia.Views
{
    public partial class GameView : UserControl
    {
        // Itt tároljuk el, hol érintette meg a képernyőt
        private Point _startPoint;

        public GameView()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            this.Focus();
        }

        // 1. Amikor az ujjad hozzáér a képernyőhöz
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Elmentjük a kezdő pozíciót
            _startPoint = e.GetPosition(this);
        }

        // 2. Amikor felemeled az ujjad (itt történik a varázslat)
        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            var endPoint = e.GetPosition(this);

            // Kiszámoljuk a különbséget
            double diffX = endPoint.X - _startPoint.X;
            double diffY = endPoint.Y - _startPoint.Y;

            // Csak akkor reagáljunk, ha a húzás elég hosszú volt (pl. 40 pixel)
            // Ez megakadályozza a véletlen kattintásokat.
            if (Math.Abs(diffX) < 40 && Math.Abs(diffY) < 40)
                return;

            // Megnézzük, hogy vízszintes vagy függőleges volt-e a húzás
            // Ha a vízszintes elmozdulás nagyobb, mint a függőleges:
            if (Math.Abs(diffX) > Math.Abs(diffY))
            {
                // Elkérjük a ViewModel-t, hogy kiadhassuk a parancsot
                if (DataContext is GameViewModel vm)
                {
                    if (diffX > 0)
                    {
                        // Jobbra húzás -> Jobbra fordul
                        if (vm.TurnRightCommand.CanExecute(null))
                            vm.TurnRightCommand.Execute(null);
                    }
                    else
                    {
                        // Balra húzás -> Balra fordul
                        if (vm.TurnLeftCommand.CanExecute(null))
                            vm.TurnLeftCommand.Execute(null);
                    }
                }
            }
            else
            {
                // Ha függőleges volt a húzás (Fel vagy Le)
                // Extra funkció: Ez lehet a SZÜNET (Pause)
                if (DataContext is GameViewModel vm)
                {
                    if (vm.PauseToggleCommand.CanExecute(null))
                        vm.PauseToggleCommand.Execute(null);
                }
            }
        }
    }
}