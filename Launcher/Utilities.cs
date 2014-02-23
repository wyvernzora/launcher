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
        
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Win32Point pt); 

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
    }
}
