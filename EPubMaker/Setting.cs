using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EPubMaker
{
    /// <summary>
    /// アプリ設定
    /// </summary>
    public class Setting
    {
        static private Setting instance = null; /// 設定のインスタンス

        /// <summary>
        /// インスタンス取得
        /// </summary>
        public static Setting Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Setting();
                }
                return instance;
            }
        }

        private bool changed;   /// 変更されてる？

        // 表示位置
        private int left;       /// メインフォーム左端
        private int top;        /// メインフォーム上端
        private int width;      /// メインフォーム幅
        private int height;     /// メインフォーム高さ
        private int distance;   /// 画像表示領域分離幅(=元画像領域幅)

        /// <summary>
        /// メインフォーム左端
        /// </summary>
        public int Left
        {
            set
            {
                left = value;
                changed = true;
            }
            get
            {
                return left;
            }
        }

        /// <summary>
        /// メインフォーム上端
        /// </summary>
        public int Top
        {
            set
            {
                top = value;
                changed = true;
            }
            get
            {
                return top;
            }
        }
        
        /// <summary>
        /// メインフォーム幅
        /// </summary>
        public int Width
        {
            set
            {
                width = value;
                changed = true;
            }
            get
            {
                return width;
            }
        }

        /// <summary>
        /// メインフォーム高さ
        /// </summary>
        public int Height
        {
            set
            {
                height = value;
                changed = true;
            }
            get
            {
                return height;
            }
        }

        /// <summary>
        /// 画像表示領域分離幅
        /// </summary>
        public int Distance
        {
            set
            {
                distance = value;
                changed = true;
            }
            get
            {
                return distance;
            }
        }

        // ePub関連設定
        private int pageWidth;      /// 出力幅
        private int pageHeight;     /// 出力高さ
        
        /// <summary>
        /// 出力幅
        /// </summary>
        public int PageWidth
        {
            set
            {
                pageWidth = value;
                changed = true;
            }
            get
            {
                return pageWidth;
            }
        }

        /// <summary>
        /// 出力高さ
        /// </summary>
        public int PageHeight
        {
            set
            {
                pageHeight = value;
                changed = true;
            }
            get
            {
                return pageHeight;
            }
        }

        // パス関係
        private string prevSrc;     /// 前回の元データフォルダ
        private string outPath;     /// 出力先フォルダ

        /// <summary>
        /// 前回の元データフォルダ
        /// </summary>
        public string PrevSrc
        {
            set
            {
                prevSrc = value;
                changed = true;
            }
            get
            {
                return prevSrc;
            }
        }

        /// <summary>
        /// 出力先フォルダ
        /// </summary>
        public string OutPath
        {
            set
            {
                outPath = value;
                changed = true;
            }
            get
            {
                return outPath;
            }
        }

        /// <summary>
        /// コンストラクタ(シングルトンなのでprivate)
        /// </summary>
        private Setting()
        {
            changed = false;

            left = -1;
            top = -1;
            width = -1;
            height = -1;

            pageWidth = 480;
            pageHeight = 800;

            prevSrc = null;
            outPath = null;
        }

        /// <summary>
        /// 設定ロード
        /// </summary>
        /// <returns>インスタンス</returns>
        public static Setting Load()
        {
            if (instance == null)
            {
                instance = new Setting();
            }

            RegistryKey reg = GetRegKey();

            instance.left = (int)reg.GetValue("x", -1);
            instance.top = (int)reg.GetValue("y", -1);
            instance.width = (int)reg.GetValue("width", -1);
            instance.height = (int)reg.GetValue("height", -1);
            instance.distance = (int)reg.GetValue("srcWidth", -1);

            instance.pageWidth = (int)reg.GetValue("pageWidth", 480);
            instance.pageHeight = (int)reg.GetValue("pageHeight", 800);

            instance.prevSrc = (string)reg.GetValue("prevSrc", null);
            instance.outPath = (string)reg.GetValue("outPath", null);

            reg.Close();

            return instance;
        }

        /// <summary>
        /// 設定保存
        /// </summary>
        public void Save()
        {
            if (changed)
            {
                RegistryKey reg = GetRegKey();

                reg.SetValue("x", left, RegistryValueKind.DWord);
                reg.SetValue("y", top, RegistryValueKind.DWord);
                reg.SetValue("width", width, RegistryValueKind.DWord);
                reg.SetValue("height", height, RegistryValueKind.DWord);
                reg.SetValue("srcWidth", distance, RegistryValueKind.DWord);

                reg.SetValue("pageWidth", pageWidth, RegistryValueKind.DWord);
                reg.SetValue("pageHeight", pageHeight, RegistryValueKind.DWord);

                if (!String.IsNullOrEmpty(prevSrc))
                {
                    reg.SetValue("prevSrc", prevSrc, RegistryValueKind.String);
                }
                if (!String.IsNullOrEmpty(outPath))
                {
                    reg.SetValue("outPath", outPath, RegistryValueKind.String);
                }

                reg.Close();

                changed = false;
            }
        }

        /// <summary>
        /// レジストリキー取得
        /// 本当は Application.UserAppDataRegistry でいいと思うのだが、なぜかバージョン番号をビルド番号まで含めたものを渡しやがるので自力で生成
        /// </summary>
        /// <returns>キー</returns>
        private static RegistryKey GetRegKey()
        {
            return Registry.CurrentUser.CreateSubKey(String.Format(@"Software\{0}\{1}", Application.CompanyName, Application.ProductName));
        }
    }
}
