using BLL.Abstraction;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MindTrackerServer.Controllers
{
    [ApiController]
    [Route("")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpPost]
        [Route("account/new")]
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

        [HttpPost]
        [Route("account/log-in")]
        public async Task<ActionResult<object>> LogIn([FromBody] Account logInRequest)
        {
            Account? foundAccount = await _accountService.LogIn(logInRequest);

            if (foundAccount == null) return BadRequest();

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(foundAccount),
                account = foundAccount
            };

            return Ok(response);
        }


        [HttpPost]
        [Route("account/refresh/{id}")]
        public async Task<ActionResult<object>> RefreshTokens([FromBody] RefreshToken oldRefreshToken, string id)
        {
            Account? account = await _accountService.UpdateRefreshToken(oldRefreshToken, id);

            if (account == null) return BadRequest();

            var response = new
            {
                access_token = _accountService.GenerateJwtToken(account),
                refresh_token = account.RefreshToken
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("account")]
        [Authorize]
        public async Task<ActionResult> GetUser()
        {
            Account? foundAccount = await _accountService.GetAccount(GetAccountId());

            return foundAccount == null ? BadRequest() : Ok(foundAccount);
        }

        [HttpPut]
        [Route("account/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateAccount([FromBody] Account account, string id)
        {
            await _accountService.UpdateAccount(account, id);

            return NoContent();
        }

        [HttpDelete]
        [Route("account/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteAccount(string id)
        {
            await _accountService.DeleteAccount(id);

            return NoContent();
        }

        private string GetAccountId() =>
          this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("");
    }
}
