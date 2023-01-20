using Microsoft.AspNetCore.Mvc;
using Server.Utils;

namespace Server.Controllers
{
    public class HtmlController : Controller
    {
        public static String HtmlFile { get; set; } = "";

        [Route("")]
        [Route("signup")]
        [Route("signin")]
        [Route("validation")]
        [Route("myaccount")]
        public ActionResult Index()
        {
            using (StreamReader s = new StreamReader(HtmlController.HtmlFile))
            {
                var result = Content(s.ReadToEnd());
                result.ContentType = "text/html; charset=UTF-8";
                return result;
            }
        }
    }
}
