using BLL.Abstraction;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

namespace MindTrackerServer.Controllers
{
    /// <summary>
    /// MoodMarksController
    /// </summary>
    [ApiController]
    [Route("")]
    public class MoodMarksController: ControllerBase
    {
        private readonly IMoodMarksService _moodMarksService;
        private readonly ILogger<MoodMarksController> _logger;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="moodMarksService"></param>
        /// <param name="logger"></param>
        public MoodMarksController(IMoodMarksService moodMarksService, ILogger<MoodMarksController> logger)
        {
            _moodMarksService = moodMarksService;
            _logger = logger;
        }

        /// <summary>
        /// testtesttest
        /// </summary>
        /// <remarks>
        /// 
        ///     just a test request
        ///
        /// </remarks>
        /// <response code="200">sends you very strange message</response>
        [HttpPost]
        [Route("test")]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        public ActionResult Test()
        {
            //var a = await _moodMarksService.getOneByAct(id);
            return Ok("asd");
        }

        /// <summary>
        /// Adds one MoodMark to DB
        /// </summary>
        /// <param name="moodMark"></param>
        /// <remarks>
        /// requires MoodMark
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     POST /mood-mark
        ///     {
        ///         "date": "2023-06-02T12:28:33.876Z",
        ///         "mood": "fine",
        ///         "activities": [
        ///             {
        ///                 "name": "finne",
        ///                 "iconName": "fineicon.png"
        ///             }
        ///         ],
        ///         "images": [
        ///         "not implemented("
        ///         ],
        ///         "note": "meh"
        ///      },
        ///
        /// </remarks>
        /// <response code="201">returns created MoodMark with Activities</response>
        [HttpPost]
        [Route("mood-mark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> InsertOne([FromBody] MoodMark moodMark)
        {
            MoodMarkWithActivities newMoodMark = await _moodMarksService.InsertOne(moodMark, GetAccountId());

            return Created(Request.Host.ToString() + "/" + newMoodMark!.Id,newMoodMark);
        }

        /// <summary>
        /// Updates one MoodMark
        /// </summary>
        /// <param name="moodMark"></param>
        /// <remarks>
        /// requires MoodMark
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     PUT /mood-mark
        ///     {
        ///         "date": "2023-06-02T12:28:33.876Z",
        ///         "mood": "fine",
        ///         "activities": [
        ///             {
        ///                 "name": "fine",
        ///                 "iconName": "fineicon.png"
        ///             }
        ///         ],
        ///         "images": [
        ///         "not implemented("
        ///         ],
        ///         "note": "meh 2"
        ///      },
        ///
        /// </remarks>
        /// <response code="200">returns updated moodMark with activities</response>
        [HttpPut]
        [Route("mood-mark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> UpdateOne([FromBody] MoodMark moodMark)
        {
            MoodMarkWithActivities moodMarkWithActivities = await _moodMarksService.UpdateOne(moodMark, GetAccountId());

            return Ok(moodMarkWithActivities);
        }

        /// <summary>
        /// Deletes one MoodMark
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>
        /// requires date of MoodMark
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     DELETE /mood-mark
        ///     {
        ///     "2023-06-02T12:37:13.876Z"
        ///     }
        ///     
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpDelete]
        [Route("mood-mark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> DeleteOne([FromBody] string id)
        {
            await _moodMarksService.DeleteOne(id, GetAccountId());

            return NoContent();
        }

        private string GetAccountId()=>
            this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("");
        
    }
}
