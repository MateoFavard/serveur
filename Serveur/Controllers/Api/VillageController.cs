using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Server.Auth;
using Server.Database;
using Server.Model;
using Server.Utils;
using System.Collections.Generic;

namespace Server.Controllers.Api
{
    [ApiController]
    [Route("api/village")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VillageController : Controller
    {

        /// <summary>
        /// Send a request to create a village
        /// </summary>
        /// <param name="village">Village paramater</param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateVillage(Model.Village village)
        {
            int? maybeId = HttpContext.User.UserId();

            if (maybeId is int id)
            {
                if (await Database.VillageDB.InsertVillage(village, id))
                {
                    return new StatusCodeResult(201);
                }
                else
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status500InternalServerError,
                        ContentType = "text/plain",
                        Content = "can't create universe",
                    };
                }
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
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/name")]
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
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/init")]
        public async Task<IActionResult> InitBat(int idVill)
        {

            bool result = await Database.VillageDB.InitBatiment(idVill);

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }


        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/level")]
        public async Task<IActionResult> GetBuildingLevel(int idVill)
        {

            Dictionary<string,int> result = await Database.VillageDB.GetLevelBatiment(idVill); ;

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }


        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/{building}/construction/get")]
        public async Task<IActionResult> GetInConstruction(string building,int idVill)
        {
            bool result = await Database.VillageDB.GetBuildingInConstruction(idVill, building);

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }


        /// <summary>
        /// Get
        /// </summary>
        /// <param name="idVill"></param>
        /// <returns></returns>
        [HttpGet("{idVill}/ressources/get")]
        public async Task<IActionResult> GetRessources(int idVill)
        {
            Model.Ressources result = await Database.VillageDB.GetRessource(idVill);
            
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }

        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/{building}/construction/set")]
        public async Task<IActionResult> SetInConstruction(string building, int idVill)
        {
            bool result = await Database.VillageDB.SetBuildingInConstruction(idVill, building);
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill"></param>
        /// <param name="gold"></param>
        /// <param name="wood"></param>
        /// <param name="stone"></param>
        /// <returns></returns>
        [HttpPost("{idVill}/ressources/set")]
        public async Task<IActionResult> SetRessources(int idVill, Model.Ressources r)
        {
            bool result = await Database.VillageDB.UpdateRessources(idVill, r.Bois, r.Pierre, r.Or);
            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/{building}/construction/gettime")]
        public async Task<IActionResult> GetInConstructionTime(string building, int idVill)
        {
            int result = await Database.VillageDB.GetBuildingInConstructionTime(idVill, building);

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/{building}/construction/up")]
        public async Task<IActionResult> LevelUpBuilding(string building, int idVill)
        {
            bool result = await Database.VillageDB.UpBatiment(idVill, building);

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }



        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVill}/character/create")]
        public async Task<IActionResult> GenerateCharacter(int idVill)
        {
            int? result = await Database.VillageDB.InitRandomPerso(idVill);

            if (result is int id)
            {
                return new ContentResult
                {
                    Content = JsonSerializer.Serialize(id),
                    ContentType = "application/json; charset=UTF-8",
                };
            }
            else
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status204NoContent,
                };
            }
        }


        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("character/get/{idPerso}")]
        public async Task<IActionResult> GetCharacterById(int idPerso)
        {
            Model.Perso result = await Database.VillageDB.GetPersoById(idPerso);

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }
        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("character/equipement/get/{idEquipement}")]
        public async Task<IActionResult> GetEquipementById(int idEquipement)
        {
            Model.Equipement result = await Database.VillageDB.GetEquipement(idEquipement);

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="idVill">id of the village</param>
        /// <returns></returns>
        [HttpGet("{idVillage}/character/get")]
        public async Task<IActionResult> GerCharacterByVillage(int idVillage)
        {
            List<Model.Perso> result = await Database.VillageDB.GetPersoInVillage(idVillage);

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(result),
                ContentType = "application/json; charset=UTF-8",
            };
        }
    }
}
