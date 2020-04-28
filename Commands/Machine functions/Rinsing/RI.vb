<Command("Rinse ", "|0-180|F for |0-99|mins", "", "", "'2"),
  Description("Rinses with water with the specified time for the specified time. The machine level is maintained at the parameter level."),
  Category("Machine Functions")>
Public Class RI : Inherits MarshalByRefObject : Implements ACCommand

#Region "Enumeration"
  Public Enum EState
    Off
    InterLock
    NotSafe
    ResetMeter
    Pause
    Rinse
    Fill
    Drain
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
      If param.GetUpperBound(0) >= 1 Then RinseTemperature = param(1) * 10
      If param.GetUpperBound(0) >= 2 Then RinseTime = param(2)
      ' this Is a foreground only function.
      'cancel foreground functions.
      'add tank
      .AD.Cancel() : .AT.Cancel() : .RC.Cancel()
      'machine
      .DR.Cancel() : .PD.Cancel()
      .FI.Cancel()
      .TM.Cancel()
      .PR.Cancel()
      .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .SA.Cancel() : .UL.Cancel()

      'temp control
      .CO.Cancel() : .HE.Cancel() : .TP.Cancel() : .WT.Cancel()
      .TemperatureControl.Cancel()

      If .ManualFillRequest Then .ManualFillRequest = False
      If .ManualDrainRequest Then .ManualDrainRequest = False

      ' Set blend control parameters
      BlendControl.Parameters(.Parameters_BlendDeadBand, .Parameters_BlendFactor, .Parameters_BlendSettleTime, .Parameters_ColdWaterTemperature, .Parameters_HotWaterTemperature)

      OverrunTimer.Minutes = RinseTime


    End With
    State = EState.InterLock
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode
      If (State > EState.ResetMeter) AndAlso (Not .TempSafe) Then State = EState.NotSafe
      Select Case State

        Case EState.Off
          StateString = ""

        Case EState.InterLock


          If .MachineSafe Then
            .TP.Cancel()
            .HE.Cancel()
            .CO.Cancel()
            .TemperatureControl.Cancel()           ' Cancel Temperature control

            State = EState.ResetMeter
          End If
          StateString = "RI: Machine not safe to rinse."
          If Not .TempSafe Then
            StateString = "RI: Temperature too high to rinse."
          End If

        Case EState.NotSafe
          Timer.Pause()
          If .MachineSafe Then State = EState.Pause
          StateString = "RI: Machine not safe to rinse."
          If Not .TempSafe Then
            StateString = "RI: Temperature too high to rinse."
          End If

        Case EState.ResetMeter
          StateString = "RI: Resetting flowmeter."
          'Wait for flowmeter counter to reset to zero
          If .VesVolume = 0 AndAlso .FlowmeterWater.Gallons = 0 Then
            BlendControl.Start(RinseTemperature)
            State = EState.Fill
            Timer.Minutes = RinseTime
          End If

        Case EState.Pause
          If Not .IO.PumpRunning Then
            StateString = "Transfer Paused: Main pump not running. " & Timer.ToString
          ElseIf Not .IO.PumpInAutoSw Then
            StateString = "Transfer Paused: Turn on Main pump. " & Timer.ToString
          ElseIf .IO.EmergencyStop Then
            StateString = "Transfer Paused: Emergency stop pushed" & Timer.ToString
          ElseIf Not .IO.Reel1Running Then
            StateString = "Transfer Paused:Reel 1 not running" & Timer.ToString
          ElseIf Not .IO.Reel2Running Then
            StateString = "Transfer Paused:Reel 2 not running" & Timer.ToString
          ElseIf Not .IO.Reel3Running Then
            StateString = "Transfer Paused:Reel 2 not running" & Timer.ToString
          ElseIf Not .IO.ReelForwardSwitch Then
            StateString = "Transfer Paused:Reels not enabled" & Timer.ToString
          ElseIf .Parent.IsPaused Then
            StateString = "Transfer Program paused" & Timer.ToString
          ElseIf .TemperatureControl.IsCrashCoolOn Then
            StateString = "Transfer Paused:Crash cooling " & Timer.ToString
          Else
            StateString = "Transfer Paused: " & Timer.ToString
          End If

          If .IO.PumpRunning AndAlso .IO.PumpInAutoSw AndAlso .IO.ReelForwardSwitch AndAlso .IO.Reel1Running AndAlso .IO.Reel2Running AndAlso
            .IO.Reel3Running AndAlso (Not (.Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn)) Then
            State = EState.Rinse
            Timer.Restart()
          End If


        Case EState.Rinse
          StateString = "RI:Rinsing for " & Timer.ToString
          BlendControl.Run(.IO.BlendFillTemp)


          'Check level and switch state if necessary
          If .VesLevel < (Parameters_RinseHighLevel - 20) Then State = EState.Fill
          If .VesLevel > (Parameters_RinseHighLevel + 50) Then State = EState.Drain

          'If pump not running go to pause state
          If Not .IO.PumpRunning OrElse Not .IO.PumpInAutoSw OrElse Not .IO.ReelForwardSwitch OrElse Not .IO.Reel1Running OrElse Not .IO.Reel2Running _
                OrElse Not .IO.Reel3Running OrElse .Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn Then
            Timer.Pause()
            State = EState.Pause
          End If

          If Timer.Finished Then
            .FillingCompleted = True
            State = EState.Off
          End If

        Case EState.Fill
          StateString = "RI:Filling for " & Timer.ToString
          BlendControl.Run(.IO.BlendFillTemp)

          'Check level and switch state if necessary
          If .VesLevel >= Parameters_RinseHighLevel Then State = EState.Rinse

          'If pump not running go to pause state
          If Not .IO.PumpRunning OrElse Not .IO.PumpInAutoSw OrElse Not .IO.ReelForwardSwitch OrElse Not .IO.Reel1Running OrElse Not .IO.Reel2Running _
                OrElse Not .IO.Reel3Running OrElse .Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn Then
            Timer.Pause()
            State = EState.Pause
          End If

          If Timer.Finished Then
            .FillingCompleted = True
            State = EState.Off
          End If

        Case EState.Drain
          StateString = "RI:Draining for " & Timer.ToString

          'Check level and switch state if necessary
          If .VesLevel <= Parameters_RinseLowLevel Then State = EState.Fill
          'If pump and reel are not running pause rinse

          'If pump not running go to pause state
          If Not .IO.PumpRunning OrElse Not .IO.PumpInAutoSw OrElse Not .IO.ReelForwardSwitch OrElse Not .IO.Reel1Running OrElse Not .IO.Reel2Running _
                OrElse Not .IO.Reel3Running OrElse .Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn Then
            Timer.Pause()
            State = EState.Pause
          End If

          If Timer.Finished Then
            .FillingCompleted = True
            State = EState.Off
          End If
      End Select

    End With
  End Function
  Sub Cancel() Implements ACCommand.Cancel
    With ControlCode
      If IsOn Then .FillingCompleted = True
      State = EState.Off
    End With
  End Sub

#Region "state and state string"
  Property State As EState
  Property StateWas As EState
  Property StateString As String
#End Region

#Region "Public Properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      IsOn = State <> EState.Off
    End Get
  End Property

  ReadOnly Property IsPaused As Boolean
    Get
      Return State = EState.Pause
    End Get
  End Property
  ReadOnly Property IsOverrun As Boolean
    Get
      Return IsRinsing AndAlso OverrunTimer.Finished
    End Get
  End Property
#End Region

#Region "Timers "
  Property Timer As New Timer
  Property OverrunTimer As New Timer

#End Region

#Region "Variables properties"
  Property RinseTime As Integer
  Property RinseTemperature As Integer
  Property BlendControl As New BlendControl

#End Region


#Region "I/O properties"


  ReadOnly Property IsResetMeter As Boolean
    Get
      Return State = EState.ResetMeter
    End Get
  End Property

  ReadOnly Property IsRinsing As Boolean
    Get
      Return State = EState.Rinse OrElse State = EState.Fill OrElse State = EState.Rinse
    End Get
  End Property
  ReadOnly Property IsFilling As Boolean
    Get
      Return State = EState.Fill OrElse State = EState.Rinse
    End Get
  End Property

  Public ReadOnly Property IsDraining As Boolean
    Get
      Return State = EState.Drain OrElse State = EState.Rinse
    End Get
  End Property

  ReadOnly Property IOBlendOutput As Integer
    Get
      Return BlendControl.IOOutput

    End Get
  End Property
#End Region

#Region "Parameters"
  <Parameter(0, 1000), Category("Rinse control"), _
    Description("The high level to fill the machine to during a rinse. In tenths of a percent.")> _
  Public Parameters_RinseHighLevel As Integer
  <Parameter(0, 1000), Category("Rinse control"), _
   Description("The low level to drain the machine to during a rinse. In tenths of a percent.")> _
  Public Parameters_RinseLowLevel As Integer
  <Parameter(0, 1000), Category("Rinse control"),
   Description("The output of the fill valve during a rinse. In tenths of a percent.")>
  Public Parameters_RinseFillOutput As Integer
#End Region


End Class


