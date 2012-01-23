using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace EPubMaker
{
    public class Page : ICloneable
    {
        public enum PageRotate
        {
            Rotate0 = 0,
            Rotate90 = 1,
            Rotate180 = 2,
            Rotate270 = 3
        }

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
        private bool locked;
        private PageRotate rotate;
        private PageFormat format;
        private int clipLeft;
        private int clipTop;
        private int clipRight;
        private int clipBottom;

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

        public bool Locked
        {
            set
            {
                locked = value;
            }
            get
            {
                return locked;
            }
        }

        public PageRotate Rotate
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

        public int ClipLeft
        {
            set
            {
                clipLeft = value;
            }
            get
            {
                return clipLeft;
            }
        }

        public int ClipTop
        {
            set
            {
                clipTop = value;
            }
            get
            {
                return clipTop;
            }
        }

        public int ClipRight
        {
            set
            {
                clipRight = value;
            }
            get
            {
                return clipRight;
            }
        }

        public int ClipBottom
        {
            set
            {
                clipBottom = value;
            }
            get
            {
                return clipBottom;
            }
        }

        public Page(string path)
        {
            this.path = path;
            index = "";
            locked = false;
            rotate = PageRotate.Rotate0;
            format = PageFormat.Undefined;
            clipLeft = 0;
            clipTop = 0;
            clipRight = 100;
            clipBottom = 100;
        }

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            Page newPage = new Page(path);
            newPage.index = index;
            newPage.locked = locked;
            newPage.rotate = rotate;
            newPage.format = format;
            newPage.clipLeft = clipLeft;
            newPage.clipTop = clipTop;
            newPage.clipRight = clipRight;
            newPage.clipBottom = clipBottom;
            return newPage;
        }

        public void GenerateImages(out Image src, int width, int height, out Image preview)
        {
            src = Image.FromFile(Path);
            switch (Rotate)
            {
                case PageRotate.Rotate0:
                    src.RotateFlip(RotateFlipType.RotateNoneFlipNone);
                    break;
                case PageRotate.Rotate90:
                    src.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case PageRotate.Rotate180:
                    src.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case PageRotate.Rotate270:
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

            int srcWidth = src.Width * (100 - clipLeft - (100 - clipRight)) / 100;
            int srcHeight = src.Height * (100 - clipTop - (100 - clipBottom)) / 100;
            if (srcWidth / (double)width < srcHeight / (double)height)
            {
                double d = srcHeight / (double)height;
                width = (int)(srcWidth / d);
            }
            else
            {
                double d = srcWidth / (double)width;
                height = (int)(srcHeight / d);
            }

            preview = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(preview);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, new Rectangle(0, 0, width, height), src.Width * clipLeft / 100, src.Height * clipTop / 100, srcWidth, srcHeight, GraphicsUnit.Pixel);
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
