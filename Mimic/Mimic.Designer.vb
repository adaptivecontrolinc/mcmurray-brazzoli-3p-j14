<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Mimic
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Me.components = New System.ComponentModel.Container()
    Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Mimic))
    Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
    Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
    Me.IO_Spray = New MimicControls.Valve()
    Me.IO_Fill = New MimicControls.Valve()
    Me.IO_Vent = New MimicControls.Valve()
    Me.IO_OverFlow = New MimicControls.Valve()
    Me.IO_Drain = New MimicControls.Valve()
    Me.IO_AddHeat = New MimicControls.Valve()
    Me.IO_AddRinse = New MimicControls.Valve()
    Me.IO_DrainPressure = New MimicControls.Valve()
    Me.IO_AddDrain = New MimicControls.Valve()
    Me.IO_AddTransfer = New MimicControls.Valve()
    Me.IO_AddRunback = New MimicControls.Valve()
    Me.IO_CoolWaterReturn = New MimicControls.Valve()
    Me.IO_Condensate = New MimicControls.Valve()
    Me.CoolSelect = New MimicControls.ProportionalValve()
    Me.SteamSelect = New MimicControls.ProportionalValve()
    Me.HeatExchanger1 = New MimicControls.HeatExchanger()
    Me.VesLevel = New MimicControls.ValueLabel()
    Me.VesTemp = New MimicControls.ValueLabel()
    Me.VesVolume = New MimicControls.ValueLabel()
    Me.IO_BlendFillTemp = New MimicControls.ValueLabel()
    Me.addlevel = New MimicControls.ValueLabel()
    Me.IO_addtemp = New MimicControls.ValueLabel()
    Me.SuspendLayout()
    '
    'Timer1
    '
    Me.Timer1.Enabled = True
    Me.Timer1.Interval = 1000
    '
    'ToolTip1
    '
    Me.ToolTip1.IsBalloon = True
    '
    'IO_Spray
    '
    Me.IO_Spray.Location = New System.Drawing.Point(663, 133)
    Me.IO_Spray.Name = "IO_Spray"
    Me.IO_Spray.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_Spray.Size = New System.Drawing.Size(20, 20)
    Me.IO_Spray.TabIndex = 11
    Me.ToolTip1.SetToolTip(Me.IO_Spray, "Spray")
    Me.IO_Spray.UIEnabled = False
    '
    'IO_Fill
    '
    Me.IO_Fill.Location = New System.Drawing.Point(435, 404)
    Me.IO_Fill.Name = "IO_Fill"
    Me.IO_Fill.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_Fill.Size = New System.Drawing.Size(20, 20)
    Me.IO_Fill.TabIndex = 10
    Me.ToolTip1.SetToolTip(Me.IO_Fill, "Pump to drain")
    Me.IO_Fill.UIEnabled = False
    '
    'IO_Vent
    '
    Me.IO_Vent.Location = New System.Drawing.Point(508, 198)
    Me.IO_Vent.Name = "IO_Vent"
    Me.IO_Vent.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_Vent.Size = New System.Drawing.Size(20, 20)
    Me.IO_Vent.TabIndex = 9
    Me.ToolTip1.SetToolTip(Me.IO_Vent, "Vent")
    Me.IO_Vent.UIEnabled = False
    '
    'IO_OverFlow
    '
    Me.IO_OverFlow.Location = New System.Drawing.Point(704, 363)
    Me.IO_OverFlow.Name = "IO_OverFlow"
    Me.IO_OverFlow.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_OverFlow.Size = New System.Drawing.Size(20, 20)
    Me.IO_OverFlow.TabIndex = 8
    Me.ToolTip1.SetToolTip(Me.IO_OverFlow, "OverFlow")
    Me.IO_OverFlow.UIEnabled = False
    '
    'IO_Drain
    '
    Me.IO_Drain.Location = New System.Drawing.Point(694, 464)
    Me.IO_Drain.Name = "IO_Drain"
    Me.IO_Drain.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_Drain.Size = New System.Drawing.Size(20, 20)
    Me.IO_Drain.TabIndex = 7
    Me.ToolTip1.SetToolTip(Me.IO_Drain, "Drain")
    Me.IO_Drain.UIEnabled = False
    '
    'IO_AddHeat
    '
    Me.IO_AddHeat.Location = New System.Drawing.Point(124, 446)
    Me.IO_AddHeat.Name = "IO_AddHeat"
    Me.IO_AddHeat.Size = New System.Drawing.Size(20, 20)
    Me.IO_AddHeat.TabIndex = 6
    Me.ToolTip1.SetToolTip(Me.IO_AddHeat, "Add Heat")
    Me.IO_AddHeat.UIEnabled = False
    '
    'IO_AddRinse
    '
    Me.IO_AddRinse.Location = New System.Drawing.Point(36, 377)
    Me.IO_AddRinse.Name = "IO_AddRinse"
    Me.IO_AddRinse.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_AddRinse.Size = New System.Drawing.Size(20, 20)
    Me.IO_AddRinse.TabIndex = 5
    Me.ToolTip1.SetToolTip(Me.IO_AddRinse, "Add Rinse")
    Me.IO_AddRinse.UIEnabled = False
    '
    'IO_DrainPressure
    '
    Me.IO_DrainPressure.Location = New System.Drawing.Point(313, 453)
    Me.IO_DrainPressure.Name = "IO_DrainPressure"
    Me.IO_DrainPressure.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_DrainPressure.Size = New System.Drawing.Size(20, 20)
    Me.IO_DrainPressure.TabIndex = 4
    Me.ToolTip1.SetToolTip(Me.IO_DrainPressure, "Pump to drain")
    Me.IO_DrainPressure.UIEnabled = False
    '
    'IO_AddDrain
    '
    Me.IO_AddDrain.Location = New System.Drawing.Point(347, 513)
    Me.IO_AddDrain.Name = "IO_AddDrain"
    Me.IO_AddDrain.Size = New System.Drawing.Size(20, 20)
    Me.IO_AddDrain.TabIndex = 3
    Me.ToolTip1.SetToolTip(Me.IO_AddDrain, "Add Drain")
    Me.IO_AddDrain.UIEnabled = False
    '
    'IO_AddTransfer
    '
    Me.IO_AddTransfer.Location = New System.Drawing.Point(383, 493)
    Me.IO_AddTransfer.Name = "IO_AddTransfer"
    Me.IO_AddTransfer.Size = New System.Drawing.Size(20, 20)
    Me.IO_AddTransfer.TabIndex = 2
    Me.ToolTip1.SetToolTip(Me.IO_AddTransfer, "Add Transfer")
    Me.IO_AddTransfer.UIEnabled = False
    '
    'IO_AddRunback
    '
    Me.IO_AddRunback.Location = New System.Drawing.Point(192, 377)
    Me.IO_AddRunback.Name = "IO_AddRunback"
    Me.IO_AddRunback.Size = New System.Drawing.Size(20, 20)
    Me.IO_AddRunback.TabIndex = 1
    Me.ToolTip1.SetToolTip(Me.IO_AddRunback, "Add Runback")
    Me.IO_AddRunback.UIEnabled = False
    '
    'IO_CoolWaterReturn
    '
    Me.IO_CoolWaterReturn.Location = New System.Drawing.Point(359, 252)
    Me.IO_CoolWaterReturn.Name = "IO_CoolWaterReturn"
    Me.IO_CoolWaterReturn.Size = New System.Drawing.Size(20, 20)
    Me.IO_CoolWaterReturn.TabIndex = 14
    Me.ToolTip1.SetToolTip(Me.IO_CoolWaterReturn, "CoolWaterReturn")
    Me.IO_CoolWaterReturn.UIEnabled = False
    '
    'IO_Condensate
    '
    Me.IO_Condensate.Location = New System.Drawing.Point(340, 169)
    Me.IO_Condensate.Name = "IO_Condensate"
    Me.IO_Condensate.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.IO_Condensate.Size = New System.Drawing.Size(20, 20)
    Me.IO_Condensate.TabIndex = 15
    Me.ToolTip1.SetToolTip(Me.IO_Condensate, "Condensate")
    Me.IO_Condensate.UIEnabled = False
    '
    'CoolSelect
    '
    Me.CoolSelect.Format = Nothing
    Me.CoolSelect.Location = New System.Drawing.Point(359, 223)
    Me.CoolSelect.Name = "CoolSelect"
    Me.CoolSelect.Size = New System.Drawing.Size(32, 23)
    Me.CoolSelect.TabIndex = 13
    '
    'SteamSelect
    '
    Me.SteamSelect.Format = Nothing
    Me.SteamSelect.Location = New System.Drawing.Point(359, 140)
    Me.SteamSelect.Name = "SteamSelect"
    Me.SteamSelect.Size = New System.Drawing.Size(32, 23)
    Me.SteamSelect.TabIndex = 12
    '
    'HeatExchanger1
    '
    Me.HeatExchanger1.ForeColor = System.Drawing.Color.Black
    Me.HeatExchanger1.Format = Nothing
    Me.HeatExchanger1.Location = New System.Drawing.Point(259, 146)
    Me.HeatExchanger1.Name = "HeatExchanger1"
    Me.HeatExchanger1.Orientation = System.Windows.Forms.Orientation.Vertical
    Me.HeatExchanger1.Size = New System.Drawing.Size(45, 100)
    Me.HeatExchanger1.TabIndex = 0
    '
    'VesLevel
    '
    Me.VesLevel.BackColor = System.Drawing.Color.White
    Me.VesLevel.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.VesLevel.ForeColor = System.Drawing.Color.Black
    Me.VesLevel.Format = "Vessel level 0.0\%"
    Me.VesLevel.Location = New System.Drawing.Point(650, 252)
    Me.VesLevel.Name = "VesLevel"
    Me.VesLevel.NumberScale = 10
    Me.VesLevel.Size = New System.Drawing.Size(92, 13)
    Me.VesLevel.TabIndex = 162
    '
    'VesTemp
    '
    Me.VesTemp.BackColor = System.Drawing.Color.White
    Me.VesTemp.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.VesTemp.ForeColor = System.Drawing.Color.Black
    Me.VesTemp.Format = "Vessel temp 0.0 F"
    Me.VesTemp.Location = New System.Drawing.Point(650, 233)
    Me.VesTemp.Name = "VesTemp"
    Me.VesTemp.NumberScale = 10
    Me.VesTemp.Size = New System.Drawing.Size(92, 13)
    Me.VesTemp.TabIndex = 161
    '
    'VesVolume
    '
    Me.VesVolume.BackColor = System.Drawing.Color.White
    Me.VesVolume.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.VesVolume.ForeColor = System.Drawing.Color.Black
    Me.VesVolume.Format = "Volume 0 Gal"
    Me.VesVolume.Location = New System.Drawing.Point(650, 214)
    Me.VesVolume.Name = "VesVolume"
    Me.VesVolume.Size = New System.Drawing.Size(68, 13)
    Me.VesVolume.TabIndex = 160
    '
    'IO_BlendFillTemp
    '
    Me.IO_BlendFillTemp.BackColor = System.Drawing.Color.White
    Me.IO_BlendFillTemp.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.IO_BlendFillTemp.ForeColor = System.Drawing.Color.Black
    Me.IO_BlendFillTemp.Format = " 0.0 F"
    Me.IO_BlendFillTemp.Location = New System.Drawing.Point(435, 146)
    Me.IO_BlendFillTemp.Name = "IO_BlendFillTemp"
    Me.IO_BlendFillTemp.NumberScale = 10
    Me.IO_BlendFillTemp.Size = New System.Drawing.Size(35, 13)
    Me.IO_BlendFillTemp.TabIndex = 168
    '
    'addlevel
    '
    Me.addlevel.BackColor = System.Drawing.Color.White
    Me.addlevel.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.addlevel.ForeColor = System.Drawing.Color.Black
    Me.addlevel.Format = "0.0\%"
    Me.addlevel.Location = New System.Drawing.Point(52, 446)
    Me.addlevel.Name = "addlevel"
    Me.addlevel.NumberScale = 10
    Me.addlevel.Size = New System.Drawing.Size(34, 13)
    Me.addlevel.TabIndex = 170
    '
    'IO_addtemp
    '
    Me.IO_addtemp.BackColor = System.Drawing.Color.White
    Me.IO_addtemp.Font = New System.Drawing.Font("Tahoma", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
    Me.IO_addtemp.ForeColor = System.Drawing.Color.Black
    Me.IO_addtemp.Format = "0.0 F"
    Me.IO_addtemp.Location = New System.Drawing.Point(52, 427)
    Me.IO_addtemp.Name = "IO_addtemp"
    Me.IO_addtemp.NumberScale = 10
    Me.IO_addtemp.Size = New System.Drawing.Size(32, 13)
    Me.IO_addtemp.TabIndex = 169
    '
    'Mimic
    '
    Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
    Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
    Me.BackColor = System.Drawing.Color.White
    Me.BackgroundImage = CType(resources.GetObject("$this.BackgroundImage"), System.Drawing.Image)
    Me.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch
    Me.Controls.Add(Me.addlevel)
    Me.Controls.Add(Me.IO_addtemp)
    Me.Controls.Add(Me.IO_BlendFillTemp)
    Me.Controls.Add(Me.VesLevel)
    Me.Controls.Add(Me.VesTemp)
    Me.Controls.Add(Me.VesVolume)
    Me.Controls.Add(Me.IO_Condensate)
    Me.Controls.Add(Me.IO_CoolWaterReturn)
    Me.Controls.Add(Me.CoolSelect)
    Me.Controls.Add(Me.SteamSelect)
    Me.Controls.Add(Me.IO_Spray)
    Me.Controls.Add(Me.IO_Fill)
    Me.Controls.Add(Me.IO_Vent)
    Me.Controls.Add(Me.IO_OverFlow)
    Me.Controls.Add(Me.IO_Drain)
    Me.Controls.Add(Me.IO_AddHeat)
    Me.Controls.Add(Me.IO_AddRinse)
    Me.Controls.Add(Me.IO_DrainPressure)
    Me.Controls.Add(Me.IO_AddDrain)
    Me.Controls.Add(Me.IO_AddTransfer)
    Me.Controls.Add(Me.IO_AddRunback)
    Me.Controls.Add(Me.HeatExchanger1)
    Me.ForeColor = System.Drawing.Color.Black
    Me.Name = "Mimic"
    Me.Size = New System.Drawing.Size(799, 533)
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents Timer1 As System.Windows.Forms.Timer
  Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
  Friend WithEvents HeatExchanger1 As HeatExchanger
  Friend WithEvents IO_AddRunback As Valve
  Friend WithEvents IO_AddTransfer As Valve
  Friend WithEvents IO_AddDrain As Valve
  Friend WithEvents IO_DrainPressure As Valve
  Friend WithEvents IO_AddRinse As Valve
  Friend WithEvents IO_AddHeat As Valve
  Friend WithEvents IO_Drain As Valve
  Friend WithEvents IO_OverFlow As Valve
  Friend WithEvents IO_Vent As Valve
  Friend WithEvents IO_Fill As Valve
  Friend WithEvents IO_Spray As Valve
  Friend WithEvents SteamSelect As ProportionalValve
  Friend WithEvents CoolSelect As ProportionalValve
  Friend WithEvents IO_CoolWaterReturn As Valve
  Friend WithEvents IO_Condensate As Valve
  Friend WithEvents VesLevel As ValueLabel
  Friend WithEvents VesTemp As ValueLabel
  Friend WithEvents VesVolume As ValueLabel
  Friend WithEvents IO_BlendFillTemp As ValueLabel
  Friend WithEvents addlevel As ValueLabel
  Friend WithEvents IO_addtemp As ValueLabel
End Class
