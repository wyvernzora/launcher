using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Point CursorLocation { get; private set; }

        static App()
        {
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            CursorLocation = Utilities.GetCursorPosition();
            base.OnStartup(e);
        }
    }
}
