<Command("Begin ReProcess"), _
Description("Starts timing reprocess."), _
Category("Production Reports Functions")> _
Public Class BR : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    [On]
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode
  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub
  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged

  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode
      .BS.Cancel()

    End With
    State = EState.On
    Return True

  End Function

  Function Run() As Boolean Implements ACCommand.Run

  End Function

  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
  End Sub
#Region "state and state string"
  Property State As EState
  Property StateString As String

#End Region

#Region "Public Properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return (State <> EState.Off)
    End Get
  End Property


#End Region





End Class
