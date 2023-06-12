using BLL.Abstraction;
using BLL.Implementation;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Principal;

namespace MindTrackerServer.Controllers
{
    /// <summary>
    /// Account Controller
    /// </summary>
    [ApiController]
    [Route("")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IMoodMarksService _moodMarksService;
        private readonly IGroupSchemaService _groupSchemaService;
        private readonly ILogger<AccountController> _logger;
        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="accountService"></param>
        /// <param name="groupSchemaService"></param>
        /// <param name="logger"></param>
        /// <param name="moodMarksService"></param>
        public AccountController(IAccountService accountService,IGroupSchemaService groupSchemaService, ILogger<AccountController> logger, IMoodMarksService moodMarksService)
        {
            _accountService = accountService;
            _groupSchemaService = groupSchemaService;
            _logger = logger;
            _moodMarksService = moodMarksService;
        }

        /// <summary>
        /// Сreates an account
        /// </summary>
        /// <param name="accountRequest"></param>
        /// <returns>jwt access token and newly created account</returns>
        /// <remarks>
        /// requires email and password
        /// Sample request:
        /// 
        ///     POST /account/new
        ///     {
        ///         "email": "string",
        ///         "password": "string"
        ///     }
        ///
        /// </remarks>
        /// <response code="201">returns URI, access jwt token and account</response>
        [HttpPost]
        [Route("account/new")]
        [ProducesResponseType(typeof(AccountWithData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> CreateAccount([FromBody] Account accountRequest)
        {
            Account? newAccount = await _accountService.CreateAccount(accountRequest);

            List<MoodGroupWithActivities> accountGroups = await _groupSchemaService.GetAllGroupsWithActivities(newAccount!.Id!);
            List<MoodMarkWithActivities> accountMarks = await _moodMarksService.GetAllMoodMarksWithActivities(newAccount!.Id!);

            AccountWithData newAccountWithData = new()
            {
                Id = newAccount.Id,
                Email = newAccount.Email,
                Password = "",
                RefreshToken = newAccount.RefreshToken,
                Groups = accountGroups,
                Records = accountMarks,
            };

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(newAccount!),
                account = newAccountWithData
            };

            return Created(Request.Host.ToString() + "/" + newAccount!.Id, response);
        }

        /// <summary>
        /// Log-in to account
        /// </summary>
        /// <param name="logInRequest"></param>
        /// <returns>jwt access token and found account</returns>
        /// <remarks>
        /// requires email and password
        /// Sample request:
        /// 
        ///     POST /account/log-in
        ///     {
        ///         "email": "string",
        ///         "password": "string"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">returns access jwt token and account</response>
        [HttpPost]
        [Route("account/log-in")]
        [ProducesResponseType(typeof(AccountWithData), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> LogIn([FromBody] Account logInRequest)
        {
            Account? foundAccount = await _accountService.LogIn(logInRequest);

            List<MoodGroupWithActivities> accountGroups = await _groupSchemaService.GetAllGroupsWithActivities(foundAccount!.Id!);
            List<MoodMarkWithActivities> accountMarks = await _moodMarksService.GetAllMoodMarksWithActivities(foundAccount!.Id!);

            AccountWithData foundAccountWithData = new()
            {
                Id = foundAccount.Id,
                Email = foundAccount.Email,
                Password = "",
                RefreshToken = foundAccount.RefreshToken,
                Groups = accountGroups,
                Records = accountMarks,
            };

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(foundAccount!),
                account = foundAccountWithData  
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates an account tokens and sends them back
        /// </summary>
        /// <param name="oldRefreshToken"></param>
        /// <param name="id"></param>
        /// <returns>new jwt access token and refresh-token</returns>
        /// <remarks>
        /// requires id and refresh-token
        /// Sample request:
        /// 
        ///     POST /account/refresh/{id}
        ///     {
        ///        "token": "string"
        ///     }
        ///
        /// </remarks>
        /// <response code="200">returns jwt access token and refresh token</response>
        [HttpPost]
        [Route("account/refresh/{id}")]
        [ProducesResponseType(typeof(RefreshToken), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> RefreshTokens([FromBody] RefreshToken oldRefreshToken, string id)
        {
            Account? account = await _accountService.UpdateRefreshToken(oldRefreshToken, id);

            //  if (account == null) return BadRequest();

            List<MoodGroupWithActivities> accountGroups = await _groupSchemaService.GetAllGroupsWithActivities(account!.Id!);
            List<MoodMarkWithActivities> accountMarks = await _moodMarksService.GetAllMoodMarksWithActivities(account!.Id!);

            AccountWithData foundAccountWithData = new()
            {
                Id = account.Id,
                Email = account.Email,
                Password = "",
                RefreshToken = account.RefreshToken,
                Groups = accountGroups,
                Records = accountMarks,
            };

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(account!),
                account = foundAccountWithData
            };

            return Ok(response);
        }

        /// <summary>
        /// Gets an account
        /// </summary>
        /// <returns>account model</returns>
        /// <remarks>
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     GET /account
        ///
        /// </remarks>
        /// <response code="200">returns account</response>
        [HttpGet]
        [Route("account")]
        [ProducesResponseType(typeof(AccountWithData), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<ActionResult<AccountWithData>> GetAccount()
        {
            Account? foundAccount = await _accountService.GetAccount(GetAccountId()) ?? throw new AccountNotFoundException("Account not found");


            List<MoodGroupWithActivities> accountGroups = await _groupSchemaService.GetAllGroupsWithActivities(foundAccount!.Id!);
            List<MoodMarkWithActivities> accountMarks = await _moodMarksService.GetAllMoodMarksWithActivities(foundAccount!.Id!);

            AccountWithData foundAccountWithData = new()
            {
                Id = foundAccount.Id,
                Email = foundAccount.Email,
                Password = "",
                RefreshToken = foundAccount.RefreshToken,
                Groups = accountGroups,
                Records = accountMarks,
            };

            return Ok(foundAccountWithData);
        }

        /// <summary>
        /// Updates an account
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        /// requires email and password
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     PUT /account
        ///     {
        ///         "NewEmail": "string", //can be null
        ///         "OldPassword": "string",
        ///         "NewPassword":"string" //can be null
        ///     }
        ///
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpPut]
        [Route("account")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> UpdateAccount([FromBody] UpdateAccountRequest request)
        {
            await _accountService.UpdateAccount(request, GetAccountId());

            return NoContent();
        }

        /// <summary>
        /// Deletes an account
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     DELETE /account
        ///
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpDelete]
        [Route("account")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> DeleteAccount()
        {
            await _accountService.DeleteAccount(GetAccountId());

            return NoContent();
        }

        /// <summary>
        /// Resetes password of account and sends to email
        /// </summary>
        /// <param name="email"></param>
        /// <remarks>
        /// requires email and password
        /// Sample request:
        /// 
        ///     POST /account/reset
        ///     {
        ///     "email@mail.com"
        ///     }
        ///
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpPost]
        [Route("account/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPasswordQuery([FromBody] string email)
        {
            await _accountService.ResetPasswordQuery(email);

            return NoContent();
        }

        /// <summary>
        /// Resetes password of account and sends to email
        /// </summary>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <remarks>
        /// requires email and password
        /// Sample request:
        /// 
        ///     POST /account/reset/?token=<token>&email=<email>
        ///
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpGet]
        [Route("account/reset")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [AllowAnonymous]
        public async Task<ActionResult> ResetPassword(string token, string email)
        {
            string cssContent = "<style>*{padding:0;margin:0;font-family:\"Averta\";}body{display:flex;height:100vh;justify-content:center;align-items:center;background-color:#f3f4f7;}.content{display:flex;height:30%;width:80%;flex-direction:column;justify-content:center;align-items:center;background-color:white;border-radius:6%;box-shadow:0px10px17px3pxrgba(106,107,109,0.2);gap:50px;}.info{display:flex;flex-direction:column;height:20%;width:100%;align-items:center;gap:5px;}h1{font-size:56px;}h2{font-size:30px;font-weight:300;}button{background-color:#9f82ba;color:white;height:15%;width:60%;font-size:30px;display:flex;align-items:center;justify-content:center;border-radius:25px;}@media(max-width:768px){.content{width:80%;height:40%;}}@media(min-width:1240px){.content{width:30%;height:50%;}h1{font-size:36px;}h2{font-size:26px;}button{font-size:26px;}}</style>";

            if ( await _accountService.ResetPassword(token, email))  return Content($"<html><head>{cssContent}<meta charset=\"UTF-8\"></head><body><div class=\"content\"><h1>Восстановление пароля</h1><div class=\"info\"><h2>Мы отправим новый пароль на адрес:</h2><h2 class=\"email\">{email}</h2></div><button onclick=\"window.close();\">Закрыть окно</button></div></body></html>",
                "text/html");

            return Content($"<html><head>{cssContent}<meta charset=\"UTF-8\"></head><body><div class=\"content\"><h1>Ссылка недействительна</h1><div class=\"info\"><h2>Данная ссылка недействительна.</h2><h2>Пароль не был изменен.</h2></div><button onclick=\"window.close();\">Закрыть окно</button></div></body></html>"
                , "text/html");
        }
        private string GetAccountId() =>
          this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("");
    }
}
