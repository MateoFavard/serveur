using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Server.Utils;
using Server.Model;
using Server.Auth;
using Server.Database;
using System.Net.Mail;
using System.Text.Json;
using System.Text;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Drawing.Printing;

namespace Server.Controllers.Api
{
    [ApiController]
    [Route("api/account")]          
    public class AccountController : Controller
    {
        [HttpPost("signup")]
        public async Task<IActionResult> SignUp(SignUpRequest signUpRequest)
        {
            Captcha captcha = new Captcha(signUpRequest.CaptchaToken);
            int? maybeId = await Database.Account.UserExistsEmail(signUpRequest.Email);
            bool regeneratePassword = false;

            if (maybeId is int id)
            {
                if (await Database.Account.UserInfo(id) is Model.Account info)
                {
                    // Si un compte temporaire existe déjà avec le même nom d'utilisateur
                    // et addresse mail, le compte est remplacé par un nouveau
                    if (!info.IsValidated && info.Username == signUpRequest.Username)
                    {
                        regeneratePassword = true;
                    }
                    else
                    {
                        return new ContentResult
                        {
                            StatusCode = StatusCodes.Status400BadRequest,
                            ContentType = "text/plain",
                            Content = "Cette adresse mail est déja utilisée",
                        };
                    }
                }
                else
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        ContentType = "text/plain",
                        Content = "Cette adresse mail est déja utilisée",
                    };
                }
            }
            else if (await Database.Account.UserExists(signUpRequest.Username))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain",
                    Content = "Ce nom d'utilisateur est déja utilisé",
                };
            }
            
            if (!(await captcha.IsValid()))
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ContentType = "text/plain",
                    Content = "Le CAPTCHA n'a pas été validé ou a expiré",
                };
            }
            
            String tempPwd = Util.RandomPassword(10);
            if (regeneratePassword)
            {
                if (!await Database.Account.UpdateMDP(tempPwd, maybeId.Value))
                {
                    return new StatusCodeResult(503);
                }
            }
            else
            {
                if (!await Database.Account.CreateTemp(signUpRequest.Email, signUpRequest.Username, tempPwd))
                {
                    return new StatusCodeResult(503);
                }
            }

            Email message = new Email(
                signUpRequest.Email,
                "Mot de passe provisoire",
                Template.Get("signup_email.html").Render(signUpRequest.Username, tempPwd)
            );

            message.Send();

            return new StatusCodeResult(201);
        }

        [Authorize(AuthenticationSchemes="Basic")]
        [HttpPost("signin")]
        public async Task<IActionResult> SignIn()
        {
            ContentResult result = new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ContentType = "text/plain",
                Content = "No result",
            };

            int? maybeId = HttpContext.User.UserId();
            bool? maybeValidated = HttpContext.User.IsValidated();

            if (maybeId is int id)
            {
                if (maybeValidated is bool validated)
                {
                    if (validated)
                    {
                        String token = Util.RandomPassword(30);

                        if (await Database.Account.CreateSession(id, token))
                        {
                            result.Content = JsonSerializer.Serialize<SignInSuccess>(
                                new SignInSuccess
                                {
                                    Validated = true,
                                    SessionToken = token,
                                },
                                Util.DefaultJsonOptions
                            );
                            result.ContentType = "application/json; charset=UTF-8";
                            result.StatusCode = StatusCodes.Status200OK;
                        }
                        else
                        {
                            result.Content = "Couldn't create session";
                            result.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        }
                    }
                    else
                    {
                        result.Content = JsonSerializer.Serialize<SignInSuccess>(
                            new SignInSuccess
                            {
                                Validated = false,
                            },
                            Util.DefaultJsonOptions
                        );
                        result.ContentType = "application/json; charset=UTF-8";
                        result.StatusCode = StatusCodes.Status200OK;
                    }

                }
            }

            return result;
        }

        [Authorize(AuthenticationSchemes = "Basic")]
        [HttpPost("validate")]
        public async Task<IActionResult> Validation([FromBody] string newPwd)
        {
            ContentResult result = new ContentResult
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ContentType = "text/plain",
                Content = "No result",
            };

            int id = HttpContext.User.UserId()!.Value;
            bool validated = HttpContext.User.IsValidated()!.Value;

            if (validated)
            {
                result.Content = "T'est déja valider par la street mon reuf";
            }
            else
            {
                bool updated = await Database.Account.UpdateMDP(newPwd, id);
                bool userValidated =await Database.Account.ValidateUser(id);

                if (userValidated && updated)
                {
                    return new StatusCodeResult(200);
                }
                else
                {
                    result.Content = "impossible de valider le compte";
                }                
            }

            return result;
        }
    }
}
