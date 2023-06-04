using BLL.Abstraction;
using BLL.Implementation;
using DAL.Abstraction;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Newtonsoft.Json;
using System.Security.Claims;

namespace MindTrackerServer.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [ApiController]
    [Route("")]
    public class GroupsAndActivitiesController : ControllerBase
    {
        private readonly ILogger<GroupsAndActivitiesController> _logger;
        private readonly IGroupSchemaService _groupSchemaService;
        private readonly IMoodMarksService _moodMarksService;

        /// <summary>
        /// 
        /// </summary>
        public GroupsAndActivitiesController(IGroupSchemaService groupSchemaService, IMoodMarksService moodMarksService, ILogger<GroupsAndActivitiesController> logger)
        {
            _groupSchemaService = groupSchemaService;
            _moodMarksService = moodMarksService;
            _logger = logger;   
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("group-shema/update")]
        [Authorize]
        public async Task<ActionResult<object>> UpdateGroupSchema([FromBody] GroupSchemaRequest request)
        {
            string error="";
            try
            {
                _logger.LogInformation("null:" + (request.CreatedGroups != null).ToString() + "-----" + "count:" + request.CreatedGroups?.Count.ToString());
                if (request.CreatedGroups != null)
                    if (request.CreatedGroups.Count > 0)
                        await _groupSchemaService.CreateGroups(request.CreatedGroups, GetAccountId());

                _logger.LogInformation("null:" + (request.UpdatedGroups != null).ToString() + "-----" + "count:" + request.UpdatedGroups?.Count.ToString());
                if (request.UpdatedGroups != null)
                    if (request.UpdatedGroups.Count > 0)
                        await _groupSchemaService.UpdateGroups(request.UpdatedGroups);

                _logger.LogInformation("null:" + (request.DeletedGroups != null).ToString() + "-----" + "count:" + request.DeletedGroups?.Count.ToString());
                if (request.DeletedGroups != null)
                    if (request.DeletedGroups.Count > 0)
                        await _groupSchemaService.RemoveGroups(request.DeletedGroups, GetAccountId());

                
            }
            catch (Exception ex)
            {
                error = "Error occured while executing request. All updetes before error was succesfully applied. Error message:" + ex.Message;
            }
            
            List<MoodGroupWithActivities> accountGroups = await _groupSchemaService.GetAllGroupsWithActivities(GetAccountId());
            List<MoodMarkWithActivities> moodMarks = await _moodMarksService.GetAllMoodMarksWithActivities(GetAccountId());
            
            if (error != "")
            {
                var ErrorResponse = new
                {
                    accountGroups = accountGroups,
                    moodMarks = moodMarks,
                    error = error
                };
                return BadRequest(ErrorResponse);
            }

            var response = new
            {
                accountGroups = accountGroups,
                moodMarks = moodMarks
            };
            return Ok(response);

        }

        private string GetAccountId() =>
            this.User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception();
    }
}
