using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JiaolongModeViewer
{
    internal sealed class ViewerForm : Form
    {
        private readonly Func<ModeSnapshot> reader;
        private readonly Timer refreshTimer = new Timer();
        private readonly Label modeLabel = new Label();
        private readonly Label readingLabel = new Label();
        private readonly Label timeLabel = new Label();
        private readonly Button refreshButton = new Button();
        private bool busy;

        internal string DisplayedMode { get { return modeLabel.Text; } }
        internal bool RefreshEnabled { get { return refreshButton.Enabled; } }

        internal ViewerForm() : this(EcReader.Read, true) { }

        internal ViewerForm(Func<ModeSnapshot> read, bool autoRefresh)
        {
            reader = read;
            Text = "蛟龙 · 当前模式";
            Font = new Font("Microsoft YaHei UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(540, 370);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(19, 24, 34);
            ForeColor = Color.FromArgb(229, 234, 242);
            Icon = SystemIcons.Information;

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(28, 22, 28, 20);
            layout.ColumnCount = 1;
            layout.RowCount = 7;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            Controls.Add(layout);

            Label heading = MakeLabel("当前硬件模式", 11, Color.FromArgb(151, 164, 186));
            layout.Controls.Add(heading, 0, 0);
            modeLabel.Text = "正在读取…";
            modeLabel.Font = new Font(Font.FontFamily, 28F, FontStyle.Bold);
            modeLabel.Dock = DockStyle.Fill;
            modeLabel.TextAlign = ContentAlignment.MiddleLeft;
            modeLabel.Margin = Padding.Empty;
            layout.Controls.Add(modeLabel, 0, 1);

            readingLabel.Font = new Font(Font.FontFamily, 9F);
            readingLabel.Dock = DockStyle.Fill;
            readingLabel.ForeColor = Color.FromArgb(179, 191, 208);
            readingLabel.Text = "正在通过原厂驱动读取 EC…";
            readingLabel.Margin = Padding.Empty;
            layout.Controls.Add(readingLabel, 0, 2);

            timeLabel.Dock = DockStyle.Fill;
            timeLabel.ForeColor = Color.FromArgb(151, 164, 186);
            timeLabel.Font = new Font(Font.FontFamily, 9F);
            timeLabel.Margin = Padding.Empty;
            layout.Controls.Add(timeLabel, 0, 3);

            refreshButton.Text = "立即刷新";
            refreshButton.Width = 124;
            refreshButton.Height = 36;
            refreshButton.BackColor = Color.FromArgb(44, 60, 82);
            refreshButton.ForeColor = ForeColor;
            refreshButton.FlatStyle = FlatStyle.Flat;
            refreshButton.FlatAppearance.BorderSize = 0;
            refreshButton.Margin = new Padding(0, 4, 0, 4);
            refreshButton.Click += async delegate { await RefreshNow(); };
            layout.Controls.Add(refreshButton, 0, 4);

            Label note = MakeLabel("只读查看 · 每 2 秒刷新 · 关闭窗口即退出\n显示 EC 模式标志，不代表全部功耗与风扇参数已验证。", 9,
                Color.FromArgb(151, 164, 186));
            layout.Controls.Add(note, 0, 6);

            refreshTimer.Interval = 2000;
            refreshTimer.Tick += async delegate { await RefreshNow(); };
            if (autoRefresh)
            {
                Shown += async delegate { refreshTimer.Start(); await RefreshNow(); };
            }
            FormClosed += delegate { refreshTimer.Stop(); };
        }

        private Label MakeLabel(string text, float size, Color color)
        {
            return new Label { Text = text, Font = new Font(Font.FontFamily, size), ForeColor = color,
                Dock = DockStyle.Fill, Margin = Padding.Empty };
        }

        internal async Task RefreshNow()
        {
            if (busy || IsDisposed || Disposing) return;
            busy = true;
            refreshButton.Enabled = false;
            // Never present the last successful sample as the current result of an in-flight read.
            modeLabel.Text = "正在读取…";
            modeLabel.ForeColor = Color.FromArgb(151, 164, 186);
            readingLabel.Text = "正在通过原厂驱动确认模式…";
            timeLabel.Text = "";
            try
            {
                ModeSnapshot snapshot = await Task.Run(reader);
                if (IsDisposed || Disposing) return;
                modeLabel.Text = snapshot.Name;
                modeLabel.ForeColor = snapshot.Name == "狂暴模式" ? Color.FromArgb(255, 181, 112)
                    : snapshot.Name == "办公模式" ? Color.FromArgb(117, 218, 185)
                    : snapshot.Name == "游戏（均衡）模式" ? Color.FromArgb(128, 184, 255)
                    : Color.FromArgb(229, 234, 242);
                readingLabel.Text = "EC 模式 0x" + snapshot.ModeByte.ToString("X2")
                    + "  ·  自定义标志 0x" + snapshot.CustomByte.ToString("X2")
                    + "\n来源：本机原厂 ACPI 驱动";
                timeLabel.Text = "读取时间：" + snapshot.ReadAt.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception e)
            {
                if (IsDisposed || Disposing) return;
                modeLabel.Text = "无法确认";
                modeLabel.ForeColor = Color.FromArgb(255, 181, 112);
                readingLabel.Text = e.Message;
                timeLabel.Text = "读取失败：" + DateTime.Now.ToString("HH:mm:ss") + " · 将自动重试";
            }
            finally
            {
                busy = false;
                if (!IsDisposed && !Disposing) refreshButton.Enabled = true;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) refreshTimer.Dispose();
            base.Dispose(disposing);
        }
    }
}
