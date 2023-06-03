﻿using BLL.Abstraction;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        private readonly ILogger<AccountController> _logger;
        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="accountService"></param>
        /// <param name="logger"></param>
        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
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
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> CreateAccount([FromBody] Account accountRequest)
        {
            Account? newAccount = await _accountService.CreateAccount(accountRequest);

            _logger.LogInformation("Account created");

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(newAccount!),
                account = newAccount
            };

            _logger.LogInformation("Token generated");

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
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        public async Task<ActionResult<object>> LogIn([FromBody] Account logInRequest)
        {
            Account? foundAccount = await _accountService.LogIn(logInRequest);

            //if (foundAccount == null) return BadRequest();

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(foundAccount!),
                account = foundAccount
            };

            return Ok(response);
        }

        /// <summary>
        /// Updates an account
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

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(account!),
                refresh_token = account!.RefreshToken
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
        [ProducesResponseType(typeof(Account), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<ActionResult> GetUser()
        {
            Account? foundAccount = await _accountService.GetAccount(GetAccountId());

            return foundAccount == null ? BadRequest() : Ok(foundAccount);
        }

        /// <summary>
        /// Updates an account
        /// </summary>
        /// <param name="account"></param>
        /// <remarks>
        /// requires email and password
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     PUT /account
        ///     {
        ///         "email": "string",
        ///         "password": "string"
        ///     }
        ///
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpPut]
        [Route("account")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> UpdateAccount([FromBody] Account account)
        {
            await _accountService.UpdateAccount(account, GetAccountId());

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
        ///     {
        ///         "email": "string",
        ///         "password": "string"
        ///     }
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
        public async Task<ActionResult> ResetPassword([FromBody] string email)
        {
            await _accountService.ResetPassword(email);

            return NoContent();
        }

        private string GetAccountId() =>
          this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("");
    }
}
