using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace EPubMaker
{
    /// <summary>
    /// ページ
    /// </summary>
    public class Page : ICloneable
    {
        /// <summary>
        /// 回転
        /// </summary>
        public enum PageRotate
        {
            Rotate0 = 0,    /// 回転なし
            Rotate90 = 1,   /// 90°回転
            Rotate180 = 2,  /// 180°回転
            Rotate270 = 3   /// 270°回転
        }

        /// <summary>
        /// 画像形式
        /// </summary>
        public enum PageFormat
        {
            Undefined = -1, /// 未定義
            FullColor = 0,  /// フルカラー
            Gray8bit = 1,   /// 8bit
            Gray4bit = 2,   /// 4bit
            Mono = 3        /// 白黒
        }

        private string path;        /// 元データ画像パス
        private string index;       /// 目次表示内容
        private bool locked;        /// ロック状態
        private PageRotate rotate;  /// 回転
        private PageFormat format;  /// 画像形式
        private int clipLeft;       /// 切り抜き(左)%
        private int clipTop;        /// 切り抜き(上)%
        private int clipRight;      /// 切り抜き(右)%
        private int clipBottom;     /// 切り抜き(下)%
        private float contrast;     /// コントラスト調整(-1.0～1.0)

        /// <summary>
        /// 元データ画像パス
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }
        }

        /// <summary>
        /// 元データ画像パスのファイル名部分
        /// </summary>
        public string Name
        {
            get
            {
                FileInfo info = new FileInfo(path);
                return info.Name;
            }
        }

        /// <summary>
        /// 目次表示内容
        /// </summary>
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

        /// <summary>
        /// ロック状態
        /// </summary>
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

        /// <summary>
        /// 回転
        /// </summary>
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

        /// <summary>
        /// 画像形式
        /// </summary>
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

        /// <summary>
        /// 切り抜き(左)%
        /// </summary>
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

        /// <summary>
        /// 切り抜き(上)%
        /// </summary>
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

        /// <summary>
        /// 切り抜き(右)%
        /// </summary>
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

        /// <summary>
        /// 切り抜き(下)%
        /// </summary>
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

        /// <summary>
        /// コントラスト
        /// </summary>
        public float Contrast
        {
            set
            {
                contrast = value;
            }
            get
            {
                return contrast;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="path">元画像パス</param>
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
            contrast = 0;
        }

        /// <summary>
        /// 文字列化
        /// </summary>
        /// <returns>元画像ファイル名</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// 複製生成
        /// </summary>
        /// <returns>複製</returns>
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

        /// <summary>
        /// 画像データ生成
        /// </summary>
        /// <param name="width">出力幅</param>
        /// <param name="height">出力高さ</param>
        /// <param name="src">元画像データ出力バッファ</param>
        /// <returns>結果画像</returns>
        public Image GenerateImages(int width, int height, out Image src)
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

            if (width <= 0 || height <= 0)
            {
                return null;
            }

            ImageAttributes attr = new ImageAttributes();
            float contrast = this.contrast + 1.0f;
            float[][] array = { new float[] {contrast, 0, 0, 0, 0},  // red
                                new float[] {0, contrast, 0, 0, 0},  // green
                                new float[] {0, 0, contrast, 0, 0},  // blue
                                new float[] {0, 0, 0, 1, 0},    // alpha
                                new float[] {(1.0f - contrast) * 0.5f, (1.0f - contrast) * 0.5f, (1.0f - contrast) * 0.5f, 0, 1}     // transform
                              };
            attr.SetColorMatrix(new ColorMatrix(array), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            attr.SetGamma(1.0f, ColorAdjustType.Bitmap);

            Image target = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(target);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, new Rectangle(0, 0, width, height), src.Width * clipLeft / 100, src.Height * clipTop / 100, srcWidth, srcHeight, GraphicsUnit.Pixel, attr);
            g.Dispose();

            if (Format > Page.PageFormat.FullColor)
            {
                target = ConvertFormat((Bitmap)target, Format);
            }

            return target;
        }

        /// <summary>
        /// 画像形式変換
        /// </summary>
        /// <param name="src">元画像データ</param>
        /// <param name="format">出力形式</param>
        /// <returns>変換結果画像</returns>
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
