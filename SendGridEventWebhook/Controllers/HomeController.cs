using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;

using System.Threading.Tasks;
using System;

namespace SendGridEventWebhook.Controllers
{
    // Display the default home page for this example
    public class HomeController : Controller
    {        
        public ActionResult Index()
        {
            return View();
        }
    }

    // Capture the SendGrid Event Webhook POST's at the /api/SendGrid endpoint
    public class apiController : Controller
    {
        [HttpPost]
        [ValidateInput(false)]
        public async Task<ActionResult> SendGrid()
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(HttpContext.Request.InputStream);
            string rawSendGridJSON = reader.ReadToEnd();
			DocumentDb ddb = DocumentDb.GetInstance();
			try 
			{
				await ddb.RunBulkImport(rawSendGridJSON);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				throw ex;
			}
            return new HttpStatusCodeResult(200);
        }
    }
}