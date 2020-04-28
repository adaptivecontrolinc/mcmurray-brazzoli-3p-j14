<Command("Sample", "", "", "", "'StandardSampleTime=5"),
Description("Signals the operator to take a sample and starts recording the sample time. If the sample time exceeds the parameter Standard Sample Time then the overrun time is logged as Sample Delay."),
Category("Operator Commands")>
Public Class SA : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    NotSafe
    ManuallyFindSeams
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
      .PR.Cancel()
      .RI.Cancel() : .RP.Cancel()
      'operator foreground
      .LD.Cancel() : .UL.Cancel()


      OverrunTimer.Minutes = Parameters_StandardSampleTime
      State = EState.Interlock

    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode

      If (State > EState.Interlock) And (Not .MachineSafe) Then State = EState.NotSafe
      Select Case State
        Case EState.Off
          StateString = ""

        Case EState.Interlock
          If .MachineSafe Then

            .TP.Cancel()
            .HE.Cancel()
            .CO.Cancel()

            .TemperatureControl.Cancel()           ' Cancel Temperature control

            State = EState.ManuallyFindSeams
            .Parent.Signal = "SA:Manually sample."
          End If

          StateString = "SA: Machine not safe to sample."
          If Not .TempSafe Then
            StateString = "SA: Temperature too high to sample."
          End If
          If Not .PressSafe Then
            StateString = "SA: Pressure too high to sample."
          End If

        Case EState.NotSafe
          If .MachineSafe Then

            State = EState.ManuallyFindSeams
              .Parent.Signal = "SA:Manually sample."
          End If
          StateString = "SA: Machine not safe to sample."
          If Not .TempSafe Then
            StateString = "SA: Temperature too high to sample."
          End If
          If Not .PressSafe Then
            StateString = "SA: Pressure too high to sample."
          End If


        Case EState.ManuallyFindSeams
          StateString = "SA:Manually sample."
          If .Parent.Signal = "" Then
            Cancel()
          End If


      End Select



    End With
  End Function

  Public Sub Cancel() Implements ACCommand.Cancel
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

  ReadOnly Property IsOverrun As Boolean
    Get
      Return IsOn AndAlso OverrunTimer.Finished
    End Get
  End Property

#End Region


#Region "timers"
  Property OverrunTimer As New Timer


#End Region

#Region "Variables"

#End Region

#Region "Parameters"

  <Parameter(0, 60), Category("Production reports"), _
  Description("The standard time for sampling the machine. In minutes.")> _
  Public Parameters_StandardSampleTime As Integer

#End Region


End Class
