using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ZamanınıKaybetme
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ProgramGetir();
            notifyIcon1.Visible = true;
            notifyIcon1.Icon = SystemIcons.Application;
            notifyIcon1.BalloonTipTitle = "Zamanını Kaybetme";
            notifyIcon1.BalloonTipText = "Zamanını Kaybetme";
            notifyIcon1.ShowBalloonTip(1000);
           // this.Hide();
        }

        Dictionary<string, int> harcananBilgi = new Dictionary<string, int>();

        async void ProgramGetir()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    string aktifUyg = getActiveWindowName();  
                    if (aktifUyg!=null)
                    {
                        if (harcananBilgi.ContainsKey(aktifUyg.Trim()))
                        {
                            harcananBilgi[aktifUyg] += 1;
                        }
                        else
                        {
                            harcananBilgi.Add(aktifUyg.Trim(), 1);
                        }
                        watch.Stop();
                        double beklemeSuresi = 1000 - watch.Elapsed.Milliseconds;
                        await Task.Delay(Convert.ToInt32(beklemeSuresi));
                    }
                    
                }
               
            });


        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        public string getActiveWindowName()
        {
            try
            {
                var activatedHandle = GetForegroundWindow();

                Process[] processes = Process.GetProcesses();
                foreach (Process clsProcess in processes)
                {
                    if (activatedHandle == clsProcess.MainWindowHandle)
                    {
                        string processName = clsProcess.ProcessName;

                        return processName;
                    }
                }
            }
            catch { }
            return null;
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                button1.Enabled = false;
                try
                {
                    foreach (var series in chart1.Series)
                    {
                        series.Points.Clear();
                    }

                }
                catch 
                {
                }
               
            bas:
                try
                {
                    foreach (var item in harcananBilgi)
                    {
                        if (this.InvokeRequired)
                        {
                            this.Invoke((MethodInvoker)delegate ()
                            {
                                chart1.Series["Series1"].Points.AddXY(item.Key, item.Value);

                            });
                        }

                    }

                }
                catch 
                {
                    goto bas;
                }

            });
            button1.Enabled = true;
           
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            
        }

        private void chart1_MouseClick(object sender, MouseEventArgs e)
        {
            Chart pie = (Chart)sender;
            int pointIndex = pieHitPointIndex(pie, e);
            if (pointIndex >= 0)
            {
                DataPoint dp = pie.Series[0].Points[pointIndex];
                TimeSpan result = TimeSpan.FromSeconds(dp.YValues[0]);
                MessageBox.Show("Kullanım Süreniz: " + new DateTime(result.Ticks).ToString("HH:mm:ss"),"Bilgilendirme",MessageBoxButtons.OK,MessageBoxIcon.Information);
            }
        }

        private void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            Chart pie = (Chart)sender;
            int pointIndex = pieHitPointIndex(pie, e);
            if (pointIndex >= 0)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }
        private int pieHitPointIndex(Chart pie, MouseEventArgs e)
        {
            HitTestResult hitPiece = pie.HitTest(e.X, e.Y, ChartElementType.DataPoint);
            HitTestResult hitLegend = pie.HitTest(e.X, e.Y, ChartElementType.LegendItem);
            int pointIndex = -1;
            if (hitPiece.Series != null) pointIndex = hitPiece.PointIndex;
            if (hitLegend.Series != null) pointIndex = hitLegend.PointIndex;
            return pointIndex;
        }

        private void Form1_ClientSizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }
    }
}
