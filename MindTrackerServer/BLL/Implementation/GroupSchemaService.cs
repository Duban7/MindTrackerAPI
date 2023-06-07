using Amazon.Runtime.Internal.Util;
using BLL.Abstraction;
using DAL.Abstraction;
using Domain.Converters;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using System.Globalization;

namespace BLL.Implementation
{
    public class GroupSchemaService : IGroupSchemaService
    {
        private readonly IMoodGroupRepository _moodGroupRepository;
        private readonly IMoodActivityRepository _moodActivityRepository;
        private readonly IMoodMarksRepository _moodMarksRepository;
        private readonly ILogger<GroupSchemaService> _logger;
        public GroupSchemaService(IMoodGroupRepository moodGroupRepository, IMoodActivityRepository moodActivityRepository, IMoodMarksRepository moodMarksRepository, ILogger<GroupSchemaService> logger)
        {
            _moodActivityRepository = moodActivityRepository;
            _moodGroupRepository = moodGroupRepository;
            _moodMarksRepository = moodMarksRepository;
            _logger = logger;
        }

        public async Task CreateGroups(List<MoodGroupWithActivities> groupsToCreate, string accountId)
        {
            foreach (MoodGroupWithActivities group in groupsToCreate)
            {
                group.Id = _moodGroupRepository.GenerateObjectId();
                group.AccountId = accountId;
                foreach(MoodActivity activity in group.Activities!)
                {
                    activity.Id = _moodActivityRepository.GenerateObjectId();
                    activity.GroupId = group.Id;
                }

                await _moodActivityRepository.InsertManyAsync(group.Activities);
            }

            await _moodGroupRepository.InsertManyAsync(MoodGroupConverter.ConverToMoodGroupList(groupsToCreate));
        }

        public async Task UpdateGroups(List<MoodGroupWithActivities> groupsToUpdate)
        {

            foreach (MoodGroupWithActivities group in groupsToUpdate)
            {
                var oldActivitiesForGRoup = await _moodActivityRepository.GetAllByGroupid(group.Id ?? throw new UpdateGroupSchemaException($"Group: {group.Name} for account: {group.AccountId} doesn't have id"));

                _logger.LogError(oldActivitiesForGRoup.ToJson());
                _logger.LogCritical("--------------------------------------------");
                var newActivitiesForGroup = group.Activities!.ExceptBy(oldActivitiesForGRoup.Select(x => x.Id), x => x.Id).ToList();//WARNING
                var deleteActivitiesForGroup = oldActivitiesForGRoup.ExceptBy(group.Activities!.Select(x => x.Id), x => x.Id).ToList();
                var updateActivitiesForGroup = group.Activities!.IntersectBy(oldActivitiesForGRoup.Select(x => x.Id), x => x.Id).ToList();
                _logger.LogInformation(newActivitiesForGroup.Count.ToString());
                _logger.LogInformation(newActivitiesForGroup.Count.ToJson());
                _logger.LogCritical("--------------------------------------------");
                _logger.LogInformation(deleteActivitiesForGroup.Count.ToString());
                _logger.LogInformation(deleteActivitiesForGroup.Count.ToJson());
                _logger.LogCritical("--------------------------------------------");
                _logger.LogInformation(updateActivitiesForGroup.Count.ToString());
                _logger.LogInformation(updateActivitiesForGroup.Count.ToJson());

                long updatedAct = 0, deletedAct = 0;

                if (newActivitiesForGroup.Count > 0)
                {
                    foreach (MoodActivity moodActivity in newActivitiesForGroup)
                        moodActivity.Id = _moodActivityRepository.GenerateObjectId();
                    await _moodActivityRepository.InsertManyAsync(newActivitiesForGroup);
                }

                if(deleteActivitiesForGroup.Count > 0)
                {
                    deletedAct = await _moodActivityRepository.RemoveManyAsync(deleteActivitiesForGroup);
                    foreach(MoodActivity moodActivity in deleteActivitiesForGroup)
                    {
                        List<MoodMark> moodMarksToUpdate = await _moodMarksRepository.GetAllByActivityIdAsync(moodActivity.Id ?? throw new UpdateGroupSchemaException($"Moodmark by {moodActivity.Name} from group {group.Name} that has to be deleted doesn't have an id"));
                    
                        foreach(MoodMark moodMark in moodMarksToUpdate)
                        {
                            moodMark.Activities!.Remove(moodActivity.Id ?? throw new UpdateGroupSchemaException($"Moodmark by {moodActivity.Name} from group {group.Name} that has to be deleted doesn't have an id"));
                        }

                        long updatedMarks = await _moodMarksRepository.UpdateManyAsync(moodMarksToUpdate);

                        if (updatedMarks != moodMarksToUpdate.Count) throw new UpdateGroupSchemaException("Updated Marks count doesn't match count of marks that has to be updated");
                    }
                }

                if (updateActivitiesForGroup.Count > 0)
                    updatedAct = await _moodActivityRepository.UpdateManyAsync(updateActivitiesForGroup);
                

                if (deletedAct != deleteActivitiesForGroup.Count) throw new UpdateGroupSchemaException();
                if (updatedAct != updateActivitiesForGroup.Count) throw new UpdateGroupSchemaException();
            }

            long updatedGroups = await _moodGroupRepository.UpdateManyAsync(MoodGroupConverter.ConverToMoodGroupList(groupsToUpdate));

            if (updatedGroups != groupsToUpdate.Count) throw new UpdateGroupSchemaException("Updated groups count doesn't match count of groups that has to be updated");
        }

        public async Task RemoveGroups(List<string> groupsToDeleteIds, string accountId)
        {
            var groupsToDelete = await _moodGroupRepository.GetMoodGroupsByIds(groupsToDeleteIds);

            if (groupsToDeleteIds.Count != groupsToDelete.Count) throw new GroupsNotFoundExeption($"{groupsToDeleteIds.Count-groupsToDelete.Count} Group(-s) not found");

            long deletedGroups = await _moodGroupRepository.RemoveManyAsync(groupsToDelete, accountId);

            if (deletedGroups != groupsToDelete.Count) throw new UpdateGroupSchemaException("Deleted groups count doesn't match count of groups that has to be deleted. Maybe you tried yo delete mark that doesn't belong to your account");

            foreach(MoodGroup group in groupsToDelete)
            {
                if (group.Activities!.Count > 0)
                {
                    var activitiesToDelete = await _moodActivityRepository.GetActivitiesByIds(group.Activities);

                    long deletedAct = await _moodActivityRepository.RemoveManyAsync(activitiesToDelete);

                    foreach (MoodActivity moodActivity in activitiesToDelete)
                    {
                        List<MoodMark> moodMarksToUpdate = await _moodMarksRepository.GetAllByActivityIdAsync(moodActivity.Id ?? throw new UpdateGroupSchemaException($"MoodActivity {moodActivity.Name} from {group.Name} that has to be deleted doesn't have an id"));

                        foreach (MoodMark moodMark in moodMarksToUpdate)
                        {
                            moodMark.Activities!.Remove(moodMark.Id ?? throw new UpdateGroupSchemaException($"MoodActivity {moodActivity.Name} from {group.Name} that has to be deleted doesn't have an id"));
                        }

                        if (moodMarksToUpdate.Count > 0)
                        {
                            long updatedMarks = await _moodMarksRepository.UpdateManyAsync(moodMarksToUpdate);

                            if (updatedMarks != moodMarksToUpdate.Count) throw new UpdateGroupSchemaException("Updated Marks count doesn't match count of marks that has to be updated");
                        }
                    }
                }
            }
        }

        public async Task<List<MoodGroupWithActivities>> GetAllGroupsWithActivities(string accountid) =>
            await _moodGroupRepository.GetAllWithActivities(accountid);
    }
}
