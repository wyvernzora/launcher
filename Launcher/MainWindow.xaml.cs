using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
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

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const Int32 TransitionDuration = 300;
        private const Int32 WindowMargin = 0;

        private Storyboard pageTransitionStoryboard;

        public MainWindow()
        {
            // Set up window dimensions
            Width = 420;
            Height = 520;

            // Put window where it should be
            AdjustWindowLocation();

            // Initialize components and set up layout
            InitializeComponent();
            contentCanvas.ClipToBounds = true;
            
            AttachEventHandlers();
        }

        void AttachEventHandlers()
        {
            // Load/Unload
            Loaded += (@s, e) => SearchBox.Focus();
            Deactivated += (@s, e) => Close();

            // Page Transitions
            contentCanvas.MouseWheel += (@s, e) =>
            {
                var count = e.Delta / Mouse.MouseWheelDeltaForOneLine;
                var targetPage = activePageIndex - count;
                if (targetPage < 0)
                    targetPage = 0;
                if (targetPage >= pagesPanel.PageCount)
                    targetPage = pagesPanel.PageCount - 1;

                TransitionToPage(targetPage, TransitionDuration);
            };

            // Temporary
            btnNext.Click += (@s, e) =>
            {
                if (activePageIndex < pagesPanel.PageCount - 1)
                    TransitionToPage(activePageIndex + 1, TransitionDuration);
            };
            btnPrev.Click += (@s, e) =>
            {
                if (activePageIndex > 0)
                    TransitionToPage(activePageIndex - 1, TransitionDuration);
            };
            btnLast.Click += (@s, e) => TransitionToPage(pagesPanel.PageCount - 1, TransitionDuration);
            btnFirst.Click += (@s, e) => TransitionToPage(0, TransitionDuration);

        }

        void AdjustWindowLocation()
        {
            var location = new Point();

            // Adjust window location according to the cursor
            var cursor = Utilities.GetCursorPosition();
            location.X = cursor.X - Width / 2;
            location.Y = cursor.Y - Height;

            // Adjust location so that it is within the monitor
            var workArea = SystemParameters.WorkArea;

            // X axis
            if (location.X < workArea.X)
                location.X = workArea.X + WindowMargin;
            if (location.X + Width > workArea.X + workArea.Width)
                location.X = workArea.X + workArea.Width - Width - WindowMargin;

            // Y axis
            if (location.Y < workArea.Y)
                location.Y = workArea.Y + WindowMargin;
            if (location.Y + Height > workArea.Y + workArea.Height)
                location.Y = workArea.Y + workArea.Height - Height - WindowMargin;


            Left = location.X;
            Top = location.Y;
        }

        #region Page Transition & Animation

        private Int32 activePageIndex;

        private void TransitionToPage(Int32 index, Int32 duration)
        {
            var animationDuration = new Duration(TimeSpan.FromMilliseconds(duration));

            var animation = new DoubleAnimation(index * -contentCanvas.ActualWidth, animationDuration);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Left)"));
            Storyboard.SetTarget(animation, pagesPanel);

            var easing = new ExponentialEase {EasingMode = EasingMode.EaseInOut};
            animation.EasingFunction = easing;

            pageTransitionStoryboard = new Storyboard();
            pageTransitionStoryboard.Children.Add(animation);

            activePageIndex = index;
            pageTransitionStoryboard.Begin();
        }
        
        #endregion
    }
}
