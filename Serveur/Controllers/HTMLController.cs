﻿using Microsoft.AspNetCore.Mvc;
using Server.Utils;

namespace Server.Controllers
{
    public class HTMLController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            var result = Content(new PageTemplate("index").render());
            result.ContentType = "text/html; charset=UTF-8";
            return result;
        }

        [Route("signup")]
        public ActionResult SignUp(bool success, String error)
        {
            object data = new {
                success = success,
                error = error,    
            };

            var result = Content(new PageTemplate("signup").render(data));
            result.ContentType = "text/html; charset=UTF-8";
            return result;
        }
    }
}
