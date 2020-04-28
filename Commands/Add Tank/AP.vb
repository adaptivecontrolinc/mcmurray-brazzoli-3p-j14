<Command("Prepare Add: ", "Prompt:|0-99| ", "", "", ""),
Description("Signals the operator to prepare the tank."),
Category("Add Tank Functions")>
Public Class AP : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Slow
    Fast
    Ready
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


      If param.GetUpperBound(0) >= 1 Then PreparePrompt = param(1)
      State = EState.Slow



      Return True
    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode

      Select Case State
        Case EState.Off
          StateString = ""

        Case EState.Slow
          StateString = "AP:Prepare add tank"
          If .AddReady Then
            State = EState.Ready
          End If

          'we are on a transfer step then make it flash more
          If .AD.IsWaitReady OrElse .AT.IsWaitReady Then
            OverrunTimer.Minutes = Parameters_AddPrepareStandardTime
            State = EState.Fast
          End If

        Case EState.Fast
          StateString = "AP:Prepare add tank. Waiting on transfer."
          If .AddReady Then
            State = EState.Ready
          End If

        Case EState.Ready
          StateString = "AP:Add tank prepared."
          If Not .AddReady Then State = EState.Slow

      End Select


    End With

  End Function

#Region "Cancel"
  Sub Cancel() Implements ACCommand.Cancel
    PreparePrompt = 0
    State = EState.Off

  End Sub

#End Region

#Region "State and state string"
  Property State As EState
  Property StateString As String
#End Region

#Region "Timers"
  Property OverrunTimer As New Timer

#End Region

#Region "Public Properties"

  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return (State <> EState.Off)
    End Get
  End Property
  ReadOnly Property IsActive As Boolean
    Get
      Return IsOn
    End Get
  End Property


  ReadOnly Property IsOverrun As Boolean
    Get
      Return (State = EState.Fast) AndAlso OverrunTimer.Finished
    End Get
  End Property


#End Region

#Region "IO properties"


  Public ReadOnly Property IOPrepareLampSlow() As Boolean
    Get
      Return (State = EState.Slow)
    End Get
  End Property
  Public ReadOnly Property IOPrepareLampFast() As Boolean
    Get
      Return (State = EState.Fast)
    End Get
  End Property
  Public ReadOnly Property IOPrepareReady() As Boolean
    Get
      Return (State = EState.Ready)
    End Get
  End Property

#End Region

#Region "Variable properties"
  Property PreparePrompt As Integer

#End Region

#Region "Parameters"
  <Parameter(0, 99), Category("Production reports"),
    Description("The standard time for the operator to prepare the add tank.  In minutes.")>
  Public Parameters_AddPrepareStandardTime As Integer

#End Region


End Class
