//using api.Interface;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading;
using System.Web;
using System.Linq;
using System.Web.Hosting;
using System.IO;

namespace web
{
    public class Global : System.Web.HttpApplication
    {
        public override void Init()
        {
            //this.BeginRequest += page.OnBeginRequest;
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            String url = Request.Path.ToLower();
            switch (url)
            {
                case "/":
                case "/index.html":
                    Context.RewritePath("/home.html");
                    break;
                case "/admin":
                case "/admin/":
                case "/admin.html":
                    Context.RewritePath("/admin.html");
                    break;
            }
        }


        protected void Application_Start(object sender, EventArgs e)
        {
        }

        void Application_End(object sender, EventArgs e)
        {
        }

        void Session_Start(object sender, EventArgs e)
        {
        }

        void Session_End(object sender, EventArgs e)
        {
        }


    }
}