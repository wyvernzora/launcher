// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
// Launcher.Panel/PanoramaPanelDragBehavior.cs
// --------------------------------------------------------------------------------
// Copyright (c) 2014, Jieni Luchijinzhou a.k.a Aragorn Wyvernzora
// 
// This file is a part of Launcher.Panel.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
// of the Software, and to permit persons to whom the Software is furnished to do 
// so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all 
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
// PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace Launcher.Panel
{
    /// <summary>
    /// </summary>
    public class PanoramaPanelDragBehavior : Behavior<FrameworkElement>
    {
        #region Fields

        private PanoramaPanel panel;
        private DelayScheduler scheduler;
        
        #endregion
        
        #region Attach/Detach

        private void Loaded(Object sender, EventArgs e)
        {
            // Get the parent PanoramaPanel
            panel = VisualTreeHelper.GetParent(AssociatedObject) as PanoramaPanel;

            // If there is no parent PanoramaPanel, disable this behavior
            if (panel == null)
                return;

            // Set up the delay scheduler
            scheduler = new DelayScheduler(AssociatedObject.Dispatcher);

            // Subscribe to related events
            AssociatedObject.PreviewMouseDown += PreviewMouseDown;
            AssociatedObject.PreviewMouseMove += PreviewMouseMove;
            AssociatedObject.PreviewMouseUp += PreviewMouseUp;
        }

        protected override void OnAttached()
        {
            AssociatedObject.Loaded += Loaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= Loaded;

            if (panel != null)
            {
                AssociatedObject.PreviewMouseDown -= PreviewMouseDown;
                AssociatedObject.PreviewMouseMove -= PreviewMouseMove;
                AssociatedObject.PreviewMouseUp -= PreviewMouseUp;
            }
        }

        #endregion

        #region Event Handlers

        private Boolean dragInitiated;

        private void PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == panel.DragButton)
            {
                scheduler.Schedule(panel.DragDelay, () =>
                {
                    // Check if mouse is still in bounds
                    if (!AssociatedObject.IsMouseOver)
                        return;

                    // Set drag initiated flag
                    dragInitiated = true;

                    // Get mouse position
                    Point position = Mouse.GetPosition(AssociatedObject);

                    // Notify the PanoramaPanel (if it's there)
                    if (panel != null)
                        panel.OnDragStart(AssociatedObject, position, Mouse.GetPosition(panel));
                });
            }
        }

        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            // Test if mouse moved out of range
            if (!AssociatedObject.IsMouseOver)
            {
                //...if so, cancel the scheduled delay
                scheduler.Cancel();
                dragInitiated = false;
                return;
            }

            // Determine whether the appropriate mouse button is down.
            // this way is so messy... come on .Net
            bool isDragging = false;
            switch (panel.DragButton)
            {
                case MouseButton.Left:
                    isDragging = e.LeftButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.Right:
                    isDragging = e.RightButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.Middle:
                    isDragging = e.MiddleButton == MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton1:
                    isDragging = e.XButton1 == MouseButtonState.Pressed;
                    break;
                case MouseButton.XButton2:
                    isDragging = e.XButton2 == MouseButtonState.Pressed;
                    break;
            }

            // Notify PagedPanel when needed
            if (isDragging)
            {
                Point position = e.GetPosition(AssociatedObject);

                if (panel != null)
                {
                    Point positionInParent = e.GetPosition(panel);
                    panel.OnDragMove(AssociatedObject, position, positionInParent);
                }
            }
        }

        private void PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (dragInitiated && e.ChangedButton == panel.DragButton)
            {
                // Cancel scheduled response
                scheduler.Cancel();

                // Notify PanoramaPanel
                Point position = Mouse.GetPosition(AssociatedObject);
                if (panel != null)
                {
                    Point positionInParent = e.GetPosition(panel);
                    panel.OnDragEnd(AssociatedObject, position, positionInParent);
                }

                // Suppress Click event if drag-drop actually took place
                e.Handled = true;
                dragInitiated = false;
            }
        }

        #endregion
    }
}