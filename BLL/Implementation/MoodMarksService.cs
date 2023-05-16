using Amazon.Runtime.Internal.Util;
using BLL.Abstraction;
using DAL.Abstraction;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
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

        public async Task DeleteAll(string accountId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteOne(DateTime date, string accountId)
        {
            DeleteResult result = await _moodMarksRepository.RemoveAsync(date, accountId);

            if (result.DeletedCount < 1) throw new DeleteMoodMarkException("Nothing has been deleted");

            if (result.DeletedCount > 1) throw new DeleteMoodMarkException("More than needed was deleted");
        }

        public async Task<List<MoodMark>> GetAllMoodMarks(string accountId)=>
            await _moodMarksRepository.GetAllAsync(accountId);

        public async Task UpdateAll(List<MoodMark> moodMarks, string accoutnId)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateOne(MoodMark moodMark)
        {
            throw new NotImplementedException();
        }
    }
}
