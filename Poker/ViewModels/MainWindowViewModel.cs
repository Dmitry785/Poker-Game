using Poker.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Poker.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly UserControl[] _pages;
        private int currentPageIndex;
        private UserControl currentPage;
        public UserControl CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPageName));
            }
        }
        public string CurrentPageName
        {
            get => (currentPageIndex == 0) ? "Game" : (currentPageIndex == 1) ? "Game info" : "Settings";
        }
        public RelayCommand PageSelectedCommand { get; }
        public MainWindowViewModel()
        {
            PageSelectedCommand = new RelayCommand(OnPageSelected);
            _pages = [
                new GamePage(),
                new GameInfoPage(),
                new SettingsPage()
            ];
            currentPage = _pages[currentPageIndex];
        }
        private void OnPageSelected(object? parameter)
        {
            if (!int.TryParse(parameter?.ToString(), out int pageNumber))
                return;
            if (pageNumber < 0 || pageNumber >= _pages.Length)
                return;
            currentPageIndex = pageNumber;
            CurrentPage = _pages[currentPageIndex];
        }
    }
}
