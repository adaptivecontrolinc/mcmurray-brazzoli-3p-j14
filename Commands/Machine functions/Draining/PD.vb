<Command("Pump to drain", "", "", "", "'StandardPumpToDrainTime=4"),
Description("Drains the machine using the main pump and the pump drain valve."),
Category("Machine Functions")>
Public Class PD
  Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    NotSafe
    StopReels
    DrainLevel
    DrainEmpty
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
      'this is a foreground only function.
      'cancel foreground functions.
      'add tank
      .AD.Cancel() : .AT.Cancel() : .RC.Cancel()
      'machine
      .DR.Cancel()
      .FI.Cancel()
      .TM.Cancel()
      .PR.Cancel()
      .RI.Cancel() : .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .SA.Cancel() : .UL.Cancel()
      'temp control
      .CO.Cancel() : .HE.Cancel() : .TP.Cancel() : .WT.Cancel()
      .TemperatureControl.Cancel()

      If .ManualFillRequest Then .ManualFillRequest = False
      If .ManualDrainRequest Then .ManualDrainRequest = False

      OverrunTimer.Minutes = Parameters_StandardpumptoDrainTime


    End With




    State = EState.Interlock
  End Function
  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode
      If (State > EState.Interlock) AndAlso (Not .MachineSafe) Then State = EState.NotSafe
      Select Case State
        Case EState.Off
          StateString = ""

        Case EState.Interlock

          If .MachineSafe Then
            .TP.Cancel()
            .CO.Cancel()
            .HE.Cancel()
            .TemperatureControl.Cancel()           ' Cancel Temperature control
            .PumpAndReel.StopReels()
            State = EState.StopReels

          End If

          StateString = "PD: Machine not safe to drain."
          If Not .TempSafe Then
            StateString = "PD: Temperature too high to drain."
          End If
          If Not .PressSafe Then
            StateString = "PD: Pressure too high to drain."
          End If

        Case EState.NotSafe
          If .TempSafe Then State = EState.StopReels
          StateString = "PD: Machine not safe to drain."
          If Not .TempSafe Then
            StateString = "PD: Temperature too high to drain."
          End If

        Case EState.StopReels

          If Not .IO.Reel1Reverse AndAlso Not .IO.Reel2Running AndAlso Not .IO.Reel3Running Then
            timer.Seconds = Parameters_PDPumpTime
            State = EState.DrainLevel
          End If
          StateString = "PD: Stopping reels."

        Case EState.DrainLevel
          StateString = "PD:Pumping to drain " & timer.ToString
          If .VesLevel >= Parameters_PDPumpLevel Then timer.Seconds = Parameters_PDPumpTime

          'if we are draining to a level stop at that level and restart the reel.
          If timer.Finished Then
            timer.Seconds = Parameters_PDDrainTime
            .PumpAndReel.AutoStop()
            State = EState.DrainEmpty
          End If



        Case EState.DrainEmpty
          StateString = "DR:Draining " & timer.ToString
          If .VesLevel > 10 Then timer.Seconds = Parameters_PDDrainTime
          If timer.Finished Then
            .VesVolume = 0
            State = EState.Off
          End If

      End Select
    End With
  End Function

  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off

  End Sub

#Region "State and state string"
  Property State As EState
  Property StateString As String
#End Region

#Region "public properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return (State <> EState.Off)
    End Get
  End Property

  ReadOnly Property IsOverrun As Boolean
    Get
      Return IsOn AndAlso OverrunTimer.Finished
    End Get
  End Property
#End Region

#Region "io properties"
  ReadOnly Property IsPumpToDrain As Boolean
    Get
      Return (State = EState.DrainLevel)
    End Get
  End Property
  Public ReadOnly Property IsDrainEmpty As Boolean
    Get
      Return State = EState.DrainEmpty
    End Get
  End Property

#End Region

#Region " Variables"

#End Region

#Region "timers"
  Property OverrunTimer As New Timer
  Property timer As New Timer

#End Region

#Region "Parameters"
  <Parameter(0, 60), Category("Production reports"),
  Description("The standard time for the machine to pump to drain. In minutes.")>
  Public Parameters_StandardPumptoDrainTime As Integer

  <Parameter(0, 1000), Category("Drain control"),
  Description("Time to continue draining the machine after turning off the pump during a PD command. In seconds.")>
  Public Parameters_PDDrainTime As Integer

  <Parameter(0, 1000), Category("Drain control"),
  Description("The speed to run the pump at during a pump to drain. In tenths of a percent.")>
  Public Parameters_PDPumpSpeed As Integer
  <Parameter(0, 1000), Category("Drain control"),
  Description("The level at which the PD pump time starts to count down to turn off the pump. In tenths of a percent.")>
  Public Parameters_PDPumpLevel As Integer
  <Parameter(0, 1000), Category("Drain control"),
  Description("The time to keep running the pump for after the pump level is reached. In seconds.")>
  Public Parameters_PDPumpTime As Integer

#End Region

End Class
