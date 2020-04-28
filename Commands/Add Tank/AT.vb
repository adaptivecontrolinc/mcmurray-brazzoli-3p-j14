<Command("Add transfer ", "|0-100|%", " ", "", "'StandardATTime = 1"),
Description("Transfers the add tank to the desired level."),
Category("Add Tank Functions")>
Public Class AT : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    WaitReady
    Pause
    TransferEmpty1
    Rinse
    TransferEmpty2
    Finished
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
      'need the cancels.
      'cancel foreground functions.
      'add tank
      .AD.Cancel()
      'machine
      .DR.Cancel() : .PD.Cancel()
      .FI.Cancel()
      .TM.Cancel()
      .PR.Cancel()
      .RI.Cancel() : .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .SA.Cancel() : .UL.Cancel()

      If param.GetUpperBound(0) >= 1 Then DesiredLevel = param(1) * 10

      State = EState.WaitReady

    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode
      Select Case State

        Case EState.Off
          StateString = ""

        Case EState.Interlock
          If .MachineSafe Then
            State = EState.WaitReady
          End If
          StateString = "AT:Machine not safe to transfer."
          If Not .TempSafe Then
            StateString = "AT:Temperature too high to transfer."
          End If
          If Not .PressSafe Then
            StateString = "AT:Pressure too high to transfer."
          End If

        Case EState.WaitReady
          'state string 

          'we are filling and the add tank is the right one.
          If .AF.IsOn Then
            StateString = .AF.StateString
          ElseIf Not .AP.IOPrepareReady Then
            StateString = .AP.StateString
          Else
            StateString = "AT:Transfer:" & Timer.ToString
          End If

          'check to see if its ready
          If (Not .AP.IsOn OrElse .AP.IOPrepareReady) AndAlso Not .AF.IsOn Then
            .RC.Cancel()
            Timer.Seconds = .Parameters_AddTransferTimeBeforeRinse
            State = EState.TransferEmpty1
          End If

        Case EState.Pause
          If Not .IO.PumpRunning Then
            StateString = "Transfer Paused: Main pump not running. " & Timer.ToString
          ElseIf Not .IO.PumpInAutoSw Then
            StateString = "Transfer Paused: Turn on Main pump. " & Timer.ToString
          ElseIf .IO.EmergencyStop Then
            StateString = "Transfer Paused: Emergency stop pushed" & Timer.ToString
          ElseIf Not .IO.Reel1Running Then
            StateString = "Transfer Paused:Reel 1 not running" & Timer.ToString
          ElseIf Not .IO.Reel2Running Then
            StateString = "Transfer Paused:Reel 2 not running" & Timer.ToString
          ElseIf Not .IO.Reel3Running Then
            StateString = "Transfer Paused:Reel 2 not running" & Timer.ToString
          ElseIf .IO.ReelForwardSwitch Then
            StateString = "Transfer Paused:Reels not enabled" & Timer.ToString
          ElseIf .Parent.IsPaused Then
            StateString = "Transfer Program paused" & Timer.ToString
          ElseIf .TemperatureControl.IsCrashCoolOn Then
            StateString = "Transfer Paused:Crash cooling " & Timer.ToString
          Else
            StateString = "Transfer Paused: " & Timer.ToString
          End If

          If .IO.PumpRunning AndAlso .IO.PumpInAutoSw AndAlso .IO.ReelForwardSwitch AndAlso .IO.Reel1Running AndAlso .IO.Reel2Running AndAlso
            .IO.Reel3Running AndAlso (Not (.Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn)) Then
            State = EState.TransferEmpty1
            Timer.Restart()
          End If

        Case EState.TransferEmpty1

          StateString = "AT:First transfer:" & Timer.ToString

          If .AddLevel > 10 Then Timer.Seconds = .Parameters_AddTransferTimeBeforeRinse
          If .AddLevel < desiredlevel Then State = EState.Finished
          If Timer.Finished Then
            Timer.Seconds = .Parameters_AddRinseTime
            State = EState.Rinse
          End If

        Case EState.Rinse

          StateString = "AT:Rinse:" & Timer.ToString

          If Timer.Finished Then
            Timer.Seconds = .Parameters_AddTransferTimeAfterRinse
            State = EState.TransferEmpty2
          End If

        Case EState.TransferEmpty2

          StateString = "AT:Second transfer:" & Timer.ToString

          If .AddLevel > 10 Then Timer.Seconds = .Parameters_AddTransferTimeAfterRinse

          If Timer.Finished Then
            State = EState.Finished
          End If

        Case EState.Finished
          .AP.Cancel()
          .AddReady = False
          State = EState.Off

      End Select
    End With

  End Function

  Sub Cancel() Implements ACCommand.Cancel
    With ControlCode
      'incase the tank was on a transfer and we jump clear all of its lists.
      If State <> EState.Off Then
        .AddReady = False
      End If

    End With

    State = EState.Off

  End Sub

#Region "State and state string"
  Property State As EState
  Property StateString As String
#End Region

#Region " Public Properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      IsOn = (State <> EState.Off)
    End Get
  End Property

  ReadOnly Property IsDelayed As Boolean
    Get
      Return IsWaitReady
    End Get
  End Property
  ReadOnly Property IsWaitReady As Boolean
    Get
      Return State = EState.WaitReady
    End Get
  End Property
  ReadOnly Property IsActive As Boolean
    Get
      Return State > EState.WaitReady
    End Get
  End Property


#End Region

#Region "Variable properties"
  Property DesiredLevel As Integer

#End Region

#Region "timers"
  Property Timer As New Timer

#End Region

#Region "I/O Propertiers"

  Public ReadOnly Property IsAddPump As Boolean
    Get
      Return State = EState.TransferEmpty1 OrElse State = EState.Rinse OrElse State = EState.TransferEmpty2
    End Get
  End Property
  Public ReadOnly Property IsRinse As Boolean
    Get
      Return State = EState.Rinse
    End Get
  End Property
  'valve 705 outlet of the pump to the machine from tank 1
  Public ReadOnly Property IsTransfer As Boolean
    Get
      Return State = EState.TransferEmpty1 OrElse State = EState.Rinse OrElse State = EState.TransferEmpty2
    End Get
  End Property
#End Region







End Class
