<Command("Add Recirculate", "", "", "", ""),
Description("Recirculates the addition tank."),
Category("Add Tank Commands")>
Public Class RC : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    NotSafe
    Fill
    Circulate
    Transfer
    Pause
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



      State = EState.Interlock
    End With

    Return True

  End Function

  Function Run() As Boolean Implements ACCommand.Run

    With ControlCode
      Select Case State

        Case EState.Interlock
          StateString = "RC:Waiting for Machine Safe"
          If Not .MachineSafe Then Exit Function
          State = EState.Fill

        Case EState.Fill
          StateString = "RC:Filling to " & .Parameters_RCHighLevel / 10 & "%"

          If .AddLevel > .Parameters_RCLowLevel + 10 Then State = EState.Circulate
          If Not .IO.PumpRunning Then State = EState.Pause

        Case EState.Circulate
          StateString = "RC:Recirculating "

          If .AddLevel < .Parameters_RCLowLevel Then State = EState.Fill
          If .AddLevel > .Parameters_RCHighLevel Then State = EState.Transfer
          If Not .IO.PumpRunning Then State = EState.Pause

        Case EState.Transfer
          StateString = "RC:Transferring to" & .Parameters_RCLowLevel / 10 & "%"
          If .AddLevel < .Parameters_RCHighLevel - 10 Then State = EState.Circulate
          If Not .IO.PumpRunning Then State = EState.Pause

        Case EState.Pause
          StateString = "RC:Recirculate Paused"

          If .IO.PumpRunning Then State = EState.Fill

      End Select



    End With


  End Function
  Public Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off

  End Sub

#Region "State and State string"
  Public Property State As EState
  Public Property StateString As String

#End Region

#Region "variables"
  Public Property Transferlevel As Integer
  Public Property Filllevel As Integer
#End Region

#Region "Timers"


  Property OverrunTimer As New Timer

#End Region

#Region "Public Properties"

  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      IsOn = (State <> EState.Off)
    End Get
  End Property

  ReadOnly Property IsOverrun As Boolean
    Get
      Return IsOn AndAlso OverrunTimer.Finished
    End Get
  End Property

  ReadOnly Property IORunback As Boolean
    Get
      Return State = EState.Circulate OrElse State = EState.Fill
    End Get
  End Property
  ReadOnly Property IOTransfer As Boolean
    Get
      Return State = EState.Circulate OrElse State = EState.Transfer
    End Get
  End Property
  ReadOnly Property IOAddPump As Boolean
    Get
      Return State = EState.Circulate OrElse State = EState.Fill OrElse State = EState.Transfer
    End Get
  End Property
#End Region
End Class
