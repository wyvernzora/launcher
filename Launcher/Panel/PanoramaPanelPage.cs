// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-
// Launcher.Panel/LauncherPanelPage.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using CPanel = System.Windows.Controls.Panel;

namespace Launcher.Panel
{
    /// <summary>
    ///     Defines a page in the LauncherPanel.
    ///     Serves as the container of elements.
    /// </summary>
    /// <remarks>
    ///     Even though this is technically a UIElement, layout of the children
    ///     is handled entirely by the parent panel.
    /// </remarks>
    [ContentProperty("Children")]
    public class PanoramaPanelPage : UIElement, IList<UIElement>
    {
        protected PanoramaPanel panel;
        protected List<UIElement> children;

        /// <summary>
        ///     Constructor.
        /// </summary>
        public PanoramaPanelPage()
        {
            children = new List<UIElement>();
        }

        #region Properties

        /// <summary>
        ///     Gets the ordered list of children in the page.
        /// </summary>
        public List<UIElement> Children
        {
            get { return children; }
        }

        /// <summary>
        /// Gets or sets the parent Panorama panel.
        /// </summary>
        public PanoramaPanel Parent
        {
            get { return panel; }
            set { panel = value; }
        }

        #endregion

        #region Interface Implementations

        #region ICollection<UIElement> Members

        /// <summary>
        ///     Adds an item to the page.
        /// </summary>
        /// <param name="item"></param>
        public void Add(UIElement item)
        {
            children.Add(item);
            if (panel != null)
                panel.Children.Add(item);
        }

        /// <summary>
        ///     Removes all the items from the page.
        /// </summary>
        public void Clear()
        {
            if (panel != null)
                foreach (var child in children)
                    Parent.Children.Remove(child);
            children.Clear();
        }

        /// <summary>
        ///     Returns a value indicating whether the page contains an element.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(UIElement item)
        {
            return children.Contains(item);
        }

        /// <summary>
        ///     Copies elements of the page to an array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(UIElement[] array, int arrayIndex)
        {
            children.CopyTo(array, arrayIndex);
        }

        /// <summary>
        ///     Returns the number of elements on the page.
        /// </summary>
        public int Count
        {
            get { return children.Count; }
        }

        /// <summary>
        ///     Returns a value indicating whether the page is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        ///     Removes a specified element from the page.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(UIElement item)
        {
            if (panel != null)
                panel.Children.Remove(item);
            return children.Remove(item);
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator<UIElement> GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return children.GetEnumerator();
        }

        #endregion

        #region IList<UIElement> Members

        public int IndexOf(UIElement item)
        {
            return children.IndexOf(item);
        }

        public void Insert(int index, UIElement item)
        {
            if (index < Count)
                children.Insert(index, item);
            else
                children.Add(item);
        }

        public void RemoveAt(int index)
        {
            if (index < children.Count)
                children.RemoveAt(index);
            else
                children.RemoveAt(children.Count - 1);
        }

        public UIElement this[int index]
        {
            get
            {
                return index < children.Count ? children[index] : children[children.Count - 1];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #endregion
    }
}