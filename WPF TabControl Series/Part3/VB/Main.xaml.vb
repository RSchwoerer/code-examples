Imports System.Windows

Partial Public Class Main
   Inherits Window
   Public Sub New()
      InitializeComponent()
   End Sub

   Private Sub cmd1_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim win As Window = New TabControl_1_Start()
      win.Show()
   End Sub

   Private Sub cmd2_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim win As Window = New TabControl_2_ScrollViewer_Simple()
      win.Show()
   End Sub

   Private Sub cmd3_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim win As Window = New TabControl_3_ScrollViewer_Advanced()
      win.Show()
   End Sub

   Private Sub cmd4_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim win As Window = New TabControl_4_ScrollViewer_Menu()
      win.Show()
   End Sub

   Private Sub cmd5_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
      Dim win As Window = New TabControl_5_ScrollableTabPanel()
      win.Show()
   End Sub
End Class
