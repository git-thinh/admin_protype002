using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace web
{
    public class Api : IHttpHandler
    {
        private static ListBase<Article> list = new ListBase<Article>();
        public static Article FindItemAndThemeContent(Func<Article, bool> condition)
        {
            if (pathDir == "")
            {
                pathDir = HttpContext.Current.Server.MapPath("~/data/");
                load();
            }

            var it = list.FindItem(condition);
            if (it != null)
            {
                var them = list.FindItem(x => x.Key == it.Theme);
                if (them != null) it.ThemeContent = them.Content;
            }
            return it;
        }


        static string pathDir = "";
        public static void load()
        {
            list.Clear();
            if (string.IsNullOrEmpty(pathDir))
                pathDir = HttpContext.Current.Server.MapPath("~/data/");

            DirectoryInfo info = new DirectoryInfo(pathDir);
            string[] fs = info.GetFiles("*.htm")
                .OrderByDescending(p => p.CreationTime)
                .Select(x => x.FullName.ToLower())
                .ToArray();
            //string[] fs = Directory.GetFiles(pathDir, "*.htm")
            //    .OrderByDescending(x => x)
            //    .ToArray();
            foreach (var fi in fs)
            {
                string text = File.ReadAllText(fi);
                var it = new Article(text, fi);
                list.Add(it);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (pathDir == "")
            {
                pathDir = context.Server.MapPath("~/data/");
                load();
            }

            HttpCookie cookie = HttpContext.Current.Request.Cookies["userid"];
            if (cookie == null || cookie.Value == null || cookie.Value == "" || User.login(cookie.Value) == false)
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("login");
                return;
            }

            string key = "", data = "";
            string method = context.Request.HttpMethod;
            switch (method)
            {
                case "GET":
                    key = context.Request.QueryString["key"];
                    if (string.IsNullOrEmpty(key))
                        data = JsonConvert.SerializeObject(list.GetAll(), Formatting.Indented);
                    else
                    {
                        key = key.ToLower().Trim();
                        data = JsonConvert.SerializeObject(list.Find(x =>
                            x.File.ToLower().Contains(key) ||
                            x.Title.ToLower().Contains(key) ||
                            x.Theme.ToLower().Contains(key) ||
                            x.Tag.ToLower().Contains(key) ||
                            x.Content.ToLower().Contains(key)
                        ), Formatting.Indented);
                    }
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(data);
                    break;
                case "POST":
                    key = context.Request.QueryString["file"];
                    if (!string.IsNullOrEmpty(key))
                    {
                        string fileKEY = key;

                        using (StreamReader stream = new StreamReader(context.Request.InputStream))
                        {
                            data = stream.ReadToEnd();
                            data = HttpUtility.UrlDecode(data).Trim();
                        }
                        var itNew = new Article(data);
                        if (itNew != null)
                        {
                            if (fileKEY == "[NEW]")
                                fileKEY = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                            string hostKey = context.Request.Url.Host.ToLower().Replace('.', '-');
                            if (itNew.Theme == "home")
                            {
                                itNew.Title = hostKey;
                                fileKEY = hostKey;
                            }

                            if (itNew.Key == "blog-" + hostKey)
                            {
                                itNew.Title = "blog-" + hostKey;
                                fileKEY = "blog-" + hostKey;
                            }

                            data = itNew.ToString();

                            string file = Path.Combine(pathDir, fileKEY + ".htm");
                            File.WriteAllText(file, data);
                            load();
                            data = "";
                            var it = list.FindItem(x => x.File == fileKEY);
                            if (it != null) data = it.Key + "|" + it.Title + "|" + JsonConvert.SerializeObject(list.GetAll(), Formatting.Indented);
                        }

                        context.Response.ContentType = "text/plain";
                        context.Response.Write(data);
                    }
                    break;
            }
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