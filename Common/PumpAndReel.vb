Public Class PumpAndReel : Inherits MarshalByRefObject
#Region "Enumeration"
  Private Enum ePumpState
    Off
    DelayOn
    PumpOn
    AutoOff
  End Enum
  Private Enum eReelState
    Off
    DelayOn
    AutoOn
    Forward
    Reverse
  End Enum
#End Region

  Public Sub Run()

    'Pump State Control
    Select Case _PumpState
      Case ePumpState.Off
        'do nothing

      Case ePumpState.DelayOn
        If _PumpEnableTimer.Finished Then
          _PumpState = ePumpState.PumpOn
        End If

      Case ePumpState.PumpOn
        If _PumpEnableTimer.Seconds > 0 Then
          _PumpState = ePumpState.DelayOn
          SetReelsToDelayOn()
        End If

      Case ePumpState.AutoOff
        If _PumpOffTimer.Finished Then
          _PumpState = ePumpState.Off
        End If

    End Select

    'Reel State Control
    Dim i As Integer
    For i = 1 To _NumberOfReels
      Select Case _ReelState(i)
        Case eReelState.Off
          'some code
        Case eReelState.DelayOn
          If _ReelOnDelayTimer.Finished Then
            _ReelState(i) = eReelState.AutoOn
          End If
        Case eReelState.AutoOn
          If _ReelOnDelayTimer.Seconds > 0 Then
            _ReelState(i) = eReelState.DelayOn
          End If
        Case eReelState.Forward
          'some code
        Case eReelState.Reverse
          'some code
      End Select
    Next

  End Sub

  Public Sub AutoStart()

    'Pump Off - Set to DelayOn
    If (_PumpState = ePumpState.Off) Then
      _PumpState = ePumpState.DelayOn
      'Used to make sure the fabric has been wet out before memorising the level
      _PumpFabricWetOutTimer.Seconds = 180
    End If

    'Pump Running (off after delay) - set to On
    If (_PumpState = ePumpState.AutoOff) Then
      _PumpState = ePumpState.PumpOn
    End If

    'Check state of each reel and change accordingly
    Dim i As Integer
    For i = 1 To _NumberOfReels
      If _ReelState(i) = eReelState.Off Then
        _ReelOnDelayTimer.Seconds = _ReelOnDelay
        _ReelState(i) = eReelState.DelayOn
      End If
      If _ReelState(i) = eReelState.Forward Then
        _ReelState(i) = eReelState.AutoOn
      End If
      If _ReelState(i) = eReelState.Reverse Then
        _ReelOnDelayTimer.Seconds = _ReelOnDelay
        _ReelState(i) = eReelState.DelayOn
      End If
    Next i

  End Sub

  Public Sub AutoStop()

    'Check to see if any reels running
    Dim ReelsRunning As Boolean, i As Integer

    'Turn all reels off
    For i = 1 To _NumberOfReels
      If IsReelForward(i) Then ReelsRunning = True
      _ReelState(i) = eReelState.Off
    Next i

    'Pump Running - stop pump after delay (ensure reels have stopped
    If _PumpState = ePumpState.PumpOn Then
      _PumpOffTimer.Seconds = _PumpOffDelay
      _PumpState = ePumpState.AutoOff
    End If
    If _PumpState = ePumpState.DelayOn Then
      _PumpState = ePumpState.Off
    End If


  End Sub

  Public Sub ResetPumpEnableTimer()
    _PumpEnableTimer.Seconds = _PumpEnableDelay
  End Sub
  Public Sub ResetPumpOffDelayTimer()
    _PumpOffTimer.Seconds = _PumpOffDelay
  End Sub
  Public Sub ResetReelOnDelayTimer()
    _ReelOnDelayTimer.Seconds = _ReelOnDelay
  End Sub
  Public Sub RequestPump()
    _PumpState = ePumpState.PumpOn
  End Sub
  Public Sub StopPump()
    _PumpState = ePumpState.Off
    Dim i As Integer
    For i = 1 To _NumberOfReels
      _ReelState(i) = eReelState.Off
    Next i
  End Sub
  Public Sub StartReelForward(ByVal ReelNumber As Integer)
    'invalid number
    If (ReelNumber > _NumberOfReels) Or (ReelNumber < 1) Then Exit Sub

    'start specific reel
    If _ReelState(ReelNumber) = eReelState.Off Then _ReelState(ReelNumber) = eReelState.DelayOn
  End Sub

  Public Sub StartReelReverse(ByVal ReelNumber As Integer)
    'invalid number
    If (ReelNumber > _NumberOfReels) Or (ReelNumber < 1) Then Exit Sub

    'start specific reel
    If _ReelState(ReelNumber) = eReelState.Off Then _ReelState(ReelNumber) = eReelState.Reverse
  End Sub

  Public Sub StopReel(ByVal ReelNumber As Integer)
    'invalid number
    If (ReelNumber > _NumberOfReels) Or (ReelNumber < 1) Then Exit Sub

    'stop specific reel
    _ReelState(ReelNumber) = eReelState.Off
  End Sub

  Public Sub StopReels()
    Dim i As Integer
    For i = 1 To _NumberOfReels
      _ReelState(i) = eReelState.Off
    Next i
  End Sub

  Public Sub StopAll()
    StopPump()
  End Sub

  Private Sub SetReelsToDelayOn()
    Dim i As Integer
    For i = 1 To _NumberOfReels
      If _ReelState(i) = eReelState.Forward Then _ReelState(i) = eReelState.DelayOn
    Next i
  End Sub

  'Properties - NOTE: all properties return false by default
  Public ReadOnly Property IsStopped() As Boolean
    Get
      'Check pump and reel
      If _PumpState <> ePumpState.Off Then Exit Property
      Dim i As Integer
      For i = 1 To _NumberOfReels
        If _ReelState(i) <> eReelState.Off Then Exit Property
      Next i

      'If we got this far everything must be off so return true
      IsStopped = True
    End Get
  End Property

  Public ReadOnly Property IsRunning(ByVal NumberOfReels As Integer) As Boolean
    Get
      'Check pump and reel
      If _PumpState <> ePumpState.PumpOn Then Exit Property
      Dim i As Integer
      For i = 1 To NumberOfReels
        If (_ReelState(i) <> eReelState.AutoOn) AndAlso _
           (_ReelState(i) <> eReelState.Forward) Then Exit Property
      Next i

      'If we got this far everything must be off so return true
      IsRunning = True
    End Get
  End Property

#Region "Pump Properties"
  Private _PumpState As ePumpState
  Private _PumpOffDelay As Integer
  Public Property PumpOffDelay() As Integer
    Get
      Return _PumpOffDelay
    End Get
    Set(ByVal value As Integer)
      _PumpOffDelay = MinMax(value, 0, 30)
    End Set
  End Property

  Private _PumpEnableDelay As Integer
  Public Property PumpEnableDelay() As Integer
    Get
      Return _PumpEnableDelay
    End Get
    Set(ByVal value As Integer)
      _PumpEnableDelay = MinMax(value, 10, 60)
    End Set
  End Property
  Private _PumpEnableTimer As New Timer
  Public ReadOnly Property PumpEnableTimeRemaining() As Integer
    Get
      Return _PumpEnableTimer.Seconds
    End Get
  End Property
  Private _PumpOffTimer As New Timer
  Public ReadOnly Property PumpOffimeRemaining() As Integer
    Get
      Return _PumpOffTimer.Seconds
    End Get
  End Property
  Private _PumpFabricWetOutTimer As New Timer
  Public ReadOnly Property pumpFabricWetOutTimer() As Integer
    Get
      Return _PumpFabricWetOutTimer.Seconds
    End Get
  End Property


  Public ReadOnly Property IsPumpEnabled() As Boolean
    Get
      'If pump on delay timer is finished we are free to start the pump so return true
      If _PumpEnableTimer.Finished Then IsPumpEnabled = True
    End Get
  End Property
  Public ReadOnly Property IsPumpStopped() As Boolean
    Get
      'If pump Off or DelayOn then return true
      If (_PumpState = ePumpState.Off) Then IsPumpStopped = True
      If (_PumpState = ePumpState.DelayOn) Then IsPumpStopped = True
    End Get
  End Property
  Public ReadOnly Property IsPumpOn() As Boolean
    Get
      'If pump State is on then return true
      If (_PumpState = ePumpState.PumpOn) Or (_PumpState = ePumpState.AutoOff) Then IsPumpOn = True
    End Get
  End Property
  Public ReadOnly Property IsPumpAutoOff() As Boolean
    Get
      'If pump State is autooff then return true
      If (_PumpState = ePumpState.AutoOff) Then IsPumpAutoOff = True
    End Get
  End Property

#End Region

#Region "Reel Properties"
  Private _MaxNumberOfReels As Integer = 3
  Private _NumberOfReels As Integer = 3
  Private _ReelState(_MaxNumberOfReels) As eReelState

  Public ReadOnly Property NumberOfReels() As Integer
    Get
      Return _NumberOfReels
    End Get
  End Property

  Private _ReelOnDelay As Integer
  Public Property ReelOnDelay As Integer
    Get
      Return _ReelOnDelay
    End Get
    Set(ByVal value As Integer)
      _ReelOnDelay = MinMax(value, 0, 120)
    End Set
  End Property

  Private _ReelOnDelayTimer As New Timer

  Public ReadOnly Property ReelOnDelayTimer() As Timer
    Get
      Return _ReelOnDelayTimer
    End Get
  End Property


  Public ReadOnly Property IsReelStopped(ByVal ReelNumber As Integer) As Boolean
    Get
      'If reel State is off then return true
      If _ReelState(ReelNumber) = eReelState.Off Then IsReelStopped = True
    End Get
  End Property
  Public ReadOnly Property IsReelForward(ByVal ReelNumber As Integer) As Boolean
    Get
      'If Not reelOnDelayTimer_.Finished Then Exit Property
      'If reel State is Forward then return true
      If _ReelState(ReelNumber) = eReelState.AutoOn Then IsReelForward = True
      If _ReelState(ReelNumber) = eReelState.Forward Then IsReelForward = True
    End Get
  End Property
  Public ReadOnly Property IsReelReverse(ByVal ReelNumber As Integer) As Boolean
    Get
      'If reel State is reverse then return true
      If _ReelState(ReelNumber) = eReelState.Reverse Then IsReelReverse = True
    End Get
  End Property

  Public ReadOnly Property Reel1State As String
    Get
      Select Case _ReelState(1)
        Case eReelState.Off
          Return ("Off")
        Case eReelState.DelayOn
          Return "Delay On " & _ReelOnDelayTimer.ToString
        Case eReelState.AutoOn
          Return "Auto On "
        Case eReelState.Forward
          Return "Forward"
        Case eReelState.Reverse
          Return "Reverse"
      End Select
      Return ""
    End Get
  End Property
 
  Public ReadOnly Property IsReel1Forward As Boolean
    Get
      Return IsReelForward(1)
    End Get
  End Property
  Public ReadOnly Property IsReel1Reverse As Boolean
    Get
      Return IsReelReverse(1)
    End Get
  End Property
 
  Public ReadOnly Property Reel2State As String
    Get
      Select Case _ReelState(2)
        Case eReelState.Off
          Return ("Off")
        Case eReelState.DelayOn
          Return "Delay On " & _ReelOnDelayTimer.ToString
        Case eReelState.AutoOn
          Return "Auto On "
        Case eReelState.Forward
          Return "Forward"
        Case eReelState.Reverse
          Return "Reverse"
      End Select
      Return ""
    End Get
  End Property

  Public ReadOnly Property IsReel2Forward As Boolean
    Get
      Return IsReelForward(2)
    End Get
  End Property
  Public ReadOnly Property IsReel2Reverse As Boolean
    Get
      Return IsReelReverse(2)
    End Get
  End Property
  Public ReadOnly Property Reel3State As String
    Get
      Select Case _ReelState(3)
        Case eReelState.Off
          Return ("Off")
        Case eReelState.DelayOn
          Return "Delay On " & _ReelOnDelayTimer.ToString
        Case eReelState.AutoOn
          Return "Auto On "
        Case eReelState.Forward
          Return "Forward"
        Case eReelState.Reverse
          Return "Reverse"
      End Select
      Return ""
    End Get
  End Property

  Public ReadOnly Property IsReel3Forward As Boolean
    Get
      Return IsReelForward(3)
    End Get
  End Property
  Public ReadOnly Property IsReel3Reverse As Boolean
    Get
      Return IsReelReverse(3)
    End Get
  End Property
#End Region
End Class
