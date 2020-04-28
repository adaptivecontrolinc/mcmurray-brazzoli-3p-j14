<Command("Batch Weight", "Weight: |0-9999| lbs", " ", "", "", CommandType.BatchParameter), _
Description("Sets batch weight. The batch weight is used in conjunction with the Liquor Ratio (LR command) to calculate the working volume."), _
Category("Setup functions")> _
Public Class BW : Inherits MarshalByRefObject : Implements ACCommand

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

      .BatchWeight = param(1)
      .WorkingLevel = 0
      .WorkingVolume = (.BatchWeight * .LiquorRatio * 3) \ 250
      .WorkingVolume = MinMax(.WorkingVolume, .FI.Parameters_FillVolumeMinimum, .FI.Parameters_FillVolumeMaximum)

      State = EState.Off
    End With
  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode

      .BatchWeight = param(1)
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

#Region "state"
  Property State As EState
#End Region

#Region "Public Properties"

  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return State <> EState.Off
    End Get
  End Property
#End Region

End Class
