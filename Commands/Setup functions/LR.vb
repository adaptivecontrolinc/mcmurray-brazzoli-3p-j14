<Command("Liquor Ratio", "|0-15|:1", " ", "", "", CommandType.BatchParameter),
Description("Sets the liquor ratio. The liquor ratio is used in conjunction with the Batch Weight (BW command) to calculate the working volume."),
Category("Setup functions")>
Public Class LR : Inherits MarshalByRefObject : Implements ACCommand

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
      .LiquorRatio = param(1) * 10
      .WorkingLevel = 0
      .WorkingVolume = (.BatchWeight * .LiquorRatio * 3) \ 250
      .WorkingVolume = MinMax(.WorkingVolume, .FI.Parameters_FillVolumeMinimum, .FI.Parameters_FillVolumeMaximum)


    End With
  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode
      .LiquorRatio = param(1) * 10
      .WorkingLevel = 0
      .WorkingVolume = (.BatchWeight * .LiquorRatio * 3) \ 250
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
