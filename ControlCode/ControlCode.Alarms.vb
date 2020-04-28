Partial Class ControlCode
  'temp alarms
  Public Alarms_HeaderRTDFaulty As Boolean
  Public Alarms_BackupRTDFaulty As Boolean
  Public Alarms_BlendFillRTDFaulty As Boolean
  Public Alarms_TemperatureTooHigh As Boolean
  Public Alarms_TemperatureRelayFault As Boolean

  'motor tripped alarms.
  Public Alarms_PumpTripped As Boolean

  Public Alarms_Reel1Tripped As Boolean
  Public Alarms_Reel2Tripped As Boolean
  Public Alarms_Reel3Tripped As Boolean
  Public Alarms_AddPumpTripped As Boolean
  Public Alarms_AddMixerTripped As Boolean

  'filling 
  Public Alarms_FillFlowmeterFault As Boolean

  'pump
  Public Alarms_LevelTooLowForPump As Boolean

  'plc alarms
  Public Alarms_Plc1OutputsFault As Boolean
  Public Alarms_Plc2InputsFault As Boolean

  'estop alarms
  Public Alarms_EmergencyStop As Boolean


  Public Alarms_ControlPowerFailure As Boolean

  Public Alarms_Reel1Tangled As Boolean
  Public Alarms_Reel2Tangled As Boolean
  Public Alarms_Reel3Tangled As Boolean


#Region "Setup Alarms"
  Private Sub CheckAlarms()



    'Only begin making alarms 5 seconds after start-up
    Static PowerUpTimer As Timer
    If PowerUpTimer Is Nothing Then PowerUpTimer = New Timer : PowerUpTimer.Seconds = 5
    If Not PowerUpTimer.Finished Then Exit Sub



    'Temperature probe errors
    Alarms_HeaderRTDFaulty = (IO.HeaderTemp < 330) OrElse (IO.HeaderTemp > 3000)
    Alarms_BackupRTDFaulty = (IO.BackupTemp < 330) OrElse (IO.BackupTemp > 3000)
    Alarms_BlendFillRTDFaulty = (IO.BlendFillTemp < 330) OrElse (IO.BlendFillTemp > 3000)
    'temp alarms
    Alarms_TemperatureTooHigh = IO.BackupTemp > 2900 OrElse IO.HeaderTemp > 2900 OrElse Not IO.TempBelow280F
    Alarms_TemperatureRelayFault = (IO.TempBelow190F AndAlso Not IO.TempBelow280F) OrElse
                                  (IO.TempBelow190F AndAlso IO.BackupTemp > 1950)


    'Main pump tripped
    Static PumpAlarmTimer As New Timer
    If (Not IO.MainPumpStart) OrElse IO.PumpRunning Then PumpAlarmTimer.Seconds = 5
    Alarms_PumpTripped = PumpAlarmTimer.Finished

    'reel 1 tripped
    Static Reel1AlarmTimer As New Timer
    If (Not IO.Reel1Forward AndAlso Not IO.Reel1Reverse) OrElse IO.Reel1Running Then Reel1AlarmTimer.Seconds = 5
    Alarms_Reel1Tripped = Reel1AlarmTimer.Finished

    'reel 2 tripped
    Static Reel2AlarmTimer As New Timer
    If (Not IO.Reel2Forward AndAlso Not IO.Reel2Reverse) OrElse IO.Reel2Running Then Reel2AlarmTimer.Seconds = 5
    Alarms_Reel2Tripped = Reel2AlarmTimer.Finished

    'reel 3 tripped
    Static Reel3AlarmTimer As New Timer
    If (Not IO.Reel3Forward AndAlso Not IO.Reel3Reverse) OrElse IO.Reel3Running Then Reel3AlarmTimer.Seconds = 5
    Alarms_Reel3Tripped = Reel3AlarmTimer.Finished

    'add pump tripped
    Static AddPumpAlarmTimer As New Timer
    If (Not IO.AddPumpStart) OrElse IO.AddPumpRunning Then AddPumpAlarmTimer.Seconds = 5
    Alarms_AddPumpTripped = AddPumpAlarmTimer.Finished

    'add pump tripped
    Static AddmixerAlarmTimer As New Timer
    If (Not IO.AddMixerStart) OrElse IO.AddMixerRunning Then AddmixerAlarmTimer.Seconds = 5
    Alarms_AddMixerTripped = AddmixerAlarmTimer.Finished

    'Power problems?
    Alarms_EmergencyStop = IO.EmergencyStop

    Alarms_FillFlowmeterFault = FlowmeterAlarmTimer.Finished

    'pump level
    Alarms_LevelTooLowForPump = (VesLevel < Parameters_PumpMinimumLevel) AndAlso Not PD.IsOn AndAlso PumpAndReel.IsPumpOn

    'plc's not talking
    Alarms_Plc1OutputsFault = IO.PLC1Timer.Finished AndAlso Parent.Mode <> Mode.Debug
    Alarms_Plc2InputsFault = IO.PLC2Timer.Finished AndAlso Parent.Mode <> Mode.Debug


    Alarms_ControlPowerFailure = Not IO.PowerResetPB


  End Sub
#End Region
End Class
