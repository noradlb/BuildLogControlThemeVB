<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
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
        Me.BuildLogWorkerbutton = New System.Windows.Forms.Button()
        Me.BuildLogWorkerStop = New System.Windows.Forms.Button()
        Me.StartBuild = New System.Windows.Forms.Button()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.BuildLogControl1 = New BuildLogControlTheme.BuildLogControl()
        Me.SuspendLayout()
        '
        'BuildLogWorkerbutton
        '
        Me.BuildLogWorkerbutton.Location = New System.Drawing.Point(0, 456)
        Me.BuildLogWorkerbutton.Name = "BuildLogWorkerbutton"
        Me.BuildLogWorkerbutton.Size = New System.Drawing.Size(167, 23)
        Me.BuildLogWorkerbutton.TabIndex = 1
        Me.BuildLogWorkerbutton.Text = "BuildLogWorker "
        Me.BuildLogWorkerbutton.UseVisualStyleBackColor = True
        '
        'BuildLogWorkerStop
        '
        Me.BuildLogWorkerStop.Location = New System.Drawing.Point(193, 456)
        Me.BuildLogWorkerStop.Name = "BuildLogWorkerStop"
        Me.BuildLogWorkerStop.Size = New System.Drawing.Size(167, 23)
        Me.BuildLogWorkerStop.TabIndex = 2
        Me.BuildLogWorkerStop.Text = "BuildLogWorkerStop"
        Me.BuildLogWorkerStop.UseVisualStyleBackColor = True
        '
        'StartBuild
        '
        Me.StartBuild.Location = New System.Drawing.Point(400, 456)
        Me.StartBuild.Name = "StartBuild"
        Me.StartBuild.Size = New System.Drawing.Size(167, 23)
        Me.StartBuild.TabIndex = 3
        Me.StartBuild.Text = "StartBuild"
        Me.StartBuild.UseVisualStyleBackColor = True
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(621, 456)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(167, 23)
        Me.Button4.TabIndex = 4
        Me.Button4.Text = "Button4"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'BuildLogControl1
        '
        Me.BuildLogControl1.AnimationDurationMs = 300
        Me.BuildLogControl1.AutoScroll = True
        Me.BuildLogControl1.BackColor = System.Drawing.Color.FromArgb(CType(CType(18, Byte), Integer), CType(CType(18, Byte), Integer), CType(CType(18, Byte), Integer))
        Me.BuildLogControl1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(40, Byte), Integer), CType(CType(40, Byte), Integer), CType(CType(40, Byte), Integer))
        Me.BuildLogControl1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.BuildLogControl1.ErrorColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(82, Byte), Integer), CType(CType(82, Byte), Integer))
        Me.BuildLogControl1.ErrorImage = Nothing
        Me.BuildLogControl1.FooterColor = System.Drawing.Color.FromArgb(CType(CType(110, Byte), Integer), CType(CType(110, Byte), Integer), CType(CType(110, Byte), Integer))
        Me.BuildLogControl1.HeaderColor = System.Drawing.Color.FromArgb(CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer), CType(CType(24, Byte), Integer))
        Me.BuildLogControl1.HeaderTitle = "BUILD LOG"
        Me.BuildLogControl1.InfoColor = System.Drawing.Color.FromArgb(CType(CType(100, Byte), Integer), CType(CType(181, Byte), Integer), CType(CType(246, Byte), Integer))
        Me.BuildLogControl1.Location = New System.Drawing.Point(0, 0)
        Me.BuildLogControl1.LogFontName = "Segoe UI"
        Me.BuildLogControl1.LogFontSize = 8.0!
        Me.BuildLogControl1.MaxRows = 500
        Me.BuildLogControl1.Name = "BuildLogControl1"
        Me.BuildLogControl1.ProgressBarColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(118, Byte), Integer))
        Me.BuildLogControl1.ProgressTrackColor = System.Drawing.Color.FromArgb(CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer), CType(CType(35, Byte), Integer))
        Me.BuildLogControl1.RowAppearDelay = 80
        Me.BuildLogControl1.RowHeight = 20
        Me.BuildLogControl1.ShowDotGreen = True
        Me.BuildLogControl1.ShowDotRed = True
        Me.BuildLogControl1.ShowDotYellow = True
        Me.BuildLogControl1.ShowHeader = True
        Me.BuildLogControl1.ShowProgress = True
        Me.BuildLogControl1.ShowTime = True
        Me.BuildLogControl1.Size = New System.Drawing.Size(800, 481)
        Me.BuildLogControl1.SuccessColor = System.Drawing.Color.FromArgb(CType(CType(0, Byte), Integer), CType(CType(230, Byte), Integer), CType(CType(118, Byte), Integer))
        Me.BuildLogControl1.SuccessImage = Nothing
        Me.BuildLogControl1.TabIndex = 0
        Me.BuildLogControl1.TimeColor = System.Drawing.Color.FromArgb(CType(CType(75, Byte), Integer), CType(CType(75, Byte), Integer), CType(CType(75, Byte), Integer))
        Me.BuildLogControl1.WarningColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(193, Byte), Integer), CType(CType(7, Byte), Integer))
        Me.BuildLogControl1.WarningImage = Nothing
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 481)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.StartBuild)
        Me.Controls.Add(Me.BuildLogWorkerStop)
        Me.Controls.Add(Me.BuildLogWorkerbutton)
        Me.Controls.Add(Me.BuildLogControl1)
        Me.Name = "Form1"
        Me.Text = "Form1"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents BuildLogControl1 As BuildLogControl
    Friend WithEvents BuildLogWorkerbutton As Button
    Friend WithEvents BuildLogWorkerStop As Button
    Friend WithEvents StartBuild As Button
    Friend WithEvents Button4 As Button
End Class
