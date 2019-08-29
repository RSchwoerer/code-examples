// Copyright (c) 2013 xmetropol.
// This code is distributed under the Microsoft Public License (Ms-PL).
// All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;

namespace xMetropol.UI.Extensions
{
  public static class FrameworkElementExtension
  {
  #region Static Fields

    public static readonly DependencyProperty ContextMenuPlacementTargetProperty = DependencyProperty.RegisterAttached
      ("ContextMenuPlacementTarget", typeof (FrameworkElement), typeof (FrameworkElementExtension),
         new PropertyMetadata(default(FrameworkElement), OnContextMenuPlacementTargetPropertyChanged));

  #endregion

  #region Methods

  public static FrameworkElement GetContextMenuPlacementTarget(UIElement element)
  {
    return (FrameworkElement) element.GetValue(ContextMenuPlacementTargetProperty);
  }

  public static void SetContextMenuPlacementTarget(UIElement element, FrameworkElement value)
  {
    element.SetValue(ContextMenuPlacementTargetProperty, value);
  }

  private static void OnContextMenuChanged(object sender, EventArgs eventArgs)
  {
    UpdateContextMenuPlacementTarget((FrameworkElement) sender);
  }

  private static void OnContextMenuPlacementTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
  {
    var frameworkElement = (FrameworkElement) d;
    var contextMenuPropertyDesc = DependencyPropertyDescriptor.FromProperty(FrameworkElement.ContextMenuProperty, frameworkElement.GetType());

    if (e.OldValue != null)
      contextMenuPropertyDesc.RemoveValueChanged(frameworkElement, OnContextMenuChanged);

    if (e.NewValue != null)
    {
      contextMenuPropertyDesc.AddValueChanged(frameworkElement, OnContextMenuChanged);
      UpdateContextMenuPlacementTarget(frameworkElement);
    }
  }

  private static void UpdateContextMenuPlacementTarget(FrameworkElement frameworkElement)
  {
    if (frameworkElement.ContextMenu != null)
      frameworkElement.ContextMenu.PlacementTarget = GetContextMenuPlacementTarget(frameworkElement);
  }

  #endregion
  }
}