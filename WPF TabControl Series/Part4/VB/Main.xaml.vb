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
      Dim win As Window = New TabControl_2_CloseButton()
      win.Show()
   End Sub

End Class
