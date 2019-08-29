// Copyright (c) 2013 xmetropol.
// This code is distributed under the Microsoft Public License (Ms-PL).
// All rights reserved.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using xMetropol.UI.Panels;
using xMetropol.Util;

namespace FlexStackPanelApp
{
  public partial class MainWindow
  {
    public static readonly RoutedCommand CloseDocument = new RoutedCommand();
    public static readonly RoutedCommand ActivateDocument = new RoutedCommand();
    private readonly ObservableCollection<string> documentItems;

    public MainWindow()
    {
      InitializeComponent();

      documentItems = new ObservableCollection<string>();

      for (var i = 0; i < 6; i++)
        AddDocument();

      DocumentTabControl.ItemsSource = documentItems;

      CommandBindings.Add(new CommandBinding(CloseDocument, (o, e) => documentItems.Remove((string)DocumentTabControl.ItemContainerGenerator.ItemFromContainer((TabItem)e.Parameter))));
      CommandBindings.Add(new CommandBinding(ActivateDocument, (o, e) =>
      {
        DocumentTabControl.SelectedItem = e.Parameter;
        OverflowTabHeaderObserver.EnsureActiveTabVisible(DocumentTabControl);
      }));
    }

    private void OnNewTabButtonClick(object sender, RoutedEventArgs e)
    {
      AddDocument();
    }

    private void AddDocument()
    {
      documentItems.Add(Enumerable.Range(1, 10000).Select(i => string.Concat("Document ", i)).First(d => documentItems.Contains(d) == false));
    }
  }

  static class OverflowTabHeaderObserver
  {
    public static readonly DependencyProperty EnableTrackingProperty = DependencyProperty.RegisterAttached
      ("EnableTracking", typeof (bool), typeof (OverflowTabHeaderObserver), 
        new PropertyMetadata(false, OnEnableTrackingPropertyChanged));

    private static readonly DependencyPropertyDescriptor isOverflowedDesc = DependencyPropertyDescriptor.FromProperty
      (FlexStackPanel.IsOverflowedProperty, typeof(TabItem));

    private static void OnEnableTrackingPropertyChanged(DependencyObject depObj, 
                                                        DependencyPropertyChangedEventArgs args)
    {
      var tabItem = (TabItem) depObj;

      if ((bool)args.OldValue)
        isOverflowedDesc.RemoveValueChanged(tabItem, OnTabItemOverflowChanged);

      if ((bool)args.NewValue)
        isOverflowedDesc.AddValueChanged(tabItem, OnTabItemOverflowChanged);
    }

    private static void OnTabItemOverflowChanged(object sender, EventArgs e)
    {
      EnsureActiveTabVisible(((TabItem)sender).VisualAncestors().OfType<TabControl>().First());
    }

    public static void EnsureActiveTabVisible(TabControl tabControl)
    {
      if (tabControl.ItemsSource == null)
        return;

      var ilist = (IList)tabControl.ItemsSource;

      var containerGenerator = tabControl.ItemContainerGenerator;
      var tabHeader = (TabItem)containerGenerator.ContainerFromItem(tabControl.SelectedItem);

      if (!FlexStackPanel.GetIsOverflowed(tabHeader) || !tabHeader.IsSelected) return;

      var item = containerGenerator.ItemFromContainer(tabHeader);
      ilist.Remove(item);
      ilist.Insert(0, item);

      tabControl.SelectedIndex = 0;
      UpdateFirstItem(tabControl);

      tabControl.InvalidateMeasure();
    }

    private static void UpdateFirstItem(TabControl tabControl)
    {
      var ilist = (IList) tabControl.ItemsSource;

      if (ilist.Count == 0)
        return;

      var containerGenerator = tabControl.ItemContainerGenerator;

      var tabItems = ilist.OfType<object>()
                          .Select(containerGenerator.ContainerFromItem)
                          .OfType<TabItem>()
                          .ToList();
      foreach (var t in tabItems)
        FlexStackPanel.SetShrinkOnOverflow(t, false);

      FlexStackPanel.SetShrinkOnOverflow(tabItems.First(), true);
    }

    public static void SetEnableTracking(UIElement element, bool value)
    {
      element.SetValue(EnableTrackingProperty, value);
    }

    public static bool GetEnableTracking(UIElement element)
    {
      return (bool) element.GetValue(EnableTrackingProperty);
    }
  }

  static class ControlEx
  {
    public static readonly DependencyProperty IconProperty =
      DependencyProperty.RegisterAttached("Icon", typeof (ImageSource), typeof (ControlEx), new PropertyMetadata(default(ImageSource)));

    public static void SetIcon(UIElement element, ImageSource value)
    {
      element.SetValue(IconProperty, value);
    }

    public static ImageSource GetIcon(UIElement element)
    {
      return (ImageSource) element.GetValue(IconProperty);
    }
  }
}