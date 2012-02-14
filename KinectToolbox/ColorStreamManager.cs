using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Research.Kinect.Nui;

namespace Kinect.Toolbox
{
    public class ColorStreamManager : Notifier
    {
        public BitmapSource ColorBitmap { get; private set; }

        public BitmapSource Update(ImageFrameReadyEventArgs e)
        {
            PlanarImage Image = e.ImageFrame.Image;

            ColorBitmap = BitmapSource.Create(Image.Width, Image.Height, 96, 96, PixelFormats.Bgr32, null, Image.Bits, Image.Width * Image.BytesPerPixel);

            RaisePropertyChanged(()=>ColorBitmap);

            return ColorBitmap;
        }
    }
}
