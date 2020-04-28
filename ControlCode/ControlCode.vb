'valdes gaston

Public Class ControlCode : Inherits MarshalByRefObject
  Implements ACControlCode
  Public Parent As ACParent, IO As IO
  Public ComputerName As String

  Public Sub New(ByVal parent As ACParent)
    Me.Parent = parent
    
    IO = New IO(Me)  ' create IO and initialize com ports from settings
    ProgramStoppedTimer.Start()



  End Sub

  Public Sub Run() Implements ACControlCode.Run
    Dim i As Integer = 0

    'get current time
    CurrentTime = Date.Now.ToString

    'Set program state change timers and determine Time-In-Step variables
    CheckProgramStateChanges()

    'Generate fast and slow flash - these will be used in a few places
    Static FastFlasher As New Flasher : Dim FastFlash As Boolean
    Static SlowFlasher As New Flasher : Dim SlowFlash As Boolean
    FastFlasher.Flash(FastFlash, 400)
    SlowFlasher.Flash(SlowFlash, 800)

    'Some useful status flags for valve control
    Dim EmergencyStop As Boolean = IO.EmergencyStop
    Dim NStop As Boolean = Not EmergencyStop
    Dim Halt As Boolean = Parent.IsPaused OrElse IO.EmergencyStop
    Dim NHalt As Boolean = Not Halt
    Dim NHSafe As Boolean = NHalt And MachineSafe

    'Use Temperature probe with highest value (for safety) unless Cooling is active
    VesTemp = IO.HeaderTemp
    If (IO.BackupTemp > IO.HeaderTemp) Then VesTemp = IO.BackupTemp

    'Is temperature reading valid ?
    TempSafe = (VesTemp < 1980) AndAlso IO.TempBelow190F AndAlso IO.TempBelow280F
    TempValid = (VesTemp > 320) AndAlso (VesTemp < 3000) AndAlso IO.TempBelow280F AndAlso
              IO.MainPumpStart AndAlso IO.PumpRunning

    'Temperature Control - set enable timer
    If Halt OrElse (Not TempValid) OrElse (Not IO.PumpRunning) OrElse (VesTemp > 2850) OrElse Not IO.TempBelow280F Then
      TemperatureControl.ResetEnableTimer()
    End If

    'Temperature Control - Set control parameters
    TemperatureControl.EnableDelay = 10
    TemperatureControl.Run(VesTemp)
    TemperatureControl.CheckErrorsAndMakeAlarms(VesTemp)

    'Setpoints
    SetpointF = TemperatureControl.TempSetpoint
    TempFinalValue = (SetpointF \ 10).ToString.PadLeft(3) & "F"

    'Is'Vent
    Vent = SafetyControl.IOVent

    'pressure safe ?
    PressSafe = Vent AndAlso IO.PressureSafeSw

    ' Run the safety control to handle pressurisation
    SafetyControl.Run(VesTemp, TempSafe, PressSafe, PR.IsOn)

    'Is Machine 
    MachineSafe = SafetyControl.IsDepressurized

    'FLOWMETER SECTION =====================================================================
    'flowmeter counters
    FlowmeterWater.GallonsPerCount = Parameters_WaterGallonsPerPulse
    If IO.FlowMeterPulse AndAlso Not flowmeterpulsewas Then
      FlowmeterWater.Counter += 1 'add one to the count.
    End If
    FlowMeterPulseWas = IO.FlowMeterPulse

    'Flowmeter   volume
    Dim CounterReset As Boolean, CounterCheck As Boolean
    CounterReset = FI.IsResetMeter OrElse RI.IsResetMeter OrElse RP.IsResetMeter
    CounterCheck = FI.IsFilling OrElse FI.IsFinished OrElse RI.IsFilling OrElse RP.IsFilling OrElse ManualFillRequest


    If CounterCheck Then
      WaterUsedTemp = CType(FlowmeterWater.Gallons, Integer)
      VesVolume = CType(FlowmeterWater.Gallons, Integer)
    Else
      If FillingCompleted Then
        WaterUsed += WaterUsedTemp
        FillingCompleted = False
      End If
    End If

    'Reset flowmeter alarm
    Static watercounterwas As Double
    If (FlowmeterWater.Gallons > watercounterwas) OrElse Not CounterCheck OrElse Not (FI.IsPaused OrElse RI.IsPaused _
       OrElse RP.IsPaused) Then
      FlowmeterAlarmTimer.Seconds = Parameters_FillFlowmeterAlarmTime
    End If

    'Reset fill flowmeter leading edge flag
    watercounterwas = FlowmeterWater.Gallons

    'reset tank volumes
    If CounterReset Then
      FlowmeterWater.Reset()
      VesVolume = 0
    End If

    'FLOWMETER SECTION  End =====================================================================

    'calibrate analog inputs =====================================================================
    'first analog input. used for temperature control. basicly we use a pressure feedback instead of a ip to determine the amount the 
    'porportional heating/cooling vavle is open.

    'Calibrate vessel level reading
    VesLevel = IO.VesselLevelInput
    Dim VLevelRange As Integer, VLevelOffset As Integer
    VLevelRange = Math.Abs(Parameters_VesselLevelTransMax - Parameters_VesselLevelTransMin)
    VLevelOffset = Math.Abs(IO.VesselLevelInput - Parameters_VesselLevelTransMin)
    If VLevelRange > 0 Then VesLevel = (VLevelOffset * 1000) \ VLevelRange
    VesLevel = MinMax(VesLevel, 0, 1000)

    'Local Add tank 1
    AddLevel = IO.AddLevelInput
    Dim AddLevelRange As Integer, AddLevelOffset As Integer
    AddLevelRange = Parameters_AddTankLevelTransMax - Parameters_AddTankLevelTransMin
    AddLevelOffset = IO.AddLevelInput - Parameters_AddTankLevelTransMin
    If AddLevelRange > 0 Then AddLevel = (AddLevelOffset * 1000) \ AddLevelRange
    AddLevel = MinMax(AddLevel, 0, 1000)

    'manual pump speed pot
    'reel speed feed back
    ManualPumpSpeed = IO.ManualPumpSpeedInput
    Dim ManualPumpSpeedRange As Integer, ManualPumpSpeedOffset As Integer
    ManualPumpSpeedRange = Parameters_ManualPumpSpeedMax - Parameters_ManualPumpSpeedMin
    ManualPumpSpeedOffset = IO.ManualPumpSpeedInput - Parameters_ManualPumpSpeedMin
    If ManualPumpSpeedRange > 0 Then ManualPumpSpeed = (ManualPumpSpeedOffset * 1000) \ ManualPumpSpeedRange
    ManualPumpSpeed = MinMax(ManualPumpSpeed, 0, 1000)

    'manual reel speed pot
    ManualReelSpeedYPM = IO.ManualReelSpeedInput
    Dim ManualReelSpeedYPMRange As Integer, ManualReelSpeedYPMOffset As Integer
    ManualReelSpeedYPMRange = Parameters_ManualReelSpeedYPMMax - Parameters_ManualReelSpeedYPMMin
    ManualReelSpeedYPMOffset = IO.ManualReelSpeedInput - Parameters_ManualReelSpeedYPMMin
    If ManualReelSpeedYPMRange > 0 Then ManualReelSpeedYPM = (ManualReelSpeedYPMOffset * Parameters_ReelSpeedMaximumYPM) \ ManualReelSpeedYPMRange
    ManualReelSpeedYPM = MinMax(ManualReelSpeedYPM, 0, Parameters_ReelSpeedMaximumYPM)

    'reel speed feed back
    Reel1SpeedFeedback = IO.Reel1SpeedFeedbackInput
    Dim Reel1SpeedFeedbackRange As Integer, Reel1SpeedFeedbackOffset As Integer
    Reel1SpeedFeedbackRange = Parameters_Reel1SpeedFeedbackMax - Parameters_Reel1SpeedFeedbackMin
    Reel1SpeedFeedbackOffset = IO.Reel1SpeedFeedbackInput - Parameters_Reel1SpeedFeedbackMin
    If Reel1SpeedFeedbackRange > 0 Then Reel1SpeedFeedback = (Reel1SpeedFeedbackOffset * Parameters_ReelSpeedMaximumYPM) \ Reel1SpeedFeedbackRange
    Reel1SpeedFeedback = MinMax(Reel1SpeedFeedback, 0, Parameters_ReelSpeedMaximumYPM)

    Reel2SpeedFeedback = IO.Reel2SpeedFeedbackInput
    Dim Reel2SpeedFeedbackRange As Integer, Reel2SpeedFeedbackOffset As Integer
    Reel2SpeedFeedbackRange = Parameters_Reel2SpeedFeedbackMax - Parameters_Reel2SpeedFeedbackMin
    Reel2SpeedFeedbackOffset = IO.Reel2SpeedFeedbackInput - Parameters_Reel2SpeedFeedbackMin
    If Reel2SpeedFeedbackRange > 0 Then Reel2SpeedFeedback = (Reel2SpeedFeedbackOffset * Parameters_ReelSpeedMaximumYPM) \ Reel2SpeedFeedbackRange
    Reel2SpeedFeedback = MinMax(Reel2SpeedFeedback, 0, Parameters_ReelSpeedMaximumYPM)

    Reel3SpeedFeedback = IO.Reel3SpeedFeedbackInput
    Dim Reel3SpeedFeedbackRange As Integer, Reel3SpeedFeedbackOffset As Integer
    Reel3SpeedFeedbackRange = Parameters_Reel3SpeedFeedbackMax - Parameters_Reel3SpeedFeedbackMin
    Reel3SpeedFeedbackOffset = IO.Reel3SpeedFeedbackInput - Parameters_Reel3SpeedFeedbackMin
    If Reel3SpeedFeedbackRange > 0 Then Reel3SpeedFeedback = (Reel3SpeedFeedbackOffset * Parameters_ReelSpeedMaximumYPM) \ Reel3SpeedFeedbackRange
    Reel3SpeedFeedback = MinMax(Reel3SpeedFeedback, 0, Parameters_ReelSpeedMaximumYPM)


    'calibrate analog inputs end =====================================================================

    'Toggle AddReady flag if add ready pushbutton is pressed 
    Static PreviousAddTankReadyPB As Boolean
    If IO.AddReadyPb AndAlso Not PreviousAddTankReadyPB Then AddReady = Not AddReady
    PreviousAddTankReadyPB = IO.AddReadyPb

    'pump control reel control =====================================================================
    'Run Pump and reel control and set Parameter delay times
    'if you have the stop input from the slide bar turn off the reel
    'if you have the slide bar and reel forward go forward
    'if you have the slide bar and reel reverse go reverse
    'if you have the stop input from the slide bar turn off the pump

    PumpAndReel.Run()
    PumpAndReel.PumpEnableDelay = Parameters_PumpMinimumLevelTime
    PumpAndReel.PumpOffDelay = Parameters_PumpOffDelayTime
    PumpAndReel.ReelOnDelay = Parameters_ReelOnDelayTime

    'Reset pump and reel delay timers if necessary
    'if pump not told to start and pump not running or we are getting the input to stop the reel
    If Not IO.PumpRunning Then PumpAndReel.ResetReelOnDelayTimer()

    If VesLevel < Parameters_PumpMinimumLevel Then PumpAndReel.ResetPumpEnableTimer()

    'pump start and stop
    If IO.PumpInAutoSw AndAlso Not PumpInAutoSwitchWas AndAlso Not IO.PumpRunning AndAlso Not IO.EmergencyStop AndAlso IO.PowerResetPB AndAlso FirstScanDone AndAlso IO.IOScanDone AndAlso PowerOnTimer.Finished Then
      PumpAndReel.RequestPump()
    End If

    If Not IO.PumpInAutoSw AndAlso IO.PumpRunning Then
      PumpAndReel.AutoStop()
    End If

    PumpInAutoSwitchWas = IO.PumpInAutoSw

    'Lifter reel. we control reel direction.

    'turn off reel in forward if stopped.
    If IO.ReelForwardSwitch AndAlso Not ReelForwardSwWas AndAlso Not IO.EmergencyStop AndAlso IO.PowerResetPB AndAlso PowerOnTimer.Finished Then
      PumpAndReel.StartReelForward(1)
      PumpAndReel.StartReelForward(2)
      PumpAndReel.StartReelForward(3)
    End If

    If IO.ReelReverseSwitch AndAlso Not ReelReverseSwWas AndAlso Not IO.EmergencyStop AndAlso IO.PowerResetPB AndAlso Not IO.PumpRunning AndAlso PowerOnTimer.Finished Then
      PumpAndReel.StartReelReverse(1)
      PumpAndReel.StartReelReverse(2)
      PumpAndReel.StartReelReverse(3)
    End If

    If Not IO.ReelForwardSwitch AndAlso Not IO.ReelReverseSwitch AndAlso (IO.Reel1Running OrElse IO.Reel2Running OrElse IO.Reel3Running) Then
      PumpAndReel.StopReel(1)
      PumpAndReel.StopReel(2)
      PumpAndReel.StopReel(3)
    End If

    ReelForwardSwWas = IO.ReelForwardSwitch
    ReelReverseSwWas = IO.ReelReverseSwitch


    'Check to see if we should be looking for a tangle on reel 1 (rope 1 )
    Dim Port1CheckTangle As Boolean
    Port1CheckTangle = (Not LD.IsOn) AndAlso (Not SA.IsOn) AndAlso (Not UL.IsOn) AndAlso PumpAndReel.IsPumpOn AndAlso PumpAndReel.IsReel1Forward
    If Not Port1CheckTangle Then Port1TangleDelayTimer.Seconds = Parameters_TangleDelayTime

    Dim Port2CheckTangle As Boolean
    Port2CheckTangle = (Not LD.IsOn) AndAlso (Not SA.IsOn) AndAlso (Not UL.IsOn) AndAlso PumpAndReel.IsPumpOn AndAlso PumpAndReel.IsReel2Forward
    If Not Port2CheckTangle Then Port2TangleDelayTimer.Seconds = Parameters_TangleDelayTime

    Dim Port3CheckTangle As Boolean
    Port3CheckTangle = (Not LD.IsOn) AndAlso (Not SA.IsOn) AndAlso (Not UL.IsOn) AndAlso PumpAndReel.IsPumpOn AndAlso PumpAndReel.IsReel3Forward
    If Not Port3CheckTangle Then Port3TangleDelayTimer.Seconds = Parameters_TangleDelayTime

    'If we have a tangle remember the rope that tangled and stop the pump and reels
    If FirstScanDone Then
      If Port1TangleDelayTimer.Finished AndAlso Not Reel1Tangled Then
        If Not IO.Tangle1Switch Then
          Reel1Tangled = True
          PumpAndReel.StopReel(1)
        End If
      End If

      If Port2TangleDelayTimer.Finished AndAlso Not Reel2Tangled Then
        If Not IO.Tangle2Switch Then
          Reel2Tangled = True
          PumpAndReel.StopReel(2)
        End If
      End If

      If Port3TangleDelayTimer.Finished AndAlso Not Reel3Tangled Then
        If Not IO.Tangle3Switch Then
          Reel3Tangled = True
          PumpAndReel.StopReel(3)
        End If
      End If
    End If

    If Reel1Tangled OrElse Reel2Tangled OrElse Reel3Tangled Then
      If (Not IO.ReelForwardSwitch AndAlso Not IO.ReelReverseSwitch) Then

        Reel1Tangled = False
        Reel2Tangled = False
        Reel3Tangled = False
        If Alarms_Reel1Tangled Then Alarms_Reel1Tangled = False
        If Alarms_Reel2Tangled Then Alarms_Reel2Tangled = False
        If Alarms_Reel3Tangled Then Alarms_Reel3tangled = False
      End If
    End If

    'pump control reel control END =====================================================================

    'Add buttons
    'add fill
    If IO.AddFillSw AndAlso Not addFillSwitchWas AndAlso AddLevel < 950 AndAlso Not IO.EmergencyStop Then
      AddFillRequest = True
    End If

    If AddFillRequest AndAlso (Not IO.AddFillSw OrElse AddLevel >= 950 OrElse IO.EmergencyStop) Then
      AddFillRequest = False
    End If
    addFillSwitchWas = IO.AddFillSw

    'add runback 
    If IO.AddRunbackSw AndAlso Not addRunbackSwitchWas AndAlso AddLevel < 950 AndAlso Not IO.EmergencyStop AndAlso MachineSafe Then
      AddRunbackRequest = True
    End If

    If AddRunbackRequest AndAlso (Not IO.AddRunbackSw OrElse AddLevel >= 950 OrElse IO.EmergencyStop OrElse Not MachineSafe) Then
      AddRunbackRequest = False
    End If
    addRunbackSwitchWas = IO.AddRunbackSw

    'add runback 
    If IO.AddTransferSw AndAlso Not AddTransferSwitchWas AndAlso Not IO.EmergencyStop AndAlso MachineSafe Then
      AddTransferRequest = True
    End If

    If AddTransferRequest AndAlso (Not IO.AddTransferSw OrElse IO.EmergencyStop OrElse Not MachineSafe) Then
      AddTransferRequest = False
    End If
    AddTransferSwitchWas = IO.AddTransferSw


    'manual fill/drain
    'fill
    If ManualFillRequest AndAlso IO.ManualFillPB AndAlso Not ManualFillPbWas Then 'was on but they pushed the button again so turn it off.
      ManualFillRequest = False
    End If

    If IO.ManualFillPB AndAlso Not ManualFillPbWas AndAlso VesLevel < 950 AndAlso Not IO.EmergencyStop AndAlso MachineSafe Then 'was off everything ok so turn on
      ManualFillRequest = True
    End If

    If ManualFillRequest AndAlso (AddLevel >= 950 OrElse IO.EmergencyStop OrElse Not MachineSafe) Then 'is on but not safe so turn it off.
      ManualFillRequest = False
    End If
    ManualFillRequest = IO.ManualFillPB

    'drain
    If ManualDrainRequest AndAlso IO.ManualDrainPB AndAlso Not ManualDrainPbWas Then 'was on but they pushed the button again so turn it off.
      ManualDrainRequest = False
    End If

    If IO.ManualDrainPB AndAlso Not ManualDrainPbWas AndAlso Not IO.EmergencyStop AndAlso MachineSafe Then 'was off everything ok so turn on
      ManualDrainRequest = True
    End If

    If ManualDrainRequest AndAlso (IO.EmergencyStop OrElse Not MachineSafe) Then 'is on but not safe so turn it off.
      ManualDrainRequest = False
    End If
    ManualDrainRequest = IO.ManualDrainPB

    'Crash Cooling
    Static PreviousCrashCoolPB As Boolean
    If IO.CrashCoolPushbutton And Not PreviousCrashCoolPB Then
      If TemperatureControl.IsCrashCoolOn Then
        TemperatureControl.CrashCoolStop()
      Else
        If VesTemp > TemperatureControl.Parameters_CrashCoolTemperature Then
          TemperatureControl.CrashCoolStart()
        End If
      End If
    End If
    PreviousCrashCoolPB = IO.CrashCoolPushbutton

    'Digital outputs
    'card 1
    IO.AlarmLamp = Parent.IsAlarmActive OrElse Parent.IsAlarmUnacknowledged
    IO.SignalLamp = Parent.IsSignalUnacknowledged OrElse (Not (Parent.ButtonText = "")) AndAlso SlowFlash
    IO.DelayLamp = IsDelayed
    IO.Siren = Parent.IsAlarmUnacknowledged OrElse Parent.IsSignalUnacknowledged
    IO.CrashCoolPushbuttonLamp = TemperatureControl.IsCrashCoolOn
    IO.MachineSafeLamp = MachineSafe
    IO.Vent = NStop AndAlso SafetyControl.IOVent
    IO.Fill = NHSafe AndAlso (FI.IsFilling OrElse RI.IsFilling OrElse RP.IsFilling OrElse ManualFillRequest)
    IO.Drain = NHSafe AndAlso (DR.IsDraining OrElse PD.IsDrainEmpty OrElse ManualDrainRequest OrElse PD.IsPumpToDrain OrElse PD.IsDrainEmpty)
    IO.PumpToDrain = NHSafe AndAlso (PD.IsPumpToDrain OrElse PD.IsDrainEmpty)
    IO.VesselDrain = NHSafe AndAlso (DR.IsDraining OrElse PD.IsDrainEmpty OrElse ManualDrainRequest)
    IO.OverFlow = NHSafe AndAlso (DR.IsDraining OrElse PD.IsDrainEmpty OrElse RI.IsDraining OrElse RP.IsDraining)

    'card 2
    IO.ContinuosWash = NHSafe AndAlso (DR.IsDraining OrElse PD.IsDrainEmpty OrElse RI.IsRinsing OrElse RP.IsRinsing)
    IO.Spray = NHalt AndAlso BO.IsOn
    IO.SteamSelect = NHalt AndAlso TemperatureControl.IsHeating AndAlso TemperatureControl.Output > 0
    IO.Condensate = NHalt AndAlso TemperatureControl.IsHeating
    IO.CoolSelect = NHalt AndAlso TemperatureControl.IsCooling AndAlso TemperatureControl.Output > 0
    IO.CoolWaterReturn = NHalt AndAlso TemperatureControl.IsCooling
    IO.HXDrain = NStop AndAlso Not TemperatureControl.IsHeating AndAlso Not TemperatureControl.IsCooling
    IO.MainPumpStart = NStop AndAlso IO.PumpInAutoSw AndAlso PumpAndReel.IsPumpOn
    IO.Reel1Forward = NStop AndAlso PumpAndReel.IsReelForward(1) AndAlso IO.ReelForwardSwitch
    IO.Reel1Reverse = NStop AndAlso PumpAndReel.IsReelReverse(1) AndAlso IO.ReelReverseSwitch
    IO.Reel2Forward = NStop AndAlso PumpAndReel.IsReelForward(2) AndAlso IO.ReelForwardSwitch
    IO.Reel2Reverse = NStop AndAlso PumpAndReel.IsReelReverse(2) AndAlso IO.ReelReverseSwitch

    'card 3
    IO.Reel3Forward = NStop AndAlso PumpAndReel.IsReelForward(3) AndAlso IO.ReelForwardSwitch
    IO.Reel3Reverse = NStop AndAlso PumpAndReel.IsReelReverse(3) AndAlso IO.ReelReverseSwitch
    IO.AddPumpStart = NStop AndAlso (AT.IsAddPump OrElse AD.IsAddPump OrElse RC.IOAddPump OrElse AddTransferRequest)
    IO.AddMixerStart = NStop AndAlso IO.AddMixerSw AndAlso Not AT.IsActive AndAlso Not AD.IsActive
    IO.AddPrepareLamp = (AP.IOPrepareLampSlow AndAlso SlowFlash) OrElse (AP.IOPrepareLampFast AndAlso FastFlash) _
                      OrElse AP.IOPrepareReady
    IO.AddTransfer = NHSafe AndAlso (AT.IsTransfer OrElse AD.IsTransfer OrElse RC.IOTransfer OrElse AddTransferRequest)
    IO.AddRunback = NHSafe AndAlso (AF.IsFillingMachine OrElse RC.IORunback OrElse AddRunbackRequest)
    IO.AddRinse = NStop AndAlso (AF.IsFillingFresh OrElse AT.IsRinse OrElse AD.IsRinse OrElse AddFillRequest)
    IO.AddHeat = NStop AndAlso IO.AddHeatSw AndAlso AddLevel > 0  'not sure when else there is not probe
    IO.AddDrain = NStop AndAlso IO.AddDrainSw 'hmm
    IO.FillPushbuttonLamp = False
    IO.DrainPushbuttonlamp = False

    'analog outputs===========================================================
    IO.HeatCoolOutput = 0
    If NHalt Then
      'temp control is master so we dont over heat machine
      If TemperatureControl.IsCooling Then IO.HeatCoolOutput = CType(MinMax(TemperatureControl.Output, 0, 1000), Short)
      If TemperatureControl.IsHeating AndAlso IO.PumpRunning Then
        IO.HeatCoolOutput = CType(MinMax(TemperatureControl.Output, 0, 1000), Short)
      End If
    End If

    IO.BlendFillOutput = 0
    If FI.IsFilling Then
      IO.BlendFillOutput = CType(MinMax(FI.IOBlendOutput, 0, 1000), Short)
    End If
    If RI.IsFilling Then
      IO.BlendFillOutput = CType(MinMax(RI.IOBlendOutput, 0, 1000), Short)
    End If
    If RP.IsFilling Then
      IO.BlendFillOutput = CType(MinMax(RP.IOBlendOutput, 0, 1000), Short)
    End If

    IO.FillOutput = 0
    If FI.IsFilling Then
      IO.FillOutput = 1000
    End If
    If RI.IsFilling OrElse RP.IsFilling Then
      IO.FillOutput = CType(MinMax(RI.Parameters_RinseFillOutput, 0, 1000), Short)
    End If

    'Pump, reel and plaiter speeds
    IO.PumpSpeedOutput = 0
    If IO.MainPumpStart Then
      If FP.IsOn Then IO.PumpSpeedOutput = CType(MinMax(FP.PSPercent, 0, 1000), Short)
      If FP.PSPercent = 0 Then IO.PumpSpeedOutput = CType(MinMax(Parameters_PumpSpeedDefault, 0, 1000), Short)
      If IO.ManualPumpSpeedSwitch Then IO.PumpSpeedOutput = CType(MinMax(ManualPumpSpeed, 0, 1000), Short)


      'Sets pump speed for display
      PumpSpeedDisplay = IO.PumpSpeedOutput \ 10 & " %"
    Else
      PumpSpeedDisplay = ""
    End If




    'reel speed
    'Reel 1
    IO.Reel1SpeedOutput = 0
    If IO.Reel1Forward OrElse IO.Reel1Reverse Then
      If RS.IsOn Then
        Reel1Ypm = RS.RSSpeed
      ElseIf DS.IsOn Then
        Reel1Ypm = DS.DSSpeed
      ElseIf WS.IsOn Then
        Reel1Ypm = WS.WSSpeed
      Else
        Reel1Ypm = Parameters_ReelSpeedDefault
      End If
      If IO.ManualReelSpeedSwitch Then Reel1Ypm = ManualReelSpeedYPM
    End If

    'Reel 2
    IO.Reel2SpeedOutput = 0
    If IO.Reel2Forward OrElse IO.Reel2Reverse Then
      If RS.IsOn Then
        Reel2Ypm = RS.RSSpeed
      ElseIf DS.IsOn Then
        Reel2Ypm = DS.DSSpeed
      ElseIf WS.IsOn Then
        Reel2Ypm = WS.WSSpeed
      Else
        Reel2Ypm = Parameters_ReelSpeedDefault
      End If
      If IO.ManualReelSpeedSwitch Then Reel2Ypm = ManualReelSpeedYPM
    End If

    'Reel 1
    IO.Reel3SpeedOutput = 0
    If IO.Reel3Forward OrElse IO.Reel3Reverse Then
      If RS.IsOn Then
        Reel3Ypm = RS.RSSpeed
      ElseIf DS.IsOn Then
        Reel3Ypm = DS.DSSpeed
      ElseIf WS.IsOn Then
        Reel3Ypm = WS.WSSpeed
      Else
        Reel3Ypm = Parameters_ReelSpeedDefault
      End If
      If IO.ManualReelSpeedSwitch Then Reel3Ypm = ManualReelSpeedYPM
    End If

    If Parameters_ReelSpeedMaximumYPM > 0 Then
      IO.Reel1SpeedOutput = CType(MinMax((Reel1Ypm * 1000) \ Parameters_ReelSpeedMaximumYPM, 0, 1000), Short)
      IO.Reel2SpeedOutput = CType(MinMax((Reel2Ypm * 1000) \ Parameters_ReelSpeedMaximumYPM, 0, 1000), Short)
      IO.Reel3SpeedOutput = CType(MinMax((Reel3Ypm * 1000) \ Parameters_ReelSpeedMaximumYPM, 0, 1000), Short)
    Else
      IO.Reel1SpeedOutput = CType(MinMax(Parameters_ReelSpeedDefault, 0, 1000), Short)
      IO.Reel2SpeedOutput = CType(MinMax(Parameters_ReelSpeedDefault, 0, 1000), Short)
      IO.Reel3SpeedOutput = CType(MinMax(Parameters_ReelSpeedDefault, 0, 1000), Short)
    End If


    'put at end of run section
    If Parent.IsPaused Then WasPausedTimer.Seconds = 15
    WasPaused = Not (WasPausedTimer.Finished)

    Delay = GetDelay()
    Parameters_MaybeSetDefaults()
    CheckAlarms()
    CalculateUtilities()

    SystemIdle = Not Parent.IsProgramRunning

    If Delay = DelayValue.NormalRunning Then
      IsDelayed = False
    Else
      IsDelayed = True
    End If

    Parent.PressButtons(IO.RemoteRun, IO.RemoteHalt, False, False, False)

    ' This should halt the control if sleeping
    If Parent.IsSleeping AndAlso (Not IsSleepingWas) Then
      IsSleepingWas = True
    End If
    If (Not Parent.IsSleeping) AndAlso IsSleepingWas Then
      IsSleepingWas = False
    End If


    If IO.UpsInBatteryMode AndAlso Not GoInToHibernate Then
      GoInToHibernate = True
      Application.SetSuspendState(PowerState.Hibernate, False, False)
    End If
    If Not IO.UpsInBatteryMode AndAlso GoInToHibernate Then

      GoInToHibernate = False
    End If


    'Set First Scan Done Flag
    FirstScanDone = True

  End Sub

#Region "Control Methods"

  Public Function ReadInputs(ByVal dinp() As Boolean, ByVal aninp() As Short, ByVal temp() As Short) As Boolean Implements ACControlCode.ReadInputs
    Return IO.ReadInputs(Parent, dinp, aninp, temp, Me)
  End Function

  Public Sub WriteOutputs(ByVal dout() As Boolean, ByVal anout() As Short) Implements ACControlCode.WriteOutputs
    IO.WriteOutputs(dout, anout, Me)
  End Sub

  Public Sub StartUp() Implements ACControlCode.StartUp
    PowerOnTimer.Seconds = 10
  End Sub

  Public Sub ShutDown() Implements ACControlCode.ShutDown
    SystemShuttingDown = True

  End Sub

  Public Sub ProgramStart() Implements ACControlCode.ProgramStart
    ProgramStoppedTime = ProgramStoppedTimer.TimeElapsed
    ProgramStoppedTimer.Pause()
    ProgramRunTimer.Start()

    Dim totaltime As TimeSpan

    For i = 0 To Parent.ProgramStepCount - 1 ' every step in the program that's about to start running
      Dim st = Parent.ProgramStep(i)
      totaltime += st.TotalTime
    Next i
    CalculatedStandardTime = Math.Round(totaltime.TotalDays, 6) 'changes to parts of a day.

    If CalculatedStandardTime <> 0 Then
      '    UpdateStandardTime()
    End If

    'change to program screen
    Parent.PressButton(ButtonPosition.Operator, 1)
  End Sub

  Public Sub ProgramStop() Implements ACControlCode.ProgramStop


  End Sub

  Public Sub UpdateStandardTime()
    Dim dyelot As String
    Dim redye As Integer
    Dim separator As String = "@"
    Dim JobSplit As String()
    JobSplit = Parent.Job.Split(CChar(separator))
    dyelot = JobSplit(0)
    If JobSplit.Length > 1 Then
      redye = CInt(JobSplit(1))
    Else
      redye = 0
    End If

    Dim sqlupdate = "Update dyelots set StandardTime = " & CalculatedStandardTime & " where dyelot='" & dyelot & "' and redye=" & redye
    '  Utilities.Sql.SqlUpdate(Settings.ConnectionString, sqlupdate)

  End Sub
#End Region

#Region "Program State Changes"
  Private Sub CheckProgramStateChanges()
    'Program running state changes
    Static ProgramWasRunning As Boolean

    If Parent.IsProgramRunning Then            'A Program is running
      CycleTime = ProgramRunTimer.TimeElapsed
      CycleTimeDisplay = TimerString(CycleTime)

      'get end time stuff.
      If GetEndTimeTimer.Finished Or (StepStandardTime <> StepStandardTimeWas) Then
        StepStandardTimeWas = StepStandardTime
        Dim totaltime As TimeSpan
        Dim Steptime As TimeSpan

        For i = 0 To Parent.ProgramStepCount - 1 ' every step in the program that's about to start running
          If i >= Parent.CurrentStep Then
            Dim st = Parent.ProgramStep(i)
            totaltime += st.TotalTime
          End If
          If i = Parent.CurrentStep Then
            Dim st = Parent.ProgramStep(i)
            Steptime = st.TimeInStep

          End If
        Next i
        EndTimeMins = CInt(totaltime.TotalMinutes)

        'see if current step is overruning or not
        If TimeInStepValue <= StepStandardTime Then
          EndTimeMins = EndTimeMins - Steptime.Minutes
        Else
          EndTimeMins -= StepStandardTime
        End If
        EndTime = Date.Now.AddMinutes(EndTimeMins).ToString
        GetEndTimeTimer.Seconds = 60
      End If

    Else
      If ProgramWasRunning Then
        ProgramRunTime = ProgramRunTimer.TimeElapsed
        ProgramRunTimer.Pause()
        ProgramStoppedTimer.Start()
        LastProgramCycleTime = CycleTime
        TemperatureControl.Cancel()
        PumpAndReel.AutoStop()
        MachineIsLoaded = False

      End If
      CycleTime = 0
      CycleTimeDisplay = "0"
      EndTime = ""
      StartTime = ""
      EndTimeMins = 0
      TimeInStepValue = 0
      StepStandardTime = 0
      WaterUsed = 0
      WaterUsedTemp = 0
      PowerKWS = 0
      PowerKWHrs = 0
      SteamUsed = 0
      AddReady = False

    End If
    ProgramWasRunning = Parent.IsProgramRunning
    If Not FirstScanDone Then
      ComputerName = My.Computer.Name
      ProgramStoppedTimer.Start()
    End If

    'time-in-step routine
    Static DisplayTIS As Boolean
    If TimeInStepValue > StepStandardTime Then
      If TwoSecondTimer.Finished Then
        DisplayTIS = Not DisplayTIS
        TwoSecondTimer.Seconds = 2
      End If
    Else
      DisplayTIS = True
    End If
    If DisplayTIS Then
      TimeInStep = CStr(TimeInStepValue)
    Else
      TimeInStep = "Overrun"
    End If

    Dim tis As String = Parent.TimeInStep
    Dim f As Integer = tis.IndexOf("/")
    If f <> -1 Then
      TimeInStepValue = CType(tis.Substring(0, f), Integer)
      StepStandardTime = CType(tis.Substring(f + 1), Integer)
    Else
      TimeInStepValue = 0 : StepStandardTime = 0
    End If
  End Sub

#End Region








End Class
