// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
// Launcher.Panel/PanoramaPanel.cs
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CPanel = System.Windows.Controls.Panel;

namespace Launcher.Panel
{
    /// <summary>
    ///     Paged wrap panel with fluid drag-and-drop behavior.
    ///     Does not include scrolling functionality.
    /// </summary>
    /// <remarks>
    ///     PanoramaPanel is not designed for use as an ItemPanel.
    /// </remarks>
    [ContentProperty("Pages")]
    public partial class PanoramaPanel : CPanel
    {
        #region Nested Types

        /// <summary>
        /// PanoramaPanel Page Collection.
        /// Delegates adding/removing operations.
        /// </summary>
        public sealed class PageCollection : UIElementCollection
        {
            private readonly PanoramaPanel parent;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="panel">Parent panel.</param>
            public PageCollection(PanoramaPanel panel)
                : base(panel, panel)
            {
                parent = panel;
            }

            /// <summary>
            /// Gets the page at the specified index.
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public new PanoramaPanelPage this[Int32 index]
            {
                get { return (PanoramaPanelPage) base[index]; }
            }

            /// <summary>
            /// Adds a page to the collection.
            /// </summary>
            /// <param name="element"></param>
            /// <returns></returns>
            public override int Add(UIElement element)
            {
                PanoramaPanelPage page = element as PanoramaPanelPage;
                if (page == null)
                    throw new Exception("LauncherPanelPage expected!");

                //parent.AddPage(page);
                // Set up the page's parent reference
                page.Parent = parent;

                // Add children of the page to the parent
                foreach (UIElement c in page)
                    parent.Children.Add(c);

                // Disable transitions and refresh layout
                parent.disableAnimation = true;
                parent.InvalidateVisual();

                return base.Add(element);
            }

            /// <summary>
            /// Removes a page from the collection.
            /// </summary>
            /// <param name="element"></param>
            public override void Remove(UIElement element)
            {
                PanoramaPanelPage page = element as PanoramaPanelPage;
                if (page == null)
                    throw new Exception("LauncherPanelPage expected!");

                //parent.RemovePage(page);

                // Remove children from the parent
                foreach (UIElement child in page)
                    parent.Children.Remove(child);

                // Refresh parent layout
                parent.disableAnimation = true;
                parent.InvalidateVisual();

                base.Remove(element);
            }
        }

        #endregion

        protected Int32 rowSize;
        protected Int32 columnSize;
        protected PageCollection pages;
        protected Boolean disableAnimation;

        /// <summary>
        /// Constructor.
        /// </summary>
        public PanoramaPanel()
        {
            pages = new PageCollection(this);
        }

        #region Properties

        /// <summary>
        ///     Gets the number of pages in the PanoramaPanel.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Int32 PageCount
        {
            get { return pages.Count; }
        }

        /// <summary>
        ///     Gets the list of pages in the PanoramaPanel.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PageCollection Pages
        {
            get { return pages; }
        }

        #endregion

        protected override Size MeasureOverride(Size availableSize)
        {
            // There are no size constrains
            Size size = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

            // Measure children
            foreach (UIElement child in InternalChildren)
                child.Measure(size);

            // Calculate the desired size
            return Orientation == Orientation.Horizontal
                ? new Size(PageWidth * PageCount, PageHeight)
                : new Size(PageWidth, PageHeight * PageCount);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            bool needEasing = !disableAnimation;

            // If animation is disabled, reinitialize the layout
            if (disableAnimation && Children.Count > 0)
            {
                for (int p = 0; p < pages.Count; p++)
                {
                    PanoramaPanelPage page = pages[p];
                    for (int i = 0; i < page.Count; i++)
                    {
                        // Get the cell position
                        Rect cell = GetCellRect(p, i);

                        // Arrange the element at (0, 0)...
                        page[i].Arrange(new Rect(0, 0, CellWidth, CellHeight));

                        // ...then move it to the correct location
                        page[i].RenderTransform = CreateTransform(cell.X, cell.Y, DefaultScale, DefaultScale);
                    }
                }

                if (!DesignerProperties.GetIsInDesignMode(this))
                    disableAnimation = false;
            }

            // Update the layout
            UpdateFluidLayout(needEasing);

            // Return parent panel size
            return Orientation == Orientation.Horizontal
                ? new Size(PageWidth * PageCount, PageHeight)
                : new Size(PageWidth, PageHeight * PageCount);
        }

        protected void UpdateFluidLayout(Boolean ease)
        {
            if (disableAnimation)
                InvalidateVisual();
            else
            {
                for (int p = 0; p < pages.Count; p++)
                {
                    PanoramaPanelPage page = pages[p];
                    for (int i = 0; i < page.Count; i++)
                    {
                        Rect cell = GetCellRect(p, i);
                        Storyboard transition = CreateTransition(page[i], new Point(cell.X, cell.Y),
                            TimeSpan.FromMilliseconds(DefaultTransitionDuration), Easing);

                        transition.Completed += (s, e) =>
                        {
                            if (dragging != null)
                                dragging.SetValue(ZIndexProperty, DragZ);
                        };

                        transition.Begin();
                    }
                }
            }
        }

        #region Layout

        protected Rect GetPageRect(Int32 index)
        {
            return Orientation == Orientation.Horizontal
                ? new Rect(index * PageWidth, 0, PageWidth, PageHeight)
                : new Rect(0, index * PageHeight, PageWidth, PageHeight);
        }

        protected Rect GetGridRect(Int32 page)
        {
            // Get the available rect
            Rect pageRect = GetPageRect(page);

            rowSize = (Int32) Math.Floor(PageWidth / CellWidth);
            columnSize = (Int32) Math.Floor(PageHeight / CellHeight);
            if (rowSize <= 0)
                rowSize = 1;
            if (columnSize <= 0)
                columnSize = 1;

            double x = pageRect.X + (PageWidth - rowSize * CellWidth) / 2.0;
            double y = pageRect.Y + (PageHeight - columnSize * CellHeight) / 2.0;

            return new Rect(x, y, rowSize * CellWidth, columnSize * CellHeight);
        }

        protected Rect GetCellRect(Int32 page, Int32 index)
        {
            // Get the rectangle of the grid for the page
            Rect gridRect = GetGridRect(page);

            // Get x and y positions of the cell
            double x = gridRect.X + index % rowSize * CellWidth;
            double y = gridRect.Y + Math.Floor((Double) index / rowSize) * CellHeight;

            return new Rect(x, y, CellWidth, CellHeight);
        }


        protected Int32 GetPageIndex(Point point)
        {
            if (Orientation == Orientation.Horizontal)
                return (Int32) Math.Floor(point.X / PageWidth);
            return (Int32) Math.Floor(point.Y / PageHeight);
        }

        protected Int32 GetCellIndex(Point point)
        {
            // Get the page index
            int pageIndex = GetPageIndex(point);

            // Convert to point relative to the page grid
            Rect pageGrid = GetGridRect(pageIndex);
            point.X -= pageGrid.X;
            point.Y -= pageGrid.Y;

            return (Int32) (Math.Floor(point.X / CellWidth) + Math.Floor(point.Y / CellHeight) * rowSize);
        }

        #endregion

        #region Layout & Transitions

        /// <summary>
        ///     Creates tranfrorm for a UI element.
        /// </summary>
        /// <param name="translateX">Translation offset along the X axis.</param>
        /// <param name="translateY">Translation offset along the Y axis.</param>
        /// <param name="scaleX">Scale along the X axis.</param>
        /// <param name="scaleY">Scale along the Y axis.</param>
        /// <returns></returns>
        protected TransformGroup CreateTransform(double translateX, double translateY, double scaleX, double scaleY)
        {
            TranslateTransform translate = new TranslateTransform {X = translateX, Y = translateY};
            ScaleTransform scale = new ScaleTransform(scaleX, scaleY);

            TransformGroup group = new TransformGroup();
            group.Children.Add(scale);
            group.Children.Add(translate);

            return group;
        }

        /// <summary>
        ///     Creates the transition animation for an element.
        /// </summary>
        /// <param name="element">Child element to move.</param>
        /// <param name="position">New position of the child element.</param>
        /// <param name="duration">Duration of the animation.</param>
        /// <returns></returns>
        protected Storyboard CreateTransition(UIElement element, Point position, TimeSpan duration,
            EasingFunctionBase easing)
        {
            // Animate along X axis
            DoubleAnimation tx = new DoubleAnimation {To = position.X, Duration = duration};
            if (Easing != null)
                tx.EasingFunction = easing;
            Storyboard.SetTarget(tx, element);
            Storyboard.SetTargetProperty(tx,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.X)"));

            // Animate along Y axis
            DoubleAnimation ty = new DoubleAnimation {To = position.Y, Duration = duration};
            if (Easing != null)
                ty.EasingFunction = easing;
            Storyboard.SetTarget(ty, element);
            Storyboard.SetTargetProperty(ty,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(TranslateTransform.Y)"));

            // Animate X axis scale
            DoubleAnimation sx = new DoubleAnimation {To = 1.0D, Duration = duration};
            if (Easing != null)
                sx.EasingFunction = easing;
            Storyboard.SetTarget(sx, element);
            Storyboard.SetTargetProperty(sx,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

            // Animate Y axis scale
            DoubleAnimation sy = new DoubleAnimation {To = 1.0D, Duration = duration};
            if (Easing != null)
                sy.EasingFunction = easing;
            Storyboard.SetTarget(sy, element);
            Storyboard.SetTargetProperty(sy,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

            // Assemble animation
            Storyboard board = new Storyboard {Duration = duration};
            board.Children.Add(tx);
            board.Children.Add(ty);
            board.Children.Add(sx);
            board.Children.Add(sy);

            board.Completed += (s, e) =>
                element.SetValue(ZIndexProperty, DefaultZ);

            return board;
        }

        #endregion

        #region Drag & Drop

        private Int32 dragSourceCell;
        private Int32 dragSourcePage;
        private Point dragStart;
        private UIElement dragging;

        internal void OnDragStart(FrameworkElement child, Point origin, Point position)
        {
            if (child == null)
                return;

            Dispatcher.Invoke(() =>
            {
                child.Opacity = DragOpacity;
                child.SetValue(ZIndexProperty, DragZ);
                // Dragging point within the moving element
                dragStart = new Point(origin.X * DragScale, origin.Y * DragScale);
                // Apply transform without moving the element
                Point translatePosition = child.TranslatePoint(new Point(-child.Margin.Left, -child.Margin.Top), this);
                child.RenderTransform = CreateTransform(translatePosition.X, translatePosition.Y, DragScale, DragScale);
                // Capture further mouse events
                child.CaptureMouse();
                // Record the initial position of the element
                dragSourcePage = GetPageIndex(position);
                dragSourceCell = GetCellIndex(position);
                // 
                dragging = child;
            });
        }

        internal void OnDragMove(FrameworkElement child, Point origin, Point position)
        {
            if (child == null || dragging == null)
                return;

            Dispatcher.Invoke(() =>
            {
                // Set up render transform to move the element
                child.RenderTransform = CreateTransform(position.X - dragStart.X, position.Y - dragStart.Y, DragScale,
                    DragScale);

                // Get current position of the dragging operation
                int page = GetPageIndex(position);
                int cell = GetCellIndex(position);


                // Make sure the page is valid
                if (page >= 0 && page < pages.Count)
                {
                    // If the cell is invalid, make it the last cell in the list
                    Rect grid = GetGridRect(page);
                    if (!grid.Contains(position))
                        cell = (rowSize * columnSize) - 1;

                    // No need to update if the element is already there
                    if (pages[page][cell] != dragging)
                    {
                        // Remove the element from the drag source
                        pages[dragSourcePage].RemoveAt(dragSourceCell);

                        // Insert it into the current position
                        pages[page].Insert(cell, dragging);

                        // Set new drag source
                        dragSourcePage = page;
                        dragSourceCell = cell;

                        // Cascade elements from "overcrowded" pages
                        for (int i = 0; i < pages.Count; i++)
                        {
                            if (pages[i].Count > rowSize * columnSize)
                            {
                                // Add a new page if needed
                                if (i + 1 == pages.Count)
                                    pages.Add(new PanoramaPanelPage());

                                // Remove the last element from the page
                                UIElement cascade = pages[i][rowSize * columnSize];
                                pages[i].RemoveAt(rowSize * columnSize);
                                pages[i + 1].Insert(0, cascade);
                            }
                        }

                        // Update Layout
                        UpdateFluidLayout(true);
                    }
                }
            });
        }

        internal void OnDragEnd(FrameworkElement child, Point origin, Point position)
        {
            if (child == null)
                return;

            Dispatcher.Invoke(() =>
            {
                // Remove empty pages
                for (int i = pages.Count - 1; i >= 0; i--)
                {
                    if (pages[i].Count == 0)
                        pages.RemoveAt(i);
                }

                // Reset opacity
                child.Opacity = DefaultOpacity;
                child.SetValue(ZIndexProperty, TransitionZ);
                child.ReleaseMouseCapture();

                dragging = null;

                UpdateFluidLayout(true);
            });
        }

        #endregion
    }
}