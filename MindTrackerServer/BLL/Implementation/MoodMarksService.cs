using BLL.Abstraction;
using DAL.Abstraction;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BLL.Implementation
{
    public class MoodMarksService : IMoodMarksService
    {
        private readonly IMoodMarksRepository _moodMarksRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ILogger<MoodMarksService> _logger;

        public MoodMarksService(IMoodMarksRepository moodMarksRepository, IAccountRepository accountRepository, ILogger<MoodMarksService> logger)
        {
            _moodMarksRepository = moodMarksRepository;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public async Task<MoodMarkWithActivities> InsertOne(MoodMark moodMark, string accountId)
        {
            moodMark.Id = ObjectId.GenerateNewId().ToString();
            moodMark.AccountId = accountId;

            await _moodMarksRepository.InsertAsync(moodMark);

            Account foundAccount = await _accountRepository.GetOneByIdAsync(accountId) ?? throw new AccountNotFoundException("Account was not found while adding new MoodMark");

            foundAccount.Marks!.Add(moodMark.Id);

            await _accountRepository.UpdateAsync(foundAccount);

            MoodMarkWithActivities moodMarkWithActivities;

            if (moodMark.Activities!.Count > 0)
                moodMarkWithActivities = await _moodMarksRepository.GetOneWithActivities(moodMark.Id) ?? throw new MoodMarkNotFoundException("Added mark not found");
            else
                moodMarkWithActivities = new()
                {
                    Id = moodMark.Id,
                    AccountId = moodMark.AccountId,
                    Date = moodMark.Date,
                    Mood = moodMark.Mood,
                    Note = moodMark.Note,
                    Images = moodMark.Images,
                    Activities = new()
                };

            return moodMarkWithActivities;
        }

        public async Task DeleteOne(string id, string accountId)
        {
            MoodMark moodMark = await _moodMarksRepository.GetOneAsync(id);
            long deletedCount = await _moodMarksRepository.RemoveAsync(id);

            if (deletedCount < 1) throw new MoodMarkNotFoundException("Mood mark was not found");

            if (deletedCount > 1) throw new DeleteMoodMarkException("More than needed has been deleted");

            Account foundAccount = await _accountRepository.GetOneByIdAsync(accountId) ?? throw new AccountNotFoundException("Account was not found while adding new MoodMark");

            foundAccount.Marks!.Remove(moodMark.Id!);

            await _accountRepository.UpdateAsync(foundAccount);
        }

        public async Task<List<MoodMarkWithActivities>> GetAllMoodMarksWithActivities(string accountId)=>
            await _moodMarksRepository.GetAllWithActivitiesAsync(accountId);

        public async Task<MoodMarkWithActivities> UpdateOne(MoodMark moodMark, string accountId) 
        {
            MoodMark oldMoodmark = await _moodMarksRepository.GetOneAsync(moodMark.Id!) ?? throw new MoodMarkNotFoundException("MoodMark not found");

            moodMark.Id = oldMoodmark.Id;
            moodMark.AccountId = accountId;

            long result =  await _moodMarksRepository.UpdateAsync(moodMark);

            if (result < 1) throw new UpdateMoodMarkException("Nothing has been updated");

            if (result > 1) throw new UpdateMoodMarkException("More than needed has been updated");

            MoodMarkWithActivities moodMarkWithActivities;

            if (moodMark.Activities!.Count > 0)
                moodMarkWithActivities = await _moodMarksRepository.GetOneWithActivities(moodMark.Id!) ?? throw new MoodMarkNotFoundException("Added mark not found");
            else
                moodMarkWithActivities = new()
                {
                    Id = moodMark.Id,
                    AccountId = moodMark.AccountId,
                    Date = moodMark.Date,
                    Mood = moodMark.Mood,
                    Note = moodMark.Note,
                    Images = moodMark.Images,
                    Activities = new()
                };

            return moodMarkWithActivities;
        } 
    }
}
