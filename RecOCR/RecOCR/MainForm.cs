using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecOCR
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const int CTRL_KEY_PTR = 1;

        private Point ClickPoint = new Point();
        private bool drawingRectangle = false;
        private Graphics g;

        private Pen EraserPen;
        private Pen MyPen = new Pen(Color.White, 2);

        private Point RectTopLeft = new Point();
        private Point RectBottomRight = new Point();

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
            this.FormClosing += MainForm_FormClosing;
            this.ShowInTaskbar = false;
            this.MouseDown += new MouseEventHandler(mouseDown);
            this.MouseUp += new MouseEventHandler(mouseUp);
            this.MouseMove += new MouseEventHandler(mouseMove);
            
            //keydo += new KeyEventHandler(keyDown);
            //this.KeyUp += new KeyEventHandler(keyUp);

            EraserPen = new Pen(this.BackColor, 2);
            //MyPen.DashOffset = 8;
    
            MyPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            RegisterHotKey(this.Handle, CTRL_KEY_PTR, 2, (int)Keys.X);
            g = this.CreateGraphics();
           
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Hide();
            notifyIcon.BalloonTipIcon = ToolTipIcon.None;
           // notifyIcon.BalloonTipText = "RecOCR Running";
            notifyIcon.BalloonTipTitle = "Running";
            notifyIcon.ShowBalloonTip(800);
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
            {
                this.Hide();
                this.Cursor = Cursors.Arrow;
                drawingRectangle = false;
            }
        }

    
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == CTRL_KEY_PTR)
            {
                this.Show();
                this.Cursor = Cursors.Cross;
                drawingRectangle = true;
            }
            base.WndProc(ref m);
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && drawingRectangle)
            {
                DrawSelection();
            }
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && drawingRectangle && RectTopLeft != RectBottomRight)
            {
                string outputString = "";
                drawingRectangle = false;
                this.Hide();
                Bitmap capture = CaptureScreen.GetRegion(RectTopLeft, RectBottomRight);

                TesseractConverter.TessConvertResult tessResult;

                if (onlyNumbersToolStripMenuItem.Checked)
                {
                    tessResult = TesseractConverter.BitmapToString(capture, TesseractConverter.OcrMode.numeric);
                }
                else
                {
                    tessResult = TesseractConverter.BitmapToString(capture, TesseractConverter.OcrMode.alphanumeric);
                }


                if (tessResult != null)
                {
                    if (onlyNumbersToolStripMenuItem.Checked)
                        outputString = tessResult.convertedText.Trim().Replace(" ", string.Empty).Replace(',', '.');
                    else
                        outputString = tessResult.convertedText.Trim();

                    Clipboard.SetText(outputString);

                    if (toolStripMenuItemShowMessages.Checked && tessResult.meanConfidence >= 0.65)
                    {
                        notifyIcon.BalloonTipIcon = ToolTipIcon.None;
                        notifyIcon.BalloonTipText = string.Format("{0}\nConfidence: {1}%", outputString, tessResult.meanConfidence * 100);
                        notifyIcon.BalloonTipTitle = "RecOCR Result";
                        notifyIcon.ShowBalloonTip(1000);  
                    }
                    else if (toolStripMenuItemShowMessages.Checked && tessResult.meanConfidence < 0.65)
                    {
                        notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
                        notifyIcon.BalloonTipText = string.Format("{0}\nConfidence: {1}%", outputString, tessResult.meanConfidence * 100);
                        notifyIcon.BalloonTipTitle = "RecOCR Low Confidence";
                        notifyIcon.ShowBalloonTip(1000);
                    }
                }

            }
        }

       

        private void mouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && drawingRectangle)
            {
                ClickPoint = e.Location;
            }
        }


        private void DrawSelection()
        {            
            g.DrawRectangle(EraserPen, RectTopLeft.X, RectTopLeft.Y, RectBottomRight.X - RectTopLeft.X, RectBottomRight.Y - RectTopLeft.Y);          

            if (Cursor.Position.X < ClickPoint.X)
            {
                RectTopLeft.X = Cursor.Position.X;
                RectBottomRight.X = ClickPoint.X;
            }
            else
            {
                RectTopLeft.X = ClickPoint.X;
                RectBottomRight.X = Cursor.Position.X;

            }
            if (Cursor.Position.Y < ClickPoint.Y)
            {
                RectTopLeft.Y = Cursor.Position.Y;
                RectBottomRight.Y = ClickPoint.Y;
            }
            else
            {
                RectTopLeft.Y = ClickPoint.Y;
                RectBottomRight.Y = Cursor.Position.Y;
            }

            g.DrawRectangle(MyPen, RectTopLeft.X, RectTopLeft.Y, RectBottomRight.X - RectTopLeft.X, RectBottomRight.Y - RectTopLeft.Y);
        }

        private void toolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            notifyIcon.BalloonTipIcon = ToolTipIcon.None;
            notifyIcon.BalloonTipTitle = "Running";
            notifyIcon.ShowBalloonTip(800);
        }
       

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Made by Piotr Redmerski @rayred93\n\nFor Perseveres <3", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

       
    }
}
