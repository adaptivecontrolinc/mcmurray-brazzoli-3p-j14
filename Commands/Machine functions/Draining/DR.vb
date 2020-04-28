<Command("Drain", "to L:|0-100|%", "", "", "'StandardDrainTime=4"),
Description("Drains the machine to the specified level. If the specified level is 0% the machine will drain to 0% level and then continue to drain."),
Category("Machine Functions")>
Public Class DR : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    NotSafe
    StopPump
    DrainLevel
    DrainEmpty
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode
  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub
  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged
    If param.GetUpperBound(0) >= 1 Then DrainLevel = param(1) * 10


  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode
      If param.GetUpperBound(0) >= 1 Then DrainLevel = param(1) * 10

      'this is a foreground only function.
      'cancel foreground functions.
      'add tank
      .AD.Cancel() : .AT.Cancel() : .RC.Cancel()
      'machine
      .PD.Cancel()
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

      OverrunTimer.Minutes = Parameters_StandardDrainTime


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
            State = EState.StopPump
            .PumpAndReel.AutoStop()
          End If

          StateString = "DR: Machine not safe to drain."
          If Not .TempSafe Then
            StateString = "DR: Temperature too high to drain."
          End If
          If Not .PressSafe Then
            StateString = "DR: Pressure too high to drain."
          End If

        Case EState.NotSafe
          If .TempSafe Then State = EState.StopPump
          StateString = "DR: Machine not safe to drain."
          If Not .TempSafe Then
            StateString = "DR: Temperature too high to drain."
          End If

        Case EState.StopPump

          If Not .IO.PumpRunning Then
            If DrainLevel > 0 Then
              State = EState.DrainLevel
            Else
              timer.Seconds = Parameters_DrainTime
              State = EState.DrainEmpty

            End If
          End If
          StateString = "DR: Stopping pump."

        Case EState.DrainLevel
          StateString = "DR:Drain to " & (DrainLevel / 10).ToString & "/" & (.VesLevel / 10).ToString
          'if we are draining to a level stop at that level and restart the reel.
          If (.VesLevel <= DrainLevel) Then
            State = EState.Off
            .PumpAndReel.AutoStart()
          End If

        Case EState.DrainEmpty
          StateString = "DR:Draining " & timer.ToString
          If .VesLevel > 10 Then timer.Seconds = Parameters_DrainTime
          If timer.Finished Then
            State = EState.Off
            .VesVolume = 0
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
  ReadOnly Property IsActive As Boolean
    Get
      Return State = EState.StopPump OrElse State = EState.DrainLevel OrElse State = EState.DrainEmpty
    End Get
  End Property

  ReadOnly Property IsOverrun As Boolean
    Get
      Return IsOn AndAlso OverrunTimer.Finished
    End Get
  End Property
#End Region

#Region "io properties"
  Public ReadOnly Property IsDraining As Boolean
    Get
      Return State = EState.DrainEmpty OrElse State = EState.DrainLevel
    End Get
  End Property

#End Region

#Region " Variables"
  Property DrainLevel As Integer
#End Region

#Region "timers"
  Property OverrunTimer As New Timer
  Property timer As New Timer

#End Region

#Region "Parameters"
  <Parameter(0, 60), Category("Production reports"),
  Description("The standard time for the machine to drain. In minutes.")>
  Public Parameters_StandardDrainTime As Integer
  <Parameter(0, 1000), Category("Drain control"),
  Description("The time to turn on the level flush during a drain. In seconds.")>
  Public Parameters_DrainTime As Integer

#End Region

End Class
