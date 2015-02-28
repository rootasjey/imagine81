using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Imagine.Common
{
    public class Helpers
    {
        /// <summary>
        /// Find a child that is in a DataTemplate
        /// based on its type
        /// </summary>
        /// <typeparam name="childItem">child type (TextBlock, ProgressRing,...)</typeparam>
        /// <param name="obj">Parent</param>
        /// <returns>Found child</returns>
        public static childItem FindVisualChild<childItem>(DependencyObject obj)
                where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        /// <summary>
        /// Find a child in a DataTemplate
        /// based on its type and a particulary condition
        /// </summary>
        /// <param name="targetElement"></param>
        public static void SearchVisualTree(DependencyObject targetElement)
        {
            var count = VisualTreeHelper.GetChildrenCount(targetElement);
            if (count == 0)
                return;

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(targetElement, i);
                if (child is TextBlock)
                {
                    TextBlock targetItem = (TextBlock)child;

                    if (targetItem.Text == "Item2")
                    {
                        //targetItem.Foreground = new SolidColorBrush(Colors.Green);
                        return;
                    }
                }
                else
                {
                    SearchVisualTree(child);
                }
            }
        }
    }
}
