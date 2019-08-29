Partial Public Class TabControl_2_CloseButton

   Public Sub New()
      InitializeComponent()
      'For the sample, the Window's DataContext is its code-behind.
      Me.DataContext = Me
   End Sub

#Region " --- CloseCommand --- "

   Private _cmdCloseCommand As Utils.RelayCommand
   ''' <summary>
   ''' Returns a command that closes a TabItem.
   ''' </summary>
   Public ReadOnly Property CloseCommand() As ICommand
      Get
         If _cmdCloseCommand Is Nothing Then
            _cmdCloseCommand = _
               New Utils.RelayCommand( _
                     AddressOf Me.CloseTab_Execute, _
                     AddressOf Me.CloseTab_CanExecute _
                  )
         End If
         Return _cmdCloseCommand
      End Get
   End Property

   ''' <summary>
   ''' Called when the command is to be executed.
   ''' </summary>
   ''' <param name="parm">
   ''' The TabItem in which the Close-button was clicked.
   ''' </param>
   Private Sub CloseTab_Execute(ByVal parm As Object)
      Dim ti As TabItem = TryCast(parm, TabItem)
      If ti IsNot Nothing Then
         tc.Items.Remove(ti)
      End If
   End Sub

   ''' <summary>
   ''' Called when the availability of the Close command needs to be determined.
   ''' </summary>
   ''' <param name="parm">
   ''' The TabItem for which to determine the availability of the Close-command.
   ''' </param>
   Private Function CloseTab_CanExecute(ByVal parm As Object) As Boolean
      'For the sample, the closing of TabItems will only be
      'unavailable for disabled TabItems and the very first TabItem.
      Dim ti As TabItem = TryCast(parm, TabItem)
      If ti IsNot Nothing AndAlso ti IsNot tc.Items(0) Then
         'We have a valid reference to a TabItem, so return 
         'true if the TabItem is enabled.
         Return ti.IsEnabled
      End If

      'If no reference to a TabItem could be obtained, the command 
      'cannot be executed
      Return False
   End Function

#End Region

End Class
