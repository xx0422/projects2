using CommunityToolkit.Mvvm.Input;
using SnakeAvalonia.Views; // Ez elvileg nem is kell ide
using Snake.Core.ViewModels; // Ez sem kell ide

namespace SnakeAvalonia.ViewModels
{
    public partial class LevelSelectViewModel
    {
        private readonly MainViewModel _mainVM;

        public LevelSelectViewModel(MainViewModel vm)
        {
            _mainVM = vm;
        }

        [RelayCommand]
       
        private void StartLevel(string levelPath)
        {
            // HELYES MEGOLDÁS:
            // Szólunk a MainViewModel-nek, hogy ő indítsa a játékot!
            // Így ő át tudja adni a "kilépés" parancsot is.
            _mainVM.StartGame(levelPath);
        }
    }
}