using BLL.Abstraction;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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
        /// Adds one MoodMark to DB
        /// </summary>
        /// <param name="moodMarkRequest"></param>
        /// <remarks>
        /// requires MoodMark
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     POST /mood-mark
        ///     {
        ///         record = json record,
        ///         newImages = [imageData1,imageData2]
        ///      },
        ///
        /// </remarks>
        /// <response code="201">returns created MoodMark with Activities</response>
        [HttpPost]
        [Route("mood-mark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult<MoodMarkWithActivities>> InsertOne([FromForm] MoodMarkRequest moodMarkRequest)
        {
            MoodMarkWithActivities newMoodMark = await _moodMarksService.InsertOneWithImages(moodMarkRequest, GetAccountId());

            return Created(Request.Host.ToString() + "/" + newMoodMark!.Id,newMoodMark);
        }

        /// <summary>
        /// Updates one MoodMark
        /// </summary>
        /// <param name="moodMarkRequest"></param>
        /// <remarks>
        /// requires MoodMark
        /// Sample request:
        /// !!!Requset needs an authorization
        /// 
        ///     PUT /mood-mark
        ///     {
        ///         record = json record,
        ///         newImages = [imageData1,imageData2],
        ///         images = [imageUrlTosave1, imageUrlToSave2],
        ///         deletedimages = [imageUrlToDelete1, ImageUrlTodelete2]
        ///      },
        ///
        /// </remarks>
        /// <response code="200">returns updated moodMark with activities</response>
        [HttpPut]
        [Route("mood-mark")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        public async Task<ActionResult<MoodMarkWithActivities>> UpdateOne([FromForm] MoodMarkRequest moodMarkRequest)
        {
            MoodMarkWithActivities moodMarkWithActivities = await _moodMarksService.InsertOneWithImages(moodMarkRequest, GetAccountId());

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
            await _moodMarksService.DeleteOneWithImages(id, GetAccountId());

            return NoContent();
        }

        private string GetAccountId()=>
            this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("");
        
    }
}
