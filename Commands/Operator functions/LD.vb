<Command("Load Fabric", "", "", "", "'StandardLoadTime=15"),
Description("Signals the operator to Load the machine and starts recording the load time. If the load time exceeds the parameter Standard Load Time then the overrun time is logged as Load Delay."),
Category("Operator Commands")>
Public Class LD : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    [on]
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
      .SA.Cancel() : .UL.Cancel()


      .Parent.Signal = "LD:Load fabric."
    End With
    OverrunTimer.Minutes = Parameters_StandardLoadTime
    State = EState.on

  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode

      Select Case State

        Case EState.Off
          StateString = ""



        Case EState.on

          If .Parent.Signal = "" Then
            Cancel()
            .MachineIsLoaded = True
          End If
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
  ReadOnly Property IsOverrun As Boolean
    Get
      Return State = EState.on AndAlso OverrunTimer.Finished
    End Get
  End Property
#End Region

#Region "Variables"

#End Region

#Region "timers"
  Property OverrunTimer As New Timer

#End Region

#Region "parameters"
  <Parameter(0, 60), Category("Production reports"), _
  Description("The standard time for loading the machine. In minutes.")> _
  Public Parameters_StandardLoadTime As Integer
  


#End Region

End Class
