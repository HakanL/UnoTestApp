using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMXCore.DMXCore100.Extensions;

public static class UiControlHelper
{
    public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);

        if (parent == null)
            return default;

        return parent is T t ? t : FindParent<T>(parent);
    }

    public static DependencyObject FindRoot(DependencyObject child)
    {
        var parent = VisualTreeHelper.GetParent(child);

        if (parent == null)
            return child;

        return FindRoot(parent);
    }

    public static T? FindChildInParentTree<T>(DependencyObject child) where T : DependencyObject
    {
        var foundChild = FindChild<T>(child);
        if (foundChild != null)
            return foundChild;

        // Check parents
        var parent = VisualTreeHelper.GetParent(child);

        if (parent == null)
            return default(T);

        return FindChildInParentTree<T>(parent);

    }

    public static bool IsVisible(DependencyObject obj)
    {
        DependencyObject current = obj;

        while (current is not null)
        {
            if (current is UIElement currentAsUIE)
            {
                if (currentAsUIE.Visibility != Visibility.Visible)
                {
                    return false;
                }
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return true;
    }

    /// <summary>
    /// Finds a Child of a given item in the visual tree. 
    /// </summary>
    /// <param name="parent">A direct parent of the queried item.</param>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="childName">x:Name or Name of child. </param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, 
    /// a null parent is being returned.</returns>
    public static T? FindChild<T>(DependencyObject parent, string? childName = null) where T : DependencyObject
    {
        // Confirm parent and childName are valid. 
        if (parent == null)
            return default;

        T foundChild = default;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            if (child is T childType)
            {
                if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // If the child's name is of the request name
                        foundChild = childType;
                        break;
                    }
                }
                else
                {
                    // Child element found
                    foundChild = childType;
                    break;
                }
            }
            else
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null)
                    break;
            }
        }

        return foundChild;
    }

    /// <summary>
    /// Finds a Child of a given item in the visual tree. 
    /// </summary>
    /// <param name="parent">A direct parent of the queried item.</param>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="predicate">Used to find a control</param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, 
    /// a null parent is being returned.</returns>
    public static T? FindChild<T>(DependencyObject parent, Func<DependencyObject, bool> predicate) where T : DependencyObject
    {
        // Confirm parent and childName are valid. 
        if (parent == null)
            return default;

        T foundChild = default;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            if (child is T childType)
            {
                if (predicate != null)
                {
                    if (predicate(child))
                    {
                        foundChild = childType;
                        break;
                    }
                }
            }
            else
            {
                // recursively drill down the tree
                foundChild = FindChild<T>(child, predicate);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null)
                    break;
            }
        }

        return foundChild;
    }

    /// <summary>
    /// Finds a Child of a given item in the visual tree. 
    /// </summary>
    /// <param name="parent">A direct parent of the queried item.</param>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="childName">x:Name or Name of child. </param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, 
    /// a null parent is being returned.</returns>
    public static IList<T> FindChildren<T>(DependencyObject parent, string? childName = null) where T : DependencyObject
    {
        // Confirm parent and childName are valid. 
        if (parent == null)
            return default;

        var list = new List<T>();

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            if (child is T childType)
            {
                if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        list.Add(childType);
                    }
                }
                else
                {
                    // child element found.
                    list.Add(childType);
                }
            }
            else
            {
                // recursively drill down the tree
                var foundChildren = FindChildren<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child.
                list.AddRange(foundChildren);
                if (foundChildren.Any())
                    break;
            }
        }

        return list;
    }

    /// <summary>
    /// Finds a Child of a given item in the visual tree. 
    /// </summary>
    /// <param name="parent">A direct parent of the queried item.</param>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="childName">x:Name or Name of child. </param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, 
    /// a null parent is being returned.</returns>
    public static IList<T> FindAllChildren<T>(DependencyObject parent, string? childName = null) where T : DependencyObject
    {
        // Confirm parent and childName are valid. 
        if (parent == null)
            return default;

        var list = new List<T>();

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            if (child is T childType)
            {
                if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        list.Add(childType);
                    }
                }
                else
                {
                    // child element found.
                    list.Add(childType);
                }
            }
            else
            {
                // recursively drill down the tree
                var foundChildren = FindAllChildren<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child.
                list.AddRange(foundChildren);
            }
        }

        return list;
    }
}
