<Command("Reel Speed", "|0-99||0-9|YPM", " ", "", "", CommandType.BatchParameter),
Description("Sets reel speed in yards per minute."),
Category("Setup functions")>
Public Class RS : Inherits MarshalByRefObject : Implements ACCommand

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
    With ControlCode
      .DS.Cancel()
      .WS.Cancel()
      If .Parameters_ReelSpeedMaximumYPM > 0 Then
        RSSpeed = param(1) * 10 + param(2)
      Else
        RSSpeed = .Parameters_ReelSpeedDefault
      End If
    End With
  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode
      .DS.Cancel()
      .WS.Cancel()
      If .Parameters_ReelSpeedMaximumYPM > 0 Then
        RSSpeed = param(1) * 10 + param(2)
      Else
        RSSpeed = .Parameters_ReelSpeedDefault
      End If
    End With
    State = EState.On
    Return True
  End Function

  Function Run() As Boolean Implements ACCommand.Run
  End Function

  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
  End Sub

#Region "Public Properties"
  Property State As EState


  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return State <> EState.Off
    End Get
  End Property
#End Region
#Region "Variable properties"

  Property RSSpeed As Integer


#End Region
End Class
