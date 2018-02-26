using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace web
{
    public class Article
    {
        public string File { set; get; }
        public string Key { set; get; }
        public string Theme { set; get; }
        public string ThemeContent { set; get; }
        public string Tag { set; get; }

        private string _Title = "";
        public string Title
        {
            set
            {
                _Title = value.Trim();
                Key = ToAscii(_Title);
            }
            get
            {
                return _Title;
            }
        }

        public string Content { set; get; }
        public long DateCreate { set; get; }

        public Article() { }
        public Article(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                string[] a = text.Split(new string[] { "\n", "\r" }, StringSplitOptions.None)
                        .Select(x => x.Trim())
                        .Where(x => x != "")
                        .ToArray();
                long date = 0;
                long.TryParse(a[0], out date);
                DateCreate = date;
                if (a.Length > 3)
                {
                    Theme = a[1];
                    Tag = a[2];
                    Title = a[3];
                    Key = ToAscii(Title);
                    a = a.Where((x, k) => k > 3).ToArray();
                    Content = string.Join(Environment.NewLine + Environment.NewLine, a);
                }
            }
        }

        public override string ToString()
        {
            return string.Format("{1}{0}{2}{0}{3}{0}{4}{0}{5}", Environment.NewLine, DateCreate, Theme, Tag, Title, Content);
        }

        public Article(string text, string file) : this(text)
        {
            if (!string.IsNullOrEmpty(Title))
            {
                string finame = Path.GetFileName(file);
                File = finame.Substring(0, finame.Length - 4);
                //if (finame != Key + ".htm")
                //{
                //    string path = Path.GetDirectoryName(file) + "\\" + Key + ".htm";
                //    System.IO.File.Move(file, path);
                //}
            }
        }

        /// <summary>
        /// Chuyển chuỗi unicode sang ascii (lọc bỏ dấu tiếng việt) 
        /// </summary>
        /// <param name="unicode"></param>
        /// <returns></returns>
        public static String ToAscii(string unicode)
        {
            if (string.IsNullOrEmpty(unicode)) return "";
            unicode = unicode.ToLower();

            unicode = Regex.Replace(unicode.Trim(), "[áàảãạăắằẳẵặâấầẩẫậ]", "a");
            unicode = Regex.Replace(unicode.Trim(), "[óòỏõọôồốổỗộơớờởỡợ]", "o");
            unicode = Regex.Replace(unicode.Trim(), "[éèẻẽẹêếềểễệ]", "e");
            unicode = Regex.Replace(unicode.Trim(), "[íìỉĩị]", "i");
            unicode = Regex.Replace(unicode.Trim(), "[úùủũụưứừửữự]", "u");
            unicode = Regex.Replace(unicode.Trim(), "[ýỳỷỹỵ]", "y");
            unicode = unicode.Trim().Replace("đ", "d").Replace("đ", "d");
            unicode = Regex.Replace(unicode.Trim(), "[-\\s+/]+", "-");
            unicode = Regex.Replace(unicode.Trim(), "\\W+", "-"); //Nếu bạn muốn thay dấu khoảng trắng thành dấu "_" hoặc dấu cách " " thì thay kí tự bạn muốn vào đấu "-"
            return unicode.ToLower().Trim();
        }
    }

}