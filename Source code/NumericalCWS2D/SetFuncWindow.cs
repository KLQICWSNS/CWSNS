using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NumericalCWS2D
{
    public partial class SetFuncWindow : Form
    {
        internal string prev;
        internal bool vb = false;
        const string codeCS = "using System;\r\nusing System.Numerics;\r\npublic class CustomWavefunction\r\n{\r\n    static Complex c;\r\n    public CustomWavefunction()\r\n    {\r\n        c = Complex.One;\r\n    }\r\n    public static Complex Wavefunction(double x1, double y1, double x2, double y2)\r\n    {\r\n        return c;\r\n    }\r\n}";
        const string codeVB = "Imports System\r\nImports System.Numerics\r\nPublic Class CustomWavefunction\r\n\r\n    Shared c As Complex\r\n\r\n    Public Sub New()\r\n        c = Complex.One\r\n    End Sub\r\n\r\n    Public Shared Function Wavefunction(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As Complex\r\n        Return c\r\n    End Function\r\n\r\nEnd Class";
        public SetFuncWindow()
        {
            InitializeComponent();
            codeText.Text = codeCS;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left) System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/dotnet/api/system.math?view=netframework-4.5");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (e.Button == MouseButtons.Left) System.Diagnostics.Process.Start("https://docs.microsoft.com/en-us/dotnet/api/system.numerics.complex?view=netframework-4.5");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string cod = codeText.Text;
            int bracket = 0;
            foreach(char i in cod)
            {
                if (i == '{') bracket++;
                if(i=='}')
                {
                    if (bracket == 0)
                    {
                        MessageBox.Show("Wrong code.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        bracket--;
                    }
                }
            }
            if (bracket != 0)
            {
                MessageBox.Show("Wrong code.", Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string errors;
            if(DynComp.Compile(cod, RefText.Text.Split(';'), vb, out errors))
            {
                prev = codeText.Text;
                Close();
            }
            else
            {
                MessageBox.Show(errors, Monte.MsgTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SetFuncWindow_Shown(object sender, EventArgs e)
        {
            prev = codeText.Text;
        }

        private void SetFuncWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            codeText.Text = prev;
        }

        private void SetFuncWindow_Resize(object sender, EventArgs e)
        {
            RefText.Width = Width - 116;
            codeText.Width = Width - 40;
            codeText.Height = Height - 169;
            label4.Top = Height - 71;
            button1.Top = Height - 73;
            button2.Top = Height - 73;
            SwitchLangButn.Top = Height - 73;
            button1.Left = Width - 200;
            button2.Left = Width - 110;
        }

        private void SwitchLangButn_Click(object sender, EventArgs e)
        {
            if (vb)
            {
                label1.Text = "Input the C# code for the wavefunction here. The amplitude of the wavefunction must not be larger than one.\r\nThe units of x1, y1, x2 and y2 are mm. See the documents:";
                codeText.Text = codeCS;
                prev = codeCS;
                SwitchLangButn.Text = "Use VB.NET";
                vb = false;
            }
            else
            {
                label1.Text = "Input the VB.NET code for the wavefunction here. The amplitude of the wavefunction must not be larger than one.\r\nThe units of x1, y1, x2 and y2 are mm. See the documents:";
                codeText.Text = codeVB;
                prev = codeVB;
                SwitchLangButn.Text = "Use C#";
                vb = true;
            }
        }
    }
}
