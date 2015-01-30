using System.Web.Mvc;
using System.Web.Routing;

using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System.Configuration;
using SendGridEventWebhook.Controllers;

namespace SendGridEventWebhook
{
    public class RouteConfig
    {
        public async static Task RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new string[] { "SendGridEventWebhook.Controllers" }
            );

            // Create stored procedure on DocumentDb 
            DocumentDb ddb = DocumentDb.GetInstance();
            await ddb.Init();
        }
	}
}
