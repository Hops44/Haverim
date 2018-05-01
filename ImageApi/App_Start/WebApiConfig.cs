using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace ImageApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            //Download Default Image
            string DefaultImagePath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\Images\\default.png";
            if (!File.Exists(DefaultImagePath))
            {
                string url = "https://image.ibb.co/h8SX4H/default.png";
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile(new Uri(url), DefaultImagePath);
                }
            }

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
