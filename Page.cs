using System;
using System.IO;
using System.Web;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace web
{
    public class Page : IHttpHandler
    {
        static string pathDir = "";
        public void ProcessRequest(HttpContext context)
        {
            if (pathDir == "")
                pathDir = context.Server.MapPath("~/");

            string htm = "", key = context.Request.Path;
            Article item = null;

            switch (key)
            {
                case "/home.html":
                    key = context.Request.Url.Host.ToLower().Replace('.', '-');
                    item = Api.FindItemAndThemeContent(x => x.Key.Equals(key));
                    if (item == null)
                        htm = key;
                    else
                        htm = item.Content; 
                    break;
                case "static":
                case "/index.html":
                case "/login.html":
                case "/admin.html": 
                    string fhtm = pathDir + key;
                    if (!fhtm.EndsWith(".html")) fhtm += ".html";
                    if (File.Exists(fhtm)) htm = File.ReadAllText(fhtm);
                    break;
                default:
                    #region [] 
                    key = key.Substring(1, key.Length - 6);

                    if(key == context.Request.Url.Host.ToLower().Replace('.', '-')) goto case "/home.html";

                    item = Api.FindItemAndThemeContent(x => x.Key.Equals(key));
                    if (item == null) item = Api.FindItemAndThemeContent(x => x.Key.Equals("404"));

                    if (item == null)
                        {
                        goto case "static";
                    }
                    else
                    {
                        #region [ === FORMAT VALUE === ]

                        List<string> lsHeadingASCII = new List<string>() { "a.", "b.", "c.", "d.", "e.", "f.", "g.", "h.", "i.", "j.", "k.", "l.", "m.", "n.", "o.", "p.", "q.", "r.", "s.", "t.", "u.", "v.", "w.", "x.", "y.", "z." };

                        List<string> lsHeading = new List<string>() { "II", "IV", "VI", "IX" };
                        string con = item.Content;
                        //List<string> listImg = new List<string>();
                        //var ms = Regex.Matches(con, "{img(.+?)img}", RegexOptions.IgnoreCase);
                        //foreach (Match mi in ms)
                        //{
                        //    string img = mi.ToString();
                        //    listImg.Add(img);
                        //}
                        string[] a = con.Split(new string[] { "\n", "\r" }, StringSplitOptions.None).Select(x => x.Trim()).Where(x => x != "").ToArray();

                        StringBuilder bi = new StringBuilder("<div class=article-cndata>");
                        bi.Append(string.Format("{0}<h1>{1}</h1>{0}", Environment.NewLine, item.Title));
                        foreach (string si in a)
                        {
                            if (string.IsNullOrEmpty(si)) continue;

                            string tag = "";
                            if (si.StartsWith("{img") && si.EndsWith("img}"))
                            {
                                string src = si.Substring(4);
                                src = src.Substring(0, src.Length - 4).Trim();
                                tag = string.Format("{0}<p class=img><img src=\"{1}\"></p>{0}", Environment.NewLine, src);
                            }
                            else
                            {
                                int len = si.Split(' ').Length;
                                if (len < 20 && (
                                    char.IsDigit(si[0]) ||
                                    si[0] == '*' ||
                                    si[0] == 'I' ||
                                    si[0] == 'V' ||
                                    si[0] == 'X' ||
                                    lsHeading.IndexOf(si.Substring(0, 2)) != -1)
                                    )
                                    tag = string.Format("{0}<h3>{1}</h3>{0}", Environment.NewLine, si);
                                else if (len < 20 && lsHeadingASCII.IndexOf(si.Substring(0, 2)) != -1)
                                    tag = string.Format("{0}<h6>{1}</h6>{0}", Environment.NewLine, si);
                                else
                                    tag = string.Format("{0}<p>{1}</p>{0}", Environment.NewLine, si);
                            }
                            bi.Append(tag);
                        }
                        bi.Append(Environment.NewLine);
                        bi.Append("</div>");

                        string CONTENT = bi.ToString();

                        #endregion

                        string theme = item.ThemeContent;
                        if (string.IsNullOrEmpty(theme))
                            htm = CONTENT;
                        else
                            htm = theme.Replace("[ARTICLE]", CONTENT).Replace("[TITLE]", item.Title);
                    }

                    #endregion
                    break;
            }
            context.Response.Clear();
            context.Response.ContentType = "text/html";
            context.Response.Write(htm);
            context.Response.Flush();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

    }


}
