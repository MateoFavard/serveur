using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Auth;
using Serveur.Database;
using Serveur.Model;
using System.Text.Json;

namespace Serveur.Controllers.Api
{
    [ApiController]
    [Route("api/universe")]
    //[Authorize(AuthenticationSchemes = "Basic")]
    public class UniverseController : Controller
    {

        [HttpPost("create")]
        public async Task<IActionResult> Create(UniverseModel universe)
        {
            string response = "";
            //String userId = HttpContext.User.Identity!.Name!;

            if (await Universe.InsertUniverse(name, password, owner))
            {
                CreateUniverseSuccess data = new CreateUniverseSuccess("Universe create successfully");
                response = JsonSerializer.Serialize<CreateUniverseSuccess>(data);
            }
            else
            {
                CreateUniverseFailure data = new CreateUniverseFailure("Universe could not be created");
                response = JsonSerializer.Serialize<CreateUniverseFailure>(data);
            }

            var result = new ContentResult
            {
                Content = response,
                ContentType = "application/json; charset=UTF-8",
            };
            return result;
        }
    }
}
