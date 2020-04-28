Partial Class ControlCode
#Region "Status"
  Public ReadOnly Property Temperature() As String
    Get
      Temperature = (VesTemp \ 10) & "F"
    End Get
  End Property

  Public ReadOnly Property Status() As String
    Get
      If Parent.Signal <> "" Then Return Parent.Signal
      If IO.EmergencyStop Then
        Return "Emergency stop Pressed "

      ElseIf Not Parent.IsProgramRunning Then
        Return "Machine Idle: " & TimerString(ProgramStoppedTimer.TimeElapsed)
      ElseIf AD.IsOn Then
        Return AD.StateString
      ElseIf AT.IsOn Then
        Return AT.StateString
      ElseIf DR.IsOn Then
        Return DR.StateString
      ElseIf pd.IsOn Then
        Return PD.StateString
      ElseIf FI.IsOn Then
        Return FI.StateString
      ElseIf RI.IsOn Then
        Return RI.StateString
      ElseIf RP.IsOn Then
        Return RP.StateString
      ElseIf tm.IsOn Then
        Return TM.StateString
      ElseIf LD.IsOn Then
        Return LD.StateString
      ElseIf SA.IsOn Then
        Return SA.StateString
      ElseIf UL.IsOn Then
        Return UL.StateString
      ElseIf CO.IsActive Then
        Return CO.StateString
      ElseIf HE.IsActive Then
        Return HE.StateString
      ElseIf WT.IsOn Then
        Return WT.StateString
      End If

      Return ""
    End Get
  End Property
#End Region

End Class
