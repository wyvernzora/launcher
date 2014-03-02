using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;

namespace Launcher.PagedPanel
{
    /// <summary>
    /// Defines the dragging behavior for PagedPanel elements.
    /// </summary>
    public class PagedPanelDragBehavior : Behavior<UIElement>
    {
        #region Fields

        private Timer timer;
        private PagedPanel panel;
        private ListBoxItem listBoxItem;
        
        #endregion

        #region Dependency Properties

        /// <summary>
        /// DragButton Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragButtonProperty =
            DependencyProperty.Register("DragButton", typeof(MouseButton), typeof(PagedPanelDragBehavior),
                new FrameworkPropertyMetadata(MouseButton.Left));

        /// <summary>
        /// Gets or sets the mouse button that initiates the drag operation.
        /// </summary>
        public MouseButton DragButton
        {
            get { return (MouseButton) GetValue(DragButtonProperty); }
            set { SetValue(DragButtonProperty, value);}
        }


        public static readonly DependencyProperty DragDelayProperty =
            DependencyProperty.Register("DragDelay", typeof(TimeSpan), typeof(PagedPanelDragBehavior),
                new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(50)));

        /// <summary>
        /// Gets or sets the delay before the drag operation is initialized.
        /// In other words, the "click-and-hold" delay.
        /// </summary>
        public TimeSpan DragDelay
        {
            get { return (TimeSpan) GetValue(DragDelayProperty); }
            set { SetValue(DragDelayProperty, value); }
        }

        #endregion

        #region Attach/Detach

        private void ObjectLoaded(Object sender, EventArgs e)
        {
            // Get the PagedPanel host.
            var ancestor = AssociatedObject as FrameworkElement;
            while (ancestor != null)
            {
                // Search for listbox item in case if PagedPanel is an item panel
                var lbAncestor = ancestor as ListBoxItem;
                if (lbAncestor != null)
                    listBoxItem = lbAncestor;

                // Search for PagedPanel that handles the drag operation
                var pagedPanel = ancestor as PagedPanel;
                if (pagedPanel != null)
                {
                    panel = pagedPanel;
                    break;
                }

                // Go up if nothing is found
                ancestor = VisualTreeHelper.GetParent(ancestor) as FrameworkElement;
            }

            // Subscribe to necessary events
            var element = listBoxItem ?? AssociatedObject;
            element.PreviewMouseDown += PreviewMouseDown;
            element.PreviewMouseMove += PreviewMouseMove;
            element.PreviewMouseUp += PreviewMouseUp;
        }

        protected override void OnAttached()
        {
            var @object = AssociatedObject as FrameworkElement;
            if (@object == null) throw new Exception("Unexpected AssociatedObject type: Expected FrameworkElement but received " + AssociatedObject.GetType().Name);

            @object.Loaded += ObjectLoaded;
        }

        protected override void OnDetaching()
        {
            var @object = AssociatedObject as FrameworkElement;
            if (@object == null) 
                throw new Exception("Unexpected AssociatedObject type: Expected FrameworkElement but received " + AssociatedObject.GetType().Name);

            @object.Loaded -= ObjectLoaded;

            var element = listBoxItem ?? AssociatedObject;
            element.PreviewMouseDown -= PreviewMouseDown;
            element.PreviewMouseMove -= PreviewMouseMove;
            element.PreviewMouseUp -= PreviewMouseUp;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Schedules a delayed event response.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        private void ScheduleDelay(Action action, TimeSpan delay)
        {
            // If there is a conflicting delay, cancel it
            CancelDelay();

            // Set up a new timer
            timer = new Timer
            {
                Interval = 300,
                AutoReset = false
            };
            timer.Elapsed += (s, e) =>
            {
                // Kill the timer
                timer.Dispose();
                timer = null;

                // Use dispatcher of the creator's thread
                Dispatcher.Invoke(DispatcherPriority.Input, action);
            };
            timer.Start();
        }

        /// <summary>
        /// Cancels currently scheduled delay task.
        /// </summary>
        private void CancelDelay()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        #endregion

        #region Event Handlers

        private void PreviewMouseDown(Object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == DragButton)
            {
                ScheduleDelay(() =>
                {
                    {
                        // Get target visual
                        var visual = listBoxItem ?? AssociatedObject;

                        // Check if the mouse is still in bounds when delay finishes
                        if (!visual.IsMouseOver)
                            return;

                        // Get mouse position
                        var position = Mouse.GetPosition(visual);
                        var fElement = AssociatedObject as FrameworkElement;
                        if (fElement != null && panel != null)
                            panel.OnStartDrag(visual, position);
                    }
                }, DragDelay);
            }
        }

        private void PreviewMouseMove(Object sender, MouseEventArgs e)
        {
            // Get the target visual
            var visual = listBoxItem ?? AssociatedObject;

            // Test if mouse moved out of range
            if (timer != null && !visual.IsMouseOver)
            {
                //...if so, cancel the scheduled delay
                CancelDelay();
                return;
            }

            // Determine whether the appropriate mouse button is down.
                // this way is so messy... come on .Net
            var isDragging = false;
            switch (DragButton)
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
                var position = e.GetPosition(visual);

                var fElement = AssociatedObject as FrameworkElement;
                if (fElement != null && panel != null)
                {
                    var positionInParent = e.GetPosition(panel);
                    panel.OnDrag(visual, position, positionInParent);
                }
            }
        }

        private void PreviewMouseUp(Object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == DragButton)
            {
                // Cancel running delay
                CancelDelay();

                // Get the target visual
                var visual = listBoxItem ?? AssociatedObject;

                // Notify PagedPanel
                var position = e.GetPosition(visual);
                var fElement = AssociatedObject as FrameworkElement;
                if (fElement != null && panel != null)
                {
                    var positionInParent = e.GetPosition(panel);
                    panel.OnEndDrag(visual, position, positionInParent);
                }
            }
        }

        #endregion


    }
}
