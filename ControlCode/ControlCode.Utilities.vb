Partial Class ControlCode
#Region "Setup Utility Calculations"
  Private Sub CalculateUtilities()
    'Utility usage calculations

    Dim HPNow As Integer
    'PowerFactor = (750 * 68) / 100
    HPNow = 0 'Reset power being used now uasge to 0
    Static UtilitiesTimer As New Timer
    If UtilitiesTimer.Finished Then
      MainPumpHP = Parameters_MainPumpHP
      ReelHP = Parameters_ReelHP
      AddPumpHP = Parameters_AddPumpHP
      AddMixerHP = Parameters_AddMixerHP

      If IO.PumpRunning Then HPNow += MainPumpHP
      If IO.Reel1Running Then HPNow += ReelHP
      If IO.Reel2Running Then HPNow += ReelHP
      If IO.Reel3Running Then HPNow += ReelHP
      If IO.AddPumpRunning Then HPNow += AddPumpHP
      If IO.AddMixerRunning Then HPNow += AddPumpHP

      'Assume all motors run at about 68% of rated capacity. 750W is equiv to HP.
      'Power factor = 68% of 750 , convert to watts
      'PowerFactor = 510 'watts which is .51kw
      PowerKWS += Convert.ToInt32(HPNow * 0.51)

      UtilitiesTimer.Seconds = 1
    End If
    If PowerKWS >= 3600 Then
      PowerKWHrs += 1
      PowerKWS -= 3600
    End If

    'Steam Consumption formula
    '120% * WorkingVolume * deg F of temp rise * weight of water (8.33lb/g) * 0.001 = lbs of steam used
    'Steam factor = 120% * 8.33 * 0.001 = 1/100
    'lbs of Steam used = working volume * Temp rise in F / 100
    FinalTemp = TemperatureControl.TempFinalTemp
    If FinalTemp <> FinalTempWas Then
      If VesTemp > FinalTempWas Then
        StartTemp = VesTemp
      Else
        StartTemp = FinalTempWas
      End If
      If FinalTemp > StartTemp Then
        TempRise = FinalTemp - StartTemp
        SteamNeeded = (VesVolume * TempRise) \ 1000
        SteamUsed += SteamNeeded
      End If
      FinalTempWas = FinalTemp
    End If
  End Sub
#End Region

End Class
