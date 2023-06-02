using BLL.Abstraction;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MindTrackerServer.Controllers
{
    //9 (12)
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

        /// <summary>
        /// testtesttest
        /// </summary>
        /// <remarks>
        /// 
        ///     just a test request
        ///
        /// </remarks>
        /// <response code="200">sends you very strange message</response>
        [HttpGet]
        [Route("test")]
        [ProducesResponseType(typeof(String), StatusCodes.Status200OK)]
        public ActionResult Test()
        {
            return Ok("Well, now you can do some request");
        }

        /// <summary>
        /// Gets all moodMark of account
        /// </summary>
        /// <returns>list of MoodMarks</returns>
        /// <remarks>
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     Get /mood-mark/all
        ///
        /// </remarks>
        /// <response code="200">returns all moodmarks for account</response>
        [HttpGet]
        [Route("mood-mark/all")]
        [ProducesResponseType(typeof(List<MoodMark>), StatusCodes.Status200OK)]
        [Authorize]
        public async Task<ActionResult<List<MoodMark>>> GetAll()
        {

            List<MoodMark> allMoodMarks = await _moodMarksService.GetAllMoodMarks(GetAccountId());

            return allMoodMarks.Count <1 ? NotFound() : Ok(allMoodMarks);
        }

        /// <summary>
        /// Updates all MoodMarks of account 
        /// </summary>
        /// <param name="account"></param>
        /// <param name="id"></param>
        /// <remarks>
        /// It deletes all moodmarks that were in db but was not mentioned there, updates all moodmarks that were in db and adds new moodmarks to db
        /// 
        /// request requieres a list of MoodMarks
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     PUT /mood-mark/all
        ///     [
        ///         {
        ///             "date": "2023-06-02T12:28:33.876Z",
        ///             "mood": "fine",
        ///             "activities": [
        ///                 {
        ///                     "name": "tired",
        ///                     "iconName": "tiredicon.png"
        ///                 }
        ///             ],
        ///             "images": [
        ///             "not implemented("
        ///             ],
        ///             "note": "i was very tired all day"
        ///         },
        ///         {
        ///             "date": "2023-06-02T12:28:33.876Z",
        ///             "mood": "fine",
        ///             "activities": [
        ///                 {
        ///                     "name": "fine",
        ///                     "iconName": "fineicon.png"
        ///                 }
        ///             ],
        ///             "images": [
        ///             "implemented"
        ///             ],
        ///             "note": "i'm fine ty"
        ///         }
        ///     ]
        ///
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpPut]
        [Route("mood-mark/all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> UpdateAll([FromBody] List<MoodMark> moodMarks)
        {
            await _moodMarksService.UpdateAll(moodMarks, GetAccountId());

            return NoContent();
        }

        /// <summary>
        /// Deletes all MoodMarks for account
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     DELETE /mood-mark/all
        ///
        /// </remarks>
        /// <response code="204">no content</response>
        [HttpDelete]
        [Route("mood-mark/all")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> DeleteAll()
        {
            await _moodMarksService.DeleteAll(GetAccountId());

            return NoContent();
        }

        /// <summary>
        /// Adds one MoodMark to DB
        /// </summary>
        /// <param name="MoodMark"></param>
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
        /// <response code="204">no content</response>
        [HttpPost]
        [Route("mood-mark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> InsertOne([FromBody] MoodMark moodMark)
        {
            await _moodMarksService.InsertOne(moodMark, GetAccountId());

            return NoContent();
        }

        /// <summary>
        /// Updates one MoodMark
        /// </summary>
        /// <param name="MoodMark"></param>
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
        /// <response code="204">no content</response>
        [HttpPut]
        [Route("mood-mark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult> UpdateOne([FromBody] MoodMark moodMark)
        {
            await _moodMarksService.UpdateOne(moodMark, GetAccountId());

            return NoContent();
        }

        /// <summary>
        /// Deletes one MoodMark
        /// </summary>
        /// <param name="account"></param>
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
        public async Task<ActionResult> DeleteOne([FromBody] DateTime date)
        {
            await _moodMarksService.DeleteOne(date, GetAccountId());

            return NoContent();
        }

        private string GetAccountId()=>
            this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("");
        
    }
}
