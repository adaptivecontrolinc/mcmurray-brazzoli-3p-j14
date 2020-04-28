<Command("Add dose ", "|0-99|m C|0-9|", " ", "", "'StandardADTime + '1 = 1 + '1"),
Description("Doses the contents of the add tank over the time specified, using one of ten curves. Curve 0 is linear, odd numbers are progressive adds and even numbers are regressive adds."),
Category("Add Tank Functions")>
Public Class AD : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    WaitReady
    Settle
    Pause
    Dose
    DosePause
    Transfer
    TransferEmpty1
    Rinse
    TransferEmpty2
    Finished
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
      'this is a foreground only function.
      'need the cancels.
      'cancel foreground functions.
      'add tank
      .AT.Cancel()
      'machine
      .DR.Cancel() : .PD.Cancel()
      .FI.Cancel()
      .TM.Cancel()
      .PR.Cancel()
      .RI.Cancel() : .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .SA.Cancel() : .UL.Cancel()

      If param.GetUpperBound(0) >= 1 Then AddTime = param(1) * 60
      If param.GetUpperBound(0) >= 2 Then AddCurve = param(2)

      State = EState.WaitReady

    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode
      If IsOn Then
        DesiredLevel = Setpoint()
      Else
        DesiredLevel = 0

      End If

      Select Case State

        Case EState.Off
          StateString = ""

        Case EState.Interlock
          If .MachineSafe Then
            State = EState.WaitReady
          End If
          StateString = "AV:Machine not safe to transfer."
          If Not .TempSafe Then
            StateString = "AV:Temperature too high to transfer."
          End If
          If Not .PressSafe Then
            StateString = "AV:Pressure too high to transfer."
          End If

        Case EState.WaitReady
          'state string 

          'we are filling and the add tank is the right one.
          If .AF.IsOn Then
            StateString = .AF.StateString
          ElseIf Not .AP.IOPrepareReady Then
            StateString = .AP.StateString
          Else
            StateString = "AV:Settling level. " & Timer.ToString
          End If

          'check to see if its ready
          If (Not .AP.IsOn OrElse .AP.IOPrepareReady) AndAlso Not .AF.IsOn Then
            .RC.Cancel()
            Timer.Seconds = 10
            State = EState.Settle
          End If

        Case EState.Settle
          StateString = "AV:Settling level. " & Timer.ToString

          'lets the level settle
          If Timer.Finished AndAlso AddTime > 0 Then
            State = EState.DosePause
            Timer.Seconds = AddTime
            StartLevel = .AddLevel

          End If

          If Timer.Finished AndAlso AddTime = 0 Then
            State = EState.TransferEmpty1
            Timer.Seconds = .Parameters_AddTransferTimeBeforeRinse
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
            State = EState.DosePause
            Timer.Restart()
          End If

        Case EState.Dose

          StateString = "AV:Dosing:" & Timer.ToString
          'Check level - if level low switch to circulate
          If .AddLevel < Setpoint() AndAlso DoseOnTimer.Finished Then State = EState.DosePause
          'Check level - if level high then switch to transfer
          If .AddLevel > (Setpoint() + 30) Then State = EState.Transfer


          'If pump not running go to pause state
          If Not .IO.PumpRunning OrElse Not .IO.PumpInAutoSw OrElse Not .IO.ReelForwardSwitch OrElse Not .IO.Reel1Running OrElse Not .IO.Reel2Running _
                OrElse Not .IO.Reel3Running OrElse .Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn Then
            Timer.Pause()
            State = EState.Pause
          End If

          'If we're finished transfer empty
          If Timer.Finished OrElse .AddLevel <= 10 Then
            State = EState.TransferEmpty1
            Timer.Seconds = .Parameters_AddTransferTimeBeforeRinse
          End If

        Case EState.Transfer
          StateString = "AV:Dosing:" & Timer.ToString
          'Check level - if level low switch to circulate
          If .AddLevel < Setpoint() AndAlso DoseOnTimer.Finished Then State = EState.DosePause

          'If pump not running go to pause state
          If Not .IO.PumpRunning OrElse Not .IO.PumpInAutoSw OrElse Not .IO.ReelForwardSwitch OrElse Not .IO.Reel1Running OrElse Not .IO.Reel2Running _
                OrElse Not .IO.Reel3Running OrElse .Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn Then
            Timer.Pause()
            State = EState.Pause
          End If

          'If we're finished transfer empty
          If Timer.Finished OrElse .AddLevel <= 10 Then
            State = EState.TransferEmpty1
            Timer.Seconds = .Parameters_AddTransferTimeBeforeRinse
          End If

        Case EState.DosePause

          StateString = "AV:Dosing paused:" & Timer.ToString
          'Check level - if high switch to dose
          If .AddLevel > Setpoint() Then State = EState.Dose
          DoseOnTimer.Seconds = .Parameters_DosingMinOpenTime

          'If pump not running go to pause state
          If Not .IO.PumpRunning OrElse Not .IO.PumpInAutoSw OrElse Not .IO.ReelForwardSwitch OrElse Not .IO.Reel1Running OrElse Not .IO.Reel2Running _
                OrElse Not .IO.Reel3Running OrElse .Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn Then
            Timer.Pause()
            State = EState.Pause
          End If

          'If we're finished transfer empty
          If Timer.Finished OrElse .AddLevel <= 10 Then
            State = EState.TransferEmpty1
            Timer.Seconds = .Parameters_AddTransferTimeBeforeRinse
          End If

        Case EState.TransferEmpty1

          StateString = "AV:First transfer:" & Timer.ToString

          If .AddLevel > 10 Then Timer.Seconds = .Parameters_AddTransferTimeBeforeRinse

          If Timer.Finished Then
            Timer.Seconds = .Parameters_AddRinseTime
            State = EState.Rinse
          End If

        Case EState.Rinse

          StateString = "AV:Rinse:" & Timer.ToString

          If Timer.Finished Then
            Timer.Seconds = .Parameters_AddTransferTimeAfterRinse
            State = EState.TransferEmpty2
          End If

        Case EState.TransferEmpty2

          StateString = "AV:Second transfer:" & Timer.ToString

          If .AddLevel > 10 Then Timer.Seconds = .Parameters_AddTransferTimeAfterRinse

          If Timer.Finished Then
            State = EState.Finished
          End If

        Case EState.Finished
          .AP.Cancel()
          .AddReady = False
          State = EState.Off

      End Select
    End With

  End Function

  Sub Cancel() Implements ACCommand.Cancel
    With ControlCode
      'incase the tank was on a transfer and we jump clear all of its lists.
      If State <> EState.Off Then
        .AddReady = False
        State = EState.Off
      End If
    End With

  End Sub

#Region "State and state string"
  Property State As EState
  Property StateString As String
#End Region

#Region " Public Properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      IsOn = (State <> EState.Off)
    End Get
  End Property
  Public ReadOnly Property IsDelayed() As Boolean
    Get
      Return IsWaitReady
    End Get
  End Property
  Public ReadOnly Property IsWaitReady As Boolean
    Get
      Return State = EState.WaitReady
    End Get
  End Property
  Public ReadOnly Property IsActive As Boolean
    Get
      Return State >= EState.Settle
    End Get
  End Property


#End Region

#Region "Variable properties"
  Property AddTime As Integer
  Property AddCurve As Integer
  Property StartLevel As Integer
  <GraphTrace(1, 1000, 0, 3333, "Blue", "%t%")> Property DesiredLevel As Integer



#End Region

#Region "timers"
  Property Timer As New Timer
  Property DoseOnTimer As New Timer

#End Region

#Region "I/O Propertiers"
  Public ReadOnly Property IsAddPump As Boolean
    Get
      Return State = EState.Dose OrElse State = EState.DosePause OrElse State = EState.Transfer OrElse
      State = EState.TransferEmpty1 OrElse State = EState.Rinse OrElse State = EState.TransferEmpty2
    End Get
  End Property

  Public ReadOnly Property IsRinse As Boolean
    Get
      Return State = EState.Rinse
    End Get
  End Property

  Public ReadOnly Property IsTransfer As Boolean
    Get
      Return State = EState.Dose OrElse State = EState.Transfer OrElse State = EState.TransferEmpty1 OrElse State = EState.Rinse OrElse State = EState.TransferEmpty2
    End Get
  End Property

#End Region

#Region "Method"
  Private Function Setpoint() As Integer
    'If timer has finished exit function
    If Timer.Finished Then
      Setpoint = 0
      Exit Function
    End If

    'Amount we should have transferred so far
    Dim ElapsedTime As Double, DesiredLevel As Double
    ElapsedTime = (AddTime - Timer.Seconds) / AddTime
    DesiredLevel = StartLevel

    'y=0x2-1x+1 linear
    If AddCurve = 0 AndAlso ElapsedTime > 0 Then
      DesiredLevel = StartLevel * ((-1 * ElapsedTime) + 1)
    End If

    'Calculate scaling factor (0-1) for progressive and digressive curves
    If AddCurve > 0 AndAlso ElapsedTime > 0 Then
      If AddCurve = 1 Then
        'y = -0.4x^2 -.6x + 1
        DesiredLevel = StartLevel * (-0.4 * (ElapsedTime * ElapsedTime) - 0.6 * (ElapsedTime) + 1) 'at 60% of the desired level at 50% time transfers 40% of the tank
      ElseIf AddCurve = 3 Then
        'y = -0.8x^2 - .2x + 1
        DesiredLevel = StartLevel * (-0.8 * (ElapsedTime * ElapsedTime) + -0.2 * (ElapsedTime) + 1) 'at 70% of the desired level at 50% time transfers 30% of the tank
      ElseIf AddCurve = 5 Then
        ' y = -1x^2 + 0x + 1
        DesiredLevel = StartLevel * (-1 * (ElapsedTime * ElapsedTime) + 1)  'at 75% of the desired level at 50% time transfers 25% of the tank
      ElseIf AddCurve = 7 Then
        'y = -1.4x^2 + .4x + 1
        DesiredLevel = StartLevel * (-1.4 * (ElapsedTime * ElapsedTime) + 0.4 * (ElapsedTime) + 1) 'at 85% of the desired level at 50% time transfers 15% of the tank
      ElseIf AddCurve = 9 Then
        ' y = -1.6x^2 + .6x + 1
        DesiredLevel = StartLevel * (-1.6 * (ElapsedTime * ElapsedTime) + 0.6 * (ElapsedTime) + 1) 'at 90% of the desired level at 50% time transfers 10% of the tank
      ElseIf AddCurve = 2 Then
        'y=.4X2 -1.4x + 1
        DesiredLevel = StartLevel * (0.4 * (ElapsedTime * ElapsedTime) - 1.4 * (ElapsedTime) + 1) 'at 40% of the desired level at 50% time transfers 60% of the tank
      ElseIf AddCurve = 4 Then
        'y=.8X2-1.8x + 1      
        DesiredLevel = StartLevel * (0.8 * (ElapsedTime * ElapsedTime) - 1.8 * (ElapsedTime) + 1) 'at 30% of the desired level at 50% time transfers 70% of the tank
      ElseIf AddCurve = 6 Then
        'y=.1X2- 2x + 1
        DesiredLevel = StartLevel * (1 * (ElapsedTime * ElapsedTime) - 2 * (ElapsedTime) + 1) 'at 25% of the desired level at 50% time transfers 75% of the tank
      ElseIf AddCurve = 8 Then
        'y=.1.4X2 -2.4x + 1
        DesiredLevel = StartLevel * (1.4 * (ElapsedTime * ElapsedTime) - 2.4 * (ElapsedTime) + 1) 'at 15% of the desired level at 50% time transfers 85% of the tank
      End If

    End If

    'Calculate setpoint and limit to 0-1000
    Setpoint = CInt(DesiredLevel)
    If Setpoint < 0 Then Setpoint = 0
    If Setpoint > 1000 Then Setpoint = 1000

    'Global variable for display purposes - yuk!!
    DesiredLevel = Setpoint
  End Function
#End Region


End Class
