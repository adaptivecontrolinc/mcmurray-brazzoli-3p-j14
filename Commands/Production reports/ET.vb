<Command("End Timing"),
Description("Used in conjunction with Bo, BS and BR commands to stop timing approvals, samples or reprocessing."),
Category("Production Reports Functions")>
Public Class ET : Inherits MarshalByRefObject : Implements ACCommand
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
      .BO.Cancel() : .BS.Cancel() : .BR.Cancel()
    End With

    State = EState.Off
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
