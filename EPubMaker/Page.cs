using System.IO;

namespace EPubMaker
{
    class Page
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
    }
}
