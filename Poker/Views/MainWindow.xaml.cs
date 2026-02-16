using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Poker.Views
{
    public partial class MainWindow : Window
    {
        private readonly List<UserControl> _pages;
        private int currentIndex;
        private bool isAnimationEnabled = false;
        public MainWindow()
        {
            _pages = new List<UserControl>()
            {
                new GamePage(),
                new GameInfoPage(),
                new SettingsPage()
            };
            InitializeComponent();
            front.Content = _pages[currentIndex];
        }

        private void PageButton_Click(object sender, RoutedEventArgs e)
        {
            if(!int.TryParse(((Button)sender)?.Tag.ToString(), out int targetIndex))
                return;
            if (targetIndex == currentIndex)
                return;

            if (!isAnimationEnabled)
            {
                currentIndex = targetIndex;
                front.Content = _pages[currentIndex];
                return;
            }
            bool isLeft = currentIndex < targetIndex;
            AnimatePageTransition(targetIndex, isLeft);
        }
        private void AnimatePageTransition(int targetIndex, bool isLeft)
        {
            //transform
            var frontTransform = new TranslateTransform();
            var backTransform = new TranslateTransform();

            front.RenderTransform = frontTransform;
            back.RenderTransform = backTransform;

            //animation
            double width = ActualWidth;
            double fromFront = 0;
            double toFront = isLeft ? -width : width;
            double fromBack = isLeft ? width : -width;
            double toBack = 0;

            var duration = new Duration(TimeSpan.FromMilliseconds(300));

            var frontAnimation = new DoubleAnimation(fromFront, toFront, duration);
            var backAnimation = new DoubleAnimation(fromBack, toBack, duration);

            back.Content = _pages[targetIndex];
            back.Visibility = Visibility.Visible;

            frontAnimation.Completed += (s, e) =>
            {
                back.Visibility = Visibility.Collapsed;
                currentIndex = targetIndex;
                front.Content = _pages[currentIndex];
            };

            frontTransform.BeginAnimation(TranslateTransform.XProperty, frontAnimation);
            backTransform.BeginAnimation(TranslateTransform.XProperty, backAnimation);
        }
    }
}