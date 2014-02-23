using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Launcher
{
    public static class Utilities
    {
        #region Win32 Calls

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }  

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Win32Point pt);
        
        [DllImport("dwmapi.dll", PreserveSig = true)]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        #endregion

        /// <summary>
        /// Gets the on screen position of the cursor.
        /// </summary>
        /// <returns></returns>
        public static Point GetCursorPosition()
        {
            var point = new Win32Point();
            GetCursorPos(ref point);
            return new Point(point.X, point.Y);
        }

        /// <summary>
        /// Drops a standard shadow to a WPF Window, even if the window isborderless. Only works with DWM (Vista and Seven).
        /// This method is much more efficient than setting AllowsTransparency to true and using the DropShadow effect,
        /// as AllowsTransparency involves a huge permormance issue (hardware acceleration is turned off for all the window).
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        public static void DropShadowToWindow(Window window)
        {
            if (!DropShadow(window))
            {
                window.SourceInitialized += new EventHandler(window_SourceInitialized);
            }
        }

        private static void window_SourceInitialized(object sender, EventArgs e) //fixed typo
        {
            Window window = (Window)sender;

            DropShadow(window);

            window.SourceInitialized -= new EventHandler(window_SourceInitialized);
        }

        /// <summary>
        /// The actual method that makes API calls to drop the shadow to the window
        /// </summary>
        /// <param name="window">Window to which the shadow will be applied</param>
        /// <returns>True if the method succeeded, false if not</returns>
        private static bool DropShadow(Window window)
        {
            try
            {
                WindowInteropHelper helper = new WindowInteropHelper(window);
                int val = 2;
                int ret1 = DwmSetWindowAttribute(helper.Handle, 2, ref val, 4);

                if (ret1 == 0)
                {
                    Margins m = new Margins { Bottom = 0, Left = 0, Right = 0, Top = 0 };
                    int ret2 = DwmExtendFrameIntoClientArea(helper.Handle, ref m);
                    return ret2 == 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Probably dwmapi.dll not found (incompatible OS)
                return false;
            }
        }
    }
}
