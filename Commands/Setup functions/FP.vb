<Command("Flow percent:", "|0-100|%", " ", "", "", CommandType.BatchParameter),
Description("Sets pump speed or flow percent."),
Category("Setup functions")>
Public Class FP : Inherits MarshalByRefObject : Implements ACCommand

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
    If State = EState.On Then
      With ControlCode

        If param(1) > 0 Then
          PSPercent = param(1) * 10
        Else
          PSPercent = .Parameters_PumpSpeedDefault
        End If

      End With
    End If
  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode

      If param(1) > 0 Then
        PSPercent = param(1) * 10
      Else
        PSPercent = .Parameters_PumpSpeedDefault
      End If

      State = EState.On
      Return True
    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
  End Function

  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
    PSPercent = 0
  End Sub


  Property State As EState


#Region "Properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return State <> EState.Off
    End Get
  End Property
#End Region

#Region "Variable properties"
  Property PSPercent As Integer

#End Region

End Class
