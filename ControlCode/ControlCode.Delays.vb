Partial Class ControlCode
#Region "Setup Delays"
  Private Function GetDelay() As DelayValue

    If BO.IsOn Then Return DelayValue.Boilout

    If BR.IsOn Then Return DelayValue.Reproccess

    If Parent.IsPaused Then Return DelayValue.Paused

    If Reel1Tangled OrElse Reel2Tangled OrElse Reel3Tangled Then Return DelayValue.Tangle

    If AP.IsOverrun Then Return DelayValue.AddPreparation

    If DR.IsOverrun OrElse PD.IsOverrun Then Return DelayValue.Drain

    If FI.IsOverrun Then Return DelayValue.Fill

    If RI.IsOverrun OrElse RP.IsOverrun Then Return DelayValue.Rinseing

    If LD.IsOverrun Then Return DelayValue.Load

    If UL.IsOverrun Then Return DelayValue.Unload

    If SA.IsOverrun OrElse BS.IsOverrun Then Return DelayValue.Sample


    If HE.IsOverrun Then Return DelayValue.Heating

    If CO.IsOverrun Then Return DelayValue.Cooling

    If TP.IsOverrun Then
      If TemperatureControl.TempFinalTemp < VesTemp Then
        Return DelayValue.Cooling
      Else
        Return DelayValue.Heating
      End If
    End If


    If Parent.IsAlarmActive Then Return DelayValue.Machine

    Return DelayValue.NormalRunning
  End Function

#End Region


End Class
