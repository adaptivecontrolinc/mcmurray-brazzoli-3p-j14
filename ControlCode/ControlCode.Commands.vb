Partial Class ControlCode
  'add tank
  Public ReadOnly AD As New AD(Me)
  Public ReadOnly AF As New AF(Me)
  Public ReadOnly AP As New AP(Me)
  Public ReadOnly AT As New AT(Me)
  Public ReadOnly RC As New RC(Me)

  'machine
  'drain
  Public ReadOnly DR As New DR(Me)
  Public ReadOnly PD As New PD(Me)


  Public ReadOnly FI As New FI(Me)

  Public ReadOnly TM As New TM(Me)

  Public ReadOnly PR As New PR(Me)


  Public ReadOnly RI As New RI(Me)
  Public ReadOnly RP As New RP(Me)

  'Operator

  Public ReadOnly LD As New LD(Me)
  Public ReadOnly SA As New SA(Me)
  Public ReadOnly UL As New UL(Me)

  'production
  Public ReadOnly BO As New BO(Me)
  Public ReadOnly BR As New BR(Me)
  Public ReadOnly BS As New BS(Me)
  Public ReadOnly ET As New ET(Me)

  'setup
  Public ReadOnly BW As New BW(Me) 'this is a batch parameter
  Public ReadOnly DS As New DS(Me) 'this is a batch parameter
  Public ReadOnly LR As New LR(Me) 'this is a batch parameter
  Public ReadOnly FP As New FP(Me) 'this is a batch parameter
  Public ReadOnly RS As New RS(Me) 'this is a batch parameter
  Public ReadOnly WS As New WS(Me) 'this is a batch parameter
  Public ReadOnly VL As New VL(Me) 'this is not a batch parameter
  Public ReadOnly VV As New VV(Me) 'this is not a batch parameter

  'temperature control
  Public ReadOnly CO As New CO(Me)
  Public ReadOnly HE As New HE(Me)
  Public ReadOnly TP As New TP(Me)
  Public ReadOnly WT As New WT(Me)

End Class
