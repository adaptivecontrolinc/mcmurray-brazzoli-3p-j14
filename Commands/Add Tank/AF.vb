<Command("Add Fill: ", "WV:|W,V| |0-100|%", "", "", ""),
Description("Fills the add tank to the desired level with fresh water(W) or water from the vessel(V)."),
Category("Add Tank Functions")>
Public Class AF : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    InterlockDrainSwitch
    FillFromFreshWater
    FillFromMachine
  End Enum
#End Region
  Private ReadOnly ControlCode As ControlCode

  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub

  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged

    If param.GetUpperBound(0) >= 1 Then FillType = param(1)
    If param.GetUpperBound(0) >= 2 Then FillLevel = MinMax(param(2) * 10, 0, 1000)

  End Sub

  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    With ControlCode
      'W=87 V=86
      If param.GetUpperBound(0) >= 1 Then FillType = param(1)
      If param.GetUpperBound(0) >= 2 Then FillLevel = MinMax(param(2) * 10, 0, 1000)

      If FillType = 87 Then
        State = EState.InterlockDrainSwitch
      Else
        State = EState.Interlock
      End If
      Return True

    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode

      Select Case State
        Case EState.Off
          StateString = ""

        Case EState.Interlock
          If .MachineSafe AndAlso Not .IO.AddDrainSw Then
            State = EState.FillFromMachine
          End If
          If .MachineSafe Then
            StateString = "AF:Interlocked drain switch"
          Else
            StateString = "AF:Interlocked machine not safe"
          End If

        Case EState.InterlockDrainSwitch
          If Not .IO.AddDrainSw Then
            State = EState.FillFromFreshWater
          End If
          StateString = "AF:Interlocked drain switch"


        Case EState.FillFromFreshWater
          StateString = "AF:Filling to " & .AddLevel / 10 & "/" & FillLevel / 10 & "%"
          If .AddLevel >= FillLevel Then
            State = EState.Off
          End If

        Case EState.FillFromMachine
          StateString = "AF:Filling to " & .AddLevel / 10 & "/" & FillLevel / 10 & "%"
          If .AddLevel >= FillLevel Then
            State = EState.Off
          End If

      End Select


    End With

  End Function

#Region "Cancel"
  Sub Cancel() Implements ACCommand.Cancel
    State = EState.Off

    FillType = 0

    FillLevel = 0
    State = EState.Off


  End Sub

#End Region

#Region "State and state string"
  Property State As EState

  Property StateString As String
#End Region

#Region "Public Properties"

  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return State <> EState.Off
    End Get
  End Property
  ReadOnly Property IsActive As Boolean
    Get
      Return IsOn
    End Get
  End Property

#End Region

#Region "IO properties"
  ReadOnly Property IsFillingFresh As Boolean
    Get
      Return State = EState.FillFromFreshWater
    End Get
  End Property
  ReadOnly Property IsFillingMachine As Boolean
    Get
      Return State = EState.FillFromMachine
    End Get
  End Property

#End Region

#Region "Variable properties"

  Property FillType As Integer
  Property FillLevel As Integer

#End Region





End Class
