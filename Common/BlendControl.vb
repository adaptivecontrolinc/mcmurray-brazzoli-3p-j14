Public Class BlendControl : Inherits MarshalByRefObject

  ' Control parameters
  Property Deadband() As Integer
  Property Factor() As Integer
  Property SettleTime As Integer
  Property AdjustmentMin() As Integer
  Property AdjustmentMax() As Integer
  Property ColdWaterTemp() As Integer
  Property HotWaterTemp() As Integer

  Property SettleTimer As New Timer

  Property TargetTemperature As Integer
  Property CurrentTemperature As Integer

  Property Delta As Integer
  Property Adjustment As Integer
  Property Output() As Integer

  Public Sub Parameters(ByVal deadband As Integer, ByVal factor As Integer, settleTime As Integer, ByVal coldWaterTemp As Integer, ByVal hotWaterTemp As Integer)
    Parameters(deadband, factor, settleTime, coldWaterTemp, hotWaterTemp, 5, 100)
  End Sub

  Public Sub Parameters(ByVal factor As Integer, ByVal deadband As Integer, settleTime As Integer, ByVal coldWaterTemp As Integer, ByVal hotWaterTemp As Integer, ByVal adjustmentMin As Integer, ByVal adjustmentMax As Integer)
    Me.Factor = MinMax(factor, 0, 100)
    Me.Deadband = MinMax(deadband, 0, 100)
    Me.SettleTime = MinMax(settleTime, 0, 8)
    Me.ColdWaterTemp = MinMax(coldWaterTemp, 400, 1200)
    Me.HotWaterTemp = MinMax(hotWaterTemp, 400, 1800)
    Me.AdjustmentMin = MinMax(adjustmentMin, 0, 100)
    Me.AdjustmentMax = MinMax(adjustmentMax, 0, 500)
  End Sub

  Public Sub Start(ByVal target As Integer)
    ' Set target temp
    Me.TargetTemperature = target

    ' Start settle timer
    SettleTimer.Seconds = SettleTime

    ' Calculate initial output
    Dim offset As Integer, range As Integer
    offset = TargetTemperature - ColdWaterTemp
    range = HotWaterTemp - ColdWaterTemp
    If range = 0 Then
      Output = 500
    Else
      Output = (offset * 1000) \ range
    End If
  End Sub

  Public Sub Run(ByVal current As Integer)
    ' Calculate Error
    Me.CurrentTemperature = current
    Me.Delta = TargetTemperature - CurrentTemperature

    ' Adjust output when timer has expired
    If SettleTimer.Finished Then
      SettleTimer.Seconds = SettleTime
      If Math.Abs(Delta) > Deadband Then
        Adjustment = (Delta * Factor) \ 1000
        If Adjustment > 0 Then Adjustment = MinMax(Adjustment, AdjustmentMin, AdjustmentMax)
        If Adjustment < 0 Then Adjustment = MinMax(Adjustment, -AdjustmentMax, -AdjustmentMin)
        Output = MinMax(Output + Adjustment, 0, 1000)
      End If
    End If
  End Sub


  ReadOnly Property IOOutput As Integer
    Get
      Return MinMax(Output, 0, 1000)
    End Get
  End Property

  ReadOnly Property IOBlendCold As Boolean
    Get
      If TargetTemperature <= ColdWaterTemp Then Return True
      If TargetTemperature >= HotWaterTemp Then Return False
      Return (IOOutput < 750)
    End Get
  End Property

  ReadOnly Property IOBlendHot As Boolean
    Get
      If TargetTemperature <= ColdWaterTemp Then Return False
      If TargetTemperature >= HotWaterTemp Then Return True
      Return (IOOutput > 250)
    End Get
  End Property

End Class
