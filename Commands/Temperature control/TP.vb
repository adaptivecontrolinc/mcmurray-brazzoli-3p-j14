<Command("Temperature", "|0-9|.|0-9| TO |0-280|F", "('1*10) + '2", " '3 ", ""), _
Description("Heat at the given gradient to the given target temperature.  A gradient and target of 0 disables any previous control."), _
Category("Temperature Functions")> _
Public Class TP : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum Estate
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
      .CO.Cancel() : .HE.Cancel() : .WT.Cancel()


      'cancel commands here if necessary
      Gradient = (param(1) * 10) + param(2)
      FinalTemp = MinMax((param(3) * 10), 0, 2800)
      State = Estate.Start

      'No gradient or final temp - cancel command
      If FinalTemp = 0 Or (FinalTemp > 2800) Then Cancel()
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
      Select Case State

        Case Estate.Off
          StateString = ""

        Case Estate.Start
          StateString = "Starting TP "
          With .TemperatureControl
            .CoolingIntegral = .Parameters_HeatIntegral
            .CoolingMaxGradient = .Parameters_HeatMaxGradient
            .CoolingPropBand = .Parameters_HeatPropBand
            .CoolingStepMargin = .Parameters_HeatStepMargin
            .TempMode = 0
            If .Parameters_HeatCoolModeChange = 1 Then .TempMode = 2
            If .Parameters_HeatCoolModeChange = 2 Then .TempMode = 2
          End With
          .TemperatureControl.Start(.VesTemp, FinalTemp, Gradient)
          State = Estate.Ramp
          Return True

        Case Estate.Ramp
          StateString = "Heating to " & (.TemperatureControl.TempFinalTemp / 10).ToString.PadLeft(3) & "F"
          If ((.VesTemp > (FinalTemp - .TemperatureControl.Parameters_CoolStepMargin)) AndAlso
             (.VesTemp < (FinalTemp + .TemperatureControl.Parameters_HeatStepMargin))) Then
            State = Estate.Hold
          End If

        Case Estate.Hold
          'Change mode to Heat and Cool if necessary
          If .TemperatureControl.Parameters_HeatCoolModeChange = 1 Then .TemperatureControl.TempMode = 0

      End Select
    End With
  End Function

  Sub Cancel() Implements ACCommand.Cancel
    State = Estate.Off
  End Sub

#Region "Public Properties"
  Property State As Estate
  Property StateString As String
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return (State <> Estate.Off)
    End Get
  End Property
  ReadOnly Property IsOverrun As Boolean
    Get
      Return (State = Estate.Ramp) AndAlso OverrunTimer.Finished
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
