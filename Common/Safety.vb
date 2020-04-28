Public Class Safety : Inherits MarshalByRefObject

  Public Enum EState
    Startup
    Safe
    Pressurizing
    Pressurized
    Depressurizing
  End Enum

  'Keep a local reference to the control code object for convenience
  Private ReadOnly controlCode As ControlCode

  Public Sub New(ByVal controlCode As ControlCode)
    Me.controlCode = controlCode
    Me.State = EState.Startup
    Me.StartTimer.Seconds = 16
  End Sub

  Public Sub Run(ByVal temperature As Integer, ByVal temperatureSafe As Boolean, ByVal PressureSafe As Boolean, ByVal PRIson As Boolean)

    'Constantly set press timer and depress timer according to conditions
    If (temperature < PressTemp) AndAlso temperatureSafe Then PressTimer.Seconds = PressTime
    If (temperature > DepressTemp) OrElse (Not temperatureSafe) OrElse PRIson Then DepressTimer.Seconds = DepressTime



    Select Case State

      Case EState.Startup
        If StartTimer.Finished Then
          If (temperature < PressTemp) AndAlso temperatureSafe AndAlso PressureSafe AndAlso Not PRIson Then State = EState.Depressurizing
          If (temperature >= PressTemp) OrElse (Not temperatureSafe) OrElse (Not PressureSafe) OrElse PRIson Then State = EState.Pressurizing
        End If

      Case EState.Safe
        If (temperature > PressTemp) OrElse (Not temperatureSafe) OrElse (Not PressureSafe) OrElse PRIson Then
          PressTimer.Seconds = PressTime
          State = EState.Pressurizing
        End If

      Case EState.Pressurizing
        If PressTimer.Finished Then State = EState.Pressurized
        If temperature < DepressTemp AndAlso temperatureSafe AndAlso Not PRIson Then
          DepressTimer.Seconds = DepressTime
          State = EState.Depressurizing
        End If

      Case EState.Pressurized
        If temperature < DepressTemp AndAlso Not PRIson Then
          DepressTimer.Seconds = DepressTime
          State = EState.Depressurizing
        End If

      Case EState.Depressurizing
        If Not PressureSafe Then DepressTimer.Seconds = DepressTime
        If DepressTimer.Finished Then
          'Not really necessary but just to be on the safe side
          If (temperature < DepressTemp) AndAlso temperatureSafe AndAlso PressureSafe AndAlso Not PRIson Then State = EState.Safe
        End If
        If temperature >= PressTemp OrElse Not temperatureSafe Then
          PressTimer.Seconds = PressTime
          State = EState.Pressurizing
        End If

    End Select

  End Sub

  Private state_ As EState
  Public Property State As EState
    Get
      Return state_
    End Get
    Private Set(ByVal value As EState)
      state_ = value
    End Set
  End Property

  Private timer_ As New Timer
  Public Property StartTimer As Timer
    Get
      Return timer_
    End Get
    Set(ByVal value As Timer)
      timer_ = value
    End Set
  End Property


  Private pressTimer_ As New Timer
  Public Property PressTimer As Timer
    Get
      Return pressTimer_
    End Get
    Private Set(ByVal value As Timer)
      pressTimer_ = value
    End Set
  End Property

  Private depressTimer_ As New Timer
  Public Property DepressTimer As Timer
    Get
      Return depressTimer_
    End Get
    Private Set(ByVal value As Timer)
      depressTimer_ = value
    End Set
  End Property

   


  Public ReadOnly Property PressTemp As Integer
    'Pressurization temperature - make sure it is sensible and at least 4C less than PressTemp
    Get
      Return MinMax(controlCode.Parameters_PressurizationTemperature, 1400, 2030)
    End Get
  End Property

  Public ReadOnly Property PressTime As Integer
    Get
      Return MinMax(controlCode.Parameters_PressurizationTime, 16, 600)
    End Get
  End Property

  Public ReadOnly Property DepressTemp As Integer
    'Depressurization temperature - make sure it is sensible and at least 4C less than PressTemp
    Get
      Dim returnValue As Integer = MinMax(controlCode.Parameters_DepressurizationTemperature, 1400, 2030)
      If returnValue > (PressTemp - 40) Then returnValue = PressTemp - 40
      Return returnValue
    End Get
  End Property

  Public ReadOnly Property DepressTime As Integer
    Get
      Return MinMax(controlCode.Parameters_DepressurizationTime, 16, 600)
    End Get
  End Property

#Region " Public state properties "

  Public ReadOnly Property IsDepressurized As Boolean
    Get
      Return State = EState.Safe
    End Get
  End Property

#End Region

#Region " Public IO properties "

  Public ReadOnly Property IOVent As Boolean
    Get
      Return (State = EState.Safe) OrElse (State = EState.Depressurizing)
    End Get
  End Property

#End Region

End Class
