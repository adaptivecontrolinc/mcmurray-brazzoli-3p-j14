<Command("Vessel Volume", "|0-9999|0Gals"),
Description("Sets working volume."),
Category("Setup functions")>
Public Class VV : Inherits MarshalByRefObject : Implements ACCommand

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

      .WorkingLevel = 0
      .WorkingVolume = (param(1) * 10)
      .WorkingVolume = MinMax(.WorkingVolume, .FI.Parameters_FillVolumeMinimum, .FI.Parameters_FillVolumeMaximum)
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
