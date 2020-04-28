
Public Class IO : Inherits MarshalByRefObject
  Public Plc1 As Ports.Modbus
  Public PLC1Timer As New Timer
  Public PLC1Fault As Boolean
  Public Plc1A As Ports.Modbus
  Public PLC1ATimer As New Timer
  Public PLC1AFault As Boolean

  Public Plc2 As Ports.Modbus
  Public PLC2Timer As New Timer
  Public PLC2Fault As Boolean

  Public WatchDogTimeout As Short
  Public IOScanDone As Boolean


#Region " DIGITAL INPUTS: 1-64 "

  ' CARD #1
  <IO(IOType.Dinp, 1), Description("Run Pushbutton")> Public RemoteRun As Boolean
  <IO(IOType.Dinp, 2), Description("Halt Pushbutton")> Public RemoteHalt As Boolean
  <IO(IOType.Dinp, 3), Description("Yes Pushbutton")> Public RemoteYes As Boolean
  <IO(IOType.Dinp, 4), Description("No Pushbutton")> Public RemoteNo As Boolean
  <IO(IOType.Dinp, 5), Description("Crash cool Pushbutton")> Public CrashCoolPushbutton As Boolean
  <IO(IOType.Dinp, 6), Description("Emergency Stop Pushbutton")> Public EmergencyStop As Boolean
  <IO(IOType.Dinp, 7), Description("Power reset pushbutton ")> Public PowerResetPB As Boolean
  <IO(IOType.Dinp, 8), Description("Temperature below 190F")> Public TempBelow190F As Boolean
  <IO(IOType.Dinp, 9), Description("Temperature below 280F")> Public TempBelow280F As Boolean
  <IO(IOType.Dinp, 10), Description("pressure safe")> Public PressureSafeSw As Boolean
  <IO(IOType.Dinp, 11), Description("Flow meter pulse")> Public FlowMeterPulse As Boolean
  <IO(IOType.Dinp, 12), Description("Main pump running")> Public PumpRunning As Boolean
  <IO(IOType.Dinp, 13), Description("Pump in automatic switch")> Public PumpInAutoSw As Boolean
  <IO(IOType.Dinp, 14), Description("manual pump speed switch")> Public ManualPumpSpeedSwitch As Boolean
  <IO(IOType.Dinp, 15), Description("Reel forward Switch")> Public ReelForwardSwitch As Boolean
  <IO(IOType.Dinp, 16), Description("Reel forward Switch")> Public ReelReverseSwitch As Boolean

  ' CARD #2
  <IO(IOType.Dinp, 17), Description("Reel manual speed switch")> Public ManualReelSpeedSwitch As Boolean
  <IO(IOType.Dinp, 18), Description("Reel 1 running")> Public Reel1Running As Boolean
  <IO(IOType.Dinp, 19), Description("Tangle 1")> Public Tangle1Switch As Boolean
  <IO(IOType.Dinp, 20), Description("Reel 2 running")> Public Reel2Running As Boolean
  <IO(IOType.Dinp, 21), Description("Tangle 2")> Public Tangle2Switch As Boolean
  <IO(IOType.Dinp, 22), Description("Reel 3 running")> Public Reel3Running As Boolean
  <IO(IOType.Dinp, 23), Description("Tangle 3")> Public Tangle3Switch As Boolean
  <IO(IOType.Dinp, 24), Description("Add pump running")> Public AddPumpRunning As Boolean
  <IO(IOType.Dinp, 25), Description("Add mixer running")> Public AddMixerRunning As Boolean
  <IO(IOType.Dinp, 26), Description("Add prepare pushbutton")> Public AddReadyPb As Boolean
  <IO(IOType.Dinp, 27), Description("Add fill switch")> Public AddFillSw As Boolean
  <IO(IOType.Dinp, 28), Description("Add runback switch")> Public AddRunbackSw As Boolean
  <IO(IOType.Dinp, 29), Description("Add transfer switch")> Public AddTransferSw As Boolean
  <IO(IOType.Dinp, 30), Description("Add mixer switch")> Public AddMixerSw As Boolean
  <IO(IOType.Dinp, 31), Description("Add heat switch")> Public AddHeatSw As Boolean
  <IO(IOType.Dinp, 32), Description("Add drain switch")> Public AddDrainSw As Boolean


  ' CARD #3
  <IO(IOType.Dinp, 33), Description("Manual fill pushbutton")> Public ManualFillPB As Boolean
  <IO(IOType.Dinp, 34), Description("Manual drain pushbutton")> Public ManualDrainPB As Boolean
  <IO(IOType.Dinp, 35), Description("")> Public Dinp35 As Boolean
  <IO(IOType.Dinp, 36), Description("")> Public Dinp36 As Boolean
  <IO(IOType.Dinp, 37), Description("")> Public Dinp37 As Boolean
  <IO(IOType.Dinp, 38), Description("")> Public Dinp38 As Boolean
  <IO(IOType.Dinp, 39), Description("")> Public Dinp39 As Boolean
  <IO(IOType.Dinp, 40), Description("")> Public Dinp40 As Boolean
  <IO(IOType.Dinp, 41), Description("")> Public Dinp41 As Boolean
  <IO(IOType.Dinp, 42), Description("")> Public Dinp42 As Boolean
  <IO(IOType.Dinp, 43), Description("")> Public Dinp43 As Boolean
  <IO(IOType.Dinp, 44), Description("")> Public Dinp44 As Boolean
  <IO(IOType.Dinp, 45), Description("")> Public Dinp45 As Boolean
  <IO(IOType.Dinp, 46), Description("")> Public Dinp46 As Boolean
  <IO(IOType.Dinp, 47), Description("")> Public Dinp47 As Boolean
  <IO(IOType.Dinp, 48), Description("Ups in battery mode input")> Public UpsInBatteryMode As Boolean


#End Region

#Region " ANALOG INPUTS "

  'F2-08AD-1 #1
  <IO(IOType.Aninp, 1), Description("Vessel Level 4-20ma")> Public VesselLevelInput As Short
  <IO(IOType.Aninp, 2), Description("Add Tank Level  4-20ma")> Public AddLevelInput As Short

  <IO(IOType.Aninp, 9), Description("main pump speed pot 0-10vdc")> Public ManualPumpSpeedInput As Short
  <IO(IOType.Aninp, 10), Description("reel speed pot 0-10vdc")> Public ManualReelSpeedInput As Short
  <IO(IOType.Aninp, 11), Description("Reel 1 speed feedback 0-10vdc")> Public Reel1SpeedFeedbackInput As Short
  <IO(IOType.Aninp, 12), Description("Reel 1 speed feedback 0-10vdc")> Public Reel2SpeedFeedbackInput As Short
  <IO(IOType.Aninp, 13), Description("Reel 1 speed feedback 0-10vdc")> Public Reel3SpeedFeedbackInput As Short


  ' Temperatures aninp 8-12
  <IO(IOType.Temp, 1), Description("Machine temp from probe")> Public HeaderTemp As Short
  <IO(IOType.Temp, 2), Description("Machine temp from probe")> Public BackupTemp As Short
  <IO(IOType.Temp, 3), Description("Blend temp from probe")> Public BlendFillTemp As Short
  <IO(IOType.Temp, 4), Description("Blend temp from probe")> Public AddTemp As Short


#End Region

#Region " DIGITAL OUTPUTS: 1 to 60 "

  ' CARD #1
  <IO(IOType.Dout, 1, Override.Allow), Description("Alarm lamp on stacklight(red)")> Public AlarmLamp As Boolean
  <IO(IOType.Dout, 2, Override.Allow), Description("Operator call lamp on stacklight(yellow)")> Public SignalLamp As Boolean
  <IO(IOType.Dout, 3, Override.Allow), Description("Delay lamp on stacklight(blue)")> Public DelayLamp As Boolean
  <IO(IOType.Dout, 4, Override.Allow), Description("Operator call siren on stacklight")> Public Siren As Boolean
  <IO(IOType.Dout, 5, Override.Allow), Description("Crash cool pushbutton")> Public CrashCoolPushbuttonLamp As Boolean
  <IO(IOType.Dout, 6, Override.Allow), Description("Illuminated Machine Safe Lamp")> Public MachineSafeLamp As Boolean
  <IO(IOType.Dout, 7, Override.Allow), Description("Vent 108")> Public Vent As Boolean
  <IO(IOType.Dout, 8, Override.Allow), Description("Fill 103")> Public Fill As Boolean
  <IO(IOType.Dout, 9, Override.Allow), Description("Drain 111")> Public Drain As Boolean
  <IO(IOType.Dout, 10, Override.Allow), Description("Drain pressure")> Public PumpToDrain As Boolean
  <IO(IOType.Dout, 11, Override.Allow), Description("Drain probe tank 147")> Public VesselDrain As Boolean
  <IO(IOType.Dout, 12, Override.Allow), Description("Over flow")> Public OverFlow As Boolean

  ' CARD #2
  <IO(IOType.Dout, 13, Override.Allow), Description("Continuos wash 134")> Public ContinuosWash As Boolean
  <IO(IOType.Dout, 14, Override.Allow), Description("Spray 176")> Public Spray As Boolean
  <IO(IOType.Dout, 15, Override.Allow), Description("Steam select")> Public SteamSelect As Boolean
  <IO(IOType.Dout, 16, Override.Allow), Description("Condensate")> Public Condensate As Boolean
  <IO(IOType.Dout, 17, Override.Allow), Description("Cool select")> Public CoolSelect As Boolean
  <IO(IOType.Dout, 18, Override.Allow), Description("Cool water return")> Public CoolWaterReturn As Boolean
  <IO(IOType.Dout, 19, Override.Allow), Description("Heat Exchanger drain")> Public HXDrain As Boolean
  <IO(IOType.Dout, 20, Override.Allow), Description("Main pump start")> Public MainPumpStart As Boolean
  <IO(IOType.Dout, 21, Override.Allow), Description("Reel 1 forward")> Public Reel1Forward As Boolean
  <IO(IOType.Dout, 22, Override.Allow), Description("Reel 1 reverse")> Public Reel1Reverse As Boolean
  <IO(IOType.Dout, 23, Override.Allow), Description("Reel 2 forward")> Public Reel2Forward As Boolean
  <IO(IOType.Dout, 24, Override.Allow), Description("Reel 2 reverse")> Public Reel2Reverse As Boolean

  ' CARD #3
  <IO(IOType.Dout, 25, Override.Allow), Description("Reel 3 forward")> Public Reel3Forward As Boolean
  <IO(IOType.Dout, 26, Override.Allow), Description("Reel 3 reverse")> Public Reel3Reverse As Boolean
  <IO(IOType.Dout, 27, Override.Allow), Description("Add pump start")> Public AddPumpStart As Boolean
  <IO(IOType.Dout, 28, Override.Allow), Description("Add mixer start")> Public AddMixerStart As Boolean
  <IO(IOType.Dout, 29, Override.Allow), Description("Add prepare lamp")> Public AddPrepareLamp As Boolean
  <IO(IOType.Dout, 30, Override.Allow), Description("Add transfer")> Public AddTransfer As Boolean
  <IO(IOType.Dout, 31, Override.Allow), Description("Add runback")> Public AddRunback As Boolean
  <IO(IOType.Dout, 32, Override.Allow), Description("Add rinse")> Public AddRinse As Boolean
  <IO(IOType.Dout, 33, Override.Allow), Description("Add heat")> Public AddHeat As Boolean
  <IO(IOType.Dout, 34, Override.Allow), Description("Add drain")> Public AddDrain As Boolean
  <IO(IOType.Dout, 35, Override.Allow), Description("Fill pushbutton lamp")> Public FillPushbuttonLamp As Boolean
  <IO(IOType.Dout, 36, Override.Allow), Description("Drain pushbutton lamp")> Public DrainPushbuttonlamp As Boolean



#End Region

#Region " ANALOG OUTPUTS: 1 to 8"

  <IO(IOType.Anout, 1, Override.Allow), Description("heat Cool output(4-20ma)")> Public HeatCoolOutput As Short
  <IO(IOType.Anout, 2, Override.Allow), Description("Blend fill output(4-20ma)")> Public BlendFillOutput As Short
  <IO(IOType.Anout, 3, Override.Allow), Description("Fill output(4-20ma)")> Public FillOutput As Short
  <IO(IOType.Anout, 4, Override.Allow), Description("Pump speed output(4-20ma)")> Public PumpSpeedOutput As Short
  <IO(IOType.Anout, 5, Override.Allow), Description("Reel 1 speed output(4-20ma)")> Public Reel1SpeedOutput As Short
  <IO(IOType.Anout, 6, Override.Allow), Description("Reel 2 speed output(4-20ma)")> Public Reel2SpeedOutput As Short
  <IO(IOType.Anout, 7, Override.Allow), Description("Reel 3 speed output(4-20ma)")> Public Reel3SpeedOutput As Short
  <IO(IOType.Anout, 8, Override.Allow), Description("Spare")> Public SpareAnout8Output As Short


#End Region


  'Raw analog/temperature inputs (before scaling or smoothing) - for calibration work
  Public AninpRaw(16) As Short

  Public Sub New(ByVal controlCode As ControlCode)
    'Setup Communication Ports 
    '>> Use this method so that if the xml file doesn't declare the port, no "is nothing" exceptions will be thrown <<
    Dim port As String = controlCode.Parent.Setting("Plc1")
    If Not String.IsNullOrEmpty(port) Then
      Try
        Plc1 = New Ports.Modbus(New Ports.ModbusTcp(port, 502))
        Plc1A = New Ports.Modbus(New Ports.ModbusTcp(port, 502))
      Catch : End Try
    End If

    Dim port2 As String = controlCode.Parent.Setting("Plc2")
    If Not String.IsNullOrEmpty(port2) Then
      Try
        Plc2 = New Ports.Modbus(New Ports.ModbusTcp(port2, 502))
      Catch : End Try
    End If

    'Reset Communication Port Alarm Timeout
    PLC1Timer.Seconds = controlCode.Parameters_PLCComsTime
    PLC2Timer.Seconds = controlCode.Parameters_PLCComsTime

  End Sub

  Public Function ReadInputs(ByVal parent As ACParent, ByVal dinp() As Boolean, ByVal aninp() As Short, ByVal temp() As Short, ByVal controlCode As ControlCode) As Boolean
    Dim i As Integer

    If Plc1 IsNot Nothing Then
      Dim DinpMain(48) As Boolean
      Select Case Plc1.Read(1, 10001, DinpMain)
        Case Ports.Modbus.Result.Fault
          PLC1Fault = True
        Case Ports.Modbus.Result.OK
          PLC1Timer.Seconds = controlCode.Parameters_PLCComsTime
          ReadInputs = True

          'An array to configure the hardware inputs so that they appear in order in the control code IO Screen
          For i = 1 To 48 : dinp(i) = DinpMain(i) : Next i
          PLC1Fault = False
          IOScanDone = True
      End Select

      Dim valueSetAnalogInputs(16) As Short
      Select Case Plc1A.Read(1, 30001, valueSetAnalogInputs)
        Case Ports.Modbus.Result.HwFault
          PLC1AFault = True
        Case Ports.Modbus.Result.Fault
          PLC1AFault = True

        Case Ports.Modbus.Result.OK
          ' Reset timer
          PLC1ATimer.Seconds = MinMax(controlCode.Parameters_PLCComsTime, 10, 100000)

          PLC1AFault = False
          PLC1ATimer.Seconds = controlCode.Parameters_PLCComsTime

          ' 16 analog inputs 
          For i = 1 To 16
            AninpRaw(i) = valueSetAnalogInputs(i)
          Next i


          'Use Raw Values

          ' Update the Control I/O
          aninp(1) = ReScale(AninpRaw(1), 0, 4095, 0, 1000)
          aninp(2) = ReScale(AninpRaw(2), 0, 4095, 0, 1000)
          aninp(9) = ReScale(AninpRaw(9), 0, 4095, 0, 1000)
          aninp(10) = ReScale(AninpRaw(10), 0, 4095, 0, 1000)
          aninp(11) = ReScale(AninpRaw(11), 0, 4095, 0, 1000)
          aninp(12) = ReScale(AninpRaw(12), 0, 4095, 0, 1000)
          aninp(13) = ReScale(AninpRaw(13), 0, 4095, 0, 1000)

          ' Rescale the I/O for the Temperatures
          temp(1) = ReScale(AninpRaw(3), 0, 4095, 0, 5000)
          temp(2) = ReScale(AninpRaw(4), 0, 4095, 0, 5000)
          temp(3) = ReScale(AninpRaw(5), 0, 4095, 0, 5000)
          temp(4) = ReScale(AninpRaw(6), 0, 4095, 0, 5000)
      End Select

    Else
      IOScanDone = False
    End If


  End Function

  Public Sub WriteOutputs(ByVal dout() As Boolean, ByVal anout() As Short, ByVal controlCode As ControlCode)
    Dim i As Integer
    'If the system is stopping or watchdog has timed out then turn all outputs off.
    If controlCode.SystemShuttingDown Then
      For i = 1 To 36
        dout(i) = False
      Next

      For i = 1 To 8
        anout(i) = 0
      Next
    End If

    If Plc2 IsNot Nothing Then
      'PLC1 : Main Control Panel
      Dim DoutPLC2(46) As Boolean
      ' Outputs are grouped by 12 but first 6 outputs are y0 -> y5 in plc
      ' second set of 6 are Y10-->y15 octal based numbers
      For i = 1 To 6 : DoutPLC2(i) = dout(i) : Next i                     'Card 1: group A (1-6)
      For i = 9 To 14 : DoutPLC2(i) = dout(i - 2) : Next i                'Card 1: group B (7-12)

      For i = 17 To 22 : DoutPLC2(i) = dout(i - 4) : Next i               'Card 2: group A (13-18)
      For i = 25 To 30 : DoutPLC2(i) = dout(i - 6) : Next i               'Card 2: group B (19-24)

      For i = 33 To 38 : DoutPLC2(i) = dout(i - 8) : Next i               'Card 3: group A (25-30)
      For i = 41 To 46 : DoutPLC2(i) = dout(i - 10) : Next i              'Card 3: group B (30-36)

      'Write the Digital Outputs
      Select Case Plc2.Write(1, 42001, DoutPLC2, Ports.WriteMode.Optimised)
        ' Case Ports.Modbus.Result.Fault, Ports.Modbus.Result.HwFault
        '   PLC1Fault = True
        Case Ports.Modbus.Result.OK
          PLC2Timer.Seconds = controlCode.Parameters_PLCComsTime
      End Select


      'Write the analog output
      Dim TempAnout(8) As Short
      For i = 1 To 8
        TempAnout(i) = CType(Math.Min(MulDiv(anout(i), 4095, 1000), Short.MaxValue), Short)
      Next
      Select Case Plc2.Write(1, 40001, TempAnout, Ports.WriteMode.Optimised)
        Case Ports.Modbus.Result.Fault
        Case Ports.Modbus.Result.OK
      End Select



      Dim WatchdogTimeout(1) As Short
      WatchdogTimeout(1) = CType(Math.Max(Math.Min((controlCode.Parameters_WatchdogTimeout * 1000), 10000), 1000), Short)
      Plc2.Write(1, 410007, WatchdogTimeout, Ports.WriteMode.Always)

      ' To Get the Watchdog to work correctly, i must send a value > 0 to the 50007 register only once successfully, then no more
      ' then the 410007 register works as a watchdog
      Static resetWatchdog As Boolean = False
      Dim resetWatchdogValue(1) As Short
      resetWatchdogValue(1) = CType(Math.Max(Math.Min(0, 10000), 1000), Short)
      If Not resetWatchdog Then
        Select Case Plc2.Write(1, 50007, resetWatchdogValue, Ports.WriteMode.Always)
          Case Ports.Modbus.Result.Fault
          Case Ports.Modbus.Result.OK
            resetWatchdog = True
        End Select
      End If
    End If







  End Sub

  
End Class
