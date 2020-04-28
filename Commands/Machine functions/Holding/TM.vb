<Command("Run", "|0-99|mins", " ", "", "'1"),
Description("Holds for specified time. If the main pump is not running the time is paused.")>
Public Class TM : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Pause
    [On]
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode
  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub
  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged

  End Sub
  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    If param.GetUpperBound(0) >= 1 Then HoldTime = param(1) * 60
    Timer.Seconds = HoldTime

    With ControlCode
      ' this Is a foreground only function.
      'cancel foreground functions.
      'add tank
      .AD.Cancel() : .AT.Cancel()
      'machine
      .DR.Cancel() : .PD.Cancel()
      .FI.Cancel()
      .RI.Cancel() : .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .SA.Cancel() : .UL.Cancel()



    End With
    State = EState.On
    CrashCoolPBWas = False
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode
      Select Case State
        Case EState.Off
          StateString = ""

        Case EState.Pause
          If Not .IO.MainPumpStart And .VesLevel < .Parameters_PumpMinimumLevel Then
            StateString = "Run Paused: Level to low for pump." & Timer.ToString
          ElseIf Not .IO.PumpInAutoSw Then
            StateString = "Run Paused: Main pump not enabled." & Timer.ToString
          ElseIf Not .IO.MainPumpStart Then
            StateString = "Run Paused: Main pump not running" & Timer.ToString
          ElseIf .IO.EmergencyStop Then
            StateString = "Run Paused: Emergency stop pushed" & Timer.ToString
          ElseIf .Parent.IsPaused Then
            StateString = "Run Program paused" & Timer.ToString
          ElseIf CrashCoolPBWas Then
            StateString = "Run Paused:Crash cooling " & Timer.ToString
          ElseIf Not .io.Reel1Running Then
            StateString = "Run Paused:Reel 1 not running" & Timer.ToString
          ElseIf Not .io.Reel2Running Then
            StateString = "Run Paused:Reel 2 not running" & Timer.ToString
          ElseIf Not .io.Reel3Running Then
            StateString = "Run Paused:Reel 2 not running" & Timer.ToString
          ElseIf .IO.ReelForwardSwitch Then
            StateString = "Run Paused:Reels not enabled" & Timer.ToString
          Else
            StateString = "Run Paused: " & Timer.ToString
          End If
          If .TemperatureControl.IsCrashCoolOn Then
            CrashCoolPBWas = True
            CrashCoolPauseTimer.Seconds = 10
          End If

          If .IO.PumpRunning AndAlso .IO.PumpInAutoSw AndAlso .IO.ReelForwardSwitch AndAlso .IO.Reel1Running AndAlso .IO.Reel2Running AndAlso .IO.Reel3Running AndAlso
              (Not (.Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn)) Then
            If Not CrashCoolPBWas Then
              State = EState.On
              Timer.Restart()
            End If
            If .TemperatureControl.IsHolding And CrashCoolPauseTimer.Finished Then CrashCoolPBWas = False
          End If

        Case EState.On
          StateString = "Holding " & Timer.ToString
          If Not .IO.PumpRunning OrElse Not .IO.PumpInAutoSw OrElse Not .IO.ReelForwardSwitch OrElse Not .IO.Reel1Running OrElse Not .IO.Reel2Running _
              OrElse Not .IO.Reel3Running OrElse .Parent.IsPaused OrElse .IO.EmergencyStop OrElse .TemperatureControl.IsCrashCoolOn Then     ' Pause timer if pump stopped
            If .TemperatureControl.IsCrashCoolOn Then
              CrashCoolPBWas = True
              CrashCoolPauseTimer.Seconds = 10
            Else
              CrashCoolPBWas = False
            End If
            State = EState.Pause
            Timer.Pause()
          End If
          If Timer.Finished Then Cancel()
      End Select

    End With
  End Function
  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
    CrashCoolPBWas = False
  End Sub

#Region "State and state string"
  Property State As EState
  Property StateString As String
#End Region

#Region "Public Properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      IsOn = (State <> EState.Off)
    End Get
  End Property
#End Region

#Region "IO Properties"
  ReadOnly Property IsPaused As Boolean
    Get
      Return (State = EState.Pause)
    End Get
  End Property

#End Region

#Region "Variables"
  Property HoldTime As Integer
  Property CrashCoolPBWas As Boolean

#End Region

#Region "timers"
  ReadOnly Property CrashCoolPauseTimer As New Timer
  ReadOnly Property Timer As New Timer

#End Region

End Class
