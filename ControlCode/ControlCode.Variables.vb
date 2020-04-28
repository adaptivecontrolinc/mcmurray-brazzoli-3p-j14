Partial Class ControlCode
 
  '==============================================================================
  'Program and step time displays
  '==============================================================================
  Private CycleTime As Integer
  Private LastProgramCycleTime As Integer
  Public CycleTimeDisplay As String
  Public CurrentTime As String

  'Time control system has been idle
  Public ProgramStoppedTimer As New TimerUp
  Public ProgramStoppedTime As Integer

  'Time program has been running
  Public ProgramRunTimer As New TimerUp
  Public ProgramRunTime As Integer

  'Current time in step and step overrun
  Private TwoSecondTimer As New Timer
  Public TimeInStepValue As Integer, StepStandardTime As Integer, TimeInStep As String

  'Variables for troubleshooting histories
  Private WasPausedTimer As New Timer

  'end time stuff
  Public CalculatedStandardTime As Double
  'importing stuff from database
  Public StartTime As String
  Public EndTime As String
  Public EndTimeMins As Integer
  Public GetEndTimeTimer As New Timer
  Public StepStandardTimeWas As Integer


  'Delay variable values
  Public Delay As DelayValue

  'tangle
  Public TangleDelayTimer As New Timer

  Public Reel1Tangled As Boolean
  Public Port1TangleDelayTimer As New Timer

  Public Reel2Tangled As Boolean
  Public Port2TangleDelayTimer As New Timer

  Public Reel3Tangled As Boolean
  Public Port3TangleDelayTimer As New Timer


  'Pump and reel control
  Public PumpAndReel As New PumpAndReel
  Public Reel1Ypm As Integer
  Public Reel2Ypm As Integer
  Public Reel3Ypm As Integer
  Public Reel1SpeedFeedback As Integer
  Public Reel2SpeedFeedback As Integer
  Public Reel3SpeedFeedback As Integer
  Public MachineIsLoaded As Boolean
  Public ManualPumpSpeed As Integer
  Public ManualReelSpeedYPM As Integer


  'Timers to reinitiate reel alarm after 1 minute
  Public PumpSpeedDisplay As String
  Public PumpMinimumLevelTimer As New Timer

  '==============================================================================
  'Analog variables to display on the graph
  '==============================================================================
  <GraphTrace(0, 3000, 0, 10000, "DeepPink", "%tF"), GraphLabel("25", 250), GraphLabel("50", 500), _
    GraphLabel("75", 750), GraphLabel("100", 1000), GraphLabel("125", 1250), GraphLabel("150", 1500), _
    GraphLabel("175", 1750), GraphLabel("200", 2000), GraphLabel("225", 2250), GraphLabel("250", 2500), _
    GraphLabel("275", 2750), GraphLabel("300", 3000)> _
    Public VesTemp As Integer

  <GraphTrace(1, 3000, 0, 10000, "Red", "%tF")> Public SetpointF As Integer
  Public TempFinalValue As String
  'vessel Level
  <GraphTrace(1, 1000, 0, 3333, "DarkGreen", "%t%")> Public VesLevel As Integer
 
  'Variables for temp, level and volume
  <GraphTrace(1, 1000, 0, 3333, "Purple", "%t%")> Public AddLevel As Integer

  <GraphTrace(0, 1, 0, 1, "Blue", )> Public WasPaused As Boolean
  <GraphTrace(0, 1, 0, 1, "Red", )> Public IsDelayed As Boolean

  '==============================================================================
  'Various Variables
  '==============================================================================
  'Safety Variables
  Public MachineSafe As Boolean
  Public TempValid As Boolean, TempSafe As Boolean
  Public PressSafe As Boolean
  Public SafetyControl As New Safety(Me)
  Public Vent As Boolean

  'BatchWeight LiquorRation WorkingVolume WorkingLevel
  Public BatchWeight As Integer, LiquorRatio As Integer
  Public WorkingVolume As Integer, WorkingLevel As Integer
  Public VesVolume As Integer
  Public WaterUsedTemp As Integer
  Public FillingCompleted As Boolean
  <GraphTrace(0, 30000, 0, 10000, "DarkSlateBlue", "%d gal")> Public WaterUsed As Integer
  Private GallonsPerPound As Integer

  'Local add tank stuff
  Public AddReady As Boolean

  'Class module for temperature control
  Public TemperatureControl As New TemperatureControl

  '************

  'For Production Reports
  Public PowerKWS As Integer
  <GraphTrace(0, 30000, 0, 10000, "SaddleBrown", "%d KWH")> Public PowerKWHrs As Integer
  <GraphTrace(0, 30000, 0, 10000, "DarkRed", "%d lbs")> Public SteamUsed As Integer
  Private MainPumpHP As Integer
  Private ReelHP As Integer
  Private AddPumpHP As Integer
  Private AddMixerHP As Integer

  Public SteamNeeded As Integer
  Public TempRise As Integer
  Public FinalTempWas As Integer
  Public FinalTemp As Integer
  Public StartTemp As Integer

  'SystemIdle variables
  Public SystemIdleTimer As New Timer
  Public SystemIdle As Boolean

  'pause the program if sleeping
  Public IsSleepingWas As Boolean

  'System shutdown flag
  Friend SystemShuttingDown As Boolean
  Public FirstScanDone As Boolean

  Public PowerOnTimer As New Timer

  Public InputsScanned As Boolean

  'flowmeter counters because they wrap.
  Public FlowmeterWater As New Flowmeter
  Public FlowmeterAlarmTimer As New Timer

  'lifter reel and pump stop input
  Public PumpInAutoSwitchWas As Boolean
  Public ReelForwardSwWas As Boolean
  Public ReelReverseSwWas As Boolean

  'hibernate
  Public GoInToHibernate As Boolean

  'add tank lid control for heating and mixing
  Public AddFillRequest As Boolean
  Public addFillSwitchWas As Boolean
  Public AddRunbackRequest As Boolean
  Public AddRunbackSwitchWas As Boolean
  Public AddTransferRequest As Boolean
  Public AddTransferSwitchWas As Boolean

  'manual fill/drain 
  Public ManualFillRequest As Boolean
  Public ManualFillPbWas As Boolean
  Public ManualDrainRequest As Boolean
  Public ManualDrainPbWas As Boolean

  Public FlowMeterPulseWas As Boolean

End Class
