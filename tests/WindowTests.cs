using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using JiaolongModeViewer;

internal static class WindowTests
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        int result = 1;
        int step = 0;
        using (ViewerForm form = new ViewerForm(delegate
        {
            if (step == 0) return new ModeSnapshot(0x30, 0);
            if (step == 1) throw new IOException("模拟驱动读取失败");
            return new ModeSnapshot(0xA0, 0);
        }, false))
        {
            form.Shown += async delegate
            {
                try
                {
                    await form.RefreshNow();
                    Check(form.DisplayedMode == "狂暴模式", "initial read");
                    Check(form.RefreshEnabled, "refresh re-enabled after success");
                    if (args.Length == 1)
                    {
                        using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
                        {
                            form.DrawToBitmap(bitmap, form.ClientRectangle);
                            bitmap.Save(args[0]);
                        }
                    }
                    step = 1;
                    await form.RefreshNow();
                    Check(form.DisplayedMode == "无法确认", "failure must clear stale turbo label");
                    Check(form.RefreshEnabled, "refresh re-enabled after failure");
                    step = 2;
                    await form.RefreshNow();
                    Check(form.DisplayedMode == "办公模式", "recovery after failure");
                    Console.WriteLine("PASS: real WinForms refresh, stale-state clearing, recovery (5 checks)");
                    result = 0;
                }
                catch (Exception e) { Console.Error.WriteLine(e); }
                finally { form.Close(); }
            };
            Application.Run(form);
        }
        return result;
    }

    private static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception("FAIL: " + message);
    }
}
