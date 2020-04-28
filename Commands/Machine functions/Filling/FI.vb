<Command("Fill:", "|0-180|F to |0-200|% ", "", "100", "'StandardFillTime=3"),
  Description("Fills the machine with water to the specified temperature and to the specified % level or % working volume."),
  Category("Machine Functions")>
Public Class FI : Inherits MarshalByRefObject : Implements ACCommand
#Region "Enumeration"
  Public Enum EState
    Off
    Interlock
    NotSafe
    ResetMeter
    FillLevel
    FillVolume
    Finished
    Paused
  End Enum
#End Region

  Private ReadOnly ControlCode As ControlCode
  Sub New(ByVal controlCode As ControlCode)
    Me.ControlCode = controlCode
  End Sub

  Sub ParametersChanged(ByVal ParamArray param() As Integer) Implements ACCommand.ParametersChanged


  End Sub

  Function Start(ByVal ParamArray param() As Integer) As Boolean Implements ACCommand.Start
    If param.GetUpperBound(0) >= 2 Then FillPercent = param(2)
    If param.GetUpperBound(0) >= 1 Then FillTemperature = param(1) * 10

    With ControlCode
      'this is a foreground only function.
      'cancel foreground functions.
      'add tank
      .AD.Cancel() : .AT.Cancel() : .RC.Cancel()
      'machine
      .DR.Cancel() : .PD.Cancel()
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

      If .WorkingLevel > 0 Then
        FillLevel = MinMax((.WorkingLevel * FillPercent) \ 100, Parameters_FillLevelMinimum, Parameters_FillLevelMaximum)
        FillVolume = 0
        State = EState.Interlock
      Else
        FillLevel = 0
        FillVolume = MinMax((.WorkingVolume * FillPercent) \ 100, Parameters_FillVolumeMinimum, Parameters_FillVolumeMaximum)
        State = EState.Interlock

      End If

      ' Set blend control parameters
      BlendControl.Parameters(.Parameters_BlendDeadBand, .Parameters_BlendFactor, .Parameters_BlendSettleTime, .Parameters_ColdWaterTemperature, .Parameters_HotWaterTemperature)

      OverrunTimer.Minutes = Parameters_StandardFillTime
    End With
  End Function

  Function Run() As Boolean Implements ACCommand.Run
    With ControlCode

      If (State > EState.Interlock) AndAlso Not .MachineSafe Then State = EState.NotSafe
      If (State > EState.ResetMeter) AndAlso (.VesLevel > Parameters_FillLevelMaximum) Then State = EState.Finished

      Select Case State
        Case EState.Off
          StateString = ""

          'make sure machine is safe
        Case EState.Interlock
          If .MachineSafe Then
            .TP.Cancel()
            .HE.Cancel()
            .CO.Cancel()
            .TemperatureControl.Cancel()            ' Cancel Temperature control
            .PumpAndReel.AutoStop()
            State = EState.ResetMeter
          End If
          StateString = "FI: Machine not safe to fill."
          If Not .TempSafe Then
            StateString = "FI: Temperature too high to fill."
          End If
          If Not .PressSafe Then
            StateString = "FI: Pressure too high to fill."
          End If


        Case EState.NotSafe
          If .MachineSafe Then State = EState.ResetMeter
          StateString = "FI: Machine not safe to fill."
          If Not .TempSafe Then
            StateString = "FI: Temperature too high to fill."
          End If
          If Not .PressSafe Then
            StateString = "FI: Pressure too high to fill."
          End If

        Case EState.ResetMeter
          StateString = "FI:Resetting flowmeter counter"

          'Wait for counter to reset to zero
          If .VesVolume = 0 AndAlso .FlowmeterWater.Gallons = 0 Then
            'Fill to volume or level
            BlendControl.Start(FillTemperature)
            State = EState.FillVolume
            If FillLevel > 0 Then State = EState.FillLevel
          End If

        Case EState.FillLevel
          StateString = "FI:Filling to level " & .VesLevel \ 10 & "%/" & (FillLevel \ 10).ToString.PadLeft(3) & "%"
          BlendControl.Run(.IO.BlendFillTemp)
          If .VesLevel >= FillLevel - Parameters_FillLevelDeadband Then
            Timer.Seconds = 3
            State = EState.Finished
          End If

          If .FlowmeterAlarmTimer.Finished Then
            StateWas = State
            State = EState.Paused
            ResetFillFlowmeterTimer.Seconds = 5
          End If

        Case EState.FillVolume
          StateString = "FI:Filling to volume " & .VesVolume & "/" & FillVolume.ToString & " Gallons"
          BlendControl.Run(.IO.BlendFillTemp)
          If .VesVolume >= FillVolume - Parameters_FillVolumeDeadband Then
            Timer.Seconds = 3
            State = EState.Finished
          End If

          If .FlowmeterAlarmTimer.Finished Then
            StateWas = State
            State = EState.Paused
            ResetFillFlowmeterTimer.Seconds = 5
          End If


        Case EState.Finished
          If Timer.Finished Then
            .FillingCompleted = True
            .PumpAndReel.AutoStart()        ' Start Circulation
            Cancel()               ' Cancel current command
          End If

        Case EState.Paused
          StateString = "FI:Flowmeter fault Press run for " & ResetFillFlowmeterTimer.ToString & " to continue."

          If Not .IO.RemoteRun Then ResetFillFlowmeterTimer.Seconds = 5
          If ResetFillFlowmeterTimer.Finished Then
            State = StateWas
            .FlowmeterAlarmTimer.Seconds = .Parameters_FillFlowmeterAlarmTime
          End If

      End Select
    End With

  End Function

  Sub Cancel() Implements ACCommand.Cancel
    With ControlCode
      If IsOn Then .FillingCompleted = True
      State = EState.Off
      StateWas = EState.Off
      FillLevel = 0
      FillVolume = 0
    End With

  End Sub

#Region "state and state string"
  Property State As EState
  Property StateWas As EState
  Property StateString As String
#End Region

#Region "Public Properties"
  ReadOnly Property IsOn As Boolean Implements ACCommand.IsOn
    Get
      Return State <> EState.Off
    End Get
  End Property

  Public ReadOnly Property IsFinished As Boolean
    Get
      Return State = EState.Finished
    End Get
  End Property
  Public ReadOnly Property IsOverrun As Boolean
    Get
      Return IsFilling AndAlso OverrunTimer.Finished
    End Get
  End Property
  Public ReadOnly Property IsPaused As Boolean
    Get
      Return State = EState.Paused
    End Get
  End Property
#End Region

#Region "IO Properties"
  Public ReadOnly Property IsResetMeter As Boolean
    Get
      Return State = EState.ResetMeter
    End Get
  End Property
  Public ReadOnly Property IsFilling As Boolean
    Get
      Return State = EState.FillLevel OrElse State = EState.FillVolume
    End Get
  End Property

  ReadOnly Property IOBlendOutput As Integer
    Get
      Return BlendControl.IOOutput

    End Get
  End Property

#End Region

#Region "Timers "
  Property Timer As New Timer
  Property ResetFillFlowmeterTimer As New Timer
  Property OverrunTimer As New Timer

#End Region

#Region "Variable properties"
  Property FillTemperature As Integer
  Property FillPercent As Integer
  Property FillVolume As Integer
  Property FillLevel As Integer

  Property BlendControl As New BlendControl

#End Region

#Region "Parameters"
  <Parameter(0, 60), Category("Production reports"), _
      Description("The standard time for the  machine to fill to the desired level. In minutes.")> _
  Public Parameters_StandardFillTime As Integer
  <Parameter(0, 3000), Category("Level control"), _
    Description("During a fill-by-volume, the machine will always fill to at least this volume, measured in gallons.")> _
  Public Parameters_FillVolumeMinimum As Integer
  <Parameter(0, 6000), Category("Level control"), _
  Description("During a fill-by-volume, the machine will never fill to more than this volume, measured in gallons.")> _
  Public Parameters_FillVolumeMaximum As Integer
  <Parameter(0, 1000), Category("Level control"), _
  Description("During a fill-by-level, the machine will always fill to at least  this level, measured in tenths %.")> _
  Public Parameters_FillLevelMinimum As Integer
  <Parameter(0, 1000), Category("Level control"), _
Description("During a fill-by-level, the machine will never fill to more than this level, measured in tenths %.")> _
  Public Parameters_FillLevelMaximum As Integer
  <Parameter(0, 1000), Category("Fill control"), _
  Description("During a fill-by-level, when the machine level gets within the desired fill level minus this amount the fill is complete., measured in tenths %.")> _
  Public Parameters_FillLevelDeadband As Integer
  <Parameter(0, 1000), Category("Fill control"), _
 Description("During a fill-by-Volume, when the machine level gets within the desired fill volume minus this amount the fill is complete., measured in gallons.")> _
  Public Parameters_FillVolumeDeadband As Integer
#End Region

End Class


