<Command("Cool", "|0-9|.|0-9| TO |0-280|", " ('1*10) + '2", "'3", ""),
Description("Cool at the desired gradient to the desired target temperature.  A gradient and target of 0 disables any previous control."),
Category("Temperature Functions")>
Public Class CO : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Start
    Ramp
    Hold
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode
  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub
  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged

  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode

      Gradient = (param(1) * 10) + param(2)
      FinalTemp = MinMax(param(3) * 10, 0, 2800)
      State = EState.Start

      'this Is a foreground only function.
      'cancel foreground functions.
      'add tank
      .AD.Cancel() : .AT.Cancel()
      'machine
      .DR.Cancel() : .PD.Cancel()
      .FI.Cancel()
      .TM.Cancel()
      .RI.Cancel() : .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .SA.Cancel() : .UL.Cancel()

      'temp control
      .HE.Cancel() : .TP.Cancel() : .WT.Cancel()


      'No Gradient or Final temp - Cancel command
      If (Gradient = 0 And FinalTemp = 0) OrElse (FinalTemp > 2800) Then Cancel()

      'For compatibility - max gradient used to be 99 rather than 0
      If Gradient = 99 Then Gradient = 0
      RateOfRise = Gradient
      If RateOfRise = 0 Then RateOfRise = 50
      If FinalTemp > .VesTemp Then

        OverrunTimer.Seconds = (((FinalTemp - .VesTemp) \ RateOfRise) * 60) + 60
      Else
        OverrunTimer.Seconds = (((.VesTemp - FinalTemp) \ RateOfRise) * 60) + 60
      End If

    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode
      Static pidPaused As Boolean
      If .TemperatureControl.IsPaused Then
        pidPaused = True
        OverrunTimer.Pause()
      Else
        If pidPaused = True Then
          pidPaused = False
          OverrunTimer.Restart()
        End If
      End If

      Select Case State
        Case EState.Start
          StateString = "CO : Begining"
          With .TemperatureControl
            .CoolingIntegral = .Parameters_CoolIntegral
            .CoolingMaxGradient = .Parameters_CoolMaxGradient
            .CoolingPropBand = .Parameters_CoolPropBand
            .CoolingStepMargin = .Parameters_CoolStepMargin
            .TempMode = 0
            If .Parameters_HeatCoolModeChange = 1 Then .TempMode = 2
            If .Parameters_HeatCoolModeChange = 2 Then .TempMode = 2
          End With
          .TemperatureControl.Start(.VesTemp, FinalTemp, Gradient)
          State = EState.Ramp

        Case EState.Ramp
          StateString = "Cooling to " & (.TemperatureControl.TempFinalTemp / 10).ToString.PadLeft(3) & "F"
          If .TemperatureControl.IsHolding Then
            If ((.VesTemp > (.TemperatureControl.TempFinalTemp - .TemperatureControl.Parameters_CoolStepMargin)) And
                (.VesTemp < (.TemperatureControl.TempFinalTemp + .TemperatureControl.Parameters_HeatStepMargin))) Then
              State = EState.Hold
              Return True
            End If
          End If


        Case EState.Hold
          'Change mode to Heat/Cool if necessary0
          StateString = "Cooling:Holding " & (.TemperatureControl.TempFinalTemp / 10).ToString.PadLeft(3) & "F"
          If .TemperatureControl.Parameters_HeatCoolModeChange = 1 Then .TemperatureControl.TempMode = 0


      End Select
    End With
  End Function

  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
  End Sub

#Region "state and state string"
  Property State As EState
  Property StateString As String

#End Region

#Region "Public Properties"

  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return (State <> EState.Off)
    End Get
  End Property
  ReadOnly Property IsActive As Boolean
    Get
      Return (State = EState.Ramp)
    End Get
  End Property
  ReadOnly Property IsOverrun As Boolean
    Get
      Return IsActive AndAlso OverrunTimer.Finished
    End Get
  End Property
#End Region

#Region "Command properties"
  Property Gradient As Integer
  Property FinalTemp As Integer
  Property RateOfRise As Integer

#End Region

#Region "Timers"
  Property OverrunTimer As New Timer

#End Region
End Class
