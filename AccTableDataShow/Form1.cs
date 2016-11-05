using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AccTableDataShow
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public string FilePath;
        public DataTable dt;

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All files (*.*)|*.*|txt files (*.txt)|*.txt";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = openFileDialog.FileName;
            }
            read_data();
            dataGridView1.DataSource = dt;
            show_data();
            if(checkBox1.Checked)
                show_data_fft();
        }

        private void read_data()
        {
            dt = new DataTable();
            dt.Columns.AddRange(new DataColumn[] {
                new DataColumn("Time",typeof(float)),
                new DataColumn("Acceleration",typeof(float)),
                new DataColumn("Velocity",typeof(float)),
                new DataColumn("Displacement",typeof(float)),
                new DataColumn("ADC_Data",typeof(string))
            });
            DataRow dr = null;

            StreamReader sr = new StreamReader(FilePath);
            while (sr.Peek() > -1)
            {
                dr = dt.NewRow();
                string line = sr.ReadLine();
                if (line.Contains("Hz"))
                {
                    textBox7.Text = line.Replace(" ", "");
                }
                if (line.Contains("°"))
                {
                    textBox8.Text = line.Split(':')[1].Replace(" ", "");
                }
                if (line.Contains("dB"))
                {
                    textBox9.Text = line.Split(':')[1].Replace(" ", "");
                }
                if (line.Contains(" rad"))
                {
                    textBox10.Text = line.Replace(" ", "");
                }
                if (line.Contains("|") && line[0] != 'T')
                {
                    string[] array = line.Split('|');
                    dr["Time"] = float.Parse(array[0]);
                    dr["Acceleration"] = float.Parse(array[1]);
                    dr["Velocity"] = float.Parse(array[2]);
                    dr["Displacement"] = float.Parse(array[3]);
                    if(array.Length > 4)
                        dr["ADC_Data"] = array[4];

                    dt.Rows.Add(dr);
                }
            }
        }
        private void show_data()
        {
            ZoomToggle(chart1, true);
            List<double> ls_acc = get_data_colomn("Acceleration");
            List<double> ls_vel = get_data_colomn("Velocity");
            List<double> ls_dis = get_data_colomn("Displacement");
            textBox1.Text = ls_acc.Max().ToString();
            textBox4.Text = ls_acc.Min().ToString();
            textBox2.Text = ls_vel.Max().ToString();
            textBox5.Text = ls_vel.Min().ToString();
            textBox3.Text = ls_dis.Max().ToString();
            textBox6.Text = ls_dis.Min().ToString();
            chart1.Series["Acc"].ToolTip = "角加速度为：#VAL rad/s";
            chart1.Series["Vel"].ToolTip = "角速度为：#VAL rad/s";
            chart1.Series["Dis"].ToolTip = "角位移为：#VAL rad";
            chart1.Series["Acc"].Points.DataBindY(ls_acc);
            chart1.Series["Vel"].Points.DataBindY(ls_vel);
            chart1.Series["Dis"].Points.DataBindY(ls_dis);
        }

        private void show_data_fft()
        {
            ZoomToggle(chart2, true);
            float Fs = 1 / float.Parse(dt.Rows[0]["Time"].ToString());
            double[] x = get_data_colomn("Acceleration").ToArray();
            int n = x.Length;
            double[] freqs = new Double[n / 2];
            for (int i = 0; i < freqs.Length; i++)
            {
                freqs[i] = Fs * i * 0.5 / freqs.Length;
            }
            complex[] y = new complex[n];//接收复数结果的数组 
            double[] z = new Double[n];//接收幅值结果的数组 
            y = airthm.dft(x, n);
            z = airthm.amplitude(y, n);
            double[] z_half = new Double[n / 2];
            Array.ConstrainedCopy(z, 0, z_half, 0, n / 2);
            chart2.Series["Series1"].Points.DataBindXY(freqs, z_half);
        }

        private List<double> get_data_colomn(string ColomnName)
        {
            List<double> ls = new List<double>();  //存放一整列所有的值 
            foreach (DataRow dr in dt.Rows)
            {
                ls.Add(float.Parse(dr[ColomnName].ToString()));
            }
            return ls;
        }

        private void ZoomToggle(System.Windows.Forms.DataVisualization.Charting.Chart mychart, bool Enabled)
        {
            // Enable range selection and zooming end user interface
            mychart.ChartAreas[0].CursorX.IsUserEnabled = Enabled;
            mychart.ChartAreas[0].CursorX.IsUserSelectionEnabled = Enabled;
            mychart.ChartAreas[0].CursorX.Interval = 0;
            mychart.ChartAreas[0].AxisX.ScaleView.Zoomable = Enabled;
            mychart.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            mychart.ChartAreas[0].AxisX.ScrollBar.ButtonStyle = System.Windows.Forms.DataVisualization.Charting.ScrollBarButtonStyles.ResetZoom;
            mychart.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 0;

            mychart.ChartAreas[0].CursorY.IsUserEnabled = Enabled;
            mychart.ChartAreas[0].CursorY.IsUserSelectionEnabled = Enabled;
            mychart.ChartAreas[0].CursorY.Interval = 0;
            mychart.ChartAreas[0].AxisY.ScaleView.Zoomable = Enabled;
            mychart.ChartAreas[0].AxisY.ScrollBar.IsPositionedInside = true;
            mychart.ChartAreas[0].AxisY.ScrollBar.ButtonStyle = System.Windows.Forms.DataVisualization.Charting.ScrollBarButtonStyles.ResetZoom;
            mychart.ChartAreas[0].AxisY.ScaleView.SmallScrollMinSize = 0;
            if (Enabled == false)
            {
                //Remove the cursor lines
                mychart.ChartAreas[0].CursorX.SetCursorPosition(double.NaN);
                mychart.ChartAreas[0].CursorY.SetCursorPosition(double.NaN);
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                show_data_fft();
            else
                chart2.Series["Series1"].Points.Clear();
        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            e.Row.HeaderCell.Value = string.Format("{0}", e.Row.Index + 1);
        }

        private void chart1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset();
        }

        private void chart2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            chart2.ChartAreas[0].AxisX.ScaleView.ZoomReset();
            chart2.ChartAreas[0].AxisY.ScaleView.ZoomReset();
        }

        private void chart2_MouseMove(object sender, MouseEventArgs e)
        {
            HitTestResult result = chart2.HitTest(e.X, e.Y);

            if (result.ChartElementType == ChartElementType.DataPoint)
            {
                Cursor = Cursors.Hand;
                label11.Text = chart2.Series[0].Points[result.PointIndex].XValue.ToString();

                label10.Text = chart2.Series[0].Points[result.PointIndex].YValues[0].ToString();

                ///

                ////var aa = result.Object as DataPoint;
                ////   txtX.Text = aa.XValue.ToString();
                ////  txtY.Text = aa.YValues[0].ToString();
            }
            else if (result.ChartElementType != ChartElementType.Nothing)
            {
                Cursor = Cursors.Default;

            }
        }
    }

    /// <summary>
    /// 快速傅立叶变换(Fast Fourier Transform)。
    /// </summary>
    struct complex//定义复数 
    {

        //复数a+bi中 
        //a为实部，b为虚部 
        public double a;
        public double b;
        public static complex commul(double x, complex y)
        {
            complex c = new complex();
            c.a = x * y.a;
            c.b = x * y.b;

            return c;
        }
        public static complex commulcc(complex x, complex y)
        {
            complex c = new complex();
            c.a = x.a * y.a - x.b * y.b;
            c.b = x.a * y.b + x.b * y.a;

            return c;
        }
        public static complex comsum(complex x, complex y)
        {
            complex c = new complex();
            c.a = x.a + y.a;
            c.b = x.b + y.b;

            return c;
        }
        public static complex comsum1(double x, complex y)
        {
            complex c = new complex();
            c.a = x + y.a;
            c.b = y.b;

            return c;
        }
        public static complex decrease(complex x, complex y)
        {
            complex c = new complex();
            c.a = x.a * y.a - x.b * y.b;
            c.b = x.a * y.b + x.b * y.a;

            return c;
        }
        public static complex powcc(complex x, double n)
        {
            int k;
            complex xout;
            xout.a = 1;
            xout.b = 0;
            for (k = 1; k<= n; k++) 
                {
                xout = complex.commulcc(xout, x);
            }
            return xout;
        }
    }
    class airthm
    {
        //计算ω=exp（j*2*pi/n） 
        public static complex omega(int n)
        {
            complex x;
            x.a = Math.Cos(0 - 2 * Math.PI / n);
            x.b = Math.Sin(0 - 2 * Math.PI / n);
            return x;
        }
        public static complex[] dft(double[] signal, int n)  //(信号，信号长度) 
        {
            int i, j;
            complex w1;
            w1 = omega(n);
            complex[] w = new complex[n];
            for (i = 0; i<n; i++) 
                {
                w[i] = complex.powcc(w1, i);
            }
            complex[] f = new complex[n];
            complex temp;  //w[i]的次方 
            complex temp1; //f中单项的值 
            for (i = 0; i <n; i++) 
                {
                f[i].a = 0;
                f[i].b = 0;
                for (j = 0; j <n; j++) 
                        {
                    temp = complex.powcc(w[i], j);
                    temp1 = complex.commul(signal[j], temp);
                    f[i] = complex.comsum(f[i], temp1);
                }
            }
            return f;
        }
        //求幅值  x  信号  n  信号长度  返回 幅值数组 
        public static double[] amplitude(complex[] x, int n)
        {
            int i;
            double temp;
            double[] amp = new double[n];
            for (i = 0; i< n; i++) 
                {
                temp = x[i].a * x[i].a + x[i].b * x[i].b;
                amp[i] = Math.Sqrt(temp);
            }
            return amp;
        }
    }

}
