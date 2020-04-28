<Command("Pressurize", "Off/On: |0-1|", "", "", ""), _
Description("Pressurize the machine (1) or depressurize (0)."), _
Category("Machine Functions")> _
Public Class PR : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Active
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode
  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub
  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged

  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    If param(1) = 0 Then Cancel()
    If param(1) = 1 Then
      State = EState.Active
      Return True
    End If
  End Function
  Function Run() As Boolean Implements ACCommand.Run
  End Function
  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
  End Sub
#Region "State properties"
  Property State As EState

  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      IsOn = State <> EState.Off
    End Get
  End Property
#End Region
End Class
