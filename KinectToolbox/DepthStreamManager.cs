using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Research.Kinect.Nui;

namespace Kinect.Toolbox
{
    public class DepthStreamManager : Notifier
    {
        byte[] depthFrame32;

        public WriteableBitmap DepthBitmap { get; private set; }

        public int XScanBox { get; private set; }
        public int YScanBox { get; private set; }

        public int HeightScanBox { get; private set; }
        public int WidthScanBox { get; private set; }
        public bool UserDetected { get; private set; }

        public void Update(ImageFrameReadyEventArgs e)
        {
            if (depthFrame32 == null)
            {
                depthFrame32 = new byte[e.ImageFrame.Image.Width * e.ImageFrame.Image.Height * 4];
            }

            ConvertDepthFrame(e.ImageFrame.Image.Bits);

            if (DepthBitmap == null)
            {
                DepthBitmap = new WriteableBitmap(e.ImageFrame.Image.Width, e.ImageFrame.Image.Height, 96, 96, PixelFormats.Bgra32, null);
            }

            DepthBitmap.Lock();

            int stride = DepthBitmap.PixelWidth * DepthBitmap.Format.BitsPerPixel / 8;
            Int32Rect dirtyRect = new Int32Rect(0, 0, DepthBitmap.PixelWidth, DepthBitmap.PixelHeight);
            DepthBitmap.WritePixels(dirtyRect, depthFrame32, stride, 0);

            DepthBitmap.AddDirtyRect(dirtyRect);
            DepthBitmap.Unlock();

            RaisePropertyChanged(()=>DepthBitmap);
        }

        void ConvertDepthFrame(byte[] depthFrame16)
        {
            int startY = int.MaxValue;
            int endY = int.MinValue;

            int startX = int.MaxValue;
            int endX = int.MinValue;

            bool userFound = false;

            for (int i16 = 0, i32 = 0; i16 < depthFrame16.Length && i32 < depthFrame32.Length; i16 += 2, i32 += 4)
            {
                int user = depthFrame16[i16] & 0x07;
                int realDepth = (depthFrame16[i16 + 1] << 5) | (depthFrame16[i16] >> 3);
                // transform 13-bit depth information into an 8-bit intensity appropriate
                // for display (we disregard information in most significant bit)
                byte intensity = (byte)(255 - (255 * realDepth / 0x0fff));

                depthFrame32[i32] = 0;
                depthFrame32[i32 + 1] = 0;
                depthFrame32[i32 + 2] = 0;
                depthFrame32[i32 + 3] = 255;

                if (user != 0)
                {
                    int y = (i32 / 4) / DepthBitmap.PixelWidth;

                    if (y < startY)
                        startY = y;

                    if (y > endY)
                        endY = y;

                    int x = (i32 / 4) - DepthBitmap.PixelWidth * y;

                    if (x < startX)
                        startX = x;

                    if (x > endX)
                        endX = x;

                    userFound = true;
                }

                switch (user)
                {
                    case 0: // no one
                        depthFrame32[i32] = (byte)(intensity / 2);
                        depthFrame32[i32 + 1] = (byte)(intensity / 2);
                        depthFrame32[i32 + 2] = (byte)(intensity / 2);
                        break;
                    case 1:
                        depthFrame32[i32] = intensity;
                        break;
                    case 2:
                        depthFrame32[i32 + 1] = intensity;
                        break;
                    case 3:
                        depthFrame32[i32 + 2] = intensity;
                        break;
                    case 4:
                        depthFrame32[i32] = intensity;
                        depthFrame32[i32 + 1] = intensity;
                        break;
                    case 5:
                        depthFrame32[i32] = intensity;
                        depthFrame32[i32 + 2] = intensity;
                        break;
                    case 6:
                        depthFrame32[i32 + 1] = intensity;
                        depthFrame32[i32 + 2] = intensity;
                        break;
                    case 7:
                        depthFrame32[i32] = intensity;
                        depthFrame32[i32 + 1] = intensity;
                        depthFrame32[i32 + 2] = intensity;
                        break;
                }
            }

            if (startY != int.MaxValue)
            {
                YScanBox = startY;
                RaisePropertyChanged(()=>YScanBox);
            }

            if (endY != int.MinValue)
            {
                HeightScanBox = endY - startY;
                RaisePropertyChanged(() => HeightScanBox);
            }

            if (startX != int.MaxValue)
            {
                XScanBox = startX;
                RaisePropertyChanged(() => XScanBox);
            }

            if (endX != int.MinValue)
            {
                WidthScanBox = endX - startX;
                RaisePropertyChanged(() => WidthScanBox);
            }

            if (userFound != UserDetected)
            {
                UserDetected = userFound;
                RaisePropertyChanged(() => UserDetected);
            }
        }
    }
}
