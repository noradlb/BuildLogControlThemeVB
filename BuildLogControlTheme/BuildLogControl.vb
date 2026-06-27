Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports System.Collections.Generic

' =============================================================================
'  BuildLogControl  v5.0
'  [NEW-1] Queue-based animation: الصفوف تظهر بتتابع بفاصل زمني بينها
'  [NEW-2] ShowDotRed / ShowDotYellow / ShowDotGreen properties للـ Header dots
'  [FIX]   كل مشاكل Transparent السابقة محلولة
' =============================================================================

<Description("Animated build log panel with RTL, progress bar, queued fade-in rows")>
<ToolboxBitmap(GetType(Panel))>
Public Class BuildLogControl
    Inherits Panel

#Region "Enum"

    Public Enum LogType
        Success
        [Error]
        Warning
        Info
        Final
    End Enum

#End Region

#Region "AnimatedProgressBar"

    Private Class AnimatedProgressBar
        Inherits Control

        Private _value As Integer = 0
        Private _displayValue As Double = 0
        Private _animTimer As System.Windows.Forms.Timer
        Private _primaryColor As Color = Color.FromArgb(0, 230, 118)
        Private _trackColor As Color = Color.FromArgb(35, 35, 35)
        Private _shimmerX As Single = -0.5F

        Public Sub New()
            Me.SetStyle(
                ControlStyles.AllPaintingInWmPaint Or
                ControlStyles.UserPaint Or
                ControlStyles.OptimizedDoubleBuffer Or
                ControlStyles.ResizeRedraw, True)
            Me.Height = 4
            Me.BackColor = Color.FromArgb(35, 35, 35)
            _animTimer = New System.Windows.Forms.Timer()
            _animTimer.Interval = 16
            AddHandler _animTimer.Tick, AddressOf OnAnimTick
            _animTimer.Start()
        End Sub

        Private Sub OnAnimTick(sender As Object, e As EventArgs)
            Dim diff As Double = _value - _displayValue
            If Math.Abs(diff) < 0.15 Then
                _displayValue = _value
            Else
                _displayValue += diff * 0.1
            End If
            _shimmerX += 0.013F
            If _shimmerX > 1.5F Then _shimmerX = -0.5F
            Me.Invalidate()
        End Sub

        Public Property ProgressValue As Integer
            Get
                Return _value
            End Get
            Set(v As Integer)
                _value = Math.Max(0, Math.Min(100, v))
            End Set
        End Property

        Public Property PrimaryColor As Color
            Get
                Return _primaryColor
            End Get
            Set(v As Color)
                _primaryColor = v
                Me.Invalidate()
            End Set
        End Property

        Public Property TrackColor As Color
            Get
                Return _trackColor
            End Get
            Set(v As Color)
                _trackColor = v
                Me.BackColor = v
                Me.Invalidate()
            End Set
        End Property

        Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
            Using b As New SolidBrush(_trackColor)
                e.Graphics.FillRectangle(b, Me.ClientRectangle)
            End Using
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            If Me.Width < 2 OrElse Me.Height < 1 Then Return
            Dim g As Graphics = e.Graphics
            g.SmoothingMode = SmoothingMode.AntiAlias

            Using b As New SolidBrush(_trackColor)
                g.FillRectangle(b, 0, 0, Me.Width, Me.Height)
            End Using

            Dim fillW As Integer = CInt((Me.Width * _displayValue) / 100.0)
            If fillW <= Me.Height Then Return

            Dim rect As New RectangleF(0, 0, fillW, Me.Height)
            Using path As New GraphicsPath()
                path.AddArc(0, 0, Me.Height, Me.Height, 90, 180)
                path.AddArc(fillW - Me.Height, 0, Me.Height, Me.Height, 270, 180)
                path.CloseFigure()

                Dim c2 As Color = Color.FromArgb(
                    CInt(_primaryColor.R * 0.55),
                    Math.Min(255, CInt(_primaryColor.G * 0.88)),
                    Math.Min(255, CInt(_primaryColor.B * 1.15)))

                Using gb As New LinearGradientBrush(rect, _primaryColor, c2, LinearGradientMode.Horizontal)
                    g.FillPath(gb, path)
                End Using

                Dim shimW As Single = fillW * 0.4F
                If shimW < 2.0F Then shimW = 2.0F
                Dim shimX As Single = CSng(fillW * _shimmerX - shimW / 2)
                Dim shimRect As New RectangleF(shimX, 0, shimW, CSng(Me.Height))

                If shimRect.Right > 0 AndAlso shimRect.Left < fillW AndAlso shimRect.Width > 1 Then
                    Try
                        Using shimBrush As New LinearGradientBrush(shimRect,
                            Color.FromArgb(0, Color.White),
                            Color.FromArgb(70, Color.White),
                            LinearGradientMode.Horizontal)
                            Dim bl As New Blend()
                            bl.Factors = {0.0F, 1.0F, 0.0F}
                            bl.Positions = {0.0F, 0.5F, 1.0F}
                            shimBrush.Blend = bl
                            g.FillPath(shimBrush, path)
                        End Using
                    Catch
                    End Try
                End If
            End Using
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            If disposing Then
                If _animTimer IsNot Nothing Then
                    _animTimer.Stop()
                    _animTimer.Dispose()
                    _animTimer = Nothing
                End If
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class

#End Region

#Region "LogRowLabel"

    Private Class LogRowLabel
        Inherits Label

        Private _isHovered As Boolean = False
        Private _rowBgColor As Color = Color.FromArgb(18, 18, 18)

        Public Sub New(bgColor As Color)
            _rowBgColor = bgColor
            Me.SetStyle(
                ControlStyles.AllPaintingInWmPaint Or
                ControlStyles.UserPaint Or
                ControlStyles.OptimizedDoubleBuffer, True)
            Me.BackColor = bgColor
        End Sub

        Public Property RowBgColor As Color
            Get
                Return _rowBgColor
            End Get
            Set(v As Color)
                _rowBgColor = v
                Me.BackColor = v
                Me.Invalidate()
            End Set
        End Property

        Protected Overrides Sub OnMouseEnter(e As EventArgs)
            _isHovered = True
            Me.Invalidate()
            MyBase.OnMouseEnter(e)
        End Sub

        Protected Overrides Sub OnMouseLeave(e As EventArgs)
            _isHovered = False
            Me.Invalidate()
            MyBase.OnMouseLeave(e)
        End Sub

        Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)
            Using b As New SolidBrush(_rowBgColor)
                e.Graphics.FillRectangle(b, Me.ClientRectangle)
            End Using
        End Sub

        Protected Overrides Sub OnPaint(e As PaintEventArgs)
            Using b As New SolidBrush(_rowBgColor)
                e.Graphics.FillRectangle(b, Me.ClientRectangle)
            End Using
            If _isHovered Then
                Using hb As New SolidBrush(Color.FromArgb(25, 255, 255, 255))
                    e.Graphics.FillRectangle(hb, Me.ClientRectangle)
                End Using
            End If
            TextRenderer.DrawText(e.Graphics, Me.Text, Me.Font, Me.ClientRectangle,
                Me.ForeColor,
                TextFormatFlags.Right Or
                TextFormatFlags.VerticalCenter Or
                TextFormatFlags.EndEllipsis Or
                TextFormatFlags.NoPrefix)
        End Sub
    End Class

#End Region

#Region "PendingRow Structure"

    ' [NEW-1] هيكل بيانات الصف المنتظر في الـ Queue
    Private Structure PendingRow
        Public Message As String
        Public LogType As LogType
        Public Sub New(msg As String, lt As LogType)
            Message = msg
            LogType = lt
        End Sub
    End Structure

#End Region

#Region "متغيرات داخلية"

    Private _table As TableLayoutPanel
    Private _panelFooter As Panel
    Private _panelProgress As Panel
    Private _panelHeader As Panel
    Private _lblStatus As Label
    Private _lblTotalTime As Label
    Private _lblTitle As Label
    Private _progressBar As AnimatedProgressBar
    Private _lblProgressLabel As Label
    Private _lblProgressPct As Label

    ' الـ Dots في الـ Header
    Private _dotRed As Panel
    Private _dotYellow As Panel
    Private _dotGreen As Panel

    Private _rowCounter As Integer = 0
    Private _elapsedTimer As System.Windows.Forms.Timer
    Private _elapsedSeconds As Integer = 0
    Private _isBuilding As Boolean = False

    ' [NEW-1] Queue + Timer للـ sequential animation
    Private _rowQueue As New Queue(Of PendingRow)
    Private _queueTimer As System.Windows.Forms.Timer
    Private _isProcessingQueue As Boolean = False

    ' ألوان
    Private _backgroundColor As Color = Color.FromArgb(18, 18, 18)
    Private _headerColor As Color = Color.FromArgb(24, 24, 24)
    Private _footerBgColor As Color = Color.FromArgb(12, 12, 12)
    Private _successColor As Color = Color.FromArgb(0, 230, 118)
    Private _errorColor As Color = Color.FromArgb(255, 82, 82)
    Private _warningColor As Color = Color.FromArgb(255, 193, 7)
    Private _infoColor As Color = Color.FromArgb(100, 181, 246)
    Private _finalColor As Color = Color.FromArgb(90, 90, 90)
    Private _timeColor As Color = Color.FromArgb(75, 75, 75)
    Private _footerColor As Color = Color.FromArgb(110, 110, 110)
    Private _borderColor As Color = Color.FromArgb(40, 40, 40)
    Private _progressColor As Color = Color.FromArgb(0, 230, 118)
    Private _progressTrackColor As Color = Color.FromArgb(35, 35, 35)

    ' صور
    Private _successImage As Image
    Private _errorImage As Image
    Private _warningImage As Image

    ' إعدادات
    Private _fontName As String = "Segoe UI"
    Private _fontSize As Single = 8.0F
    Private _showTime As Boolean = True
    Private _showProgress As Boolean = True
    Private _showHeader As Boolean = True
    Private _headerTitle As String = "BUILD LOG"
    Private _animationDuration As Integer = 300
    Private _rowHeight As Integer = 20
    Private _maxRows As Integer = 500

    ' [NEW-2] Dot visibility
    Private _showDotRed As Boolean = True
    Private _showDotYellow As Boolean = True
    Private _showDotGreen As Boolean = True

    ' [NEW-1] فاصل زمني بين ظهور كل صف (ms)
    Private _rowDelay As Integer = 80

#End Region

#Region "Properties"

    <Category("Build Log"), Description("صورة علامة النجاح")>
    Public Property SuccessImage As Image
        Get
            Return _successImage
        End Get
        Set(v As Image)
            _successImage = v
        End Set
    End Property

    <Category("Build Log"), Description("صورة علامة الخطأ")>
    Public Property ErrorImage As Image
        Get
            Return _errorImage
        End Get
        Set(v As Image)
            _errorImage = v
        End Set
    End Property

    <Category("Build Log"), Description("صورة علامة التحذير")>
    Public Property WarningImage As Image
        Get
            Return _warningImage
        End Get
        Set(v As Image)
            _warningImage = v
        End Set
    End Property

    <Category("Build Log"), Description("إظهار الطوابع الزمنية")>
    Public Property ShowTime As Boolean
        Get
            Return _showTime
        End Get
        Set(v As Boolean)
            _showTime = v
        End Set
    End Property

    <Category("Build Log"), Description("إظهار شريط التقدم")>
    Public Property ShowProgress As Boolean
        Get
            Return _showProgress
        End Get
        Set(v As Boolean)
            _showProgress = v
            If _panelProgress IsNot Nothing Then _panelProgress.Visible = v
        End Set
    End Property

    <Category("Build Log"), Description("إظهار شريط العنوان العلوي")>
    Public Property ShowHeader As Boolean
        Get
            Return _showHeader
        End Get
        Set(v As Boolean)
            _showHeader = v
            If _panelHeader IsNot Nothing Then _panelHeader.Visible = v
        End Set
    End Property

    <Category("Build Log"), Description("نص عنوان الـ Header")>
    Public Property HeaderTitle As String
        Get
            Return _headerTitle
        End Get
        Set(v As String)
            _headerTitle = v
            If _lblTitle IsNot Nothing Then _lblTitle.Text = v
        End Set
    End Property

    ' [NEW-2] ===== Dot Properties =====

    <Category("Build Log — Header Dots"), Description("إظهار النقطة الحمراء")>
    Public Property ShowDotRed As Boolean
        Get
            Return _showDotRed
        End Get
        Set(v As Boolean)
            _showDotRed = v
            If _dotRed IsNot Nothing Then _dotRed.Visible = v
        End Set
    End Property

    <Category("Build Log — Header Dots"), Description("إظهار النقطة الصفراء")>
    Public Property ShowDotYellow As Boolean
        Get
            Return _showDotYellow
        End Get
        Set(v As Boolean)
            _showDotYellow = v
            If _dotYellow IsNot Nothing Then _dotYellow.Visible = v
        End Set
    End Property

    <Category("Build Log — Header Dots"), Description("إظهار النقطة الخضراء")>
    Public Property ShowDotGreen As Boolean
        Get
            Return _showDotGreen
        End Get
        Set(v As Boolean)
            _showDotGreen = v
            If _dotGreen IsNot Nothing Then _dotGreen.Visible = v
        End Set
    End Property

    ' [NEW-1] فاصل زمني بين كل صف
    <Category("Build Log"), Description("الفاصل الزمني بين ظهور كل صف بالمللي ثانية")>
    Public Property RowAppearDelay As Integer
        Get
            Return _rowDelay
        End Get
        Set(v As Integer)
            _rowDelay = Math.Max(0, v)
            If _queueTimer IsNot Nothing Then _queueTimer.Interval = Math.Max(1, _rowDelay)
        End Set
    End Property

    <Category("Build Log"), Description("أقصى عدد صفوف")>
    Public Property MaxRows As Integer
        Get
            Return _maxRows
        End Get
        Set(v As Integer)
            _maxRows = Math.Max(10, v)
        End Set
    End Property

    <Category("Build Log"), Description("ارتفاع كل صف")>
    Public Property RowHeight As Integer
        Get
            Return _rowHeight
        End Get
        Set(v As Integer)
            _rowHeight = Math.Max(14, v)
        End Set
    End Property

    <Category("Build Log"), Description("مدة Fade-in بالمللي ثانية (0 لتعطيله)")>
    Public Property AnimationDurationMs As Integer
        Get
            Return _animationDuration
        End Get
        Set(v As Integer)
            _animationDuration = Math.Max(0, v)
        End Set
    End Property

    <Category("Build Log"), Description("اسم الخط")>
    Public Property LogFontName As String
        Get
            Return _fontName
        End Get
        Set(v As String)
            _fontName = v
        End Set
    End Property

    <Category("Build Log"), Description("حجم الخط")>
    Public Property LogFontSize As Single
        Get
            Return _fontSize
        End Get
        Set(v As Single)
            _fontSize = Math.Max(6, v)
        End Set
    End Property

    ' ===== Colors =====

    <Category("Build Log — Colors")>
    Public Overrides Property BackColor As Color
        Get
            Return _backgroundColor
        End Get
        Set(v As Color)
            _backgroundColor = v
            MyBase.BackColor = v
            If _table IsNot Nothing Then _table.BackColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property HeaderColor As Color
        Get
            Return _headerColor
        End Get
        Set(v As Color)
            _headerColor = v
            If _panelHeader IsNot Nothing Then _panelHeader.BackColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property SuccessColor As Color
        Get
            Return _successColor
        End Get
        Set(v As Color)
            _successColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property ErrorColor As Color
        Get
            Return _errorColor
        End Get
        Set(v As Color)
            _errorColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property WarningColor As Color
        Get
            Return _warningColor
        End Get
        Set(v As Color)
            _warningColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property InfoColor As Color
        Get
            Return _infoColor
        End Get
        Set(v As Color)
            _infoColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property TimeColor As Color
        Get
            Return _timeColor
        End Get
        Set(v As Color)
            _timeColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property FooterColor As Color
        Get
            Return _footerColor
        End Get
        Set(v As Color)
            _footerColor = v
            If _lblStatus IsNot Nothing Then _lblStatus.ForeColor = v
            If _lblTotalTime IsNot Nothing Then _lblTotalTime.ForeColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property ProgressBarColor As Color
        Get
            Return _progressColor
        End Get
        Set(v As Color)
            _progressColor = v
            If _progressBar IsNot Nothing Then _progressBar.PrimaryColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property ProgressTrackColor As Color
        Get
            Return _progressTrackColor
        End Get
        Set(v As Color)
            _progressTrackColor = v
            If _progressBar IsNot Nothing Then _progressBar.TrackColor = v
        End Set
    End Property

    <Category("Build Log — Colors")>
    Public Property BorderColor As Color
        Get
            Return _borderColor
        End Get
        Set(v As Color)
            _borderColor = v
            Me.Invalidate()
        End Set
    End Property

    ' ===== ReadOnly =====

    <Browsable(False)>
    Public ReadOnly Property LogRowCount As Integer
        Get
            Return _rowCounter
        End Get
    End Property

    <Browsable(False)>
    Public ReadOnly Property IsBuilding As Boolean
        Get
            Return _isBuilding
        End Get
    End Property

    <Browsable(False)>
    Public ReadOnly Property ElapsedSeconds As Integer
        Get
            Return System.Threading.Interlocked.CompareExchange(_elapsedSeconds, 0, 0)
        End Get
    End Property

#End Region

#Region "Constructor & Init"

    Public Sub New()
        Me.SetStyle(
            ControlStyles.AllPaintingInWmPaint Or
            ControlStyles.UserPaint Or
            ControlStyles.ResizeRedraw Or
            ControlStyles.OptimizedDoubleBuffer, True)
        Me.UpdateStyles()
        MyBase.BackColor = _backgroundColor
        Me.Dock = DockStyle.Fill
        Me.AutoScroll = True
        Me.Size = New Size(400, 180)
        Me.Padding = New Padding(0)

        ' Elapsed timer
        _elapsedTimer = New System.Windows.Forms.Timer()
        _elapsedTimer.Interval = 1000
        AddHandler _elapsedTimer.Tick, AddressOf OnElapsedTick

        ' [NEW-1] Queue timer — يُطلق صفاً واحداً كل _rowDelay ms
        _queueTimer = New System.Windows.Forms.Timer()
        _queueTimer.Interval = Math.Max(1, _rowDelay)
        AddHandler _queueTimer.Tick, AddressOf OnQueueTick

        InitializeComponents()
    End Sub

    Private Sub InitializeComponents()
        Me.Controls.Clear()
        SuspendLayout()

        ' ==========================================
        ' 1. Header
        ' ==========================================
        _panelHeader = New Panel()
        _panelHeader.Dock = DockStyle.Top
        _panelHeader.Height = 32
        _panelHeader.BackColor = _headerColor
        _panelHeader.Visible = _showHeader

        ' [NEW-2] Red dot
        _dotRed = New Panel()
        _dotRed.Size = New Size(10, 10)
        _dotRed.Location = New Point(10, 11)
        _dotRed.BackColor = Color.FromArgb(255, 95, 87)
        _dotRed.Visible = _showDotRed
        Dim gpR As New GraphicsPath()
        gpR.AddEllipse(0, 0, 10, 10)
        _dotRed.Region = New System.Drawing.Region(gpR)
        _panelHeader.Controls.Add(_dotRed)

        ' [NEW-2] Yellow dot
        _dotYellow = New Panel()
        _dotYellow.Size = New Size(10, 10)
        _dotYellow.Location = New Point(26, 11)
        _dotYellow.BackColor = Color.FromArgb(255, 189, 46)
        _dotYellow.Visible = _showDotYellow
        Dim gpY As New GraphicsPath()
        gpY.AddEllipse(0, 0, 10, 10)
        _dotYellow.Region = New System.Drawing.Region(gpY)
        _panelHeader.Controls.Add(_dotYellow)

        ' [NEW-2] Green dot
        _dotGreen = New Panel()
        _dotGreen.Size = New Size(10, 10)
        _dotGreen.Location = New Point(42, 11)
        _dotGreen.BackColor = Color.FromArgb(39, 200, 63)
        _dotGreen.Visible = _showDotGreen
        Dim gpG As New GraphicsPath()
        gpG.AddEllipse(0, 0, 10, 10)
        _dotGreen.Region = New System.Drawing.Region(gpG)
        _panelHeader.Controls.Add(_dotGreen)

        _lblTitle = New Label()
        _lblTitle.Text = _headerTitle
        _lblTitle.ForeColor = Color.FromArgb(80, 80, 80)
        _lblTitle.Font = New Font(_fontName, 7.5F, FontStyle.Bold)
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter
        _lblTitle.Dock = DockStyle.Fill
        _lblTitle.BackColor = _headerColor
        _panelHeader.Controls.Add(_lblTitle)
        _lblTitle.SendToBack()
        Me.Controls.Add(_panelHeader)

        ' ==========================================
        ' 2. Progress
        ' ==========================================
        _panelProgress = New Panel()
        _panelProgress.Dock = DockStyle.Top
        _panelProgress.Height = 26
        _panelProgress.BackColor = Color.FromArgb(14, 14, 14)
        _panelProgress.Visible = _showProgress

        Dim progressTopPanel As New Panel()
        progressTopPanel.Dock = DockStyle.Top
        progressTopPanel.Height = 16
        progressTopPanel.BackColor = Color.FromArgb(14, 14, 14)
        progressTopPanel.Padding = New Padding(8, 2, 8, 0)

        _lblProgressLabel = New Label()
        _lblProgressLabel.Text = "جاهز"
        _lblProgressLabel.ForeColor = Color.FromArgb(85, 85, 85)
        _lblProgressLabel.Font = New Font(_fontName, Math.Max(6, _fontSize - 2.0F), FontStyle.Regular)
        _lblProgressLabel.TextAlign = ContentAlignment.MiddleRight
        _lblProgressLabel.Dock = DockStyle.Fill
        _lblProgressLabel.BackColor = Color.FromArgb(14, 14, 14)
        _lblProgressLabel.RightToLeft = RightToLeft.Yes
        progressTopPanel.Controls.Add(_lblProgressLabel)

        _lblProgressPct = New Label()
        _lblProgressPct.Text = "0%"
        _lblProgressPct.ForeColor = Color.FromArgb(65, 65, 65)
        _lblProgressPct.Font = New Font(_fontName, Math.Max(6, _fontSize - 2.0F), FontStyle.Bold)
        _lblProgressPct.TextAlign = ContentAlignment.MiddleLeft
        _lblProgressPct.Width = 36
        _lblProgressPct.Dock = DockStyle.Left
        _lblProgressPct.BackColor = Color.FromArgb(14, 14, 14)
        progressTopPanel.Controls.Add(_lblProgressPct)

        _progressBar = New AnimatedProgressBar()
        _progressBar.Dock = DockStyle.Bottom
        _progressBar.Height = 4
        _progressBar.Margin = New Padding(8, 0, 8, 4)
        _progressBar.PrimaryColor = _progressColor
        _progressBar.TrackColor = _progressTrackColor

        _panelProgress.Controls.Add(_progressBar)
        _panelProgress.Controls.Add(progressTopPanel)
        Me.Controls.Add(_panelProgress)

        ' ==========================================
        ' 3. Table
        ' ==========================================
        _table = New TableLayoutPanel()
        _table.Dock = DockStyle.Top
        _table.AutoSize = True
        _table.AutoSizeMode = AutoSizeMode.GrowAndShrink
        _table.BackColor = _backgroundColor
        _table.RightToLeft = RightToLeft.Yes
        _table.Padding = New Padding(4, 3, 4, 3)
        _table.ColumnCount = 3
        _table.ColumnStyles.Clear()
        _table.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 20))
        _table.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100))
        _table.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, 70))
        _table.RowCount = 0
        _table.RowStyles.Clear()
        Me.Controls.Add(_table)

        ' ==========================================
        ' 4. Footer
        ' ==========================================
        _panelFooter = New Panel()
        _panelFooter.Dock = DockStyle.Bottom
        _panelFooter.Height = 28
        _panelFooter.BackColor = _footerBgColor
        _panelFooter.Padding = New Padding(8, 0, 10, 0)

        Dim footerSep As New Panel()
        footerSep.Dock = DockStyle.Top
        footerSep.Height = 1
        footerSep.BackColor = Color.FromArgb(36, 36, 36)
        _panelFooter.Controls.Add(footerSep)

        _lblStatus = New Label()
        _lblStatus.Text = "● جاهز"
        _lblStatus.ForeColor = Color.FromArgb(85, 85, 85)
        _lblStatus.Font = New Font(_fontName, Math.Max(6, _fontSize - 1.0F), FontStyle.Bold)
        _lblStatus.TextAlign = ContentAlignment.MiddleRight
        _lblStatus.Dock = DockStyle.Fill
        _lblStatus.BackColor = _footerBgColor
        _lblStatus.RightToLeft = RightToLeft.Yes
        _panelFooter.Controls.Add(_lblStatus)

        _lblTotalTime = New Label()
        _lblTotalTime.Text = "00:00:00"
        _lblTotalTime.ForeColor = Color.FromArgb(55, 55, 55)
        _lblTotalTime.Font = New Font(_fontName, Math.Max(6, _fontSize - 2.0F), FontStyle.Regular)
        _lblTotalTime.TextAlign = ContentAlignment.MiddleLeft
        _lblTotalTime.Dock = DockStyle.Fill
        _lblTotalTime.BackColor = _footerBgColor
        _lblTotalTime.RightToLeft = RightToLeft.No
        _panelFooter.Controls.Add(_lblTotalTime)

        Me.Controls.Add(_panelFooter)
        _panelFooter.SendToBack()

        ResumeLayout(True)
    End Sub

#End Region

#Region "Queue System — [NEW-1]"

    ' يُضيف الرسالة للـ Queue ويُشغّل الـ Timer إذا كان واقفاً
    Private Sub EnqueueRow(message As String, logType As LogType)
        _rowQueue.Enqueue(New PendingRow(message, logType))
        If Not _queueTimer.Enabled Then
            _queueTimer.Start()
        End If
    End Sub

    ' كل Tick يُخرج صفاً واحداً من الـ Queue ويرسمه
    Private Sub OnQueueTick(sender As Object, e As EventArgs)
        If _rowQueue.Count = 0 Then
            _queueTimer.Stop()
            Return
        End If

        Dim row As PendingRow = _rowQueue.Dequeue()
        RenderRow(row.Message, row.LogType)
    End Sub

#End Region

#Region "عداد الوقت"

    Private Sub OnElapsedTick(sender As Object, e As EventArgs)
        System.Threading.Interlocked.Increment(_elapsedSeconds)
        Dim s As Integer = _elapsedSeconds
        Dim h As Integer = s \ 3600
        Dim m As Integer = (s Mod 3600) \ 60
        Dim sec As Integer = s Mod 60
        If _lblTotalTime IsNot Nothing Then
            _lblTotalTime.Text = String.Format("{0:00}:{1:00}:{2:00}", h, m, sec)
        End If
    End Sub

#End Region

#Region "Public API — إضافة السجلات"

    Public Sub AddSuccessLog(message As String)
        EnqueueRow(message, LogType.Success)
    End Sub

    Public Sub AddErrorLog(message As String)
        EnqueueRow(message, LogType.Error)
    End Sub

    Public Sub AddWarningLog(message As String)
        EnqueueRow(message, LogType.Warning)
    End Sub

    Public Sub AddInfoLog(message As String)
        EnqueueRow(message, LogType.Info)
    End Sub

    Public Sub AddFinalLog(message As String)
        EnqueueRow(message, LogType.Final)
    End Sub

    Public Sub AddLog(message As String, logType As LogType)
        EnqueueRow(message, logType)
    End Sub

    ' الرسم الفعلي — يُستدعى فقط من OnQueueTick
    Private Sub RenderRow(message As String, logType As LogType)
        If _table Is Nothing Then InitializeComponents()
        If _rowCounter >= _maxRows Then RemoveFirstRow()

        _table.SuspendLayout()
        _table.RowCount += 1
        _table.RowStyles.Add(New RowStyle(SizeType.Absolute, _rowHeight))
        Dim currentRow As Integer = _table.RowCount - 1

        Dim rowColor As Color
        Dim rowImage As Image = Nothing
        Dim isFinal As Boolean = (logType = LogType.Final)
        Dim iconChar As String

        Select Case logType
            Case LogType.Success
                rowColor = _successColor
                rowImage = _successImage
                iconChar = "v"
            Case LogType.Error
                rowColor = _errorColor
                rowImage = _errorImage
                iconChar = "x"
            Case LogType.Warning
                rowColor = _warningColor
                rowImage = _warningImage
                iconChar = "!"
            Case LogType.Info
                rowColor = _infoColor
                iconChar = "i"
            Case LogType.Final
                rowColor = _finalColor
                iconChar = "-"
            Case Else
                rowColor = _footerColor
                iconChar = "."
        End Select

        ' --- أيقونة ---
        If rowImage IsNot Nothing Then
            Dim picBox As New PictureBox()
            picBox.SizeMode = PictureBoxSizeMode.Zoom
            picBox.Dock = DockStyle.Fill
            picBox.BackColor = _backgroundColor
            picBox.Margin = New Padding(0, 2, 2, 2)
            picBox.Image = rowImage
            _table.Controls.Add(picBox, 0, currentRow)
        Else
            Dim lblIcon As New Label()
            lblIcon.Text = iconChar
            lblIcon.ForeColor = If(isFinal, Color.FromArgb(50, 50, 50), rowColor)
            lblIcon.Font = New Font(_fontName, Math.Max(6, _fontSize - 1.5F), FontStyle.Bold)
            lblIcon.TextAlign = ContentAlignment.MiddleCenter
            lblIcon.Dock = DockStyle.Fill
            lblIcon.BackColor = _backgroundColor
            lblIcon.Margin = New Padding(0)
            _table.Controls.Add(lblIcon, 0, currentRow)
        End If

        ' --- نص ---
        Dim lblText As New LogRowLabel(_backgroundColor)
        lblText.Text = message
        lblText.TextAlign = ContentAlignment.MiddleRight
        lblText.Dock = DockStyle.Fill
        lblText.Margin = New Padding(0, 1, 4, 1)
        lblText.AutoSize = False
        lblText.ForeColor = If(isFinal, _finalColor, rowColor)
        lblText.Font = New Font(_fontName, _fontSize,
                                If(isFinal, FontStyle.Bold, FontStyle.Regular))
        _table.Controls.Add(lblText, 1, currentRow)

        ' --- وقت ---
        Dim lblTime As New Label()
        If _showTime AndAlso Not isFinal Then
            lblTime.Text = DateTime.Now.ToString("hh:mm:ss")
        Else
            lblTime.Text = ""
        End If
        lblTime.ForeColor = _timeColor
        lblTime.Font = New Font(_fontName, Math.Max(6, _fontSize - 2.0F), FontStyle.Regular)
        lblTime.TextAlign = ContentAlignment.MiddleLeft
        lblTime.Dock = DockStyle.Fill
        lblTime.BackColor = _backgroundColor
        lblTime.Margin = New Padding(0, 1, 2, 1)
        lblTime.AutoSize = False
        _table.Controls.Add(lblTime, 2, currentRow)

        _table.ResumeLayout(True)

        ' Fade-in للصف الحالي
        If _animationDuration > 0 Then
            FadeInRow(lblText, lblTime)
        End If

        Me.ScrollControlIntoView(lblText)
        _rowCounter += 1
    End Sub

    Private Sub RemoveFirstRow()
        If _table Is Nothing OrElse _table.RowCount < 1 Then Return
        _table.SuspendLayout()

        Dim toRemove As New List(Of Control)
        For Each ctrl As Control In _table.Controls
            If _table.GetRow(ctrl) = 0 Then toRemove.Add(ctrl)
        Next
        For Each ctrl As Control In toRemove
            _table.Controls.Remove(ctrl)
            ctrl.Dispose()
        Next
        For Each ctrl As Control In _table.Controls
            Dim r As Integer = _table.GetRow(ctrl)
            If r > 0 Then _table.SetRow(ctrl, r - 1)
        Next
        If _table.RowStyles.Count > 0 Then _table.RowStyles.RemoveAt(0)
        _table.RowCount = Math.Max(0, _table.RowCount - 1)
        _table.ResumeLayout(True)
        _rowCounter = Math.Max(0, _rowCounter - 1)
    End Sub

#End Region

#Region "Public API — التحكم في البناء"

    Public Sub StartBuild(Optional label As String = "البناء جارٍ...")
        System.Threading.Interlocked.Exchange(_elapsedSeconds, 0)
        _isBuilding = True
        _elapsedTimer.Start()
        SetProgress(0, label)
        SetStatusText("● يعمل", _successColor)
    End Sub

    Public Sub StopBuild(Optional success As Boolean = True)
        _isBuilding = False
        _elapsedTimer.Stop()
        If success Then
            SetStatusText("● اكتمل", _successColor)
        Else
            SetStatusText("● فشل", _errorColor)
        End If
    End Sub

    Public Sub SetProgress(value As Integer, Optional label As String = "")
        Dim clamped As Integer = Math.Max(0, Math.Min(100, value))
        If _progressBar IsNot Nothing Then _progressBar.ProgressValue = clamped
        If _lblProgressPct IsNot Nothing Then _lblProgressPct.Text = clamped & "%"
        If _lblProgressLabel IsNot Nothing AndAlso label.Length > 0 Then
            _lblProgressLabel.Text = label
        End If
    End Sub

    Public Sub SetFooter(status As String, Optional totalTime As String = "")
        SetStatusText("● " & status, _footerColor)
        If totalTime.Length > 0 AndAlso _lblTotalTime IsNot Nothing Then
            _lblTotalTime.Text = totalTime
        End If
    End Sub

    Public Sub ClearLogs()
        ' إيقاف الـ Queue أولاً
        _queueTimer.Stop()
        _rowQueue.Clear()

        If _table IsNot Nothing Then
            _table.SuspendLayout()
            _table.Controls.Clear()
            _table.RowCount = 0
            _table.RowStyles.Clear()
            _table.ResumeLayout(False)
        End If
        _rowCounter = 0
        _elapsedTimer.Stop()
        System.Threading.Interlocked.Exchange(_elapsedSeconds, 0)
        _isBuilding = False
        SetProgress(0, "جاهز")
        SetStatusText("● جاهز", Color.FromArgb(85, 85, 85))
        If _lblTotalTime IsNot Nothing Then _lblTotalTime.Text = "00:00:00"
    End Sub

    Public Function ExportLogs() As String
        Dim sb As New System.Text.StringBuilder()
        If _table Is Nothing Then Return sb.ToString()
        For Each ctrl As Control In _table.Controls
            If TypeOf ctrl Is LogRowLabel Then
                sb.AppendLine(ctrl.Text)
            End If
        Next
        Return sb.ToString()
    End Function

#End Region

#Region "Fade-in Animation"

    Private Sub FadeInRow(lbl As Control, lTime As Control)
        Dim steps As Integer = 10
        Dim intervalMs As Integer = Math.Max(1, _animationDuration \ steps)
        Dim baseColor As Color = lbl.ForeColor
        Dim baseTimeColor As Color = lTime.ForeColor
        Dim counter As Integer = 0

        ' نبدأ من alpha = 0
        lbl.ForeColor = Color.FromArgb(0, baseColor)
        lTime.ForeColor = Color.FromArgb(0, baseTimeColor)

        Dim t As New System.Windows.Forms.Timer()
        t.Interval = intervalMs

        AddHandler t.Tick, Sub(s, e)
                               If lbl.IsDisposed OrElse lTime.IsDisposed Then
                                   DirectCast(s, System.Windows.Forms.Timer).Stop()
                                   DirectCast(s, System.Windows.Forms.Timer).Dispose()
                                   Return
                               End If
                               counter += 1
                               Dim prog As Double = counter / steps
                               Dim eased As Double = 1.0 - Math.Pow(1.0 - prog, 3)
                               Dim alpha As Integer = Math.Max(0, Math.Min(255, CInt(255 * eased)))
                               lbl.ForeColor = Color.FromArgb(alpha, baseColor)
                               lTime.ForeColor = Color.FromArgb(alpha, baseTimeColor)
                               If counter >= steps Then
                                   lbl.ForeColor = baseColor
                                   lTime.ForeColor = baseTimeColor
                                   DirectCast(s, System.Windows.Forms.Timer).Stop()
                                   DirectCast(s, System.Windows.Forms.Timer).Dispose()
                               End If
                           End Sub

        t.Start()
    End Sub

#End Region

#Region "Helpers / Paint / Dispose"

    Private Sub SetStatusText(text As String, col As Color)
        If _lblStatus IsNot Nothing Then
            _lblStatus.Text = text
            _lblStatus.ForeColor = col
        End If
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias
        Using pen As New Pen(_borderColor, 1)
            e.Graphics.DrawRectangle(pen, 0, 0, Me.Width - 1, Me.Height - 1)
        End Using
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)
        If _table IsNot Nothing Then _table.Width = Me.Width - 8
    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then
            If _elapsedTimer IsNot Nothing Then
                _elapsedTimer.Stop()
                _elapsedTimer.Dispose()
                _elapsedTimer = Nothing
            End If
            If _queueTimer IsNot Nothing Then
                _queueTimer.Stop()
                _queueTimer.Dispose()
                _queueTimer = Nothing
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

#End Region

End Class

' =============================================================================
'  BuildLogHelper
' =============================================================================
Public Class BuildLogHelper

    Private ReadOnly _ctrl As BuildLogControl

    Public Sub New(control As BuildLogControl)
        _ctrl = control
    End Sub

    Public Sub StartBuild(Optional label As String = "البناء جارٍ...")
        InvokeOnUI(Sub()
                       _ctrl.ClearLogs()
                       _ctrl.StartBuild(label)
                   End Sub)
    End Sub

    Public Sub Ok(message As String, Optional progress As Integer = -1)
        InvokeOnUI(Sub()
                       _ctrl.AddSuccessLog(message)
                       If progress >= 0 Then _ctrl.SetProgress(progress)
                   End Sub)
    End Sub

    Public Sub Fail(message As String, Optional progress As Integer = -1)
        InvokeOnUI(Sub()
                       _ctrl.AddErrorLog(message)
                       If progress >= 0 Then _ctrl.SetProgress(progress)
                   End Sub)
    End Sub

    Public Sub Warn(message As String, Optional progress As Integer = -1)
        InvokeOnUI(Sub()
                       _ctrl.AddWarningLog(message)
                       If progress >= 0 Then _ctrl.SetProgress(progress)
                   End Sub)
    End Sub

    Public Sub Info(message As String, Optional progress As Integer = -1)
        InvokeOnUI(Sub()
                       _ctrl.AddInfoLog(message)
                       If progress >= 0 Then _ctrl.SetProgress(progress)
                   End Sub)
    End Sub

    Public Sub Finish(success As Boolean, Optional summaryMessage As String = "")
        InvokeOnUI(Sub()
                       _ctrl.SetProgress(100, If(success, "اكتمل", "فشل"))
                       If summaryMessage.Length > 0 Then _ctrl.AddFinalLog(summaryMessage)
                       _ctrl.StopBuild(success)
                   End Sub)
    End Sub

    Private Sub InvokeOnUI(action As Action)
        If _ctrl.IsDisposed Then Return
        If _ctrl.InvokeRequired Then
            Try
                _ctrl.Invoke(action)
            Catch ex As ObjectDisposedException
            End Try
        Else
            action()
        End If
    End Sub

End Class

' =============================================================================
'  BuildLogWorker
' =============================================================================
Public Class BuildLogWorker

    Public Class BuildContext
        Private ReadOnly _ctrl As BuildLogControl
        Private ReadOnly _cts As System.Threading.CancellationTokenSource

        Friend Sub New(ctrl As BuildLogControl, cts As System.Threading.CancellationTokenSource)
            _ctrl = ctrl
            _cts = cts
        End Sub

        Public ReadOnly Property IsCancelled As Boolean
            Get
                Return _cts.IsCancellationRequested
            End Get
        End Property

        Public ReadOnly Property CancellationToken As System.Threading.CancellationToken
            Get
                Return _cts.Token
            End Get
        End Property

        Public Sub Log(message As String,
                       Optional logType As BuildLogControl.LogType = BuildLogControl.LogType.Info,
                       Optional progress As Integer = -1)
            InvokeOnUI(Sub()
                           _ctrl.AddLog(message, logType)
                           If progress >= 0 Then _ctrl.SetProgress(progress)
                       End Sub)
        End Sub

        Public Sub ReportProgress(value As Integer, Optional label As String = "")
            InvokeOnUI(Sub() _ctrl.SetProgress(value, label))
        End Sub

        Public Sub SetStatus(text As String)
            InvokeOnUI(Sub() _ctrl.SetFooter(text))
        End Sub

        Private Sub InvokeOnUI(action As Action)
            If _ctrl.IsDisposed Then Return
            If _ctrl.InvokeRequired Then
                Try
                    _ctrl.Invoke(action)
                Catch ex As ObjectDisposedException
                End Try
            Else
                action()
            End If
        End Sub
    End Class

    Private ReadOnly _ctrl As BuildLogControl
    Private _cts As System.Threading.CancellationTokenSource
    Private _isRunning As Boolean = False

    Public Event BuildCompleted(sender As Object, success As Boolean, elapsedSeconds As Integer)
    Public Event BuildCancelled(sender As Object)
    Public Event BuildFailed(sender As Object, ex As Exception)

    Public ReadOnly Property IsRunning As Boolean
        Get
            Return _isRunning
        End Get
    End Property

    Public Sub New(control As BuildLogControl)
        _ctrl = control
    End Sub

    Public Async Function RunAsync(
        buildTask As Func(Of BuildContext, System.Threading.Tasks.Task),
        Optional startLabel As String = "البناء جارٍ..."
    ) As System.Threading.Tasks.Task(Of Boolean)

        If _isRunning Then
            Throw New InvalidOperationException("توجد مهمة بناء قيد التشغيل بالفعل.")
        End If

        _isRunning = True
        _cts = New System.Threading.CancellationTokenSource()
        Dim ctx As New BuildContext(_ctrl, _cts)

        InvokeOnUI(Sub()
                       _ctrl.ClearLogs()
                       _ctrl.StartBuild(startLabel)
                   End Sub)

        Try
            Await System.Threading.Tasks.Task.Run(
                Async Function()
                    Await buildTask(ctx).ConfigureAwait(False)
                End Function,
                _cts.Token).ConfigureAwait(False)

        Catch ex As System.Threading.Tasks.TaskCanceledException
            ' [FIX-THREAD] كل RaiseEvent داخل InvokeOnUI لضمان تشغيله على UI thread
            InvokeOnUI(Sub()
                           _ctrl.AddLog("── تم إلغاء البناء ──", BuildLogControl.LogType.Warning)
                           _ctrl.StopBuild(False)
                           RaiseEvent BuildCancelled(Me)
                       End Sub)
            Return False

        Catch ex As System.OperationCanceledException
            InvokeOnUI(Sub()
                           _ctrl.AddLog("── تم إلغاء البناء ──", BuildLogControl.LogType.Warning)
                           _ctrl.StopBuild(False)
                           RaiseEvent BuildCancelled(Me)
                       End Sub)
            Return False

        Catch ex As AggregateException
            Dim inner As Exception = ex.Flatten().InnerExceptions(0)
            If TypeOf inner Is System.OperationCanceledException Then
                InvokeOnUI(Sub()
                               _ctrl.AddLog("── تم إلغاء البناء ──", BuildLogControl.LogType.Warning)
                               _ctrl.StopBuild(False)
                               RaiseEvent BuildCancelled(Me)
                           End Sub)
            Else
                Dim captured As Exception = inner
                InvokeOnUI(Sub()
                               _ctrl.AddLog("خطأ: " & captured.Message, BuildLogControl.LogType.Error)
                               _ctrl.StopBuild(False)
                               RaiseEvent BuildFailed(Me, captured)
                           End Sub)
            End If
            Return False

        Catch ex As Exception
            Dim captured As Exception = ex
            InvokeOnUI(Sub()
                           _ctrl.AddLog("خطأ غير متوقع: " & captured.Message, BuildLogControl.LogType.Error)
                           _ctrl.StopBuild(False)
                           RaiseEvent BuildFailed(Me, captured)
                       End Sub)
            Return False

        Finally
            _isRunning = False
            If _cts IsNot Nothing Then
                _cts.Dispose()
                _cts = Nothing
            End If
        End Try

        Dim elapsed As Integer = _ctrl.ElapsedSeconds
        InvokeOnUI(Sub()
                       _ctrl.AddFinalLog(String.Format("── اكتمل في {0} ثانية ──", elapsed))
                       _ctrl.StopBuild(True)
                       RaiseEvent BuildCompleted(Me, True, elapsed)
                   End Sub)
        Return True
    End Function

    Public Sub Cancel()
        If _isRunning AndAlso _cts IsNot Nothing Then _cts.Cancel()
    End Sub

    Private Sub InvokeOnUI(action As Action)
        If _ctrl.IsDisposed Then Return
        If _ctrl.InvokeRequired Then
            Try
                _ctrl.Invoke(action)
            Catch ex As ObjectDisposedException
            End Try
        Else
            action()
        End If
    End Sub

End Class