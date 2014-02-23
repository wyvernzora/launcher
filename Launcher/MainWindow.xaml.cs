using System;
using System.Collections.Generic;
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
        private const Int32 Margin = 0;

        public MainWindow()
        {
            // Set up window dimensions
            Width = 420;
            Height = 520;

            AdjustWindowLocation();

            InitializeComponent();
            AttachEventHandlers();

            // Enable Shadow
            Utilities.DropShadowToWindow(this);
        }

        void AttachEventHandlers()
        {
            Deactivated += (@s, e) => Close();
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
                location.X = workArea.X + Margin;
            if (location.X + Width > workArea.X + workArea.Width)
                location.X = workArea.X + workArea.Width - Width - Margin;

            // Y axis
            if (location.Y < workArea.Y)
                location.Y = workArea.Y + Margin;
            if (location.Y + Height > workArea.Y + workArea.Height)
                location.Y = workArea.Y + workArea.Height - Height - Margin;


            Left = location.X;
            Top = location.Y;
        }
    }
}
