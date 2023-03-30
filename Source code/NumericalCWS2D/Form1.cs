using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NumericalCWS2D
{
    public partial class Form1 : Form
    {
        int pairs;//Number of generated pairs
        Control[] todisable;
        Control[] todisable2;
        bool useFourier;
        int po2;
        bool deact = false;
        SetFuncWindow sfw;
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            todisable = new Control[] { GeneButn, SaveButn, Save1Btn, LoadButn, groupBox2, SetFuncBtn, ImportButn };
            todisable2 = new Control[] { GeneButn, SaveButn, Save1Btn, LoadButn, procBtn, CalcFidButn, ExpDataButn, ImportButn, SetFuncBtn, NoIFTCheck, TheoRecButn };
            var ci = new System.Globalization.CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            Algor.mw = this;
            Monte.mw = this;
            sfw = new SetFuncWindow();
        }
        [DllImport("User32.dll")]
        private static extern void FlashWindow(IntPtr hwnd, bool bInvert);
        private void Flash()
        {
            if(deact) FlashWindow(Handle, true);
        }
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            deact = true;
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            deact = false;
        }
        private void SetPicWidth()
        {
            int w = Monte.campixs > 160 ? 160 : Monte.campixs;
            IPic.Width = w;
            IPic.Height = w;
            I1Pic.Width = w;
            I1Pic.Height = w;
            I2Pic.Width = w;
            I2Pic.Height = w;
            I1h.Width = w;
            I1h.Height = w;
            I2h.Width = w;
            I2h.Height = w;
            IPic.Image = null;
            I1Pic.Image = null; 
            I2Pic.Image = null;
            I1h.Image = null;
            I2h.Image = null;
        }
        private bool MemoryGiveUp()
        {
            long n = Monte.campixs;
            //Need to store: Intensity distribution, several temporary calculation status sign in reconstruction, final wavefront distribution
            long memoneeded = n * n * n * n * (useFourier ? 32L : 24L);
            double GB = memoneeded / 1073741824.0;
            if (GB >= 4.0)
            {
                string am;
                if (GB >= 1073741824.0)
                {
                    am = (GB / 1073741824.0).ToString("f2") + " EB";
                }
                else if (GB >= 1048576.0)
                {
                    am = (GB / 1048576.0).ToString("f2") + " PB";
                }
                else if (GB >= 1024.0)
                {
                    am = (GB / 1024.0).ToString("f2") + " TB";
                }
                else
                {
                    am = GB.ToString("f2") + " GB";
                }
                if (!Environment.Is64BitOperatingSystem)
                {
                    if (MessageBox.Show("It take " + am + " of memory space to store the data. A 32-bit operating system possibly cannot process it. Would you like to continue?", Monte.MsgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return true;
                }
                if (GB >= 120.0)
                {
                    if (MessageBox.Show("It take " + am + " of memory space to store the data. A personal computer generally cannot reach this value. Continue if you are working on a special one.", Monte.MsgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.No) return true;
                }
                else
                {
                    if (MessageBox.Show("It take " + am + " of memory space to store the data. Would you like to continue?", Monte.MsgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return true;
                }
            }
            return false;
        }
        internal void CalcSPCor()
        {
            Monte.dispcor = (int)(Monte.l / Monte.campixlen + 0.5);
            if (Monte.dispcor > 1)
            {
                correcttext.Text = "Savart plate displacement correction of " + Monte.dispcor.ToString() + " pixels will be applied.";
            }
            else
            {
                correcttext.Text = "Savart plate displacement correction of " + Monte.dispcor.ToString() + " pixel will be applied.";
            }
            Monte.dover2l = Monte.campixlen / (Monte.l * 2.0);
        }
        Bitmap tbi, tbi1, tbi2, tbi1h, tbi2h;
        internal bool ParseValues()
        {
            try
            {
                pairs = (int)PairsText.Value;
                Funcs.a = double.Parse(aText.Text);
                Funcs.b = double.Parse(bText.Text);
                Funcs.bb = double.Parse(bbText.Text);
                int funcindex = FourierCheck.Checked ? 0 : 3;
                useFourier = FourierCheck.Checked;
                int funcnfindex = 0;
                if (radioButton1.Checked)
                {
                    funcindex += 1;
                    funcnfindex = 4;
                }
                else if (radioButton2.Checked)
                {
                    funcindex += 2;
                    funcnfindex = 5;
                }
                else if (radioButton3.Checked)
                {
                    funcindex += 3;
                    funcnfindex = 6;
                }
                if (radioButton4.Checked)
                {
                    funcindex = 0;
                    funcnfindex = 0;
                    if (DynComp.dyn == null)
                    {
                        MessageBox.Show("Please set the wavefunction first.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    Monte.wf = DynComp.dyn;
                    Monte.wfnof = DynComp.dyn;
                }
                else
                {
                    Monte.wf = Funcs.FuncList[funcindex];
                    Monte.wfnof = Funcs.FuncList[funcnfindex];
                }
                if (funcindex == 4)
                {
                    Monte.goodnof = true;
                    Monte.range2 = Math.Sqrt(40.0 / Funcs.b);
                }
                else Monte.goodnof = false;
                Funcs.wl = double.Parse(wlText.Text) / 1E6;
                Funcs.f = double.Parse(fText.Text) * 10.0;
                if (PatnCheck.Checked)
                {
                    string pat = PatnPathText.Text;
                    if (pat == "")
                    {
                        MessageBox.Show("Please input the image path.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                    Funcs.LoadBitmap(pat, double.Parse(PatnWidText.Text));
                }
                else
                {
                    Funcs.ClearBitmap();
                }
                Funcs.kfact = 2.0 * Math.PI / (Funcs.wl * Funcs.f);
                Monte.l = double.Parse(ltimes2Text.Text) / 2000.0;
                Monte.range = double.Parse(ROItext.Text);
                Monte.hran = Monte.range / 2.0;
                Monte.campixlen = double.Parse(PixWidthText.Text) / 1000.0;
                Monte.campixs = (int)(Monte.range / Monte.campixlen + 0.000001);
                po2 = Monte.campixs / 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (Funcs.a <= 0 || Funcs.b <= 0 || Funcs.bb <= 0 || Funcs.wl <= 0 || Funcs.f <= 0 || Monte.l <= 0 || Monte.range <= 0 || Monte.campixlen <= 0)
            {
                MessageBox.Show("Invalid input.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (Monte.campixs > 23170)
            {
                MessageBox.Show("Too many pixels.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (Monte.campixs > 255)
            {
                if (MessageBox.Show("Current runtime cannot handle a length of more than 255 pixels. Continue if you are sure you are working on a runtime that allows the allocation of arrays with more than 2^32 elements.", Monte.MsgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return false;
            }
            if (Funcs.b <= Funcs.bb)
            {
                if (MessageBox.Show("It seems the Good b is not larger than the Bad b. Would you like to continue?", Monte.MsgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return false;
            }
            return true;
        }
        internal bool DrawTheoreticalPhase()
        {
            double po2_ = Monte.campixs / 2.0, ax, ay;
            tbi = new Bitmap(Monte.campixs, Monte.campixs);
            tbi1 = new Bitmap(Monte.campixs, Monte.campixs);
            tbi2 = new Bitmap(Monte.campixs, Monte.campixs);
            tbi1h = new Bitmap(Monte.campixs, Monte.campixs);
            tbi2h = new Bitmap(Monte.campixs, Monte.campixs);
            try
            {
                for (int x = 0; x < Monte.campixs; x++)
                {
                    ax = (x - po2_ + 0.5) * Monte.campixlen;
                    for (int y = 0; y < Monte.campixs; y++)
                    {
                        ay = (y - po2_ + 0.5) * Monte.campixlen;
                        tbi.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Monte.wfnof(ax, ay, ax, ay), true));
                        tbi1.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Monte.wfnof(ax, 0.0, ay, 0.0), true));
                        tbi2.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Monte.wfnof(0.0, 0.0, ax, ay), true));
                        tbi1h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Monte.wfnof(0.0, ax, 0.0, ay), true));
                        tbi2h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Monte.wfnof(ax, ay, 0.0, 0.0), true));
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred when drawing the theoretical phase distribution:\r\n" + ex.Message, Monte.MsgTitle,MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        internal async void GeneButn_Click(object sender, EventArgs e)
        {
            if (!ParseValues()) return;
            if (MemoryGiveUp()) return;
            if(!DrawTheoreticalPhase()) return;
            CalcSPCor();
            SetPicWidth();
            DebugText.Text = "";
            prog1.Value = 0;
            prog2.Value = 0;
            Monte.progr = 0;
            int halfp = pairs / 10000;
            prog1.Maximum = halfp * 2;
            DateTime startt = DateTime.Now;
            foreach (Control i in todisable) i.Enabled = false;
            Cursor = Cursors.WaitCursor;
            if (!Monte.NewAlloc())
            {
                MessageBox.Show("Out of memory.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                prog1.Value = 0;
                GeneButn.Enabled = true;
                LoadButn.Enabled = true;
                SetFuncBtn.Enabled = true;
                Monte.Dealloc();
                Cursor = Cursors.Default;
                return;
            }
            Monte.saturated = false;
            Action dox = () => {
                Parallel.For(0, pairs, i =>
                {
                    Monte.GenOnex();
                    if (Monte.progr % 10000 == 0) Invoke(new Action(() => prog1.Value = Monte.progr / 10000));
                });
            }; 
            DebugAppend("Generating kx data...");
            try
            {
                await Task.Run(dox);
            }
            catch
            {
                MessageBox.Show("Exception occurred. Please check the amplitude of the wavefunction if you are using the custom wavefunction.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                prog1.Value = 0;
                GeneButn.Enabled = true;
                LoadButn.Enabled = true;
                SetFuncBtn.Enabled = true;
                Monte.Dealloc();
                Cursor = Cursors.Default;
                return;
            }
            prog1.Value = halfp;
            Monte.progr = 0;
            Action doy = () => {
                Parallel.For(0, pairs, i =>
                {
                    Monte.GenOney();
                    if (Monte.progr % 10000 == 0) Invoke(new Action(() => prog1.Value = Monte.progr / 10000 + halfp));
                });
            };
            DebugAppend("Generating ky data...");
            try
            {
                await Task.Run(doy);
            }
            catch
            {
                MessageBox.Show("Exception occurred. Please check the amplitude of the wavefunction if you are using the custom wavefunction.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                prog1.Value = 0;
                GeneButn.Enabled = true;
                LoadButn.Enabled = true;
                SetFuncBtn.Enabled = true;
                Monte.Dealloc();
                Cursor = Cursors.Default;
                return;
            }
            prog1.Value = prog1.Maximum;
            DateTime endt = DateTime.Now;
            double secs = (endt - startt).TotalSeconds;
            int totmin = (int)(secs / 60.0);
            secs -= 60.0 * totmin;
            timeText.Text = (totmin == 0 ? "" : (totmin.ToString() + "min ")) + secs.ToString("f2") + "s";

            int max0 = 0, max1 = 0, max2 = 0, max1h = 0, max2h = 0;
            for (int x = 0; x < Monte.campixs; x++)
            {
                for (int y = 0; y < Monte.campixs; y++)
                {
                    if (Monte.Ifromxcor(x, y, x, y) > max0) max0 = Monte.Ifromxcor(x, y, x, y);
                    if (Monte.Ifromxcor(x, po2, y, po2) > max1) max1 = Monte.Ifromxcor(x, po2, y, po2);
                    if (Monte.Ifromxcor(po2, po2, x, y) > max2) max2 = Monte.Ifromxcor(po2, po2, x, y);
                    if (Monte.Ifromxcor(po2, x, po2, y) > max1h) max1h = Monte.Ifromxcor(po2, x, po2, y);
                    if (Monte.Ifromxcor(x, y, po2, po2) > max2h) max2h = Monte.Ifromxcor(x, y, po2, po2);
                }
            }
            Bitmap bi = new Bitmap(Monte.campixs, Monte.campixs), bi1 = new Bitmap(Monte.campixs, Monte.campixs), bi2 = new Bitmap(Monte.campixs, Monte.campixs);
            Bitmap bi1h = new Bitmap(Monte.campixs, Monte.campixs), bi2h = new Bitmap(Monte.campixs, Monte.campixs);
            for (int x = 0; x < Monte.campixs; x++)
            {
                for (int y = 0; y < Monte.campixs; y++)
                {
                    bi.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(x, y, x, y) / (double)max0), double.NaN));
                    bi1.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(x, po2, y, po2) / (double)max1), double.NaN));
                    bi2.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(po2, po2, x, y) / (double)max2), double.NaN));

                    bi1h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(po2, x, po2, y) / (double)max1h), double.NaN));
                    bi2h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(x, y, po2, po2) / (double)max2h), double.NaN));
                }
            }
            IPic.Image = bi;
            I1Pic.Image = bi1;
            I2Pic.Image = bi2;
            I1h.Image = bi1h;
            I2h.Image = bi2h;
            Flash();
            TheoRecButn.Text = "Show theory";
            rbi = null;
            rbi1 = null;
            rbi2 = null;
            rbi1h = null;
            rbi2h = null;
            foreach (Control i in todisable) i.Enabled = true;
            Cursor = Cursors.Default;
            if (Monte.saturated)
            {
                MessageBox.Show("Saturation happened: Some joint intensity data have reached 65535.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        internal void Prog(int p)
        {
            if (prog1.Value < p) prog1.Value = p;
        }
        bool formerFourier;
        bool formerphaspatn;
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                formerFourier = FourierCheck.Checked;
                formerphaspatn = PatnCheck.Checked;
                FourierCheck.Checked = false;
                FourierCheck.Enabled = false;
                PatnCheck.Checked = false;
                PatnCheck.Enabled = false;
            }
            else
            {
                FourierCheck.Checked = formerFourier;
                FourierCheck.Enabled = true;
                PatnCheck.Checked = formerphaspatn;
                PatnCheck.Enabled = true;
            }
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/KLQICWSNS/CWSNS");
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        private async void SaveButn_Click(object sender, EventArgs e)
        {
            if(Monte.I_xLL != null)
            {
                foreach (Control i in todisable) i.Enabled = false;
                string pref = DatFilePrefText.Text;
                if (pref == "")
                {
                    MessageBox.Show("Please input the file prefix.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int n = Monte.campixs;
                prog1.Value = 0;
                prog1.Maximum = n * 8;
                Action acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xLL", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_xLL[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + 1));
                        }
                    };

                };
                await Task.Run(acti);
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xLR", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_xLR[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + n + 1));
                        }
                    }
                };
                int nowbasen = 2 * n;
                await Task.Run(acti);
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xRL", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_xRL[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xRR", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_xRR[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yLL", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_yLL[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yLR", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_yLR[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yRL", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_yRL[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yRR", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) fs.Write(BitConverter.GetBytes(Monte.I_yRR[i1, i2, i3, i4]), 0, 2);
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                foreach (Control i in todisable) i.Enabled = true;
                Flash();
            }
            else
            {
                MessageBox.Show("Please generate count data first.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private async void LoadButn_Click(object sender, EventArgs e)
        {
            string pref = DatFilePrefText.Text;
            if (pref == "")
            {
                MessageBox.Show("Please input the file prefix.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (File.Exists(pref + "_xLL")&& File.Exists(pref + "_xLR") && File.Exists(pref + "_xRL") && File.Exists(pref + "_xRR")&& File.Exists(pref + "_yLL") && File.Exists(pref + "_yLR") && File.Exists(pref + "_yRL") && File.Exists(pref + "_yRR"))
            {
                var fi = new FileInfo(pref + "_xLL");
                var fl = fi.Length;
                if ((fl & 1L) == 1L)
                {
                    MessageBox.Show("File format error: odd file size.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                long nn = (long)Math.Round(Math.Sqrt(Math.Sqrt(fl / 2L)));
                if (nn * nn * nn * nn * 2L != fl)
                {
                    MessageBox.Show("File format error: the file size is not 2 times a fourth power number bytes.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if(fl!= new FileInfo(pref + "_xLR").Length|| fl != new FileInfo(pref + "_xRL").Length || fl != new FileInfo(pref + "_xRR").Length || fl != new FileInfo(pref + "_yLL").Length || fl != new FileInfo(pref + "_yLR").Length || fl != new FileInfo(pref + "_yRL").Length || fl != new FileInfo(pref + "_yRR").Length)
                {
                    MessageBox.Show("File format error: file sizes are not equal.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (!ParseValues()) return;
                int n = (int)nn;
                if (nn != Monte.campixs)
                {
                    MessageBox.Show("The number of pixels in the data does not agree with the sensor parameter.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (MemoryGiveUp()) return;
                if (!DrawTheoreticalPhase()) return;
                CalcSPCor();
                SetPicWidth();
                foreach (Control i in todisable) i.Enabled = false;
                prog1.Value = 0;
                prog2.Value = 0;
                prog1.Maximum = n * 8;
                Monte.NewAlloc();
                byte[] buf = new byte[2];
                Action acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xLL", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++) 
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_xLL[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + 1));
                        }
                    };
                };
                await Task.Run(acti);
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xLR", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++)
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_xLR[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + n + 1));
                        }
                    }
                };
                int nowbasen = 2 * n;
                await Task.Run(acti);
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xRL", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++)
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_xRL[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_xRR", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++)
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_xRR[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yLL", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++)
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_yLL[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yLR", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++)
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_yLR[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yRL", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++)
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_yRL[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                nowbasen += n;
                acti = () => {
                    using (FileStream fs = new FileStream(pref + "_yRR", FileMode.Open))
                    {
                        for (int i1 = 0; i1 < n; i1++)
                        {
                            for (int i2 = 0; i2 < n; i2++) for (int i3 = 0; i3 < n; i3++) for (int i4 = 0; i4 < n; i4++)
                                    {
                                        fs.Read(buf, 0, 2);
                                        Monte.I_yRR[i1, i2, i3, i4] = BitConverter.ToUInt16(buf, 0);
                                    }
                            Invoke(new Action(() => prog1.Value = i1 + nowbasen + 1));
                        }
                    }
                };
                await Task.Run(acti);
                int max0 = 0, max1 = 0, max2 = 0, max1h = 0, max2h = 0;
                for (int x = 0; x < Monte.campixs; x++)
                {
                    for (int y = 0; y < Monte.campixs; y++)
                    {
                        if (Monte.Ifromxcor(x, y, x, y) > max0) max0 = Monte.Ifromxcor(x, y, x, y);
                        if (Monte.Ifromxcor(x, po2, y, po2) > max1) max1 = Monte.Ifromxcor(x, po2, y, po2);
                        if (Monte.Ifromxcor(po2, po2, x, y) > max2) max2 = Monte.Ifromxcor(po2, po2, x, y);
                        if (Monte.Ifromxcor(po2, x, po2, y) > max1h) max1h = Monte.Ifromxcor(po2, x, po2, y);
                        if (Monte.Ifromxcor(x, y, po2, po2) > max2h) max2h = Monte.Ifromxcor(x, y, po2, po2);
                    }
                }
                Bitmap bi = new Bitmap(Monte.campixs, Monte.campixs), bi1 = new Bitmap(Monte.campixs, Monte.campixs), bi2 = new Bitmap(Monte.campixs, Monte.campixs);
                Bitmap bi1h = new Bitmap(Monte.campixs, Monte.campixs), bi2h = new Bitmap(Monte.campixs, Monte.campixs);
                for (int x = 0; x < Monte.campixs; x++)
                {
                    for (int y = 0; y < Monte.campixs; y++)
                    {
                        bi.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(x, y, x, y) / (double)max0), double.NaN));
                        bi1.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(x, po2, y, po2) / (double)max1), double.NaN));
                        bi2.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(po2, po2, x, y) / (double)max2), double.NaN));

                        bi1h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(po2, x, po2, y) / (double)max1h), double.NaN));
                        bi2h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Math.Sqrt(Monte.Ifromxcor(x, y, po2, po2) / (double)max2h), double.NaN));
                    }
                }
                IPic.Image = bi;
                I1Pic.Image = bi1;
                I2Pic.Image = bi2;
                I1h.Image = bi1h;
                I2h.Image = bi2h;
                TheoRecButn.Text = "Show theory";
                rbi = null;
                rbi1 = null;
                rbi2 = null;
                rbi1h = null;
                rbi2h = null;
                foreach (Control i in todisable) i.Enabled = true;
                Flash();
            }
            else
            {
                MessageBox.Show("Required files do not exist.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        internal void DebugAppend(string s)
        {
            if (DebugText.Text == "")
            {
                DebugText.Text = s;
            }
            else
            {
                DebugText.Text = DebugText.Text + "\r\n" + s;
            }
        }
        private async void procBtn_Click(object sender, EventArgs e)
        {
            if (prog1.Value == 0)
            {
                MessageBox.Show("Please generate count data first.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            foreach (Control i in todisable2) i.Enabled = false;
            Cursor = Cursors.WaitCursor;
            int trytimes = (int)recTimesText.Value;
            DebugText.Text = "";
            prog2.Value = 0;
            bool useIFT = useFourier && (!NoIFTCheck.Checked);
            prog2.Maximum = useIFT ? (Monte.campixs * 3 + trytimes * 2 + Monte.campixs * Monte.campixs) : (Monte.campixs * 2 + trytimes * 2);
            int maxi = 0, maxx1 = 0, maxy1 = 0, maxx2 = 0, maxy2 = 0;
            int[] maxit = new int[Monte.campixs];
            int[] maxy1t = new int[Monte.campixs];
            int[] maxx2t = new int[Monte.campixs];
            int[] maxy2t = new int[Monte.campixs];
            Algor.pixs = Monte.campixs;
            Action findmax = () => {
                Parallel.For(0, Monte.campixs, x1 =>
                {
                    int ic;
                    for (int y1 = 0; y1 < Monte.campixs; y1++) for (int x2 = 0; x2 < Monte.campixs; x2++) for (int y2 = 0; y2 < Monte.campixs; y2++)
                            {
                                ic = Monte.Ifromxcor(x1, y1, x2, y2);
                                if (ic > maxit[x1])
                                {
                                    maxit[x1] = ic;
                                    maxy1t[x1] = y1;
                                    maxx2t[x1] = x2;
                                    maxy2t[x1] = y2;
                                }
                            }
                    Invoke(new Action(prog2p));
                });//prog2:campixs
                for(int x1 = 0; x1 < Monte.campixs; x1++)
                {
                    if (maxit[x1] > maxi)
                    {
                        maxi = maxit[x1];
                        maxx1 = x1;
                        maxy1 = maxy1t[x1];
                        maxx2 = maxx2t[x1];
                        maxy2 = maxy2t[x1];
                    }
                }
            };
            await Task.Run(findmax);
            try
            {
                Algor.FinalRes = new FloatComplex[Monte.campixs, Monte.campixs, Monte.campixs, Monte.campixs];
            }
            catch
            {
                MessageBox.Show("Out of memory.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                prog2.Value = 0;
                foreach (Control i in todisable2) i.Enabled = true;
                Cursor = Cursors.Default;
                return;
            }
            DebugAppend("Launching phase reconstruction...");
            maxit = null;
            maxy1t = null;
            maxx2t = null;
            maxy2t = null;
            Algor.startx1 = maxx1;
            Algor.starty1 = maxy1;
            Algor.startx2 = maxx2;
            Algor.starty2 = maxy2;
            Algor.maxi = maxi;
            GC.Collect();
            prog2p();//prog2:campixs
            Action trying = () =>
            {
                for (int i = 0; i < trytimes; i++)
                {
                    Algor.DoWork();
                    Invoke(new Action(prog2p));
                    Invoke(new Action(prog2p));//prog2:campixs+trytimes*2
                }
            };
            await Task.Run(trying);
            //prog2.Value = Monte.campixs + trytimes;
            DebugAppend("Recording the complex wavefunction...");
            Action phas2comp = () =>
            {
                Parallel.For(0, Monte.campixs, x1 =>
                {
                    double amp;
                    double phas;
                    for (int y1 = 0; y1 < Monte.campixs; y1++) for (int x2 = 0; x2 < Monte.campixs; x2++) for (int y2 = 0; y2 < Monte.campixs; y2++)
                            {
                                if(Monte.Ifromxcor(x1, y1, x2, y2) == 0 || Monte.Ifromycor(x1, y1, x2, y2) == 0)
                                {
                                    Algor.FinalRes[x1, y1, x2, y2] = FloatComplex.Zero;
                                }
                                else
                                {
                                    amp = Math.Sqrt(Monte.Ifromxcor(x1, y1, x2, y2) / (double)maxi);
                                    phas = Algor.FinalRes[x1, y1, x2, y2].Real / trytimes;
                                    Algor.FinalRes[x1, y1, x2, y2] = new FloatComplex(amp * Math.Cos(phas), amp * Math.Sin(phas));
                                }
                            }
                    Invoke(new Action(prog2p));//prog2:campixs+trytimes*2+campixs
                });
            };
            //prog2.Value = Monte.campixs * 2 + trytimes;
            await Task.Run(phas2comp);
            GC.Collect();
            if (useIFT)//A FFT algorithm possibly takes too much memory
            {
                DebugAppend("Performing inverse Fourier transform...");
                FloatComplex[,,,] iftres;
                try
                {
                    iftres = new FloatComplex[Monte.campixs, Monte.campixs, Monte.campixs, Monte.campixs];
                }
                catch
                {
                    MessageBox.Show("Out of memory. The inverse Fourier transform cannot be performed.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    goto skipift;
                }
                double lengkfact = Monte.campixlen * Funcs.kfact;
                Complex[] reflist;
                Action iftprep,ift;
                Func<int, int, int, int, int> indexfunc;
                if ((Monte.campixs & 1) == 1) //odd
                {
                    reflist = new Complex[4 * po2 * po2 + 1];
                    indexfunc = Odd2index;
                    iftprep = () =>
                    {
                        Parallel.For(0, Monte.campixs, ix => // i is source momentum, j is new position
                        {
                            double kx = (ix - po2) * lengkfact;
                            double ky, prod;
                            int indx;
                            for (int iy = 0; iy < Monte.campixs; iy++)
                            {
                                ky = (ix - po2) * lengkfact;
                                for (int jx = 0; jx < Monte.campixs; jx++)
                                {
                                    for (int jy = 0; jy < Monte.campixs; jy++)
                                    {
                                        prod = (kx * (jx - po2) + ky * (jy - po2)) * Monte.campixlen;
                                        indx = Odd2index(ix, jx, iy, jy);
                                        if (reflist[indx] == Complex.Zero) reflist[indx] = new Complex(Math.Cos(prod), Math.Sin(prod));
                                    }
                                }
                            }
                            Invoke(new Action(prog2p));
                        });
                    };
                }
                else //even
                {
                    reflist = new Complex[8 * po2 * po2 - 8 * po2 + 3];
                    indexfunc = Even2index;
                    iftprep = () =>
                    {
                        Parallel.For(0, Monte.campixs, ix => // i is source momentum, j is new position
                        {
                            double kx = (ix - po2 + 0.5) * lengkfact;
                            double ky, prod;
                            int indx;
                            for (int iy = 0; iy < Monte.campixs; iy++)
                            {
                                ky = (iy - po2 + 0.5) * lengkfact;
                                for (int jx = 0; jx < Monte.campixs; jx++)
                                {
                                    for (int jy = 0; jy < Monte.campixs; jy++)
                                    {
                                        prod = (kx * (jx - po2 + 0.5) + ky * (jy - po2 + 0.5)) * Monte.campixlen;
                                        indx = Even2index(ix, jx, iy, jy);
                                        if (reflist[indx] == Complex.Zero) reflist[indx] = new Complex(Math.Cos(prod), Math.Sin(prod));
                                    }
                                }
                            }
                            Invoke(new Action(prog2p));//prog2:campixs+trytimes*2+campixs+campixs
                        });
                    };
                }
                await Task.Run(iftprep);
                //prog2.Value = Monte.campixs * 3 + trytimes;
                double[,] iftmax1 = new double[Monte.campixs, Monte.campixs];
                ift = () =>
                {
                    Parallel.For(0, Monte.campixs, x2 =>
                    {
                        for (int y2 = 0; y2 < Monte.campixs; y2++)
                        {
                            for (int x1o = 0; x1o < Monte.campixs; x1o++)//original provides momentum
                            {
                                for (int y1o = 0; y1o < Monte.campixs; y1o++)
                                {
                                    for (int x1 = 0; x1 < Monte.campixs; x1++)//new
                                    {
                                        for (int y1 = 0; y1 < Monte.campixs; y1++)
                                        {
                                            iftres[x1, y1, x2, y2] += Algor.FinalRes[x1o, y1o, x2, y2] * (FloatComplex)reflist[indexfunc(x1o, x1, y1o, y1)];
                                        }
                                    }
                                }
                            }
                            Invoke(new Action(prog2p));////prog2:campixs+trytimes*2+campixs+campixs+campixs^2
                        }
                    });
                };
                await Task.Run(ift);
                Algor.FinalRes = iftres;
                GC.Collect();
            }
            skipift:
            DrawPhase();
            recwffromcalc = true;
            prog2.Value = prog2.Maximum;
            Flash();
            TheoRecButn.Text = "Show theory";
            rbi = null;
            rbi1 = null;
            rbi2 = null;
            rbi1h = null;
            rbi2h = null;
            foreach (Control i in todisable2) i.Enabled = true;
            Cursor = Cursors.Default;
        }
        internal void DrawPhase()
        {
            Bitmap bi = new Bitmap(Monte.campixs, Monte.campixs), bi1 = new Bitmap(Monte.campixs, Monte.campixs), bi2 = new Bitmap(Monte.campixs, Monte.campixs);
            Bitmap bi1h = new Bitmap(Monte.campixs, Monte.campixs), bi2h = new Bitmap(Monte.campixs, Monte.campixs); ;
            double dispmax0 = 0.0, dispmax1 = 0.0, dispmax2 = 0.0, dispmax1h = 0.0, dispmax2h = 0.0, mag0, mag1, mag2, mag1h, mag2h;
            for (int x = 0; x < Monte.campixs; x++)
            {
                for (int y = 0; y < Monte.campixs; y++)
                {
                    mag0 = Algor.FinalRes[x, y, x, y].Magnitude;
                    mag1 = Algor.FinalRes[x, po2, y, po2].Magnitude;
                    mag2 = Algor.FinalRes[po2, po2, x, y].Magnitude;
                    mag1h = Algor.FinalRes[po2, x, po2, y].Magnitude;
                    mag2h = Algor.FinalRes[x, y, po2, po2].Magnitude;

                    if (mag0 > dispmax0) dispmax0 = mag0;
                    if (mag1 > dispmax1) dispmax1 = mag1;
                    if (mag2 > dispmax2) dispmax2 = mag2;
                    if (mag1h > dispmax1h) dispmax1h = mag1h;
                    if (mag2h > dispmax2h) dispmax2h = mag2h;
                }
            }
            for (int x = 0; x < Monte.campixs; x++)
            {
                for (int y = 0; y < Monte.campixs; y++)
                {
                    bi.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Algor.FinalRes[x, y, x, y] / dispmax0));
                    bi1.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Algor.FinalRes[x, po2, y, po2] / dispmax1));
                    bi2.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Algor.FinalRes[po2, po2, x, y] / dispmax2));

                    bi1h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Algor.FinalRes[po2, x, po2, y] / dispmax1h));
                    bi2h.SetPixel(x, Monte.campixs - 1 - y, ColorProc.Complex2Color(Algor.FinalRes[x, y, po2, po2] / dispmax2h));
                }
            }
            IPic.Image = bi;
            I1Pic.Image = bi1;
            I2Pic.Image = bi2;
            I1h.Image = bi1h;
            I2h.Image = bi2h;
        }
        private int Odd2index(int ix, int jx, int iy, int jy) => (ix - po2) * (jx - po2) + (iy - po2) * (jy - po2) + 2 * po2 * po2;
        private int Even2index(int ix, int jx, int iy, int jy) => (int)(((ix - po2 + 0.5) * (jx - po2 + 0.5) + (iy - po2 + 0.5) * (jy - po2 + 0.5) + 2.0 * po2 * po2 - po2 * 2 + 0.5)*2.0+0.5);
        private void prog2p()
        {
            if (prog2.Value != prog2.Maximum) prog2.Value++;
        }
        string experr;
        private async void ExpDataButn_Click(object sender, EventArgs e)
        {
            if (prog2.Value == 0)
            {
                MessageBox.Show("Please process the count data or load the wavefunction first.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ExpDataPrefText.Text == "")
            {
                MessageBox.Show("Please input the file prefix.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            foreach (Control i in todisable2) i.Enabled = false;
            Cursor = Cursors.WaitCursor;
            prog2.Value = 0;
            prog2.Maximum = Monte.campixs;
            experr = "";
            await Task.Run(DoExport);
            if (experr != "")
            {
                MessageBox.Show(experr, Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            prog2.Value = prog2.Maximum;
            Flash();
            foreach (Control i in todisable2) i.Enabled = true;
            Cursor = Cursors.Default;
        }
        private void DoExport()
        {
            string prf = ExpDataPrefText.Text;
            FileStream fsR, fsI;
            FloatComplex fc;
            try
            {
                fsR = new FileStream(prf + "_RealPart", FileMode.Create);
                fsI = new FileStream(prf + "_ImaginaryPart", FileMode.Create);
                for (int x1 = 0; x1 < Monte.campixs; x1++)
                {
                    for (int y1 = 0; y1 < Monte.campixs; y1++)
                    {
                        for (int x2 = 0; x2 < Monte.campixs; x2++)
                        {
                            for (int y2 = 0; y2 < Monte.campixs; y2++)
                            {
                                fc = Algor.FinalRes[x1, y1, x2, y2];
                                fsR.Write(BitConverter.GetBytes(fc.Real), 0, 4);
                                fsI.Write(BitConverter.GetBytes(fc.Imaginary), 0, 4);
                            }
                        }
                    }
                    Invoke(new Action(prog2p));
                }
                fsR.Close();
                fsI.Close();
            }
            catch (Exception ex)
            {
                experr = "An error occured: " + ex.Message;
            }
            fsR = null;
            fsI = null;
        }
        bool recwffromcalc = false;
        private async void ImportButn_Click(object sender, EventArgs e)
        {
            if (ExpDataPrefText.Text == "")
            {
                MessageBox.Show("Please input the file prefix.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if((!(Algor.FinalRes == null)) && recwffromcalc)
            {
                if (MessageBox.Show("The previous reconstruction result will be overwritten. Would you like to continue?", Monte.MsgTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No) return;
            }
            ParseValues();
            if (!DrawTheoreticalPhase()) return;
            foreach (Control i in todisable2) i.Enabled = false;
            Cursor = Cursors.WaitCursor;
            prog2.Value = 0;
            prog2.Maximum = Monte.campixs;
            experr = "";
            SetPicWidth();
            await Task.Run(DoImport);
            if (experr != "")
            {
                MessageBox.Show(experr, Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                prog2.Value = 0;
            }
            else
            {
                prog2.Value = prog2.Maximum;
                groupBox2.Enabled = true;
                DrawPhase();
            }
            recwffromcalc = false;
            foreach (Control i in todisable2)
            {
                if(i == SaveButn || i == Save1Btn)
                {
                    SaveButn.Enabled = prog1.Value != 0;
                    Save1Btn.Enabled = prog1.Value != 0;
                }
                else i.Enabled = true;
            }
            TheoRecButn.Text = "Show theory";
            rbi = null;
            rbi1 = null;
            rbi2 = null;
            rbi1h = null;
            rbi2h = null;
            Flash();
            Cursor = Cursors.Default;
        }
        private void DoImport()
        {
            string prf = ExpDataPrefText.Text;
            string prR = prf + "_RealPart";
            string prI = prf + "_ImaginaryPart";
            if (!(File.Exists(prR) && File.Exists(prI)))
            {
                experr = "Required files do not exist.";
                return;
            }
            FileStream fsR, fsI;
            Algor.FinalRes = new FloatComplex[Monte.campixs, Monte.campixs, Monte.campixs, Monte.campixs];
            try
            {
                var fiR = new FileInfo(prR);
                var flR = fiR.Length;
                var fiI = new FileInfo(prI);
                var flI = fiI.Length;
                if (flR != flI)
                {
                    experr = "File format error: the sizes of the real and the imaginary part file are not the same.";
                    return;
                }
                long nn = (long)Math.Round(Math.Sqrt(Math.Sqrt(flR / 4L)));
                if (nn * nn * nn * nn * 4L != flR)
                {
                    MessageBox.Show("File format error: the file size is not 4 times a fourth power number bytes.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int n = (int)nn;
                if (nn != Monte.campixs)
                {
                    MessageBox.Show("The number of pixels in the data does not agree with the sensor parameter.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                fsR = new FileStream(prR, FileMode.Open);
                fsI = new FileStream(prI, FileMode.Open);
                byte[] buf = new byte[4];
                float re, im;
                for (int x1 = 0; x1 < Monte.campixs; x1++)
                {
                    for (int y1 = 0; y1 < Monte.campixs; y1++)
                    {
                        for (int x2 = 0; x2 < Monte.campixs; x2++)
                        {
                            for (int y2 = 0; y2 < Monte.campixs; y2++)
                            {
                                fsR.Read(buf, 0, 4);
                                re = BitConverter.ToSingle(buf, 0);
                                fsI.Read(buf, 0, 4);
                                im = BitConverter.ToSingle(buf, 0);
                                Algor.FinalRes[x1, y1, x2, y2] = new FloatComplex(re, im);
                            }
                        }
                    }
                    Invoke(new Action(prog2p));
                }
                fsR.Close();
                fsI.Close();
            }
            catch (Exception ex)
            {
                //throw;
                experr = "An error occured: " + ex.Message;
            }
            fsR = null;
            fsI = null;
        }
        bool fidwarning = false;
        private async void CalcFidButn_Click(object sender, EventArgs e)
        {
            if (prog2.Value == 0)
            {
                MessageBox.Show("Please process the count data or load the wavefunction first.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ParseValues();
            if (!DrawTheoreticalPhase()) return;
            foreach (Control i in todisable2) i.Enabled = false;
            Cursor = Cursors.WaitCursor;
            prog2.Value = 0;
            prog2.Maximum = Monte.campixs;
            await Task.Run(DoCalcFid);
            prog2.Value = prog2.Maximum;
            Flash();
            foreach (Control i in todisable2) i.Enabled = true;
            Cursor = Cursors.Default;
            if (!fidwarning)
            {
                MessageBox.Show("The calculated fidelity is affected by many factors, such as the finite ROI size when performing inverse Fourier transform, the pixelization, and the phase reconstruction algorithm. The value may not be high enough.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                fidwarning = true;
            }

        }
        private void DoCalcFid()
        {
            double wfmod = 0.0, recmod = 0.0;
            double ax1,ay1,ax2,ay2;
            double po2 = Monte.campixs / 2.0;
            Func<double, double, double, double, Complex> func;
            if (NoIFTCheck.Checked)
            {
                func = Monte.wf;
            }
            else
            {
                func = Monte.wfnof;
            }
            Complex wf, rec, fid1 = Complex.Zero;
            for (int x1 = 0; x1 < Monte.campixs; x1++)
            {
                ax1 = (x1 - po2 + 0.5) * Monte.campixlen;
                for (int y1 = 0; y1 < Monte.campixs; y1++)
                {
                    ay1 = (y1 - po2 + 0.5) * Monte.campixlen;
                    for (int x2 = 0; x2 < Monte.campixs; x2++)
                    {
                        ax2 = (x2 - po2 + 0.5) * Monte.campixlen;
                        for (int y2 = 0; y2 < Monte.campixs; y2++)
                        {
                            ay2 = (y2 - po2 + 0.5) * Monte.campixlen;
                            wf = func(ax1, ay1, ax2, ay2);
                            rec = Algor.FinalRes[x1, y1, x2, y2];
                            wfmod += wf.Real * wf.Real + wf.Imaginary * wf.Imaginary;
                            recmod += rec.Real * rec.Real + rec.Imaginary * rec.Imaginary;
                            if (rec != Complex.Zero)
                            {
                                fid1 += rec * Complex.Conjugate(wf);
                            }
                        }
                    }
                }
                Invoke(new Action(prog2p));
            }
            double fid = (fid1.Real * fid1.Real + fid1.Imaginary * fid1.Imaginary) / (recmod * wfmod);
            Invoke(new Action(() => FidText.Text = fid.ToString()));
        }
        private void SetFuncBtn_Click(object sender, EventArgs e)
        {
            sfw.ShowDialog();
        }
        private async void Save1Btn_Click(object sender, EventArgs e)
        {
            if (Monte.I_xLL != null)
            {
                foreach (Control i in todisable) i.Enabled = false;
                string pref = DatFilePrefText.Text;
                if (pref == "")
                {
                    MessageBox.Show("Please input the file prefix.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                int n = Monte.campixs;
                prog1.Value = 0;
                prog1.Maximum = n + 13;
                int[,] x1L = new int[n, n];
                int[,] x1R = new int[n, n];
                int[,] x2L = new int[n, n];
                int[,] x2R = new int[n, n];
                int[,] y1L = new int[n, n];
                int[,] y1R = new int[n, n];
                int[,] y2L = new int[n, n];
                int[,] y2R = new int[n, n];
                float[,] kx1 = new float[n, n];
                float[,] ky1 = new float[n, n];
                float[,] kx2 = new float[n, n];
                float[,] ky2 = new float[n, n];
                Action act = () =>
                {
                    for (int x1 = 0; x1 < n; x1++)
                    {
                        for (int y1 = 0; y1 < n; y1++)
                        {
                            for (int x2 = 0; x2 < n; x2++)
                            {
                                for (int y2 = 0; y2 < n; y2++)
                                {
                                    x1L[x1, y1] += Monte.I_x1L(x1, y1, x2, y2);
                                    x1R[x1, y1] += Monte.I_x1R(x1, y1, x2, y2);
                                    y1L[x1, y1] += Monte.I_y1L(x1, y1, x2, y2);
                                    y1R[x1, y1] += Monte.I_y1R(x1, y1, x2, y2);
                                    x2L[x2, y2] += Monte.I_x2L(x1, y1, x2, y2);
                                    x2R[x2, y2] += Monte.I_x2R(x1, y1, x2, y2);
                                    y2L[x2, y2] += Monte.I_y2L(x1, y1, x2, y2);
                                    y2R[x2, y2] += Monte.I_y2R(x1, y1, x2, y2);
                                }
                            }
                        }
                        Invoke(new Action(() => prog1.Value = x1+1));
                    }
                    int intenx=0, inteny=0,xcor,ycor;
                    for(int x = 0; x < n; x++)
                    {
                        for (int y = 0; y < n; y++)
                        {
                            ycor = y + Monte.dispcor;
                            xcor = x - Monte.dispcor;
                            if (ycor >= n || xcor < 0)
                            {
                                kx1[x, y] = float.NaN;
                                ky1[x, y] = float.NaN;
                                kx2[x, y] = float.NaN;
                                ky2[x, y] = float.NaN;
                            }
                            else
                            {
                                intenx = x1L[x, ycor] + x1R[x, ycor];
                                inteny = y1L[xcor, y] + y1R[xcor, y];
                                if (intenx == 0 || inteny == 0)
                                {
                                    kx1[x, y] = float.NaN;
                                    ky1[x, y] = float.NaN;
                                }
                                else
                                {
                                    kx1[x, y] = (float)(Math.Asin((x1R[x, ycor] - x1L[x, ycor]) / (double)intenx) * Monte.dover2l);
                                    ky1[x, y] = (float)(Math.Asin((y1L[xcor, y] - y1R[xcor, y]) / (double)inteny) * Monte.dover2l);
                                }
                                intenx = x2L[x, ycor] + x2R[x, ycor];
                                inteny = y2L[xcor, y] + y2R[xcor, y];
                                if (intenx == 0 || inteny == 0)
                                {
                                    kx2[x, y] = float.NaN;
                                    ky2[x, y] = float.NaN;
                                }
                                else
                                {
                                    kx2[x, y] = (float)(Math.Asin((x2R[x, ycor] - x2L[x, ycor]) / (double)intenx) * Monte.dover2l);
                                    ky2[x, y] = (float)(Math.Asin((y2L[xcor, y] - y2R[xcor, y]) / (double)inteny) * Monte.dover2l);
                                }
                            }
                        }
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_x1L", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(x1L[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_x1R", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(x1R[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_x2L", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(x2L[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_x2R", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(x2R[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_y1L", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(y1L[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_y1R", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(y1R[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_y2L", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(y2L[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_y2R", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(y2R[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_1kx", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(kx1[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_1ky", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(ky1[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_2kx", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(kx2[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                    using (FileStream fs = new FileStream(pref + "_single_2ky", FileMode.Create))
                    {
                        for (int i1 = 0; i1 < n; i1++) for (int i2 = 0; i2 < n; i2++) fs.Write(BitConverter.GetBytes(ky2[i1, i2]), 0, 4);
                    }
                    Invoke(new Action(() => prog1.Value++));
                };
                await Task.Run(act);
                foreach (Control i in todisable) i.Enabled = true;
                Flash();
            }
            else
            {
                MessageBox.Show("Please generate count data first.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        Image rbi, rbi1, rbi2, rbi1h, rbi2h;

        private void TheoRecButn_Click(object sender, EventArgs e)
        {
            if(TheoRecButn.Text == "Show theory")
            {
                rbi = IPic.Image;
                rbi1 = I1Pic.Image;
                rbi2 = I2Pic.Image;
                rbi1h = I1h.Image;
                rbi2h = I2h.Image;
                IPic.Image = tbi;
                I1Pic.Image = tbi1;
                I2Pic.Image = tbi2;
                I1h.Image = tbi1h;
                I2h.Image = tbi2h;
                TheoRecButn.Text = "Show reconstruction";
            }
            else
            {
                IPic.Image = rbi;
                I1Pic.Image = rbi1;
                I2Pic.Image = rbi2;
                I1h.Image = rbi1h;
                I2h.Image = rbi2h;
                TheoRecButn.Text = "Show theory";
            }
        }

        private void I1h_Click(object sender, EventArgs e)
        {
            I1Pic.Visible = true;
            I1h.Visible = false;
            pic2s.Visible = true;
            L1h.Visible = false;
        }

        private void I2h_Click(object sender, EventArgs e)
        {
            I2Pic.Visible = true;
            I2h.Visible = false;
            pic3s.Visible = true;
            L2h.Visible = false;
        }

        private void I1Pic_Click(object sender, EventArgs e)
        {
            I1Pic.Visible = false;
            I1h.Visible = true;
            pic2s.Visible = false;
            L1h.Visible = true;
        }

        private void I2Pic_Click(object sender, EventArgs e)
        {
            I2Pic.Visible = false;
            I2h.Visible = true;
            pic3s.Visible = false;
            L2h.Visible = true;
        }
    }
}
