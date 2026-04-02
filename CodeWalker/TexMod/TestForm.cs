using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.TexMod
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var source = new AsyncImageFileSource("CodeWalker\\Resources\\CustomUVChecker_byValle_1K.png");
            d2DCanvas1.SetImage(source);
            d2DCanvas1.onBitmapLoaded = canvas =>
            {
                var imageSize = canvas.GetImageSize();
                PictureBoxRectTool.SetRect(canvas, new RectangleF(0, 0, imageSize.Width, imageSize.Height));
                //PictureBoxViewer.FitViewer(canvas, imageSize.Width, imageSize.Height);
            };
            d2DCanvas1.onPaint = (canvas, target, bitmap) =>
            {
                PictureBoxViewer.Paint(canvas, bitmap);
                canvas.DrawBitmap(bitmap, 0, 0);
                PictureBoxRectTool.Paint(canvas);
            };
            PictureBoxViewer.AddFeature(d2DCanvas1);
            PictureBoxRectTool.AddFeature(d2DCanvas1, null);
        }
    }
}