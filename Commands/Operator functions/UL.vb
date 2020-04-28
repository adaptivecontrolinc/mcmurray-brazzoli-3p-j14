<Command("Unload Machine", "", "", "", "'StandardUnloadTime=15"),
Description("Signals the operator to unload the machine and starts recording the unload time. If the unload time exceeds the parameter Standard Unload Time then the overrun time is logged as Load Delay."),
Category("Operator Commands")>
Public Class UL : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Unload
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode
  Public Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub
  Public Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged

  End Sub
  Public Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
   
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
      .LD.Cancel() : .SA.Cancel()

      OverrunTimer.Minutes = Parameters_StandardUnloadTime
      .MachineIsLoaded = False
      State = EState.Unload
      .Parent.Signal = "UL:Unload fabric."
    End With
  End Function

  Public Function Run() As Boolean Implements ACCommand.Run
    With ControlCode

      Select Case State

        Case EState.Off
          StateString = ""

        Case EState.Unload
          StateString = "UL:Unload fabric."
          If .Parent.Signal = "" Then
            Cancel()
          End If


      End Select

    End With
  End Function

  Public Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off
   
  End Sub

#Region "State and state string"
  Property State As EState
  Property StateString As String
#End Region

#Region "Public Properties"
  Public ReadOnly Property IsOn() As Boolean Implements ACCommand.IsOn
    Get
      Return (State <> EState.Off)
    End Get
  End Property

  Public ReadOnly Property IsOverrun() As Boolean
    Get
      Return State = EState.Unload AndAlso OverrunTimer.Finished
    End Get
  End Property
#End Region

#Region "timers"
  Private _OverrunTimer As New Timer
  Public Property OverrunTimer() As Timer
    Get
      Return _OverrunTimer
    End Get
    Private Set(ByVal value As Timer)
      _OverrunTimer = value
    End Set
  End Property

#End Region

#Region "Variables"
  

#End Region

#Region "Parameters"
  <Parameter(0, 60), Category("Production reports"), _
  Description("The standard time for unloading the machine. In minutes.")> _
  Public Parameters_StandardUnloadTime As Integer
  
#End Region

#Region "IO Variables"
 
#End Region
End Class
