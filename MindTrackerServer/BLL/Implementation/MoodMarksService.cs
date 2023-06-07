using BLL.Abstraction;
using DAL.Abstraction;
using Domain.Exceptions;
using Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace BLL.Implementation
{
    public class MoodMarksService : IMoodMarksService
    {
        private readonly IMoodMarksRepository _moodMarksRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<MoodMarksService> _logger;

        public MoodMarksService(IMoodMarksRepository moodMarksRepository,
                                IAccountRepository accountRepository,
                                ICloudinaryService cloudinaryService,
                                ILogger<MoodMarksService> logger)
        {
            _moodMarksRepository = moodMarksRepository;
            _accountRepository = accountRepository;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<MoodMarkWithActivities> InsertOne(MoodMark moodMark, string accountId)
        {
            Account foundAccount = await _accountRepository.GetOneByIdAsync(accountId) ?? throw new AccountNotFoundException("Account was not found while adding new MoodMark");

            moodMark.Id = ObjectId.GenerateNewId().ToString();
            moodMark.AccountId = foundAccount.Id;

            await _moodMarksRepository.InsertAsync(moodMark);

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

        public async Task<MoodMarkWithActivities> InsertOneWithImages(MoodMarkRequest moodMarkRequest, string accountId)
        {
            MoodMark? record = JsonConvert.DeserializeObject<MoodMark>(moodMarkRequest!.Record!.ToString()!);

            MoodMark moodMark = record ?? throw new MoodMarkNotFoundException("MoodMark wasn't sent");

            moodMark.Id = ObjectId.GenerateNewId().ToString();
            moodMark.AccountId = accountId;
            moodMark.Images = new();

            if (moodMarkRequest.NewImages?.Count > 0)
            {
                foreach (IFormFile formFile in moodMarkRequest.NewImages!)
                {
                    string imageUrl = await _cloudinaryService.UploadAsync(formFile);

                    moodMark.Images.Add(imageUrl);
                }
            }

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

        public async Task DeleteOneWithImages(string id, string accountId)
        {
            MoodMark moodMark = await _moodMarksRepository.GetOneAsync(id);

            if (moodMark.Images!.Count > 0)
                foreach (string imageUrl in moodMark.Images)
                   await _cloudinaryService.DestroyAsycn(imageUrl);

            long deletedCount = await _moodMarksRepository.RemoveAsync(id);

            if (deletedCount < 1) throw new MoodMarkNotFoundException("Mood mark was not found");

            if (deletedCount > 1) throw new DeleteMoodMarkException("More than needed has been deleted");

            Account foundAccount = await _accountRepository.GetOneByIdAsync(accountId) ?? throw new AccountNotFoundException("Account was not found while adding new MoodMark");

            foundAccount.Marks!.Remove(moodMark.Id!);

            await _accountRepository.UpdateAsync(foundAccount);
        }

        public async Task<List<MoodMarkWithActivities>> GetAllMoodMarksWithActivities(string accountId)=>
            await _moodMarksRepository.GetAllWithActivitiesAsync(accountId);

        public async Task<MoodMarkWithActivities> UpdateOne(MoodMark moodMark) 
        {
            _= await _moodMarksRepository.GetOneAsync(moodMark.Id!) ?? throw new MoodMarkNotFoundException("MoodMark not found");

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

        public async Task<MoodMarkWithActivities> UpdateOneWithImages(MoodMarkRequest moodMarkRequest)
        {
            List<string> Images = JsonConvert.DeserializeObject<List<string>>(moodMarkRequest!.Images!) ?? throw new Exception("Unable to read images");
            List<string> DeletedImages = JsonConvert.DeserializeObject<List<string>>(moodMarkRequest!.DeletedImages!) ?? throw new Exception("Unable to read deleted images");
            MoodMark moodMark = JsonConvert.DeserializeObject<MoodMark>(moodMarkRequest!.Record!.ToString()!) ?? throw new MoodMarkNotFoundException("MoodMark wasn't sent");

            _ = await _moodMarksRepository.GetOneAsync(moodMark.Id!) ?? throw new MoodMarkNotFoundException("MoodMark not found");

            moodMark.Images = new();

            if (Images?.Count != 0)
                moodMark.Images.AddRange(Images!);
           

            if (moodMarkRequest.NewImages?.Count > 0)
            {
                foreach (IFormFile formFile in moodMarkRequest.NewImages!)
                {
                    string imageUrl = await _cloudinaryService.UploadAsync(formFile);

                    moodMark.Images.Add(imageUrl);
                }
            }

            if (DeletedImages?.Count > 0)
                foreach (string imageUrl in DeletedImages)
                    await _cloudinaryService.DestroyAsycn(imageUrl);

            long result = await _moodMarksRepository.UpdateAsync(moodMark);

            //if (result < 1) throw new UpdateMoodMarkException("Nothing has been updated");

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
