using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace 刷屏器 {
    public partial class Form1 : Form {
        HelpFrm helpFrm = new HelpFrm();
        AboutFrm aboutFrm = new AboutFrm();
        private Timer _spamTimer;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const byte VK_RETURN = 0x0D;
        public Form1() {
            InitializeComponent();
            _spamTimer = new Timer();
            _spamTimer.Tick += SpamTimer_Tick;
        }

        private void Form1_Load(object sender, EventArgs e) {
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
        }

        private void SpamTimer_Tick(object sender, EventArgs e) {
            try {
                SendKeys.Send(txtContent.Text);
                keybd_event(VK_RETURN, 0, 0, 0);
                keybd_event(VK_RETURN, 0, KEYEVENTF_KEYUP, 0);
            } catch (Exception ex) {
                labelStatus.Text = $"状态：发送失败 - {ex.Message}";
                labelStatus.ForeColor = Color.Red;
                btnStop_Click(null, null);
            }
        }

        private void btnStart_Click(object sender, EventArgs e) {
            if (IsSafeRunnning()) {
                MessageBox.Show("检测到杀毒软件在运行！请尝试关闭杀毒软件!", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtContent.Text)) {
                MessageBox.Show("请输入刷屏内容！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtContent.Focus();
                return;
            }
            if (!int.TryParse(txtInterval.Text, out int interval) || interval < 100) {
                MessageBox.Show("请输入有效的间隔时间（≥100毫秒）！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtInterval.Focus();
                return;
            }
            if (interval < 500) {
                if (MessageBox.Show("间隔过小可能导致程序/系统卡顿，是否继续？", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) {
                    return;
                }
            }
            _spamTimer.Interval = interval;
            _spamTimer.Start();
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            txtContent.Enabled = false;
            txtInterval.Enabled = false;
            labelStatus.Text = $"状态：刷屏中（间隔{interval}ms）";
            labelStatus.ForeColor = Color.Green;
        }

        private static bool IsSafeRunnning() {
            string[] processNames = {
                "360tray",
                "360sd",
                "360safe",
                "360rp",
                "360rps",
                "HipsTray",
                "HipsMain",

            };

            foreach(var items in processNames) {
                Process[] processes = Process.GetProcessesByName(items);
                if (processes.Length > 0) return true;
            }
            return false;
        }

        private void btnStop_Click(object sender, EventArgs e) {
            if (_spamTimer.Enabled) {
                _spamTimer.Stop();
            }
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            txtContent.Enabled = true;
            txtInterval.Enabled = true;
            labelStatus.Text = "状态：已停止";
            labelStatus.ForeColor = Color.Orange;
        }
        private void btnClose_Click(object sender, EventArgs e) {
            if (_spamTimer.Enabled) _spamTimer.Stop();
            this.Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.Escape) {
                btnClose_Click(null, null);
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e) {
            helpFrm.Show();
        }

        private void 关于3ToolStripMenuItem_Click(object sender, EventArgs e) {
            aboutFrm.Show();
        }
    }
}
