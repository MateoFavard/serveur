﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Auth;
using Server.Database;
using Server.Model;
using Server.Utils;
using System.Text.Json;

namespace Server.Controllers.Api
{
    [ApiController]
    [Route("api/universe")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class UniverseController : Controller
    {

        [HttpPost("create")]
        public async Task<IActionResult> Create(Model.Universe universe)
        {
            int? maybeId = HttpContext.User.UserId();

            if (maybeId is int id)
                {
                if (await Database.Universe.InsertUniverse(universe, id))
                {
                    return new StatusCodeResult(201);
                }
                else
                {
                    return new ContentResult {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        ContentType = "text/plain",
                        Content = "can't create universe",
                    };
                }
            }
            else
            {
                return new ContentResult {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentType = "text/plain",
                    Content = "unknown user",
                };
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> All()
        {
            int? maybeId = HttpContext.User.UserId();

            if (maybeId is int id)
            {
                var result = await Database.Universe.ReturnUniverse();

                return new ContentResult {
                    Content = JsonSerializer.Serialize<List<Model.Universe>>(result, Util.DefaultJsonOptions),
                    ContentType = "application/json; charset=UTF-8",
                };
            }
            else
            {
                return new ContentResult {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentType = "text/plain",
                    Content = "unknown user",
                };
            }
        }


        [HttpGet("joined")]
        public async Task<IActionResult> Joined()
        {
            int? maybeId = HttpContext.User.UserId();

            if (maybeId is int id)
            {
                var result = await Database.Universe.UniversPlayer(id);

                return new ContentResult {
                    Content = JsonSerializer.Serialize<List<Model.Universe>>(result, Util.DefaultJsonOptions),
                    ContentType = "application/json; charset=UTF-8",
                };
            }
            else
            {
                return new ContentResult {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentType = "text/plain",
                    Content = "unknown user",
                };
            }
        }

        /// <summary>
        /// Sends back the universe of the player
        /// </summary>
        /// <returns></returns>
        [HttpGet("owned")]
        public async Task<IActionResult> Owned()
        {
            int? maybeId = HttpContext.User.UserId();

            if (maybeId is int id)
            {
                var result = await Database.Universe.UniversOwned(id);

                return new ContentResult {
                    Content = JsonSerializer.Serialize<Model.Universe>(result, Util.DefaultJsonOptions),
                    ContentType = "application/json; charset=UTF-8",
                };
            }
            else
            {
                return new ContentResult {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentType = "text/plain",
                    Content = "unknown user",
                };
            }
        }

        /// <summary>
        /// Send back if the users had a village 
        /// </summary>
        /// <param name="idUniv"></param>
        /// <returns></returns>
        [HttpGet("{idUniv}")]
        public async Task<IActionResult> HasVillage(int idUniv)
        {
            int? maybeId = HttpContext.User.UserId();

            if (maybeId is int id)
            {
                var result = new Model.Universe()
                {
                    Town = await Database.Universe.PlayerHaveVillageInUnivers(id, idUniv),
                };

                return new ContentResult
                {
                    Content = JsonSerializer.Serialize<Model.Universe>(result, Util.DefaultJsonOptions),
                    ContentType = "application/json; charset=UTF-8",
                };
            }
            else
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentType = "text/plain",
                    Content = "unknown user",
                };
            }
        }

        /// <summary>
        /// Send the village name
        /// </summary>
        /// <param name="idVill"></param>
        /// <returns></returns>
        [HttpGet("{idVill}Village")]
        public async Task<IActionResult> GetVillageName(int idVill)
        {
            int? maybeId = HttpContext.User.UserId();

            if (maybeId is int id)
            {

                var result = new { Name = await Database.Universe.PlayerVillageName(id, idVill) };

                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(result),
                    ContentType = "application/json; charset=UTF-8",
                };
            }
            else
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ContentType = "text/plain",
                    Content = "unknown user",
                };
            }
        }


        /// <summary>
        /// Send the major faction of an universe
        /// </summary>
        /// <param name="idUniv"></param>
        /// <returns></returns>
        [HttpGet("{idUniv}Faction")]
        public async Task<IActionResult> GetMajorFaction(int idUniv)
        {
            var result = new { Faction = await Database.Universe.MajoritaryFaction(idUniv) };

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",

            };
        }

    }
}
