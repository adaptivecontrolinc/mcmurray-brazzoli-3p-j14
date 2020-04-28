'Pid and Temperature Control are in here

#Region "PID Control"

'PID Control - NOTE: Max Gradient = 0 rather than 99
Public Class PIDcontrol
  Private Enum State ' Are we heating, cooling or holding temp
    RampUp
    RampDown
    Hold
  End Enum
  Private state_ As State

  Private startTemp_ As Integer ' Start temperature for PID
  Private finalTemp_ As Integer ' Final temperature for PID
  Private gradient_ As Integer ' Gradient in degrees per minute
  Private ReadOnly gradientTimer_ As New TimerUp ' Timer for gradient control
  Private setpoint_ As Integer ' Calculated setpoint
  Private pidOutput As Integer ' pid output in tenths of percent

  Private propTerm_, integralTerm_, heatLossTerm_, gradientTerm_ As Integer

  Private errorSum_, errorSumCounter_ As Integer
  Private ReadOnly errorSumTimer_ As New Timer

  ' Control parameters
  Private propBand_, integral_, maxGradient_, balanceTemp_, balancePercent_ As Integer
  Private holdMargin_ As Integer ' go to hold if within this margin of setpoint

  Private paused_, cancelled_ As Boolean

  Public Sub New()
    ' Set the default PID parameters (assuming Farenheit)
    propBand_ = 22 : integral_ = 250 : maxGradient_ = 30
    balanceTemp_ = 250 : balancePercent_ = 100 : holdMargin_ = 22
    cancelled_ = True
  End Sub

  Public Sub Start(ByVal startTempInTenths As Integer, ByVal finalTempInTenths As Integer, ByVal gradientInTenthsPerMinute As Integer)
    'Set parameters
    startTemp_ = startTempInTenths
    finalTemp_ = finalTempInTenths
    gradient_ = gradientInTenthsPerMinute
    gradientTimer_.Start()

    'Reset control
    paused_ = False
    cancelled_ = False
    ResetIntegral()

    'Are we heating or cooling ?
    state_ = State.RampUp
    If finalTemp_ < startTemp_ Then state_ = State.RampDown
  End Sub

  Public Sub Run(ByVal currentTempInTenths As Integer)
    'Check to see if PID cancelled
    If cancelled_ Then Exit Sub

    'Calculate Setpoint
    Dim rampFinished As Boolean
    If state_ = State.Hold Then rampFinished = True
    If setpoint_ > 0 Then
      If state_ = State.RampUp AndAlso (setpoint_ >= finalTemp_) Then rampFinished = True
      If state_ = State.RampDown AndAlso (setpoint_ <= finalTemp_) Then rampFinished = True
    End If
    If ((currentTempInTenths > (finalTemp_ - holdMargin_)) AndAlso (currentTempInTenths < (finalTemp_ + holdMargin_))) OrElse IsMaxGradient Then
      state_ = State.Hold
      setpoint_ = finalTemp_
      rampFinished = True
    End If

    If Not (IsMaxGradient OrElse rampFinished) Then
      If state_ = State.RampUp Then setpoint_ = startTemp_ + ((gradientTimer_.TimeElapsed * gradient_) \ 60)
      If state_ = State.RampDown Then setpoint_ = startTemp_ - ((gradientTimer_.TimeElapsed * gradient_) \ 60)
    Else
      setpoint_ = finalTemp_
      If IsRampUp AndAlso (currentTempInTenths > (finalTemp_ - holdMargin_)) Then
        state_ = State.Hold
      End If
      If IsRampDown AndAlso (currentTempInTenths < (finalTemp_ + holdMargin_)) Then
        state_ = State.Hold
      End If
    End If

    'Calculate error
    Dim tempError As Integer = setpoint_ - currentTempInTenths

    'Calculate proportional Term
    'Stops div by 0
    If propBand_ = 0 Then propBand_ = 1
    propTerm_ = (tempError * 1000) \ propBand_

    'If PID output is maxxed out stop Integral action.
    'This should prevent Integral saturation i.e. Error Sum/Integral term getting huge!
    Dim stopIntegral As Boolean
    If (pidOutput = 1000) AndAlso (tempError > 0) Then stopIntegral = True
    If (pidOutput = -1000) AndAlso (tempError < 0) Then stopIntegral = True
    'Calculate Error Sum for integral term - add once a second if allowed
    If (Not stopIntegral) AndAlso errorSumTimer_.Finished Then
      errorSumTimer_.TimeRemaining = 1
      errorSum_ = errorSum_ + tempError
    End If

    'Calculate Integral Term - limit to +/- 100%
    integralTerm_ = (errorSum_ * integral_) \ 6000
    If integralTerm_ > 1000 Then integralTerm_ = 1000
    If integralTerm_ < -1000 Then integralTerm_ = -1000

    'Calculate heat loss term - limit to +/- 100%
    heatLossTerm_ = ((currentTempInTenths - balanceTemp_) * balancePercent_) \ 1000
    If heatLossTerm_ > 1000 Then heatLossTerm_ = 1000
    If heatLossTerm_ < -1000 Then heatLossTerm_ = -1000

    'Calculate gradient term - limit to +/- 100%
    'stop div by 0
    If maxGradient_ = 0 Then maxGradient_ = 1
    gradientTerm_ = (1000 * gradient_) \ maxGradient_
    If state_ = State.RampDown Then
      gradientTerm_ = -gradientTerm_
    End If
    If gradientTerm_ > 1000 Then gradientTerm_ = 1000
    If gradientTerm_ < -1000 Then gradientTerm_ = -1000
    If IsHolding Then gradientTerm_ = 0

    'Calculate Output  - limit to +/- 100%
    pidOutput = propTerm_ + integralTerm_ + heatLossTerm_ + gradientTerm_
    If pidOutput > 1000 Then pidOutput = 1000
    If pidOutput < -1000 Then pidOutput = -1000
  End Sub

  Public Sub Pause()
    paused_ = True
    gradientTimer_.Pause()
  End Sub

  Public Sub Restart()
    paused_ = False
    gradientTimer_.Restart()
  End Sub

  Public Sub Reset(ByVal CurrentTempInTenths As Integer)
    paused_ = False
    startTemp_ = CurrentTempInTenths
    gradientTimer_.Start()
    ResetIntegral()
  End Sub

  Public Sub Cancel()
    cancelled_ = True
    ResetIntegral()
    startTemp_ = 0
    finalTemp_ = 0
    gradient_ = 0
    setpoint_ = 0
    pidOutput = 0
  End Sub

  Private Sub ResetIntegral()
    errorSum_ = 0
    errorSumTimer_.TimeRemaining = 1
    errorSumCounter_ = 0
  End Sub

#Region "Properties"
  Public WriteOnly Property PropBand() As Integer
    Set(ByVal value As Integer)
      propBand_ = value
      If propBand_ < 0 Then propBand_ = 0
      If propBand_ > 1000 Then propBand_ = 1000
    End Set
  End Property
  Public WriteOnly Property Integral() As Integer
    Set(ByVal value As Integer)
      integral_ = value
      If integral_ < 0 Then integral_ = 0
      If integral_ > 1000 Then integral_ = 1000
    End Set
  End Property

  Public WriteOnly Property MaxGradient() As Integer
    Set(ByVal value As Integer)
      maxGradient_ = value
      If maxGradient_ < 0 Then maxGradient_ = 0
      If maxGradient_ > 1000 Then maxGradient_ = 1000
    End Set
  End Property

  Public Property BalanceTemp() As Integer
    Get
      Return balanceTemp_
    End Get
    Set(ByVal value As Integer)
      balanceTemp_ = value
      balanceTemp_ = MinMax(value, 0, 1000)
    End Set
  End Property
  Public Property BalancePercent() As Integer
    Get
      Return balancePercent_
    End Get
    Set(ByVal value As Integer)
      balancePercent_ = MinMax(value, 0, 1000)
    End Set
  End Property

  Public WriteOnly Property HoldMargin() As Integer
    Set(ByVal value As Integer)
      holdMargin_ = value
      If holdMargin_ < 10 Then holdMargin_ = 10
      If holdMargin_ > 100 Then holdMargin_ = 100
    End Set
  End Property

  Public ReadOnly Property Output() As Integer
    Get
      Output = pidOutput
      If cancelled_ Then Output = 0
    End Get
  End Property
  Public ReadOnly Property Gradient() As Integer
    Get
      Return gradient_
    End Get
  End Property
  Public ReadOnly Property FinalTemp() As Integer
    Get
      Return finalTemp_
    End Get
  End Property
  Public ReadOnly Property Setpoint() As Integer
    Get
      Return setpoint_
    End Get
  End Property
  Public ReadOnly Property IsRampUp() As Boolean
    Get
      Return state_ = State.RampUp
    End Get
  End Property
  Public ReadOnly Property IsRampDown() As Boolean
    Get
      Return state_ = State.RampDown
    End Get
  End Property
  Public ReadOnly Property IsHolding() As Boolean
    Get
      Return state_ = State.Hold
    End Get
  End Property

  Public ReadOnly Property IsRamping() As Boolean
    Get
      Return Not (IsMaxGradient OrElse IsHolding)
    End Get
  End Property
  Public ReadOnly Property IsMaxGradient() As Boolean
    Get
      Return gradient_ = 0
    End Get
  End Property
  Public ReadOnly Property IsPaused() As Boolean
    Get
      Return paused_
    End Get
  End Property
  Public ReadOnly Property PropTerm() As Integer
    Get
      Return propTerm_
    End Get
  End Property
  Public ReadOnly Property IntegralTerm() As Integer
    Get
      Return integralTerm_
    End Get
  End Property
  Public ReadOnly Property HeatLossTerm() As Integer
    Get
      Return heatLossTerm_
    End Get
  End Property
  Public ReadOnly Property GradientTerm() As Integer
    Get
      Return gradientTerm_
    End Get
  End Property
#End Region

End Class

#End Region

#Region "Temperature Control"

Public Class TemperatureControl : Inherits MarshalByRefObject
  ' Here are the alarms we generate
  Public Alarms_TemperatureHigh, Alarms_TemperatureLow, Alarms_CrashCooling, Alarms_CrashCoolingDone As Boolean

#Region "Parameters"
  'PID Heating Parameters - pass to PID while temperature state = heating
  <Parameter(1, 10000), Category("Temperature Control"), Description("The proportional band value, while heating, in tenths F.")> _
    Public Parameters_HeatPropBand As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("The rate of integral action, while cooling,  in repeats per minute.")> _
    Public Parameters_HeatIntegral As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("The maximum heating rate that the machine can maintain, measured in F per minute.")> _
    Public Parameters_HeatMaxGradient As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("When measured temp is within this many degrees F of setpoint, while heating, then setpoint has been achieved.")> _
    Public Parameters_HeatStepMargin As Integer

  'PID Cooling Parameters - pass to PID while temperature state = cooling
  <Parameter(0, 10000), Category("Temperature Control"), Description("The proportional band value, while cooling, in tenths of degree F.")> _
   Public Parameters_CoolPropBand As Integer
  <Parameter(1, 10000), Category("Temperature Control"), Description("The rate of integral action, while cooling,  in repeats per minute.")> _
   Public Parameters_CoolIntegral As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("The maximum cooling rate that the machine can maintain, measured in F per minute.")> _
    Public Parameters_CoolMaxGradient As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("When measured temp is within this many degrees F of setpoint, while cooling, then setpoint has been achieved.")> _
  Public Parameters_CoolStepMargin As Integer

  'This next one is read by other commands, but not by us - this is a bit odd
  <Parameter(0, 10000), Category("Temperature Control"), Description("0 means switch any time; 1 means switch only when holding temp; 2 means switch only on new temp command.")> _
   Public Parameters_HeatCoolModeChange As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("Tempeture, in tenths, that the machine will maintain with not heating.")> _
   Public Parameters_BalPercent As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("If control is holding, the machine temp must be above setpoint by this much, in tenths F, before switching to cooling.")> _
   Public Parameters_HeatExitDeadband As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("The tempe bandwidth in tenths of a degree. If the actual temp is this many degrees higher/lower than the setpoint temp, an alarm will occur.")> _
    Public Parameters_TemperatureAlarmBand As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("The delay time in seconds before an alarm will occur if the Temp Alarm Band is exceed.")> _
    Public Parameters_TemperatureAlarmDelay As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("If the difference between the setpoint and the measured temp is greater than this value, then stop the setpoint changing.")> _
    Public Parameters_TemperaturePidPause As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("If the temp gradient has been paused, the measured value must be within this many degrees F of the setpoint before the gradient can restart.")> _
   Public Parameters_TemperaturePidRestart As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("If the difference between the setpoint and the measured temp is greater than this value, then reset the temp control.")> _
    Public Parameters_TemperaturePidReset As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("At the beginning of a heating step, the heat-exchanger is vented / purged for this many seconds.")> _
   Public Parameters_HeatVentTime As Integer
  <Parameter(0, 10000), Category("Temperature Control"), Description("At the beginning of a cooling step, the heat-exchanger is vented / purged for this many seconds.")> _
   Public Parameters_CoolVentTime As Integer
  <Parameter(0, 10000), Category("Setup"), Description("Temperature in tenths of a degree that the machine will cool to when the crash cool button is pushed.")> _
    Public Parameters_CrashCoolTemperature As Integer
  <Parameter(0, 99), Category("Setup"), Description("The rate in tenths of a degree per minute to cool the machine at during a crash cool. Zero is 100% cooling")> _
  Public Parameters_CrashCoolGradient As Integer

#End Region

  Private Enum Mode
    HeatAndCool
    HeatOnly
    CoolOnly
    ChangeDisabled
  End Enum

  Private mode_ As Mode

  Private Enum State
    Off
    Start
    Pause
    PreHeatVent
    Heat
    PostHeatVent
    PreCoolVent
    Cool
    PostCoolVent
    CrashCoolStart
    CrashCoolPause
    CrashCoolVent
    CrashCool
    CrashCoolDone
    CrashCoolRestart
  End Enum
  Private state_ As State, previousState_ As State
  Private ReadOnly pid_ As New PIDcontrol

  ' We use all sorts of timers
  Private ReadOnly idleTimer_ As New Timer, stateTimer_ As New Timer, enableTimer_ As New Timer, _
                   heatDelayTimer_ As New Timer, coolDelayTimer_ As New Timer, modeChangeTimer_ As New Timer

  'PID setpoints - these are useful for restarting temp control after crash cooling
  Private pidStartTemp_ As Integer
  Private pidFinalTemp_ As Integer
  Private pidGradient_ As Integer

  'Temperature control parameters
  'Temp enabled must be made for this time before we can heat or cool
  'Can be zero (default = 10)
  Private tempEnableDelay_ As Integer
  'If we we're cooling and now we want to heat - delay heating for this time (and vice versa)
  'This can be used to ensure the heat exchanger drain valve has been open long enough to
  'fully drain the heat exchanger prior to heating or cooling.
  'Can be zero (default = 10)
  Private tempHeatCoolDelay_ As Integer
  'Usual mode change delay - can be zero (default = 120)
  Private heatCoolModeChangeDelay_ As Integer
  'Time to vent prior to heating - can be zero (default = 10)
  Private tempPreHeatVentTime_ As Integer
  'Time to vent after heating - can be zero (default = 10)
  Private tempPostHeatVentTime_ As Integer
  'Time to vent prior to cooling - can be zero (default = 10)
  Private tempPreCoolVentTime_ As Integer
  'Time to vent after cooling - can be zero (default = 10)
  Private tempPostCoolVentTime_ As Integer

  'Crash Cool setpoint
  Private tempCrashCoolTemp_ As Integer

  Private firstTempStart_ As Boolean

  Private coolPropBand_ As Integer
  Private coolIntegral_ As Integer
  Private coolMaxGradient_ As Integer
  Private coolStepMargin_ As Integer

  Public Sub New()
    'Default values for temperature control
    mode_ = Mode.HeatAndCool
    state_ = State.Off
    stateTimer_.TimeRemaining = 10
    enableTimer_.TimeRemaining = 10

    'Set parameters to defaults
    tempEnableDelay_ = 10
    tempHeatCoolDelay_ = 30
    heatCoolModeChangeDelay_ = 120
    tempPreHeatVentTime_ = 10
    tempPostHeatVentTime_ = 10
    tempPreCoolVentTime_ = 10
    tempPostCoolVentTime_ = 10

    tempCrashCoolTemp_ = 1700

    firstTempStart_ = True
  End Sub

  Public Sub InitializeControl(ByVal heatstart As Boolean)

    tempPreCoolVentTime_ = Parameters_CoolVentTime


    If heatstart Then
      CoolingIntegral = Parameters_HeatIntegral

    Else
      CoolingIntegral = Parameters_CoolIntegral


    End If
  End Sub
  Public Sub Start(ByVal VesTemp As Integer, ByVal FinalTempInTenths As Integer, ByVal GradientInTenthsPerMinute As Integer)
    'Set start temperature, target temperature and gradient
    pidStartTemp_ = VesTemp
    pidFinalTemp_ = FinalTempInTenths
    pidGradient_ = GradientInTenthsPerMinute

    'sets the vent time
    If Parameters_HeatVentTime > 10 Then
      tempPreHeatVentTime_ = Parameters_HeatVentTime
    End If
    If Parameters_CoolVentTime > 10 Then
      tempPreCoolVentTime_ = Parameters_CoolVentTime
    End If


    'Set pid parameters to heat by default
    pid_.PropBand = Parameters_HeatPropBand
    pid_.Integral = Parameters_HeatIntegral
    pid_.MaxGradient = Parameters_HeatMaxGradient
    pid_.HoldMargin = Parameters_HeatStepMargin

    firstTempStart_ = True
    'If crash cooling exit - PID will start when crash cool done
    If IsCrashCoolOn Then Exit Sub

    'Start the PID
    pid_.Start(pidStartTemp_, pidFinalTemp_, pidGradient_)

    'Decide wether we should heat, cool or wait and see
    'Decision is deliberately biased in favour of heating - because that's more likely

    'If we're already heating (or about to heat) and the current temp is lower than
    'the Final Temp (and a bit) then keep going i.e. don't change state
    If IsHeating OrElse IsPreHeatVent Then
      If VesTemp <= (FinalTempInTenths + Parameters_HeatPropBand) Then Exit Sub
    End If

    'If we're already cooling (or about to cool) and the current temp is higher than
    'the Final Temp (by a bit) then keep going i.e. don't change state
    If IsCooling OrElse IsPreCoolVent Then
      If VesTemp > (FinalTempInTenths + Parameters_CoolPropBand) Then Exit Sub
    End If

    'If we're venting after heat or cool then let vent finish and allow start state to
    'decide which way to go
    If IsPostHeatVent OrElse IsPostCoolVent Then Exit Sub

    'Set state to start
    state_ = State.Start
    stateTimer_.TimeRemaining = 5

  End Sub

  Public Sub Run(ByVal VesTemp As Integer)

    Select Case state_
      Case State.Off
        'Set previous state variable
        previousState_ = State.Start
        'Set state timer
        stateTimer_.TimeRemaining = 5

      Case State.Start
        'Set previous state variable
        previousState_ = State.Start
        'If no start temp or final temp then set state to off
        If pidStartTemp_ = 0 OrElse pidFinalTemp_ = 0 Then state_ = State.Off
        'Wait for state timer
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        'Wait for temperature enable
        If (enableTimer_.TimeRemaining > 0) Then Exit Sub
        'Reset PID to clear out any "funnies"
        'Comment this out to see if it cures cool on gradient bug
        'PID.Reset VesTemp
        'Run PID to decide whether to heat or cool
        pid_.Run(VesTemp)
        'Added to allow us to go straight to cooling on a controlled gradient
        'If we need to cool, don't think about heating yet
        If firstTempStart_ Then
          firstTempStart_ = False
          If VesTemp > (pidFinalTemp_ + Parameters_CoolPropBand) Then
            state_ = State.PreCoolVent
            stateTimer_.TimeRemaining = tempPreCoolVentTime_
            Exit Sub
          End If
        End If
        'If PID Output is greater than zero then heat
        If pid_.Output > 0 Then
          'Are we allowed to heat ?
          If mode_ = Mode.CoolOnly Then Exit Sub
          'Have we waited long enough before switching to heating
          If (heatDelayTimer_.TimeRemaining > 0) Then Exit Sub
          state_ = State.PreHeatVent
          stateTimer_.TimeRemaining = tempPreHeatVentTime_
        End If
        'If PID Output is less than zero then cool
        If (pid_.Output < 0) Then
          'Are we allowed to cool ?
          If mode_ = Mode.HeatOnly Then Exit Sub
          'Have we waited long enough before switching to cooling
          If (coolDelayTimer_.TimeRemaining > 0) Then Exit Sub
          state_ = State.PreCoolVent
          stateTimer_.TimeRemaining = tempPreCoolVentTime_
        End If

      Case State.Pause
        'Pause PID
        pid_.Pause()
        'Wait for state timer
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        'Wait for temperature enable
        If (enableTimer_.TimeRemaining > 0) Then Exit Sub
        'Switch back to previous state
        state_ = previousState_
        'Restart PID
        pid_.Restart()
        'If venting set timer to parameter value
        If state_ = State.PreHeatVent Then stateTimer_.TimeRemaining = tempPreHeatVentTime_
        If state_ = State.PostHeatVent Then stateTimer_.TimeRemaining = tempPostHeatVentTime_
        If state_ = State.PreCoolVent Then stateTimer_.TimeRemaining = tempPreCoolVentTime_
        If state_ = State.PostCoolVent Then stateTimer_.TimeRemaining = tempPostCoolVentTime_

        'Heating

      Case State.PreHeatVent
        'Set previous state variable
        previousState_ = State.PreHeatVent
        'Reset cool delay timer
        coolDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If enable timer set switch to pause state
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.Pause
        'Wait for parameter time then switch to heating
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        state_ = State.Heat
        'Reset mode change timer
        modeChangeTimer_.TimeRemaining = heatCoolModeChangeDelay_
        'Reset PID to start from current temp
        pid_.Reset(VesTemp)

      Case State.Heat
        'Set previous state variable
        previousState_ = State.Heat
        'Reset cool delay timer
        coolDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If enable timer set switch to pause state
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.Pause
        'Set PID parameters here so that changes apply immediately
        pid_.PropBand = Parameters_HeatPropBand
        pid_.Integral = Parameters_HeatIntegral
        pid_.MaxGradient = Parameters_HeatMaxGradient
        pid_.HoldMargin = Parameters_HeatStepMargin
        'Run PID Control
        pid_.Run(VesTemp)
        'Reset mode change timer if mode change disabled (Note parameter could be zero)
        If mode_ = Mode.ChangeDisabled Then modeChangeTimer_.TimeRemaining = heatCoolModeChangeDelay_ + 1
        'Reset mode change timer while we're still calling for heating

        If pid_.Output >= 0 Then modeChangeTimer_.TimeRemaining = heatCoolModeChangeDelay_
        If (VesTemp <= pidFinalTemp_ + Parameters_HeatExitDeadband) Then modeChangeTimer_.TimeRemaining = heatCoolModeChangeDelay_
        If modeChangeTimer_.Finished Then
          state_ = State.PostHeatVent
          stateTimer_.TimeRemaining = tempPostHeatVentTime_
        End If

      Case State.PostHeatVent
        'Set previous state variable
        previousState_ = State.PostHeatVent
        'Reset cool delay timer
        coolDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If enable timer set switch to pause state
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.Pause
        'Wait for parameter time then switch to idle state
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        state_ = State.Start
        stateTimer_.TimeRemaining = 5

        'Cooling

      Case State.PreCoolVent
        'Set previous state variable
        previousState_ = State.PreCoolVent
        'Reset heat delay timer
        heatDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If enable timer set switch to pause state
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.Pause
        'Wait for parameter time then switch to cooling
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        state_ = State.Cool
        'Reset mode change timer
        modeChangeTimer_.TimeRemaining = heatCoolModeChangeDelay_
        'Reset PID to start from current temp
        pid_.Reset(VesTemp)

      Case State.Cool
        'Set previous state variable
        previousState_ = State.Cool
        'Reset heat delay timer
        heatDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If enable timer set switch to pause state
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.Pause
        'Set PID parameters here so that changes apply immediately
        pid_.PropBand = Parameters_CoolPropBand
        pid_.Integral = Parameters_CoolIntegral
        pid_.MaxGradient = Parameters_CoolMaxGradient
        pid_.HoldMargin = Parameters_CoolStepMargin
        'Run PID Control
        pid_.Run(VesTemp)
        'Reset mode change timer if mode change disabled (Note parameter could be zero)
        If mode_ = Mode.ChangeDisabled Then modeChangeTimer_.TimeRemaining = heatCoolModeChangeDelay_ + 1
        'Reset mode change timer while we're still calling for cooling
        If pid_.Output <= 0 Then modeChangeTimer_.TimeRemaining = heatCoolModeChangeDelay_
        If modeChangeTimer_.Finished Then
          state_ = State.PostCoolVent
          stateTimer_.TimeRemaining = tempPostCoolVentTime_
        End If

      Case State.PostCoolVent
        'Set previous state variable
        previousState_ = State.PostCoolVent
        'Reset heat delay timer
        heatDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If enable timer set switch to pause state
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.Pause
        'Wait for parameter time then switch to idle state
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        'Switch to cooling state
        state_ = State.Start
        stateTimer_.TimeRemaining = 5

        'Crash cooling

      Case State.CrashCoolStart
        CrashCoolTemp = Parameters_CrashCoolTemperature
        'Wait for parameter time
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        'Wait for cool delay time
        If (coolDelayTimer_.TimeRemaining > 0) Then Exit Sub
        'Switch to crash cool vent
        state_ = State.CrashCoolVent
        stateTimer_.TimeRemaining = tempPreCoolVentTime_
        'Start PID
        pid_.Start(VesTemp, (tempCrashCoolTemp_ - 20), Parameters_CrashCoolGradient)

      Case State.CrashCoolPause
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        If (enableTimer_.TimeRemaining > 0) Then Exit Sub
        'Everythings okay so go back to what we were doing before
        state_ = previousState_
        'Reset PID
        pid_.Reset(VesTemp)

      Case State.CrashCoolVent
        'Set previous state variable
        previousState_ = State.CrashCoolVent
        'Reset heat delay timer
        heatDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If enable timer set switch to pause state
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.CrashCoolPause
        'Wait for parameter time
        If (stateTimer_.TimeRemaining > 0) Then Exit Sub
        'Switch to Crash Cool
        state_ = State.CrashCool
        'Reset PID to start from current temp
        pid_.Reset(VesTemp)

      Case State.CrashCool
        'Set previous state variable
        previousState_ = State.CrashCool
        'Reset heat delay timer
        heatDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If pump not running switch to pause
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.CrashCoolPause
        'Run the PID
        pid_.Run(VesTemp)
        'If we're at or below crash cool temperature start holding
        If VesTemp <= tempCrashCoolTemp_ Then state_ = State.CrashCoolDone

      Case State.CrashCoolDone
        'Set previous state variable
        previousState_ = State.CrashCoolDone
        'Reset heat delay timer
        heatDelayTimer_.TimeRemaining = tempHeatCoolDelay_
        'If pump not running switch to vent
        If (enableTimer_.TimeRemaining > 0) Then state_ = State.CrashCoolPause
        'Run the PID
        pid_.Run(VesTemp)

      Case State.CrashCoolRestart
        'TODO: Check hold time / gradient and decide wether to use original gradient, max
        'gradient or cancel and go to next step ?
        'Go to Post cool vent
        state_ = State.PostCoolVent
        stateTimer_.TimeRemaining = tempPostCoolVentTime_
        'Check to see if we were cooling - cool to final temp
        If (pidStartTemp_ > pidFinalTemp_) AndAlso (pidFinalTemp_ < Parameters_CrashCoolTemperature) Then
          pid_.Start(VesTemp, Parameters_CrashCoolTemperature, pidGradient_)
          Exit Sub
        End If
        'If temperature control not active then clear PID (?) and carry on
        If pidStartTemp_ = 0 OrElse pidFinalTemp_ = 0 Then
          pid_.Cancel()
          Exit Sub
        End If
        'Restart pid with original final temp and gradient
        pid_.Start(VesTemp, pidFinalTemp_, pidGradient_)
    End Select
  End Sub

  Public Sub CheckErrorsAndMakeAlarms(ByVal VesTemp As Integer)
    'Check pid pause and reset
    Dim tempError As Integer
    If IsHeating Then tempError = TempSetpoint - VesTemp
    If IsCooling Then tempError = VesTemp - TempSetpoint
    Dim ignoreErrors As Boolean = (IsCrashCoolOn OrElse IsMaxGradient)
    If Not ignoreErrors Then
      If tempError > Parameters_TemperaturePidReset Then PidReset(VesTemp)
      If tempError > Parameters_TemperaturePidPause Then PidPause()
      If IsPidPaused Then
        If tempError < Parameters_TemperaturePidRestart Then PidRestart()
      End If
    End If
    MakeAlarms(VesTemp, ignoreErrors)
  End Sub

  Public Sub Cancel()
    pid_.Cancel()
    state_ = State.Off
    pidStartTemp_ = 0
    pidFinalTemp_ = 0
    pidGradient_ = 0
  End Sub

  Public Sub CrashCoolStart()
    If Not IsCrashCoolOn Then state_ = State.CrashCoolStart
  End Sub

  Public Sub CrashCoolStop()
    If IsCrashCoolOn Then state_ = State.CrashCoolRestart
  End Sub

  Public Sub PidPause()
    pid_.Pause()
  End Sub
  Public Sub PidRestart()
    pid_.Restart()
  End Sub

  Public Sub PidReset(ByRef vesTemp As Integer)
    pid_.Reset(vesTemp)
  End Sub
  Public Sub ResetEnableTimer()
    enableTimer_.TimeRemaining = tempEnableDelay_
  End Sub

  ' Make alarms for temperature control
  Private Sub MakeAlarms(ByVal vesTemp As Integer, ByVal ignoreErrors As Boolean)
    'Temperature low/high s
    Static TempLoAlarmTimer As New Timer
    Static TempHiAlarmTimer As New Timer
    If ignoreErrors OrElse (Not (IsHeating OrElse IsCooling)) Then
      TempLoAlarmTimer.TimeRemaining = Parameters_TemperatureAlarmDelay
      TempHiAlarmTimer.TimeRemaining = Parameters_TemperatureAlarmDelay
    End If
    If vesTemp > (TempSetpoint - Parameters_TemperatureAlarmBand) Then
      TempLoAlarmTimer.TimeRemaining = Parameters_TemperatureAlarmDelay
    End If
    If vesTemp < (TempSetpoint + Parameters_TemperatureAlarmBand) Then
      TempHiAlarmTimer.TimeRemaining = Parameters_TemperatureAlarmDelay
    End If
    'Alarms_TemperatureLow = TempLoAlarmTimer.Finished 'AndAlso ((PID.GradientTerm > 0) OrElse PID.IsHolding)
    'Alarms_TemperatureHigh = TempHiAlarmTimer.Finished 'AndAlso ((PID.GradientTerm > 0) OrElse PID.IsHolding)

    'Crash Cool s
    Alarms_CrashCooling = IsCrashCoolOn AndAlso (Not IsCrashCoolDone)
    Alarms_CrashCoolingDone = IsCrashCoolDone
  End Sub

#Region "Properties"
  Public Property Parameters_BalanceTemperature() As Integer
    Get
      Return pid_.BalanceTemp
    End Get
    Set(ByVal value As Integer)
      pid_.BalanceTemp = value
    End Set
  End Property
  'Public Property Get Parameters_BalancePercent() As Long
  'Parameters_BalancePercent = PID.BalancePercent
  'End Property
  Public WriteOnly Property Parameters_BalancePercent() As Integer
    Set(ByVal value As Integer)
      pid_.BalancePercent = value
    End Set
  End Property
  Public Property Parameters_HeatCoolModeChangeDelay() As Integer
    Get
      Return heatCoolModeChangeDelay_
    End Get
    Set(ByVal value As Integer)
      heatCoolModeChangeDelay_ = MinMax(value, 0, 600)
    End Set
  End Property

  Public WriteOnly Property EnableDelay() As Integer
    Set(ByVal value As Integer)
      tempEnableDelay_ = value
      If tempEnableDelay_ < 0 Then tempEnableDelay_ = 0
      If tempEnableDelay_ > 30 Then tempEnableDelay_ = 30
    End Set
  End Property
  Public WriteOnly Property TempMode() As Integer
    Set(ByVal value As Integer)
      mode_ = Mode.HeatAndCool
      If value = 2 Then mode_ = Mode.ChangeDisabled
      If value = 3 Then mode_ = Mode.HeatOnly
      If value = 4 Then mode_ = Mode.CoolOnly
    End Set
  End Property
  Public WriteOnly Property CoolingPropBand() As Integer
    Set(ByVal value As Integer)
      coolPropBand_ = value
    End Set
  End Property
  Public WriteOnly Property CoolingIntegral() As Integer
    Set(ByVal value As Integer)
      coolIntegral_ = value
    End Set
  End Property
  Public WriteOnly Property CoolingMaxGradient() As Integer
    Set(ByVal value As Integer)
      coolMaxGradient_ = value
    End Set
  End Property
  Public WriteOnly Property CoolingStepMargin() As Integer
    Set(ByVal value As Integer)
      coolStepMargin_ = value
    End Set
  End Property

  Public WriteOnly Property PreHeatVentTime() As Integer
    Set(ByVal value As Integer)
      tempPreHeatVentTime_ = value
      If tempPreHeatVentTime_ < 0 Then tempPreHeatVentTime_ = 0
      If tempPreHeatVentTime_ > 60 Then tempPreHeatVentTime_ = 60
    End Set
  End Property
  Public WriteOnly Property PostHeatVentTime() As Integer
    Set(ByVal value As Integer)
      tempPostHeatVentTime_ = value
      If tempPostHeatVentTime_ < 0 Then tempPostHeatVentTime_ = 0
      If tempPostHeatVentTime_ > 60 Then tempPostHeatVentTime_ = 60
    End Set
  End Property
  Public WriteOnly Property PreCoolVentTime() As Integer
    Set(ByVal value As Integer)
      tempPreCoolVentTime_ = value
      If tempPreCoolVentTime_ < 0 Then tempPreCoolVentTime_ = 0
      If tempPreCoolVentTime_ > 60 Then tempPreCoolVentTime_ = 60
    End Set
  End Property
  Public WriteOnly Property PostCoolVentTime() As Integer
    Set(ByVal value As Integer)
      tempPostCoolVentTime_ = value
      If tempPostCoolVentTime_ < 0 Then tempPostCoolVentTime_ = 0
      If tempPostCoolVentTime_ > 60 Then tempPostCoolVentTime_ = 60
    End Set
  End Property
  Public WriteOnly Property CrashCoolTemp() As Integer
    Set(ByVal value As Integer)
      tempCrashCoolTemp_ = value
      If tempCrashCoolTemp_ < 1500 Then tempCrashCoolTemp_ = 1500
      If tempCrashCoolTemp_ > 1900 Then tempCrashCoolTemp_ = 1900
    End Set
  End Property
  Public ReadOnly Property IsEnabled() As Boolean
    Get
      Return enableTimer_.Finished
    End Get
  End Property
  Public ReadOnly Property IsIdle() As Boolean
    Get
      Return (state_ = State.Start) OrElse (state_ = State.Off)
    End Get
  End Property
  Public ReadOnly Property IsPaused() As Boolean
    Get
      Return (state_ = State.Pause)
    End Get
  End Property
  Public ReadOnly Property IsHeating() As Boolean
    Get
      Return (state_ = State.Heat)
    End Get
  End Property
  Public ReadOnly Property IsCooling() As Boolean
    Get
      Return (state_ = State.Cool) OrElse IsCrashCooling
    End Get
  End Property
  Public ReadOnly Property IsMaxGradient() As Boolean
    Get
      'Set max gradient only if we are ramping up/down (it's used to disable alarms)
      Return (Not pid_.IsHolding) AndAlso pid_.IsMaxGradient
    End Get
  End Property
  Public ReadOnly Property IsHolding() As Boolean
    Get
      'Make sure IsHolding returns false during Crashcooling
      Return (Not IsCrashCoolOn) AndAlso pid_.IsHolding
    End Get
  End Property
  Public ReadOnly Property IsPreHeatVent() As Boolean
    Get
      Return (state_ = State.PreHeatVent)
    End Get
  End Property
  Public ReadOnly Property IsPostHeatVent() As Boolean
    Get
      Return (state_ = State.PostHeatVent)
    End Get
  End Property
  Public ReadOnly Property IsPreCoolVent() As Boolean
    Get
      Return (state_ = State.PreCoolVent) OrElse IsCrashCoolVent
    End Get
  End Property
  Public ReadOnly Property IsPostCoolVent() As Boolean
    Get
      Return (state_ = State.PostCoolVent)
    End Get
  End Property

  Public ReadOnly Property Output() As Integer
    Get
      'Analog Output
      Output = 0
      If (state_ = State.Heat) Then Output = pid_.Output
      If (state_ = State.Cool) OrElse (state_ = State.CrashCool) Then Output = -pid_.Output

      'Limit output
      If Output < 0 Then Output = 0
      If Output > 1000 Then Output = 1000
    End Get
  End Property
  Public ReadOnly Property IsCrashCoolOn() As Boolean
    Get
      Return (state_ = State.CrashCoolStart) OrElse (state_ = State.CrashCoolPause) OrElse (state_ = State.CrashCoolRestart) _
             OrElse IsCrashCoolVent OrElse IsCrashCooling OrElse IsCrashCoolDone
    End Get
  End Property
  Public ReadOnly Property IsCrashCoolDone() As Boolean
    Get
      Return (state_ = State.CrashCoolDone)
    End Get
  End Property
  Private ReadOnly Property IsCrashCoolVent() As Boolean
    Get
      Return state_ = State.CrashCoolVent
    End Get
  End Property
  Private ReadOnly Property IsCrashCooling() As Boolean
    Get
      Return (state_ = State.CrashCool) OrElse IsCrashCoolDone
    End Get
  End Property
  Public ReadOnly Property IsPidPaused() As Boolean
    Get
      Return pid_.IsPaused
    End Get
  End Property
  Public ReadOnly Property TempGradient() As Integer
    Get
      Return pid_.Gradient
    End Get
  End Property
  Public ReadOnly Property TempFinalTemp() As Integer
    Get
      Return pid_.FinalTemp
    End Get
  End Property
  Public ReadOnly Property TempSetpoint() As Integer
    Get
      Return pid_.Setpoint
    End Get
  End Property
  Public ReadOnly Property pidPropTerm() As Integer
    Get
      Return pid_.PropTerm
    End Get
  End Property
  Public ReadOnly Property pidIntegralTerm() As Integer
    Get
      Return pid_.IntegralTerm
    End Get
  End Property
  Public ReadOnly Property pidHeatLossTerm() As Integer
    Get
      Return pid_.HeatLossTerm
    End Get
  End Property
  Public ReadOnly Property pidGradientTerm() As Integer
    Get
      Return pid_.GradientTerm
    End Get
  End Property
#End Region

End Class

#End Region

