Partial Class ControlCode
#Region "DrawScreen"
  '/ doesn't round off the tenth while \ does => used to display strings on drawscreen
  <ScreenButton("Main", 1, ButtonImage.Vessel),
  ScreenButton("Speed", 2, ButtonImage.Information),
  ScreenButton("Temp", 3, ButtonImage.Thermometer),
   ScreenButton("Add", 4, ButtonImage.SideVessel),
      ScreenButton("Data", 5, ButtonImage.SideVessel)>
  Public Sub DrawScreen(ByVal screen As Integer, ByVal row() As String) Implements ACControlCode.DrawScreen
    If BatchWeight <> 0 Then GallonsPerPound = (WaterUsed * 10) \ BatchWeight

    Dim TextRow7 As String
    TextRow7 = ""
    If AP.IsActive Then
      If (AP.PreparePrompt >= 1) Or (AP.PreparePrompt <= 99) Then
        TextRow7 = Parent.Message(AP.PreparePrompt)
      End If
    End If
    row(7) = TextRow7

    Select Case screen

      Case 1 'MAIN
        row(1) = "Machine Level " & (VesLevel / 10).ToString.PadLeft(4) & "%"
        row(2) = "Machine Volume " & VesVolume.ToString & "Gals"
        Dim PressureStateText As String
        PressureStateText = "Pressurized"
        If SafetyControl.IOVent Then PressureStateText = "Depressurizing"
        If SafetyControl.IsDepressurized Then PressureStateText = "Depressurized"
        row(3) = PressureStateText

      Case 2
        row(1) = "Pump Speed " & (IO.PumpSpeedOutput / 10).ToString.PadLeft(3) & "%"

        row(2) = "Reel 1 Speed " & Reel1SpeedFeedback & "/" & Reel1Ypm & "YPM"
        row(3) = "Reel 2 Speed " & Reel2SpeedFeedback & "/" & Reel2Ypm & "YPM"
        row(4) = "Reel 3 Speed " & Reel3SpeedFeedback & "/" & Reel3Ypm & "YPM"


      Case 3  'temp
        row(1) = "Gradient " & (TemperatureControl.TempGradient / 10).ToString.PadLeft(4) & "F/m"
        row(2) = "Final Temp " & (TemperatureControl.TempFinalTemp / 10).ToString.PadLeft(3) & "F"
        row(3) = "Setpoint " & (TemperatureControl.TempSetpoint / 10).ToString.PadLeft(5) & "F"
        If TemperatureControl.IsHeating Then
          row(4) = "Valve Output " & (IO.HeatCoolOutput / 10).ToString("#0.00") & "%"
        End If
        If TemperatureControl.IsCooling Then
          row(4) = "Valve Output " & (IO.HeatCoolOutput / 10).ToString("#0.00") & "%"
        End If

        Dim StateText3 As String
        StateText3 = ""
        If TemperatureControl.IsHeating Then StateText3 = "Heating"
        If TemperatureControl.IsCooling Then StateText3 = "Cooling"
        If TemperatureControl.IsPreHeatVent Then StateText3 = "Vent before heat"
        If TemperatureControl.IsPostHeatVent Then StateText3 = "Vent after heat"
        If TemperatureControl.IsPreCoolVent Then StateText3 = "Vent before cool"
        If TemperatureControl.IsPostCoolVent Then StateText3 = "Vent after cool"
        row(5) = StateText3


      Case 4
        row(1) = "Add Tank 1"
        row(2) = "Level " & (AddLevel / 10).ToString.PadLeft(3) & "%"
        row(3) = "Temp:" & (IO.AddTemp / 10) & "F"
        If AF.IsOn Then
          row(3) = AF.StateString
        ElseIf rc.IsOn Then
          row(3) = RC.StateString
        ElseIf ap.IsOn Then
          row(3) = AP.StateString
        End If

        If AT.IsOn Then
          row(4) = AT.StateString
        ElseIf AD.IsOn Then
          row(4) = AD.StateString
          row(5) = "Actual level:" & (AddLevel / 10).ToString.PadLeft(3) & "%"
          row(6) = "Desired level:" & (AD.DesiredLevel / 10).ToString.PadLeft(3) & "%"
        End If

      Case 5
        Dim Page8Text2 As String
        Dim Page8Text3 As String
        Dim Page8Text6 As String
        If Parent.IsProgramRunning Then
          Page8Text2 = "Cycle Time " & TimerString(CycleTime)
          Page8Text3 = "Water Used " & WaterUsed.ToString & "Gals"
        Else
          Page8Text2 = "Cycle Time " & TimerString(LastProgramCycleTime)
          Page8Text3 = "Water Used " & WaterUsed.ToString & "Gals"
        End If
        row(1) = Page8Text2
        row(2) = Page8Text3
        row(3) = "Liquor Ratio: " & (LiquorRatio / 10).ToString.PadLeft(4) & ":1"
        row(4) = "Batch Weight: " & BatchWeight.ToString & "lbs"
        row(5) = "Gallons per lb: " & (GallonsPerPound / 10).ToString.PadLeft(4)
        row(6) = Page8Text6

    End Select
  End Sub
#End Region


End Class
