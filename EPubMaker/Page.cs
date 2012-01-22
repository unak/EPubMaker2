using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace EPubMaker
{
    public class Page
    {
        public enum PageFormat
        {
            Undefined = -1,
            FullColor = 0,
            Gray8bit = 1,
            Gray4bit = 2,
            Mono = 3
        }

        private string path;
        private string index;
        private int rotate;
        private PageFormat format;

        public string Path
        {
            get
            {
                return path;
            }
        }

        public string Name
        {
            get
            {
                FileInfo info = new FileInfo(path);
                return info.Name;
            }
        }

        public string Index
        {
            set
            {
                index = value.Trim();
            }
            get
            {
                return index;
            }
        }

        public int Rotate
        {
            set
            {
                rotate = value;
            }
            get
            {
                return rotate;
            }
        }

        public PageFormat Format
        {
            set
            {
                format = value;
            }
            get
            {
                return format;
            }
        }

        public Page(string path)
        {
            this.path = path;
            this.index = "";
            this.rotate = 0;
            this.format = PageFormat.Undefined;
        }

        public override string ToString()
        {
            return Name;
        }

        public void GenerateImages(out Image src, int width, int height, out Image preview)
        {
            src = Image.FromFile(Path);
            switch (Rotate)
            {
                case 0:
                    src.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                    break;
                case 1:
                    src.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 2:
                    src.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 3:
                    src.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
            }
            if (Format < 0)
            {
                switch (src.PixelFormat)
                {
                    default:
                        /* フルカラー */
                        Format = Page.PageFormat.FullColor;
                        break;
                    case PixelFormat.Format8bppIndexed:
                        /* 8bitグレイスケール */
                        Format = Page.PageFormat.Gray8bit;
                        break;
                    case PixelFormat.Format16bppGrayScale:
                        /* 8bitグレイスケール */
                        Format = Page.PageFormat.Gray8bit;
                        break;
                    case PixelFormat.Format4bppIndexed:
                        /* 4bitグレイスケール */
                        Format = Page.PageFormat.Gray4bit;
                        break;
                    case PixelFormat.Format1bppIndexed:
                        /* 白黒 */
                        Format = Page.PageFormat.Mono;
                        break;
                }
            }

            if (src.Width / (double)width < src.Height / (double)height)
            {
                double d = src.Height / (double)height;
                width = (int)(src.Width / d);
            }
            else
            {
                double d = src.Width / (double)width;
                height = (int)(src.Height / d);
            }

            preview = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(preview);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, new Rectangle(0, 0, width, height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel);
            g.Dispose();

            if (Format > Page.PageFormat.FullColor)
            {
                preview = ConvertFormat((Bitmap)preview, Format);
            }
        }

        private static unsafe Bitmap ConvertFormat(Bitmap src, Page.PageFormat format)
        {
            PixelFormat pf;
            switch (format)
            {
                case Page.PageFormat.Gray8bit:
                    pf = PixelFormat.Format8bppIndexed;
                    break;
                case Page.PageFormat.Gray4bit:
                    pf = PixelFormat.Format4bppIndexed;
                    break;
                case Page.PageFormat.Mono:
                    pf = PixelFormat.Format1bppIndexed;
                    break;
                default:
                    return src;
            }

            Bitmap dst = new Bitmap(src.Width, src.Height, pf);
            BitmapData srcData = src.LockBits(new Rectangle(0, 0, src.Width, src.Height), ImageLockMode.ReadOnly, pf);
            BitmapData dstData = dst.LockBits(new Rectangle(0, 0, dst.Width, dst.Height), ImageLockMode.WriteOnly, pf);
            for (int y = 0; y < srcData.Height; ++y)
            {
                byte* srcRow = (byte*)srcData.Scan0 + (y * srcData.Stride);
                byte* dstRow = (byte*)dstData.Scan0 + (y * dstData.Stride);
                for (int i = 0; i < srcData.Stride; ++i)
                {
                    dstRow[i] = srcRow[i];
                }
            }
            dst.UnlockBits(dstData);
            src.UnlockBits(srcData);

            return dst;
        }
    }
}
