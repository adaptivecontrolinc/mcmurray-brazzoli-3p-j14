Public Class Mimic
  Public ControlCode As ControlCode


  Public Sub New()
    DoubleBuffered = True  ' no flicker 
    InitializeComponent()
  End Sub

  Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles Timer1.Tick
    Try


      If ControlCode Is Nothing Then Exit Sub
      If Runtime.Remoting.RemotingServices.IsTransparentProxy(ControlCode) Then
      
      End If


      With ControlCode

        'heat exchanger.
        HeatExchanger1.Value = .IO.HeatCoolOutput  'this is how much its open
        If .IO.SteamSelect Then
          SteamSelect.Value = .IO.HeatCoolOutput
          CoolSelect.Value = 0
          HeatExchanger1.TemperatureChange = TemperatureChange.Heating
        ElseIf .IO.CoolSelect Then
          SteamSelect.Value = 0
          CoolSelect.Value = .IO.HeatCoolOutput
          HeatExchanger1.TemperatureChange = TemperatureChange.Cooling
        Else
          SteamSelect.Value = 0
          CoolSelect.Value = 0
          HeatExchanger1.TemperatureChange = TemperatureChange.None
        End If

      End With


    Catch ex As Exception

    End Try
  End Sub









End Class
