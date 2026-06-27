Public Class Form1

    ' [FIX-1] _worker كـ field وليس داخل الـ Sub
    Private _worker As BuildLogWorker

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' مثال بسيط عند التحميل
        BuildLogControl1.AddInfoLog("جاهز للبناء")
    End Sub

    Private Async Sub Button1_Click(sender As Object, e As EventArgs) Handles BuildLogWorkerbutton.Click
        BuildLogWorkerbutton.Enabled = False

        ' [FIX-2] استخدام _worker وليس worker محلي حتى يعمل زر الإلغاء
        _worker = New BuildLogWorker(BuildLogControl1)

        ' [FIX-3] الأحداث تعمل الآن على UI thread من داخل BuildLogWorker
        '         لذا يمكنك تعديل الـ Controls مباشرة بدون Invoke
        AddHandler _worker.BuildCompleted, Sub(s, success, elapsed)
                                               BuildLogWorkerbutton.Enabled = True
                                               MessageBox.Show("اكتمل في " & elapsed & " ثانية")
                                           End Sub

        AddHandler _worker.BuildFailed, Sub(s, ex)
                                            BuildLogWorkerbutton.Enabled = True
                                            MessageBox.Show("خطأ: " & ex.Message)
                                        End Sub

        AddHandler _worker.BuildCancelled, Sub(s)
                                               BuildLogWorkerbutton.Enabled = True
                                               MessageBox.Show("تم الإلغاء")
                                           End Sub

        Await _worker.RunAsync(Async Function(ctx)
                                   ctx.Log("تحميل الملفات...", BuildLogControl.LogType.Info, 10)
                                   Await Task.Delay(800)

                                   ctx.Log("تجميع الكود", BuildLogControl.LogType.Success, 40)
                                   Await Task.Delay(1000)

                                   If ctx.IsCancelled Then Return

                                   ctx.Log("رفع الملفات", BuildLogControl.LogType.Success, 75)
                                   Await Task.Delay(600)

                                   ctx.Log("التحقق النهائي", BuildLogControl.LogType.Success, 95)
                                   Await Task.Delay(400)

                               End Function, "جاري البناء الكامل...")
    End Sub

    Private Sub BuildLogWorkerStop_Click(sender As Object, e As EventArgs) Handles BuildLogWorkerStop.Click
        ' [FIX-4] تحقق أن _worker موجود قبل الإلغاء
        If _worker IsNot Nothing Then
            _worker.Cancel()
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles StartBuild.Click
        BuildLogControl1.ClearLogs()
        BuildLogControl1.StartBuild("جاري البناء...")

        BuildLogControl1.AddSuccessLog("تم تحميل الملفات")
        BuildLogControl1.AddWarningLog("تحذير: إصدار قديم")
        BuildLogControl1.AddErrorLog("فشل الاتصال بالخادم")
        BuildLogControl1.AddInfoLog("إعادة المحاولة...")
        BuildLogControl1.AddSuccessLog("تم البناء بنجاح")
        BuildLogControl1.SetProgress(100, "اكتمل")

        BuildLogControl1.AddFinalLog("── انتهى البناء ──")
        BuildLogControl1.StopBuild(True)
    End Sub

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Dim log As New BuildLogHelper(BuildLogControl1)

        log.StartBuild("جاري البناء...")
        log.Ok("تحميل الملفات", 20)
        log.Warn("تحذير: مكتبة قديمة", 40)
        log.Fail("فشل في ملف معين", 50)
        log.Ok("تم التصحيح والمتابعة", 70)
        log.Ok("اكتمل التجميع", 90)
        log.Finish(True, "── اكتمل البناء بنجاح ──")
    End Sub
End Class