<Command("Wait Temp", "", "", "", ""), _
Description("Wait at this step until desired temp is reached."), _
Category("Temperature Functions")> _
Public Class WT : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Wait
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode
  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub
  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
  End Sub
  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged

  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode
      'this Is a foreground only function.
      'cancel foreground functions.
      'add tank
      .AD.Cancel() : .AT.Cancel()
      'machine
      .DR.Cancel() : .PD.Cancel()
      .FI.Cancel()
      .TM.Cancel()
      .RI.Cancel() : .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .SA.Cancel() : .UL.Cancel()


      State = EState.Wait
      If (Not (.TP.IsOn Or .HE.IsOn Or .CO.IsOn)) Then Cancel()
    End With
  End Function

  Public Function Run() As Boolean Implements ACCommand.Run
    With ControlCode
      Select Case State

        Case EState.Off
          StateString = ""

        Case EState.Wait
          StateString = "Waiting for " & (.TemperatureControl.TempFinalTemp \ 10).ToString.PadLeft(3) & "F"
          'If we don't have a final temp ..
          If .TemperatureControl.TempFinalTemp = 0 Then Cancel()
          If .TemperatureControl.IsHolding Then Cancel()

      End Select
    End With
  End Function

#Region "Public Properties"
  Property State As EState
  Property StateString As String

  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return (State <> EState.Off)
    End Get
  End Property
#End Region

End Class
