using BLL.Abstraction;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MindTrackerServer.Controllers
{
    [ApiController]
    [Route("")]
    public class MoodMarksController: ControllerBase
    {
        private readonly IMoodMarksService _moodMarksService;
        private readonly ILogger<MoodMarksController> _logger;

        public MoodMarksController(IMoodMarksService moodMarksService, ILogger<MoodMarksController> logger)
        {
            _moodMarksService = moodMarksService;
            _logger = logger;
        }

        [HttpGet]
        [Route("start")]
        public ActionResult Init()
        {
            return Ok("Well, now you can do some request");
        }

        [HttpGet]
        [Route("mood-mark/all")]
        [Authorize]
        public async Task<ActionResult<List<MoodMark>>> GetAll()
        {

            List<MoodMark> allMoodMarks = await _moodMarksService.GetAllMoodMarks(GetAccountId());

            return allMoodMarks.Count <1 ? NotFound() : Ok(allMoodMarks);
        }

        [HttpPut]
        [Route("mood-mark/all")]
        [Authorize]
        public async Task<ActionResult> UpdateAll([FromBody] List<MoodMark> moodMarks)
        {
            await _moodMarksService.UpdateAll(moodMarks, GetAccountId());

            return NoContent();
        }

        [HttpDelete]
        [Route("mood-mark/all")]
        [Authorize]
        public async Task<ActionResult> DeleteAll()
        {
            await _moodMarksService.DeleteAll(GetAccountId());

            return NoContent();
        }

        [HttpPost]
        [Route("mood-mark")]
        [Authorize]
        public async Task<ActionResult> InsertOne([FromBody] MoodMark moodMark)
        {
            await _moodMarksService.InsertOne(moodMark);

            return NoContent();
        }

        [HttpPut]
        [Route("mood-mark")]
        [Authorize]
        public async Task<ActionResult> UpdateOne([FromBody] MoodMark moodMark)
        {
            await _moodMarksService.UpdateOne(moodMark);

            return NoContent();
        }

        [HttpDelete]
        [Route("mood-mark")]
        [Authorize]
        public async Task<ActionResult> DeleteOne([FromBody] DateTime date)
        {
            await _moodMarksService.DeleteOne(date, GetAccountId());

            return NoContent();
        }

        private string GetAccountId()=>
            this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("");
        
    }
}
