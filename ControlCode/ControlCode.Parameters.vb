'<Category("Type Of Parameter"), Description("Help For parameter")> Public Parameters_SetUpValue As Integer
Partial Class ControlCode

#Region "Add tank 1 control"
  <Parameter(0, 1000), Category("Add tank control"),
   Description("During a transfer of the add tank, after the rinse has completed and the tank is empty, it will continue transferring for this amount of time, in seconds.")>
  Public Parameters_AddTransferTimeAfterRinse As Integer
  <Parameter(0, 1000), Category("Add tank control"),
      Description("During a transfer of the add tank,after the tank level reads empty, it will continue transferring for this many seconds before starting the rinse.")>
  Public Parameters_AddTransferTimeBeforeRinse As Integer
  <Parameter(0, 1000), Category("Add tank control"),
      Description("During a transfer of the add tank, the top rinse to the machine will last for this many seconds.")>
  Public Parameters_AddRinseTime As Integer
  <Parameter(0, 1000), Category("Add tank control"),
Description("During a dose the dosing valve will stay open at least this long, measured in seconds.")>
  Public Parameters_DosingMinOpenTime As Integer
  <Parameter(0, 1000), Category("Add tank control"),
Description("High level during a rc command, measured in tenths.")>
  Public Parameters_RCHighLevel As Integer
  <Parameter(0, 1000), Category("Add tank control"),
Description("Low level during a rc command, measured in tenths.")>
  Public Parameters_RCLowLevel As Integer
#End Region

#Region "Analog input Calibration"
  <Parameter(0, 1000), Category("Analog input calibration"),
 Description("The value that the controller reads from the  level transmitter when the add tank is full.  In tenths of a percent.")>
  Public Parameters_AddTankLevelTransMax As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
   Description("The value that the controller reads from the  level transmitter when the add tank is empty.  In tenths of a percent.")>
  Public Parameters_AddTankLevelTransMin As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
   Description("The value that the controller reads from the level transmitter when the machine is full.")>
  Public Parameters_VesselLevelTransMax As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
  Description("The value that the controller reads from the level transmitter when the machine is empty.")>
  Public Parameters_VesselLevelTransMin As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
   Description("The value that the controller reads from the reel 1 inverter at full speed. In tenths of a percent")>
  Public Parameters_Reel1SpeedFeedbackMax As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
  Description("The value that the controller reads from the reel 1 inverter at min speed. In tenths of a percent")>
  Public Parameters_Reel1SpeedFeedbackMin As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
   Description("The value that the controller reads from the reel 2 inverter at full speed. In tenths of a percent")>
  Public Parameters_Reel2SpeedFeedbackMax As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
  Description("The value that the controller reads from the reel 2 inverter at min speed. In tenths of a percent")>
  Public Parameters_Reel2SpeedFeedbackMin As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
   Description("The value that the controller reads from the reel 3 inverter at full speed. In tenths of a percent")>
  Public Parameters_Reel3SpeedFeedbackMax As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
  Description("The value that the controller reads from the reel 3 inverter at min speed. In tenths of a percent")>
  Public Parameters_Reel3SpeedFeedbackMin As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
   Description("The value that the controller reads from the manual pump speed pot at full speed. In tenths of a percent")>
  Public Parameters_ManualPumpSpeedMax As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
  Description("The value that the controller reads from the manual pump speed pot at min speed. In tenths of a percent")>
  Public Parameters_ManualPumpSpeedMin As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
   Description("The value that the controller reads from the manual pump speed pot at full speed. In tenths of a percent")>
  Public Parameters_ManualReelSpeedYPMMax As Integer
  <Parameter(0, 1000), Category("Analog input calibration"),
  Description("The value that the controller reads from the manual pump speed pot at min speed. In tenths of a percent")>
  Public Parameters_ManualReelSpeedYpmMin As Integer
#End Region

#Region " Blend Control "

  <Parameter(0, 100), Category("Blend Control"), Description("If the blend fill temperature error from the setpoint is less than this, F, the valve doesn't adjust.")>
  Public Parameters_BlendDeadBand As Integer

  <Parameter(0, 100), Category("Blend Control"), Description("Determines blend adjustment amount during a fill or rinse.")>
  Public Parameters_BlendFactor As Integer

  <Parameter(0, 100), Category("Blend Control"), Description("Determines how quickly the blend valve adjusts during a fill or rinse, seconds.")>
  Public Parameters_BlendSettleTime As Integer

  <Parameter(0, 180), Category("Blend Control"), Description("Set to the temperature of the cold water supplied to the machine, in F.")>
  Public Parameters_ColdWaterTemperature As Integer

  <Parameter(0, 180), Category("Blend Control"), Description("Set to the temperature of the hot water supplied to the machine, in F.")>
  Public Parameters_HotWaterTemperature As Integer

#End Region

#Region "Motor setting"
  <Parameter(0, 100), Category("Motor Settings"),
    Description("Main pump motor horsepower.")>
  Public Parameters_MainPumpHP As Integer
  <Parameter(0, 100), Category("Motor Settings"),
      Description("Reel motor horsepower.")>
  Public Parameters_ReelHP As Integer
  <Parameter(0, 100), Category("Motor Settings"),
      Description("Add pump motor horsepower.")>
  Public Parameters_AddPumpHP As Integer
  <Parameter(0, 100), Category("Motor Settings"),
      Description("Add mixer horsepower.")>
  Public Parameters_AddMixerHP As Integer

#End Region

#Region "Pump Control"
  'pump control
  <Parameter(0, 1000), Category("Pump control"),
    Description("The machine level must be above Pump Minimum Level for this many seconds before the pump is allowed to start")>
  Public Parameters_PumpMinimumLevelTime As Integer
  <Parameter(0, 1000), Category("Pump control"),
    Description("The machine level must be above this level for Pump Minimum Level Time seconds before the pump is allowed to start")>
  Public Parameters_PumpMinimumLevel As Integer
  <Parameter(0, 1000), Category("Pump control"),
    Description("When turning pump off automatically, this is the time to delay before turning it off.")>
  Public Parameters_PumpOffDelayTime As Integer
  <Parameter(0, 1000), Category("Pump control"),
  Description("The default pump speed in tenths of a percent.")>
  Public Parameters_PumpSpeedDefault As Integer

#End Region

#Region "Production reports"
  <Parameter(0, 2000), Category("Production reports"),
  Description("The standard time for the operator.")>
  Public Parameters_StandardOperatorTime As Integer

  <Parameter(0, 60), Category("Production reports"),
      Description("The standard time for the  machine to AD command. In minutes.")>
  Public Parameters_StandardADTime As Integer
  <Parameter(0, 60), Category("Production reports"),
      Description("The standard time for the  machine to AT command. In minutes.")>
  Public Parameters_StandardATTime As Integer
  <Parameter(0, 60), Category("Production reports"),
      Description("The standard time for the  machine to RP command. In minutes.")>
  Public Parameters_StandardRinsePulseTime As Integer
#End Region

#Region "Reel control"
  'reel control
  <Parameter(0, 1000), Category("Reel control"),
    Description("Time to delay before turning reels on after reels requested.")>
  Public Parameters_ReelOnDelayTime As Integer
  'reel control
  <Parameter(0, 600), Category("Reel control"),
    Description("Default reel speed in ypm.")>
  Public Parameters_ReelSpeedDefault As Integer
  <Parameter(0, 600), Category("Reel control"),
Description("Reel speed at 100% inerter output.")>
  Public Parameters_ReelSpeedMaximumYPM As Integer

#End Region

#Region "Setup"
  'SETUP SECTION

  <Parameter(0, 10000), Category("Setup"),
    Description("Resets all parameters to default value if magic value is entered")>
  Public Parameters_InitializeParameters As Integer
  <Parameter(1000, 10000), Category("Setup"),
    Description("Communications timout to turn off all outputs.  Value in seconds.")>
  Public Parameters_WatchdogTimeout As Integer
  <Parameter(1, 10000), Category("Setup"),
    Description("Time, in seconds, to wait after communication lost before signalling and turning off all outputs")>
  Public Parameters_PLCComsTime As Integer
  <Parameter(0, 1), Category("Setup"),
    Description("Set to '1' to disregard communications timeout from network loss.")>
  Public Parameters_PLCComsLossDisregard As Integer

#End Region

#Region "Volume Calibration"
  <Parameter(0, 1000), Category("Volume Calibration"),
  Description("The k factor for the water flowmeter.")>
  Public Parameters_WaterGallonsPerPulse As Integer
  <Parameter(0, 1000), Category("Volume Calibration"),
  Description("The number of seconds allowed between each flowmeter pulse before an alarm is raised")>
  Public Parameters_FillFlowmeterAlarmTime As Integer
#End Region

#Region " Safety "
  <Parameter(1750, 2000), Category("Safety"),
    Description("Temp, in tenths, to pressurize the machine at.")>
  Public Parameters_PressurizationTemperature As Integer
  <Parameter(15, 60), Category("Safety"),
    Description("Time, in seconds, to pressurize the machine.")>
  Public Parameters_PressurizationTime As Integer
  <Parameter(1700, 1950), Category("Safety"),
    Description("Temp, in tenths, to de-pressurize the machine.")>
  Public Parameters_DepressurizationTemperature As Integer
  <Parameter(30, 60), Category("Safety"),
    Description("Time, in seconds, it takes to de-pressurize the machine.")>
  Public Parameters_DepressurizationTime As Integer


#End Region

#Region "Tangle control"
  'tangle control
  <Parameter(0, 1000), Category("Tangle control"),
    Description("Tangle detection starts this many seconds after the reel starts.")>
  Public Parameters_TangleDelayTime As Integer


#End Region

#Region "Setup Parameters"

  Private Sub Parameters_MaybeSetDefaults()
    If (Parameters_InitializeParameters = 9387) Or
       (Parameters_InitializeParameters = 8741) Or
       (Parameters_InitializeParameters = 8742) Or
       (Parameters_InitializeParameters = 8743) Or
       (Parameters_InitializeParameters = 8744) Then
      Parameters_SetDefaults()
    End If
  End Sub

  Private Sub Parameters_SetDefaults()




    With TemperatureControl
      .Parameters_BalPercent = 750
      .Parameters_BalancePercent = 0
      .Parameters_BalanceTemperature = 1000
      .Parameters_CoolIntegral = 250
      .Parameters_CoolMaxGradient = 100
      .Parameters_CoolPropBand = 120
      .Parameters_CoolStepMargin = 20
      .Parameters_CoolVentTime = 20
      .Parameters_HeatCoolModeChange = 0
      .Parameters_HeatExitDeadband = 20
      .Parameters_HeatIntegral = 200
      .Parameters_HeatMaxGradient = 100
      .Parameters_HeatPropBand = 120
      .Parameters_HeatStepMargin = 20
      .Parameters_HeatVentTime = 20
      .Parameters_TemperatureAlarmBand = 90
      .Parameters_TemperatureAlarmDelay = 120
      .Parameters_TemperaturePidPause = 100
      .Parameters_TemperaturePidReset = 250
      .Parameters_TemperaturePidRestart = 50
    End With


    Parameters_InitializeParameters = 0
  End Sub
#End Region
End Class
