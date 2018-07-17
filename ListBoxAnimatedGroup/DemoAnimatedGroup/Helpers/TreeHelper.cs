using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace DemoAnimatedGroup.Helpers
{
    /// <summary>
    /// Helper methods to go through the logical and the visual tree
    /// </summary>
    public class TreeHelper
    {
        /// <summary>
        /// Determiner whether an element is the logical ancestor of another item
        /// </summary>
        /// <param name="parent">Parent element</param>
        /// <param name="source">Child element</param>
        /// <returns>True if the Child element has the Parent element as its parent in the logical tree</returns>
        public static bool IsLogicalAncestorOf(FrameworkElement parent, FrameworkElement source)
        {
            if (parent == source)
            {
                return true;
            }

            FrameworkElement current = source;
            while (current != null && current.Parent != null)
            {
                if (current.Parent == parent)
                {
                    return true;
                }

                current = current.Parent as FrameworkElement;
            }

            return false;
        }

        /// <summary>
        /// Returns the first ancester of specified type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
        {
            current = VisualTreeHelper.GetParent(current);

            while (current != null)
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };
            return null;
        }

        /// <summary>
        /// Returns a specific ancester of an object
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, T lookupItem)
        where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T && current == lookupItem)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };
            return null;
        }

        /// <summary>
        /// Finds an ancestor object by name and type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, string parentName)
        where T : DependencyObject
        {
            while (current != null)
            {
                if (!string.IsNullOrEmpty(parentName))
                {
                    var frameworkElement = current as FrameworkElement;
                    if (current is T && frameworkElement != null && frameworkElement.Name == parentName)
                    {
                        return (T)current;
                    }
                }
                else if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };

            return null;

        }

        /// <summary>
        /// Looks for a child control within a parent by name
        /// </summary>
        public static T FindChild<T>(DependencyObject parent, string childName)
        where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                    else
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<T>(child, childName);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null) break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Looks for a child control within a parent by type
        /// </summary>
        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }

        /// <summary>
        /// Find all visual children of a given type
        /// </summary>
        /// <typeparam name="T">Type of the children to find</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <returns>A list of T objects that are visual children of the source dependency object</returns>
        public static List<T> FindVisualChildren<T>(DependencyObject obj) where T : DependencyObject
        {
            List<T> matches = new List<T>();
            return FindVisualChildren(obj, matches);
        }

        /// <summary>
        /// Find the first visual child of a given type
        /// </summary>
        /// <typeparam name="T">Type of the visual child to retrieve</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <returns>First child that matches the given type</returns>
        public static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfchild = FindVisualChild<T>(child);
                    if (childOfchild != null)
                    {
                        return childOfchild;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find the first visual ancestor of a given type
        /// </summary>
        /// <typeparam name="T">Type of the ancestor</typeparam>
        /// <param name="element">Source visual</param>
        /// <returns>First ancestor that matches the type in the visual tree</returns>
        public static T FindVisualAncestor<T>(UIElement element) where T : class
        {
            while (element != null && !(element is T))
            {
                element = (UIElement)VisualTreeHelper.GetParent(element);
            }

            return element as T;
        }

        /// <summary>
        /// Find all visual children of a given type
        /// </summary>
        /// <typeparam name="T">Type of the children to find</typeparam>
        /// <param name="obj">Source dependency object</param>
        /// <param name="matches">List of matches</param>
        /// <returns>A list of T objects that are visual children of the source dependency object</returns>
        private static List<T> FindVisualChildren<T>(DependencyObject obj, List<T> matches) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject children = VisualTreeHelper.GetChild(obj, i);
                if (children != null && children is T)
                {
                    matches.Add((T)children);
                }
                FindVisualChildren(children, matches);
            }

            return matches;
        }
    }
}