using Amazon.Runtime.Internal.Util;
using BLL.Abstraction;
using DAL.Abstraction;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Implementation
{
    public class MoodMarksService : IMoodMarksService
    {
        private readonly IMoodMarksRepository _moodMarksRepository;
        private readonly ILogger<MoodMarksService> _logger;

        public MoodMarksService(IMoodMarksRepository moodMarksRepository, ILogger<MoodMarksService> logger)
        {
            _moodMarksRepository = moodMarksRepository;
            _logger = logger;
        }

        public async Task InsertOne(MoodMark moodMark, string accountId)
        {
            moodMark.Id = ObjectId.GenerateNewId().ToString();
            moodMark.AccountId = accountId;

            await _moodMarksRepository.InsertAsync(moodMark);
        }
        public async Task DeleteAll(string accountId) =>
            await _moodMarksRepository.RemoveAllAsync(accountId);

        public async Task DeleteOne(DateTime date, string accountId)
        {
            long deletedCount = await _moodMarksRepository.RemoveAsync(date, accountId);

            if (deletedCount < 1) throw new DeleteMoodMarkException("Nothing has been deleted");

            if (deletedCount > 1) throw new DeleteMoodMarkException("More than needed has been deleted");
        }

        public async Task<List<MoodMark>> GetAllMoodMarks(string accountId) =>
            await _moodMarksRepository.GetAllAsync(accountId);

        public async Task UpdateAll(List<MoodMark> moodMarks, string accoutnId)
        {
            List<MoodMark> currentMoodMarks = await _moodMarksRepository.GetAllAsync(accoutnId);

            List<MoodMark> marksToUpdate = moodMarks.IntersectBy(currentMoodMarks.Select(x => x.Date.ToShortDateString()), x => x.Date.ToShortDateString()).ToList();
            List<MoodMark> marksToDelete = currentMoodMarks.ExceptBy(moodMarks.Select(x => x.Date.ToShortDateString()), x => x.Date.ToShortDateString()).ToList();
            List<MoodMark> marksToInsert = moodMarks.ExceptBy(currentMoodMarks.Select(x => x.Date.ToShortDateString()), x => x.Date.ToShortDateString()).ToList();

            long updatedMoodMarks = 0;
            long deleteddMoodMarks = 0;

            if (marksToUpdate.Count > 0)
            {
                var oldMarks = currentMoodMarks.IntersectBy(marksToUpdate.Select(x => x.Date.ToShortDateString()), x => x.Date.ToShortDateString()).ToList();

                if (marksToUpdate.Count != oldMarks.Count) throw new UpdateMoodMarkException("fuck"+ marksToUpdate.Count+" "+ oldMarks.Count);

                for(int i = 0; i<marksToUpdate.Count; i++) 
                {
                    marksToUpdate[i].Id = oldMarks[i].Id;
                    marksToUpdate[i].AccountId = accoutnId;
                }
                updatedMoodMarks = await _moodMarksRepository.UpdateManyAsync(marksToUpdate, accoutnId);
            }

            if (marksToDelete.Count > 0) deleteddMoodMarks = await _moodMarksRepository.RemoveManyAsync(marksToDelete);

            if (marksToInsert.Count > 0)
            {
                foreach (MoodMark mark in marksToInsert)
                {
                    mark.Id = ObjectId.GenerateNewId().ToString();
                    mark.AccountId = accoutnId;
                }
                await _moodMarksRepository.InsertManyAsync(marksToInsert);
            }

            if (updatedMoodMarks != marksToUpdate.Count) throw new UpdateMoodMarkException("Updated MoodMarks count doesn't match count MoodMarks that has to be updated");
            if (deleteddMoodMarks != marksToDelete.Count) throw new DeleteMoodMarkException("Deleted MoodMarks count doesn't match count MoodMarks that has to be deleted");
        }

        public async Task UpdateOne(MoodMark moodMark, string accountId) 
        {
            moodMark.AccountId = accountId;

            long result =  await _moodMarksRepository.UpdateAsync(moodMark);

            if (result < 1) throw new UpdateMoodMarkException("Nothing has been updated");

            if (result > 1) throw new UpdateMoodMarkException("More than needed has been updated");
        } 
    }
}
