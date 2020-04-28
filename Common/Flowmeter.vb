Public Class Flowmeter

  Private previousGallons As Double       'Used to calculate a flow rate
  Private previousGallonsTime As Date     '

  'The raw counter value from the PLC... (converted to an integer)
  Private counter_ As Integer
  Public Property Counter() As Integer
    Get
      Return counter_
    End Get
    Set(ByVal value As Integer)
      ' If CounterValid(value) Then
      'If the new value is less than the current value then the counter must have wrapped
      If value < Counter Then CounterWraps += 1
      counter_ = value
      CalculateFlowRate()
      'End If
    End Set
  End Property

  'Number of times the counter has wrapped - the counter is normally an unsigned 16-bit integer (2^16 = 65536)
  Private counterWraps_ As Integer
  Public Property CounterWraps() As Integer
    Get
      Return counterWraps_
    End Get
    Set(ByVal value As Integer)
      counterWraps_ = value
    End Set
  End Property

  Private counterStart_ As Integer
  Public Property CounterStart() As Integer
    Get
      Return counterStart_
    End Get
    Set(ByVal value As Integer)
      counterStart_ = value
    End Set
  End Property

  Public Sub Reset()
    CounterStart = Counter
    CounterWraps = 0
    previousGallons = Gallons
    previousGallonsTime = Date.Now
  End Sub

  Public ReadOnly Property Count() As Integer
    Get
      Return ((Counter - CounterStart) + (65536 * CounterWraps))
    End Get
  End Property

  Private GalllonsPerCount_ As Double
  Public Property GallonsPerCount() As Double
    Get
      Return GalllonsPerCount_
    End Get
    Set(ByVal value As Double)
      GalllonsPerCount_ = value
    End Set
  End Property

  Public ReadOnly Property Gallons() As Double
    Get
      Return Count * GallonsPerCount
    End Get
  End Property
  Private gallonsperminute_ As Double
  Public Property GallonsPerMinute() As Double
    Get
      Return GallonsPerMinute_
    End Get
    Private Set(ByVal value As Double)
      GallonsPerMinute_ = value
    End Set
  End Property


  Private Sub CalculateFlowRate()
    Try
      'Make sure we have actually recorded a previous amount
      If previousGallonsTime = Nothing Then Exit Sub

      'Calculate change in liters and time
      Dim deltaGallons As Double = Gallons - previousGallons
      Dim deltaSeconds As Double = Date.Now.Subtract(previousGallonsTime).TotalSeconds
      Dim deltaMinutes As Double = deltaSeconds / 60

      If (deltaSeconds > 1) Then
        If deltaMinutes > 0 Then GallonsPerMinute = deltaGallons / deltaMinutes
        previousGallons = Gallons
        previousGallonsTime = Date.Now
      End If
    Catch ex As Exception
      Debug.Print(ex.Message, ex.StackTrace)
    End Try
  End Sub

  Private Function CounterValid(ByVal value As Integer) As Boolean
    'We seem to be getting some noise on the counter which is triggering a false counter wrap
    '   so keep track of the last few counter values and only accept the counter if the values are pretty close
    Try
      Static lastFourValues(3) As Integer

      'Log the attempted wrap...
      If value < Counter Then
        Dim message As String = "Counter wrap (" & value.ToString & ") ->"
        For i As Integer = 0 To 3 : message &= " (" & lastFourValues(i).ToString & ") " : Next
        'Utilities.Log.LogEvent(message)
      End If

      Dim averageValue As Integer
      For i As Integer = 0 To 3 : averageValue += lastFourValues(i) : Next
      averageValue = averageValue \ 4

      If lastFourValues(0) <> value Then
        'If this is a new value shuffle the older values up and put the new one at index 0
        lastFourValues(3) = lastFourValues(2)
        lastFourValues(2) = lastFourValues(1)
        lastFourValues(1) = lastFourValues(0)
        lastFourValues(0) = value
      End If

      'If the last two value are close allow this value to be used
      '  may switch this to a test against average (commented out below) but try this first
      If Math.Abs(value - lastFourValues(1)) < (GallonsPerCount * 4) Then
        Return True
      End If

      'If the new value is close enough to the average allow it to be used
      'If Math.Abs(value - averageValue) < (CountsPerLiter * 4) Then
      'Return True
      'End If

      Return False
    Catch ex As Exception
      'Ignore errors - if any
      Debug.Print(ex.Message, ex.StackTrace)
    End Try
    Return True
  End Function

End Class
