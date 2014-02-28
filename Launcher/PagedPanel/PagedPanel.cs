using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Launcher.PagedPanel
{
    public class PagedPanel : Panel
    {
        #region Constants

        private const Double NormalScale = 1.0d;
        private const Double DefaultDragScale = 1.0d;
        private const Double NormalOpacity = 1.0d;
        private const Double DefaultDragOpacity = 1.0d;
        private const Double MinimumOpacity = 1.0d;
        private const Int32 NormalZIndex = 0;
        private const Int32 TransitionZIndex = 1;
        private const Int32 DragZIndex = Int32.MaxValue;
        private const Int32 DefaultTransitionDuration = 400;
        private const Int32 FirstTimeTransitionDuration = 320;
        private const Int32 DefaultNoEaseTransitionDuration = 200;

        #endregion

        public PagedPanel()
        {
            //pages = new List<List<UIElement>>();
            pageCount = 1;
            initRequired = true;

            // DEBUG CODE
#if DEBUG

            //pages.Add(new List<UIElement>());
            //pages.Add(new List<UIElement>());
            pageCount += 2;

#endif
        }

        #region Fields

        private Point dragStart;
        private UIElement draggingElement;
        private UIElement lastDraggedElement;
        private Boolean initRequired;

        private Int32 pageCount;
        private Size pageGridSize;

        #endregion

        #region Dependency Properties

        #region DragDropEasing

        /// <summary>
        /// DragEasing Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragDropEasingProperty =
            DependencyProperty.Register("DragDropEasing", typeof(EasingFunctionBase), typeof(PagedPanel),
                new FrameworkPropertyMetadata((OnDragDropEasingChanged)));

        /// <summary>
        /// Gets or sets the easing function for drag drop transitions.
        /// </summary>
        public EasingFunctionBase DragDropEasing
        {
            get { return (EasingFunctionBase)GetValue(DragDropEasingProperty); }
            set { SetValue(DragDropEasingProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DragEasing property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragDropEasingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel)d;
            EasingFunctionBase oldDragEasing = (EasingFunctionBase)e.OldValue;
            EasingFunctionBase newDragEasing = panel.DragDropEasing;
            panel.OnDragDropEasingChanged(oldDragEasing, newDragEasing);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DragEasing property.
        /// </summary>
        /// <param name="oldEasing">Old Value</param>
        /// <param name="newEasing">New Value</param>
        protected virtual void OnDragDropEasingChanged(EasingFunctionBase oldEasing, EasingFunctionBase newEasing)
        { }

        #endregion

        #region DragOpacity

        /// <summary>
        /// DragOpacity Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragOpacityProperty =
            DependencyProperty.Register("DragOpacity", typeof(double), typeof(PagedPanel),
                new FrameworkPropertyMetadata(DefaultDragOpacity, OnDragOpacityChanged, CoerceDragOpacity));

        /// <summary>
        /// Gets or sets the opacity of the dragged item.
        /// </summary>
        public double DragOpacity
        {
            get { return (double)GetValue(DragOpacityProperty); }
            set { SetValue(DragOpacityProperty, value); }
        }

        /// <summary>
        /// Coerces the opacity of the dragged item to an acceptable value
        /// </summary>
        /// <param name="d">Dependency Object</param>
        /// <param name="value">Value</param>
        /// <returns>Coerced Value</returns>
        private static object CoerceDragOpacity(DependencyObject d, object value)
        {
            double opacity = (double)value;

            if (opacity < MinimumOpacity)
                opacity = MinimumOpacity;
            else if (opacity > NormalOpacity)
                opacity = NormalOpacity;

            return opacity;
        }

        /// <summary>
        /// Handles changes to the DragOpacity property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel)d;
            double oldDragOpacity = (double)e.OldValue;
            double newDragOpacity = panel.DragOpacity;
            panel.OnDragOpacityChanged(oldDragOpacity, newDragOpacity);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DragOpacity property.
        /// </summary>
        /// <param name="oldDragOpacity">Old Value</param>
        /// <param name="newDragOpacity">New Value</param>
        protected virtual void OnDragOpacityChanged(double oldDragOpacity, double newDragOpacity)
        { }

        #endregion

        #region DragScale

        /// <summary>
        /// DragScale Dependency Property
        /// </summary>
        public static readonly DependencyProperty DragScaleProperty =
            DependencyProperty.Register("DragScale", typeof(double), typeof(PagedPanel),
                new FrameworkPropertyMetadata(DefaultDragScale, OnDragScaleChanged));

        /// <summary>
        /// Gets or sets the scale of the dragged item.
        /// </summary>
        public double DragScale
        {
            get { return (double)GetValue(DragScaleProperty); }
            set { SetValue(DragScaleProperty, value); }
        }

        /// <summary>
        /// Handles changes to the DragScale property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnDragScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel)d;
            double oldDragScale = (double)e.OldValue;
            double newDragScale = panel.DragScale;
            panel.OnDragScaleChanged(oldDragScale, newDragScale);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the DragScale property.
        /// </summary>
        /// <param name="oldDragScale">Old Value</param>
        /// <param name="newDragScale">New Value</param>
        protected virtual void OnDragScaleChanged(double oldDragScale, double newDragScale)
        { }

        #endregion

        #region MovementEasing

        /// <summary>
        /// ElementEasing Dependency Property
        /// </summary>
        public static readonly DependencyProperty MovementEasingProperty =
            DependencyProperty.Register("MovementEasing", typeof(EasingFunctionBase), typeof(PagedPanel),
                new FrameworkPropertyMetadata((OnMovementEasingChanged)));

        /// <summary>
        /// Gets or sets the easing function for element movement animation.
        /// </summary>
        public EasingFunctionBase MovementEasing
        {
            get { return (EasingFunctionBase)GetValue(MovementEasingProperty); }
            set { SetValue(MovementEasingProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ElementEasing property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnMovementEasingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel)d;
            EasingFunctionBase oldElementEasing = (EasingFunctionBase)e.OldValue;
            EasingFunctionBase newElementEasing = panel.MovementEasing;
            panel.OnMovementEasingChanged(oldElementEasing, newElementEasing);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ElementEasing property.
        /// </summary>
        /// <param name="oldElementEasing">Old Value</param>
        /// <param name="newElementEasing">New Value</param>
        /// 
        protected virtual void OnMovementEasingChanged(EasingFunctionBase oldElementEasing, EasingFunctionBase newElementEasing)
        { }

        #endregion

        #region IsComposing

        /// <summary>
        /// IsComposing Dependency Property
        /// </summary>
        public static readonly DependencyProperty IsComposingProperty =
            DependencyProperty.Register("IsComposing", typeof(bool), typeof(PagedPanel),
                new FrameworkPropertyMetadata((OnIsComposingChanged)));

        /// <summary>
        /// Gets or sets a value that indicates if the PagedPanel is in Composing mode.
        /// </summary>
        public bool IsComposing
        {
            get { return (bool)GetValue(IsComposingProperty); }
            set { SetValue(IsComposingProperty, value); }
        }

        /// <summary>
        /// Handles changes to the IsComposing property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnIsComposingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel)d;
            bool oldIsComposing = (bool)e.OldValue;
            bool newIsComposing = panel.IsComposing;
            panel.OnIsComposingChanged(oldIsComposing, newIsComposing);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the IsComposing property.
        /// </summary>
        /// <param name="oldIsComposing">Old Value</param>
        /// <param name="newIsComposing">New Value</param>
        protected virtual void OnIsComposingChanged(bool oldIsComposing, bool newIsComposing)
        { }

        #endregion

        #region CellHeight

        /// <summary>
        /// ItemHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty CellHeightProperty =
            DependencyProperty.Register("CellHeight", typeof(double), typeof(PagedPanel),
                new FrameworkPropertyMetadata(0.0, OnCellHeightChanged));

        /// <summary>
        /// Gets or sets the ItemHeight property. This dependency property 
        /// indicates the height of each item.
        /// </summary>
        public double CellHeight
        {
            get { return (double)GetValue(CellHeightProperty); }
            set { SetValue(CellHeightProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ItemHeight property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnCellHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel fwPanel = (PagedPanel)d;
            double oldItemHeight = (double)e.OldValue;
            double newItemHeight = fwPanel.CellHeight;
            fwPanel.OnCellHeightChanged(oldItemHeight, newItemHeight);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemHeight property.
        /// </summary>
        /// <param name="oldItemHeight">Old Value</param>
        /// <param name="newItemHeight">New Value</param>
        protected void OnCellHeightChanged(double oldItemHeight, double newItemHeight)
        { }

        #endregion

        #region CellWidth

        /// <summary>
        /// ItemWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty CellWidthProperty =
            DependencyProperty.Register("CellWidth", typeof(double), typeof(PagedPanel),
                new FrameworkPropertyMetadata(0.0, OnCellWidthChanged));

        /// <summary>
        /// Gets or sets the ItemWidth property. This dependency property 
        /// indicates the width of each item.
        /// </summary>
        public double CellWidth
        {
            get { return (double)GetValue(CellWidthProperty); }
            set { SetValue(CellWidthProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ItemWidth property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnCellWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel fwPanel = (PagedPanel)d;
            double oldItemWidth = (double)e.OldValue;
            double newItemWidth = fwPanel.CellWidth;
            fwPanel.OnCellWidthChanged(oldItemWidth, newItemWidth);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemWidth property.
        /// </summary>
        /// <param name="oldItemWidth">Old Value</param>
        /// <param name="newItemWidth">New Value</param>
        protected void OnCellWidthChanged(double oldItemWidth, double newItemWidth)
        {

        }

        #endregion
        
        #region PageHeight

        /// <summary>
        /// PageHeight Dependency Property
        /// </summary>
        public static readonly DependencyProperty PageHeightProperty =
            DependencyProperty.Register("PageHeight", typeof(double), typeof(PagedPanel),
                new FrameworkPropertyMetadata(100.0, OnPageHeightChanged));

        /// <summary>
        /// Gets or sets the height of a panel page.
        /// </summary>
        public double PageHeight
        {
            get { return (double)GetValue(PageHeightProperty); }
            set { SetValue(PageHeightProperty, value); }
        }

        /// <summary>
        /// Handles changes to the PageHeight property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPageHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel fwPanel = (PagedPanel)d;
            double oldHeight = (double)e.OldValue;
            double newHeight = fwPanel.CellHeight;
            fwPanel.OnCellHeightChanged(oldHeight, newHeight);
            fwPanel.InvalidateMeasure();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the PageHeight property.
        /// </summary>
        /// <param name="oldHeight">Old Value</param>
        /// <param name="newHeight">New Value</param>
        protected void OnPageHeightChanged(double oldHeight, double newHeight)
        { }

        #endregion

        #region PageWidth

        /// <summary>
        /// PageWidth Dependency Property
        /// </summary>
        public static readonly DependencyProperty PageWidthProperty =
            DependencyProperty.Register("PageWidth", typeof(double), typeof(PagedPanel),
                new FrameworkPropertyMetadata(100.0, OnPageWidthChanged));

        /// <summary>
        /// Gets or sets the width of a panel page.
        /// </summary>
        public double PageWidth
        {
            get { return (double)GetValue(PageWidthProperty); }
            set { SetValue(PageWidthProperty, value); }
        }

        /// <summary>
        /// Handles changes to the PageWidth property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnPageWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel fwPanel = (PagedPanel)d;
            double oldWidth = (double)e.OldValue;
            double newWidth = fwPanel.PageWidth;
            fwPanel.OnPageWidthChanged(oldWidth, newWidth);
            fwPanel.InvalidateMeasure();
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemWidth property.
        /// </summary>
        /// <param name="oldWidth">Old Value</param>
        /// <param name="newWidth">New Value</param>
        protected void OnPageWidthChanged(double oldWidth, double newWidth)
        {

        }

        #endregion

        #region PagePadding

        /// <summary>
        /// PagePadding dependency property.
        /// </summary>
        public static readonly DependencyProperty PagePaddingProperty =
            DependencyProperty.Register("PagePadding", typeof(Thickness), typeof(PagedPanel),
                new FrameworkPropertyMetadata(new Thickness(0), OnPagePaddingChanged));

        /// <summary>
        /// Gets or sets the padding around page borders.
        /// </summary>
        public Thickness PagePadding
        {
            get { return (Thickness) GetValue(PagePaddingProperty); }
            set { SetValue(PagePaddingProperty, value); }
        }

        /// <summary>
        /// Handles changes to PagePadding property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnPagePaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel) d;
            var oldPadding = (Thickness) e.OldValue;
            var newPadding = panel.PagePadding;
            panel.OnPagePaddingChanged(oldPadding, newPadding);
        }

        /// <summary>
        /// Gives derived classes opportunity to handle PagePadding changes.
        /// </summary>
        /// <param name="oldPadding"></param>
        /// <param name="newPadding"></param>
        protected virtual void OnPagePaddingChanged(Thickness oldPadding, Thickness newPadding)
        {
            
        }

        #endregion

        #region ItemsSource

        /// <summary>
        /// ItemsSource Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PagedPanel),
                new FrameworkPropertyMetadata(OnItemsSourceChanged));

        /// <summary>
        /// Gets or sets the ItemsSource property. This dependency property 
        /// indicates the bindable collection.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get { return (ObservableCollection<UIElement>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the ItemsSource property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel)d;
            IEnumerable oldItemsSource = (ObservableCollection<UIElement>)e.OldValue;
            IEnumerable newItemsSource = panel.ItemsSource;
            panel.OnItemsSourceChanged(oldItemsSource, newItemsSource);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the ItemsSource property.
        /// </summary>
        /// <param name="oldItemsSource">Old Value</param>
        /// <param name="newItemsSource">New Value</param>
        protected void OnItemsSourceChanged(IEnumerable oldItemsSource, IEnumerable newItemsSource)
        {
            // Clear the previous items in the Children property
            ClearItemsSource();

            // Add the new children
            foreach (UIElement child in newItemsSource)
            {
                Children.Add(child);
            }

            initRequired = true;

            InvalidateVisual();
        }

        #endregion

        #region Orientation

        /// <summary>
        /// Orientation Dependency Property
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(PagedPanel),
                new FrameworkPropertyMetadata(Orientation.Horizontal, OnOrientationChanged));

        /// <summary>
        /// Gets or sets the Orientation property. This dependency property 
        /// indicates the orientation of arrangement of items in the panel.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Orientation property.
        /// </summary>
        /// <param name="d">PagedPanel</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagedPanel panel = (PagedPanel)d;
            Orientation oldOrientation = (Orientation)e.OldValue;
            Orientation newOrientation = panel.Orientation;
            panel.OnOrientationChanged(oldOrientation, newOrientation);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Orientation property.
        /// </summary>
        /// <param name="oldOrientation">Old Value</param>
        /// <param name="newOrientation">New Value</param>
        protected virtual void OnOrientationChanged(Orientation oldOrientation, Orientation newOrientation)
        {
            InvalidateVisual();
        }

        #endregion

        #region (Page)
        
        /// <summary>
        /// Page attached property.
        /// </summary>
        public static readonly DependencyProperty PageProperty =
            DependencyProperty.RegisterAttached("Page", typeof(Int32), typeof(PagedPanel),
                new FrameworkPropertyMetadata(-1, OnPositionChanged));

        /// <summary>
        /// Reads the attached property Page from the given element.
        /// </summary>
        /// <param name="element">The element from which to read the Page attached property.</param>
        /// <returns>The property's value.</returns>
        public static Int32 GetPage(UIElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return (Int32) element.GetValue(PageProperty);
        }

        /// <summary>
        /// Writes the attached ptoperty Page to the given element.
        /// </summary>
        /// <param name="element">The element to which to write the Page attahced property.</param>
        /// <param name="page">The page index to set.</param>
        public static void SetPage(UIElement element, Int32 page)
        {
            if (element == null) throw new ArgumentNullException("element");
            element.SetValue(PageProperty, page);
        }

        /// <summary>
        /// Index attached property.
        /// </summary>
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.RegisterAttached("Index", typeof(Int32), typeof(PagedPanel),
                new FrameworkPropertyMetadata(-1, OnPositionChanged));

        /// <summary>
        /// Reads the attached property Index from the given element.
        /// </summary>
        /// <param name="element">The element from which to read the Index attached property.</param>
        /// <returns>The property's value.</returns>
        public static Int32 GetIndex(UIElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return (Int32)element.GetValue(IndexProperty);
        }

        /// <summary>
        /// Writes the attached ptoperty Index to the given element.
        /// </summary>
        /// <param name="element">The element to which to write the Index attahced property.</param>
        /// <param name="index">The index to set.</param>
        public static void SetIndex(UIElement element, Int32 index)
        {
            if (element == null) throw new ArgumentNullException("element");
            element.SetValue(IndexProperty, index);
        }

        static void OnPositionChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var panel = source as PagedPanel;
            if (panel != null) panel.InvalidateVisual();
        }

        #endregion

        #endregion

        #region Properties
 
        /// <summary>
        /// Gets the number of pages within this control
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int PageCount
        {
            get { return pageCount; }
        }

        #endregion

        #region Layout

        protected override Size MeasureOverride(Size availableSize)
        {
            // Child constraint
            var childSize = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

            // Calculate the desired size
            var size = Orientation == Orientation.Horizontal ? 
                new Size(PageWidth * pageCount, PageHeight) : 
                new Size(PageWidth, PageHeight * pageCount);
            
            
            // Measure elements
            foreach (UIElement child in InternalChildren)
            {
                if (child == null)
                    continue;
                child.Measure(childSize);
            }
            
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            bool needEasing = !initRequired;

            // If not there are uninitialized items, initialize them first
            if (initRequired && Children.Count > 0) {
                InitializeLayout();
                if (!DesignerProperties.GetIsInDesignMode(this))
                    initRequired = false;
            }

            // Update Layout
            UpdateFluidLayout(needEasing);

            // Return the panel size
            return Orientation == Orientation.Horizontal ? 
                new Size(PageWidth * pageCount, PageHeight) : 
                new Size(PageWidth, PageHeight * pageCount);
        }

        protected void InitializeLayout()
        {
            // This panel does not use any special initial locations
                // if an iOS-like fly-in effect is desired, position elements here
            foreach (UIElement child in InternalChildren)
            {
                // Ignore children not managed by the fluid behavior.
                if (child == null || GetPage(child) < 0 || GetIndex(child) < 0)
                    continue;

                // Calculate its position
                var cell = GetCellRect(GetPage(child), GetIndex(child));

                // Render it
                //child.RenderTransform = CreateTransform(cell.X, cell.Y, NormalScale, NormalScale);
                child.Arrange(cell);
            }
        }

        protected void UpdateFluidLayout(Boolean ease)
        {
            
        }

        #endregion

        #region Layout Helpers

        /// <summary>
        /// Gets the page rectangle.
        /// </summary>
        /// <param name="pageIndex">Index of the page to get.</param>
        /// <returns></returns>
        protected Rect GetPageRect(Int32 pageIndex)
        {
            if (Orientation == Orientation.Horizontal)
                return new Rect(pageIndex * PageWidth, 0, PageWidth, PageHeight);
            return new Rect(0, pageIndex * PageHeight, PageWidth, PageHeight);
        }

        /// <summary>
        /// Gets the working rectangle of the page, after considering
        /// page paddings.
        /// </summary>
        /// <returns></returns>
        protected Rect GetPageGridRect(Int32 pageIndex)
        {
            // Calculate Margins
            var availableWidth = PageWidth - PagePadding.Left - PagePadding.Right;
            var availableHeight = PageHeight - PagePadding.Top - PagePadding.Bottom;

            // Calculate the maximum grid size that fits the page
            pageGridSize = new Size(Math.Floor(availableWidth / CellWidth), 
                Math.Floor(availableHeight / CellHeight));


            // Center the grid rect in page rect
            var x = (availableWidth - (pageGridSize.Width * CellWidth)) / 2;
            var y = (availableHeight - (pageGridSize.Height * CellHeight)) / 2;

            // Get Page Rect
            var pageRect = GetPageRect(pageIndex);
            return new Rect(pageRect.X + PagePadding.Left + x, pageRect.Y + PagePadding.Right + y, 
                pageGridSize.Width * CellWidth, pageGridSize.Height * CellHeight);
        }

        /// <summary>
        /// Gets the rectangular bound of a specific cell on a specific page.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected Rect GetCellRect(Int32 page, Int32 index)
        {
            var gridRect = GetPageGridRect(page);

            var x = gridRect.X + index % pageGridSize.Width * CellWidth;
            var y = gridRect.Y + Math.Floor(index / pageGridSize.Width) * CellHeight;

            return new Rect(x, y, CellWidth, CellHeight);
        }

        /// <summary>
        /// Creates tranfrorm for a UI element.
        /// </summary>
        /// <param name="translateX">Translation offset along the X axis.</param>
        /// <param name="translateY">Translation offset along the Y axis.</param>
        /// <param name="scaleX">Scale along the X axis.</param>
        /// <param name="scaleY">Scale along the Y axis.</param>
        /// <param name="rotationAngle">Rotation angle.</param>
        /// <returns></returns>
        protected TransformGroup CreateTransform(double translateX, double translateY, double scaleX, double scaleY, double rotationAngle = 0)
        {
            var translate = new TranslateTransform() { X = translateX, Y = translateY };
            var scale = new ScaleTransform(scaleX, scaleY);
            var rotate = new RotateTransform(rotationAngle);

            var group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(rotate);
            group.Children.Add(translate);

            return group;
        }

        #endregion

        private void ClearItemsSource()
        {
            throw new NotImplementedException();
        }
    }
}
