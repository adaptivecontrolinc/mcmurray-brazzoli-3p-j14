<Command("Vessel Level", "|0-99|%"),
Description("Sets working level."),
Category("Setup functions")>
Public Class VL : Inherits MarshalByRefObject : Implements ACCommand

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
      .WorkingVolume = 0
      .WorkingLevel = (param(1) * 10)
      .WorkingLevel = MinMax(.WorkingLevel, .FI.Parameters_FillLevelMinimum, .FI.Parameters_FillLevelMaximum)
      State = EState.Off
    End With
  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode
      .WorkingVolume = 0
      .WorkingLevel = (param(1) * 10)
      .WorkingLevel = MinMax(.WorkingLevel, .FI.Parameters_FillLevelMinimum, .FI.Parameters_FillLevelMaximum)
      State = EState.Off
    End With
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

End Class
