using System;
using System.Web;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace web
{
    public class DEL : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            string key = "", data = "";
            string method = context.Request.HttpMethod;
            switch (method)
            {
                case "POST":
                    key = context.Request.QueryString["file"];
                    if (!string.IsNullOrEmpty(key))
                    {
                        string file = Path.Combine(context.Server.MapPath("~/data/"), key + ".htm");
                        if (File.Exists(file))
                        {
                            File.Delete(file);
                            Api.load();
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

    public class Img : IHttpHandler
    {
        private static List<string> list = new List<string>();

        static string pathDir = "";
        public static void load()
        {
            list.Clear();
            if (string.IsNullOrEmpty(pathDir))
                pathDir = HttpContext.Current.Server.MapPath("~/data/");

            DirectoryInfo info = new DirectoryInfo(pathDir);
            string[] fs = info.GetFiles()
                .OrderByDescending(p => p.CreationTime)
                .Select(x => x.Name.ToLower())
                .Where(x => x.EndsWith(".jpg") || x.EndsWith(".png"))
                .ToArray();

            //string[] fs = Directory.GetFiles(pathDir, "*.*")
            //    .Select(x => Path.GetFileName(x).ToLower())
            //    .Where(x => x.EndsWith(".jpg") || x.EndsWith(".png"))
            //    .OrderByDescending(x => x)
            //    .ToArray();
            list.AddRange(fs);
        }

        public void ProcessRequest(HttpContext context)
        {
            if (pathDir == "")
            {
                pathDir = context.Server.MapPath("~/images/");
                load();
            }

            string key = "", data = "";
            string method = context.Request.HttpMethod;
            switch (method)
            {
                case "GET":
                    key = context.Request.QueryString["key"];
                    if (string.IsNullOrEmpty(key))
                        data = JsonConvert.SerializeObject(list, Formatting.Indented);
                    else
                    {
                        key = key.ToLower().Trim();
                        data = JsonConvert.SerializeObject(list.Find(x => x.ToLower().Contains(key)), Formatting.Indented);
                    }
                    context.Response.ContentType = "text/plain";
                    context.Response.Write(data);
                    break;
                case "POST":
                    key = context.Request.QueryString["file"];
                    if (!string.IsNullOrEmpty(key))
                    {
                        //write your handler implementation here.
                        if (context.Request.Files.Count <= 0)
                        {
                            data = "No file uploaded";
                        }
                        else
                        {
                            string title = context.Request.Headers["Key"];
                            string fh = context.Request.Headers["FileName"];

                            for (int i = 0; i < context.Request.Files.Count; ++i)
                            {
                                HttpPostedFile file = context.Request.Files[i];
                                var fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff-");// + file.FileName;

                                if (!string.IsNullOrEmpty(title))
                                    fileName += "-" + title;

                                if (!string.IsNullOrEmpty(fh))
                                    fileName += "-" + fh.ToString();

                                string pathFile = Path.Combine(pathDir, fileName);
                                file.SaveAs(pathFile);
                                data = fileName;
                                load();
                            }
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