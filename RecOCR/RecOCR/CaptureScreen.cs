using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace RecOCR
{
    class CaptureScreen
    {
        public static Bitmap GetRegion(Point upLeftPoint, Point downRightPoint)
        {
            Bitmap outputImage;
            Size rectSize = new Size(downRightPoint.X - upLeftPoint.X, downRightPoint.Y - upLeftPoint.Y);
            using (Bitmap bitmap = new Bitmap(rectSize.Width, rectSize.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {                  
                    g.CopyFromScreen(upLeftPoint.X, upLeftPoint.Y, 0, 0, rectSize);
                    outputImage = new Bitmap(bitmap);
                }
            }
            return outputImage;
        }
    }
}
