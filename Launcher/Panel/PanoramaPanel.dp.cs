// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
// Launcher.Panel/PanoramaPanel.dp.cs
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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Launcher.Panel
{
    // This is file for dependency property definitions.
    /// <summary>
    ///     Paged wrap panel witth fluid drag-and-drop behavior.
    /// </summary>
    public partial class PanoramaPanel
    {
        // Default Values
        protected const Double DefaultScale = 1.0;
        protected const Double DefaultOpacity = 1.0;
        protected const Double MinimumOpacity = 0.1;

        // Default Drag Values
        protected const Double DefaultDragScale = 1.0;
        protected const Double DefaultDragOpacity = 1.0;

        // Z-Index
        protected const Int32 DefaultZ = 0;
        protected const Int32 TransitionZ = 1;
        protected const Int32 DragZ = Int32.MaxValue;

        // Default Transition Durations
        protected const Int32 DefaultTransitionDuration = 300;

        #region Easing

        /// <summary>
        ///     Easing Dependency Property
        /// </summary>
        public static readonly DependencyProperty EasingProperty =
            DependencyProperty.Register("Easing", typeof(EasingFunctionBase), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(OnEasingChanged));

        /// <summary>
        ///     Gets or sets the easing function for all transitions within the panel.
        /// </summary>
        [Category("Transitions")]
        [Description("Defines the easing function for all animated transitions.")]
        public EasingFunctionBase Easing
        {
            get { return (EasingFunctionBase) GetValue(EasingProperty); }
            set { SetValue(EasingProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the Easing property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnEasingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel panel = d as PanoramaPanel;
            if (panel != null)
                panel.OnEasingChanged((EasingFunctionBase) e.OldValue, (EasingFunctionBase) e.NewValue);
        }

        /// <summary>
        ///     Derived classes can override this method to handle changes to the Easing property.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnEasingChanged(EasingFunctionBase oldValue, EasingFunctionBase newValue)
        {
        }

        #endregion

        #region DragScale

        /// <summary>
        ///     DragScale Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragScaleProperty =
            DependencyProperty.Register("DragScale", typeof(double), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(DefaultDragScale, OnDragScaleChanged));

        /// <summary>
        ///     Gets or sets the scale of the dragged item.
        /// </summary>
        [Category("Drag & Drop")]
        [Description("Defines the scaling factor of the element when being dragged.")]
        public double DragScale
        {
            get { return (double) GetValue(DragScaleProperty); }
            set { SetValue(DragScaleProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the DragScale property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel panel = (PanoramaPanel) d;
            double oldDragScale = (double) e.OldValue;
            double newDragScale = panel.DragScale;
            panel.OnDragScaleChanged(oldDragScale, newDragScale);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the DragScale property.
        /// </summary>
        /// <param name="oldDragScale">Old Value</param>
        /// <param name="newDragScale">New Value</param>
        protected virtual void OnDragScaleChanged(double oldDragScale, double newDragScale)
        {
        }

        #endregion

        #region DragOpacity

        /// <summary>
        ///     DragOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragOpacityProperty =
            DependencyProperty.Register("DragOpacity", typeof(double), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(DefaultDragOpacity, OnDragOpacityChanged, CoerceDragOpacity));

        /// <summary>
        ///     Gets or sets the opacity of the dragged item.
        /// </summary>
        [Category("Drag & Drop")]
        [Description("Defines the opacity of an element while being dragged.")]
        public double DragOpacity
        {
            get { return (double) GetValue(DragOpacityProperty); }
            set { SetValue(DragOpacityProperty, value); }
        }

        /// <summary>
        ///     Coerces the opacity of the dragged item to an acceptable value
        /// </summary>
        /// <param name="d">Dependency Object</param>
        /// <param name="value">Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceDragOpacity(DependencyObject d, object value)
        {
            double opacity = (double) value;

            if (opacity < MinimumOpacity)
                opacity = MinimumOpacity;
            else if (opacity > DefaultOpacity)
                opacity = DefaultOpacity;

            return opacity;
        }

        /// <summary>
        ///     Handles changes to the DragOpacity property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel panel = (PanoramaPanel) d;
            double oldDragOpacity = (double) e.OldValue;
            double newDragOpacity = panel.DragOpacity;
            panel.OnDragOpacityChanged(oldDragOpacity, newDragOpacity);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the DragOpacity property.
        /// </summary>
        /// <param name="oldDragOpacity">Old Value</param>
        /// <param name="newDragOpacity">New Value</param>
        protected virtual void OnDragOpacityChanged(double oldDragOpacity, double newDragOpacity)
        {
        }

        #endregion

        #region Orientation

        /// <summary>
        ///     Orientation Dependency Property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOrientationChanged));

        /// <summary>
        ///     Gets or sets the Orientation property. This dependency property
        ///     indicates the orientation of arrangement of items in the panel.
        /// </summary>
        [Category("Layout")]
        [Description("Determines the orientation of pages in the panel.")]
        public Orientation Orientation
        {
            get { return (Orientation) GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the Orientation property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel panel = (PanoramaPanel) d;
            Orientation oldOrientation = (Orientation) e.OldValue;
            Orientation newOrientation = panel.Orientation;
            panel.OnOrientationChanged(oldOrientation, newOrientation);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the Orientation property.
        /// </summary>
        /// <param name="oldOrientation">Old Value</param>
        /// <param name="newOrientation">New Value</param>
        protected virtual void OnOrientationChanged(Orientation oldOrientation, Orientation newOrientation)
        {
            InvalidateVisual();
        }

        #endregion

        #region ActivePage

        /// <summary>
        ///     ActivePage Dependency Property.
        /// </summary>
        public static readonly DependencyProperty ActivePageProperty =
            DependencyProperty.Register("ActivePage", typeof(Int32), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(-1, OnActivePageChanged));

        /// <summary>
        ///     Gets or sets the index of the active page.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Int32 ActivePage
        {
            get { return (Int32) GetValue(ActivePageProperty); }
            set { SetValue(ActivePageProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the ActivePages property.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void OnActivePageChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel panel = o as PanoramaPanel;
            if (panel != null)
                panel.OnActivePageChanged((Int32) e.OldValue, (Int32) e.NewValue);
        }

        /// <summary>
        ///     Derived types can override this method to handle changes to the ActivePage property.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        protected virtual void OnActivePageChanged(Int32 oldValue, Int32 newValue)
        {
        }

        #endregion

        #region CellHeight

        /// <summary>
        ///     ItemHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty CellHeightProperty =
            DependencyProperty.Register("CellHeight", typeof(double), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnCellHeightChanged));

        /// <summary>
        ///     Gets or sets the ItemHeight property. This dependency property
        ///     indicates the height of each item.
        /// </summary>
        [Category("Layout")]
        [Description("Height of a single item cell")]
        public double CellHeight
        {
            get { return (double) GetValue(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the ItemHeight property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnCellHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel fwPanel = (PanoramaPanel) d;
            double oldItemHeight = (double) e.OldValue;
            double newItemHeight = fwPanel.CellHeight;
            fwPanel.OnCellHeightChanged(oldItemHeight, newItemHeight);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the ItemHeight property.
        /// </summary>
        /// <param name="oldItemHeight">Old Value</param>
        /// <param name="newItemHeight">New Value</param>
        protected void OnCellHeightChanged(double oldItemHeight, double newItemHeight)
        {
        }

        #endregion

        #region CellWidth

        /// <summary>
        ///     ItemWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty CellWidthProperty =
            DependencyProperty.Register("CellWidth", typeof(double), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnCellWidthChanged));

        /// <summary>
        ///     Gets or sets the ItemWidth property. This dependency property
        ///     indicates the width of each item.
        /// </summary>
        [Category("Layout")]
        [Description("Width of a single item cell")]
        public double CellWidth
        {
            get { return (double) GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the ItemWidth property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnCellWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel fwPanel = (PanoramaPanel) d;
            double oldItemWidth = (double) e.OldValue;
            double newItemWidth = fwPanel.CellWidth;
            fwPanel.OnCellWidthChanged(oldItemWidth, newItemWidth);
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the ItemWidth property.
        /// </summary>
        /// <param name="oldItemWidth">Old Value</param>
        /// <param name="newItemWidth">New Value</param>
        protected void OnCellWidthChanged(double oldItemWidth, double newItemWidth)
        {
        }

        #endregion

        #region PageHeight

        /// <summary>
        ///     PageHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty PageHeightProperty =
            DependencyProperty.Register("PageHeight", typeof(double), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(100.0, /* FrameworkPropertyMetadataOptions.AffectsMeasure, */
                    OnPageHeightChanged));

        /// <summary>
        ///     Gets or sets the height of a panel page.
        /// </summary>
        [Category("Layout")]
        [Description("Height of a single page.")]
        public double PageHeight
        {
            get { return (double) GetValue(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the PageHeight property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPageHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel fwPanel = (PanoramaPanel) d;
            double oldHeight = (double) e.OldValue;
            double newHeight = fwPanel.CellHeight;
            fwPanel.OnCellHeightChanged(oldHeight, newHeight);
            fwPanel.InvalidateMeasure();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the PageHeight property.
        /// </summary>
        /// <param name="oldHeight">Old Value</param>
        /// <param name="newHeight">New Value</param>
        protected void OnPageHeightChanged(double oldHeight, double newHeight)
        {
            InvalidateMeasure();
        }

        #endregion

        #region PageWidth

        /// <summary>
        ///     PageWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty PageWidthProperty =
            DependencyProperty.Register("PageWidth", typeof(double), typeof(PanoramaPanel),
                new FrameworkPropertyMetadata(100.0, /*FrameworkPropertyMetadataOptions.AffectsMeasure, */
                    OnPageWidthChanged));

        /// <summary>
        ///     Gets or sets the width of a panel page.
        /// </summary>
        [Category("Layout")]
        [Description("Width of a single page.")]
        public double PageWidth
        {
            get { return (double) GetValue(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }

        /// <summary>
        ///     Handles changes to the PageWidth property.
        /// </summary>
        /// <param name="d">LauncherPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPageWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PanoramaPanel fwPanel = (PanoramaPanel) d;
            double oldWidth = (double) e.OldValue;
            double newWidth = fwPanel.PageWidth;
            fwPanel.OnPageWidthChanged(oldWidth, newWidth);
            fwPanel.InvalidateMeasure();
        }

        /// <summary>
        ///     Provides derived classes an opportunity to handle changes to the ItemWidth property.
        /// </summary>
        /// <param name="oldWidth">Old Value</param>
        /// <param name="newWidth">New Value</param>
        protected void OnPageWidthChanged(double oldWidth, double newWidth)
        {
            InvalidateMeasure();
        }

        #endregion

        // PanoramaPanelDragBehavior Interfacing
        #region DragButton

        /// <summary>
        ///     DragButton Dependency Property.
        /// </summary>
        public static readonly DependencyProperty DragButtonProperty =
            DependencyProperty.Register("DragButton", typeof(MouseButton), typeof(PanoramaPanel),
                new PropertyMetadata(MouseButton.Left));

        /// <summary>
        ///     Gets or sets the mouse button that initiates the drag operation.
        /// </summary>
        [Category("Drag & Drop")]
        public MouseButton DragButton
        {
            get { return (MouseButton) GetValue(DragButtonProperty); }
            set { SetValue(DragButtonProperty, value); }
        }

        #endregion

        #region DragDelay

        /// <summary>
        ///     DragDelay Dependency Property.
        /// </summary>
        public static readonly DependencyProperty DragDelayProperty =
            DependencyProperty.Register("DragDelay", typeof(TimeSpan), typeof(PanoramaPanel),
                new PropertyMetadata(TimeSpan.FromMilliseconds(300)));

        /// <summary>
        ///     Gets or sets the delay before the drag operation is initialized.
        ///     In other words, the "click-and-hold" delay.
        /// </summary>
        [Category("Drag & Drop")]
        public TimeSpan DragDelay
        {
            get { return (TimeSpan) GetValue(DragDelayProperty); }
            set { SetValue(DragDelayProperty, value); }
        }

        #endregion

    }
}