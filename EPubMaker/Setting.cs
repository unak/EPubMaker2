using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace EPubMaker
{
    public class Setting
    {
        static private Setting instance = null;

        private bool changed;

        // 表示位置
        private int x;
        private int y;
        private int width;
        private int height;
        public int Top
        {
            set
            {
                x = value;
                changed = true;
            }
            get
            {
                return x;
            }
        }
        public int Left
        {
            set
            {
                y = value;
                changed = true;
            }
            get
            {
                return y;
            }
        }
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

        // ePub関連設定
        private int pageWidth;
        private int pageHeight;
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
        private string prevSrc;
        private string outPath;
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

        private Setting()
        {
            changed = false;

            x = -1;
            y = -1;
            width = -1;
            height = -1;

            pageWidth = 480;
            pageHeight = 800;

            prevSrc = null;
            outPath = null;
        }

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

        public static Setting Load()
        {
            if (instance == null)
            {
                instance = new Setting();
            }

            RegistryKey reg = Application.UserAppDataRegistry;

            instance.x = (int)reg.GetValue("x", -1);
            instance.y = (int)reg.GetValue("y", -1);
            instance.width = (int)reg.GetValue("width", -1);
            instance.height = (int)reg.GetValue("height", -1);

            instance.pageWidth = (int)reg.GetValue("pageWidth", 480);
            instance.pageHeight = (int)reg.GetValue("pageHeight", 800);

            instance.prevSrc = (string)reg.GetValue("prevSrc", null);
            instance.outPath = (string)reg.GetValue("outPath", null);

            reg.Close();

            return instance;
        }

        public void Save()
        {
            if (changed)
            {
                RegistryKey reg = Application.UserAppDataRegistry;

                reg.SetValue("x", x, RegistryValueKind.DWord);
                reg.SetValue("y", y, RegistryValueKind.DWord);
                reg.SetValue("width", width, RegistryValueKind.DWord);
                reg.SetValue("height", height, RegistryValueKind.DWord);

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
    }
}
