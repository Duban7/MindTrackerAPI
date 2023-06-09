using BLL.Abstraction;
using BLL.Jwt;
using Domain.Models;
using DAL.Abstraction;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Exceptions;
using MimeKit;
using MailKit.Net.Smtp;
using FluentValidation;
using FluentValidation.Results;
using MindTrackerServer.Validators;
using Microsoft.AspNetCore.Identity;
using BLL.Gmail;

namespace BLL.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMoodActivityRepository _moodActivityRepository;
        private readonly IMoodGroupRepository _moodGroupRepository;
        private readonly IMoodMarksRepository _moodMarkRepository;
        private readonly JwtOptions _jwtOptions;
        private readonly GmailOptions _gmailOptions;
        private readonly ILogger<AccountService> _logger;
        private readonly IValidator<Account> _accountValidator;

        public AccountService(IAccountRepository accountRepository,
                            IMoodActivityRepository moodActivityRepository,
                            IMoodGroupRepository moodGroupRepository,
                            IMoodMarksRepository moodMarksRepository,
                            IOptions<JwtOptions> jwtOptions,
                            IOptions<GmailOptions> gmailOptions,
                            ILogger<AccountService> logger,
                            IValidator<Account> validator)
        {
            _accountRepository = accountRepository;
            _moodActivityRepository = moodActivityRepository;
            _moodGroupRepository = moodGroupRepository;
            _moodMarkRepository = moodMarksRepository;
            _jwtOptions = jwtOptions.Value;
            _gmailOptions = gmailOptions.Value;
            _logger = logger;
            _accountValidator = validator;
        }

        public async Task<Account?> CreateAccount(Account newAccount)
        {
            ValidationResult result = await _accountValidator.ValidateAsync(newAccount);

            if (!result.IsValid)
            {
                StringBuilder errors = new();

                _logger.LogError("Invalid account");

                foreach (var error in result.Errors)
                {
                    _logger.LogError(error.ErrorMessage);
                    errors.Append(error.ErrorMessage);
                    errors.Append(";\n");
                }

                throw new InvalidAccountException(errors.ToString());
            }

            var user = await _accountRepository.GetOneByEmailAsync(newAccount.Email!);

            if (user != null) throw new AccountAlreadyExistsException("Account is alread exists");

            Account createdAccount = new()
            {
                Id = _accountRepository.GenerateObjectID(),
                Email = newAccount.Email,
                Password = Hasher.Hash(newAccount.Password ?? throw new InvalidAccountException("no password for account")),
                RefreshToken = GenerateRefreshToken(),
                Marks = new()
            };

            createdAccount.Groups = await CreateDefaultGroupsAndActivities(createdAccount.Id);

            await _accountRepository.CreateAsync(createdAccount);

            return createdAccount;
        }

        public async Task UpdateAccount(Account account, string id)
        {
            _ = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("Account doesn't exist");

            await _accountRepository.UpdateAsync(account);
        }

        public async Task DeleteAccount(string id)
        {
            Account foundAccount = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("Account doesn't exist");

            List<MoodGroup> moodGroups = await _moodGroupRepository.GetAllByAccountId(foundAccount.Id!);

            foreach(MoodGroup moodGroup in moodGroups)
            {
                long removedActivities = await _moodActivityRepository.RemoveManyByIdsAsync(moodGroup.Activities!);

                if (removedActivities != moodGroup.Activities!.Count) throw new Exception("Some activities were not removed");
            }

            await _moodGroupRepository.RemoveAllAsyncByAccountId(foundAccount.Id!);

            await _moodMarkRepository.RemoveAllAsync(foundAccount.Id!);

            await _accountRepository.RemoveAsync(id);
        }

        public async Task<Account?> LogIn(Account logInAccount)
        {
            Account? foundAccount = await _accountRepository.GetOneByEmailAsync(logInAccount.Email?? throw new InvalidAccountException("Email wasn't sent")) ?? throw new AccountNotFoundException("Account not found");

            if (!Hasher.Verify(logInAccount.Password ?? throw new InvalidAccountException("Password wasn't sent"), foundAccount!.Password!)) throw new InvalidAccountException("Wrong password");

            return foundAccount ?? throw new AccountNotFoundException("Account doesn't exist");
        }

        public async Task<Account?> GetAccount(string id) =>
             await _accountRepository.GetOneByIdAsync(id);

        public async Task<Account?> UpdateRefreshToken(RefreshToken oldRefreshToken, string id)
        {
            Account? foundAccount = await _accountRepository.GetOneByIdAsync(id) ?? throw new AccountNotFoundException("Account doesn't exist");

            if (foundAccount.RefreshToken!.Token != oldRefreshToken.Token) throw new AccountRefreshTokenException("Account token doesn't match request token");

            if (foundAccount.RefreshToken.Expires < DateTime.UtcNow) throw new AccountRefreshTokenException("Refresh token is expired");

            RefreshToken newRefreshToken = GenerateRefreshToken();
            foundAccount.RefreshToken = newRefreshToken;
            await _accountRepository.UpdateAsync(foundAccount);

            return foundAccount;
        }

        public async Task ResetPasswordQuery(string email)
        {
            Account account = await _accountRepository.GetOneByEmailAsync(email) ?? throw new AccountNotFoundException("Account with this email doesn't exist");
            string token = account.RefreshToken!.Token!.Replace("/", "slash")!.Replace("+","plus");
            using var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Солнышко ☀", _gmailOptions.Email));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Попытка сброса пароля для MoodSun";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = $"Попытка сбросить пароль, для подтверждения перейдите по ссылке: https://sunmoodapi.onrender.com/account/reset?idHash={token}&email={email}"
            };

            using SmtpClient client = new();

            await client.ConnectAsync(_gmailOptions.SMTP, _gmailOptions.Port, _gmailOptions.UseSSL);
            await client.AuthenticateAsync(_gmailOptions.Email, _gmailOptions.Password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }

        public async Task<bool> ResetPassword(string token, string email)
        {
            Account account = await _accountRepository.GetOneByEmailAsync(email) ?? throw new AccountNotFoundException("Account with this email doesn't exist");

            if (account.RefreshToken!.Token != token.Replace("slash", "/").Replace("plus","+")) return false;
            if (account.Email != email) throw new Exception("found email doesn't match sent email");

            string newPassword = CreatePassword();
            using var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("Администрация сайта", _gmailOptions.Email));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = "Сброс пароля для MoodSun";
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain)
            {
                Text = $"Для вашего аккаунта сброшен пароль. Новый пароль: {newPassword}"
            };

            using SmtpClient client = new();

            await client.ConnectAsync(_gmailOptions.SMTP, _gmailOptions.Port, _gmailOptions.UseSSL);
            await client.AuthenticateAsync(_gmailOptions.Email, _gmailOptions.Password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);

            account.Password = Hasher.Hash(newPassword);
            account.RefreshToken = GenerateRefreshToken();

            await _accountRepository.UpdateAsync(account);

            return true;
        }
        public string GenerateJwtToken(Account user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Email, user.Email!),
            };
            JwtSecurityToken jwt = new(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromDays(1)),
                signingCredentials: new SigningCredentials(_jwtOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
           
            string encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        private static string CreatePassword(int length=10)
        {
            const string symbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new();
            while (0 < length--)
            {
                res.Append(symbols[RandomNumberGenerator.GetInt32(symbols.Length)]);
            }
            if (!AccountValidator.IsPasswordValid(res.ToString()))
            {
                res.Append(symbols[RandomNumberGenerator.GetInt32(0, 25)]);
                res.Append(symbols[RandomNumberGenerator.GetInt32(26, 51)]);
                res.Append(symbols[RandomNumberGenerator.GetInt32(52, symbols.Length)]);
            }
            return res.ToString();
        }

        private static RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow
            };
            return refreshToken;
        }

        private async Task<List<string>> CreateDefaultGroupsAndActivities(string accountId)
        {

            MoodGroup social = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "Совместные занятия",
                Visible = true,
                Order = 0
            };
            List<MoodActivity> socialActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = social.Id,
                    Name = "семья",
                    IconName = "family"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = social.Id,
                    Name = "друзья",
                    IconName = "friends"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = social.Id,
                    Name = "возлюбленный",
                    IconName = "beloved"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = social.Id,
                    Name = "незнакомец",
                    IconName = "stranger"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = social.Id,
                    Name = "никто",
                    IconName = "none"
                },
            };
            social.Activities = socialActivities.Select(x => x.Id).ToList()!;

            MoodGroup emotions = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "Эмоции",
                Visible = true,
                Order = 1
            };
            List<MoodActivity> emotionsActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "взволнованный",
                    IconName = "excited"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "расслабленный",
                    IconName = "relaxed"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "гордый",
                    IconName = "proud"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "надеющийся",
                    IconName = "hopeful"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "счастливый",
                    IconName = "happy"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "восторженный",
                    IconName = "enthusiastic"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "окрыленный",
                    IconName = "butterflies"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "обновленный",
                    IconName = "refreshed"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "мрачный",
                    IconName = "gloomy"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "одинокий",
                    IconName = "lonely"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "тревожный",
                    IconName = "anxious"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "грустный",
                    IconName = "sad"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "сердитый",
                    IconName = "angry"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "усталый",
                    IconName = "tired"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = emotions.Id,
                    Name = "раздражённый",
                    IconName = "annoyed"
                }
            };
            emotions.Activities = emotionsActivities.Select(x => x.Id).ToList()!;

            MoodGroup hobbies = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "Хобби",
                Visible = true,
                Order = 2
            };
            List<MoodActivity> hobbiesActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "кино и тв",
                    IconName = "moviesAndTv"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "чтение",
                    IconName = "reading"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "игры",
                    IconName = "gaming"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "спорт",
                    IconName = "exercise"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "прогулка",
                    IconName = "takingAWalk"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "рисование",
                    IconName = "painting"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "игра на инструменте",
                    IconName = "instrumentPlaying"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = hobbies.Id,
                    Name = "рукоделие",
                    IconName = "crafts"
                },
            };
            hobbies.Activities = hobbiesActivities.Select(x => x.Id).ToList()!;

            MoodGroup weather = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "Погода",
                Visible = false,
                Order = 3
            };
            List<MoodActivity> weatherActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = weather.Id,
                    Name = "солнечно",
                    IconName = "sunny"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = weather.Id,
                    Name = "дождь",
                    IconName = "rainy"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = weather.Id,
                    Name = "облачно",
                    IconName = "cloudy"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = weather.Id,
                    Name = "снег",
                    IconName = "snowy"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = weather.Id,
                    Name = "ветренно",
                    IconName = "windy"
                },
            };
            weather.Activities = weatherActivities.Select(x => x.Id).ToList()!;
 
            MoodGroup food = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "Еда",
                Visible = false,
                Order = 4
            };
            List<MoodActivity> foodActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = food.Id,
                    Name = "здоровая еда",
                    IconName = "healthyFood"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = food.Id,
                    Name = "вредная пища",
                    IconName = "junkFood"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = food.Id,
                    Name = "домашняя",
                    IconName = "homeCooked"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = food.Id,
                    Name = "ресторан",
                    IconName = "restaurant"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = food.Id,
                    Name = "доставка",
                    IconName = "delivery"
                },
            };
            food.Activities = foodActivities.Select(x => x.Id).ToList()!;

            MoodGroup chores = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "Домашние дела",
                Visible = false,
                Order = 5
            };
            List<MoodActivity> choresActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = chores.Id,
                    Name = "уборка",
                    IconName = "cleaning"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = chores.Id,
                    Name = "прачечная",
                    IconName = "laundry"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = chores.Id,
                    Name = "приготовление пищи",
                    IconName = "cooking"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = chores.Id,
                    Name = "уход за растениями",
                    IconName = "plantCare"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = chores.Id,
                    Name = "покупка продуктов",
                    IconName = "groceryShopping"
                },
            };
            chores.Activities = choresActivities.Select(x => x.Id).ToList()!;

            MoodGroup beauty = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "Уход за собой",
                Visible = false,
                Order = 6
            };
            List<MoodActivity> beautyActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = beauty.Id,
                    Name = "стрижка",
                    IconName = "haircut"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = beauty.Id,
                    Name = "маникюр",
                    IconName = "manicure"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = beauty.Id,
                    Name = "уход за кожей",
                    IconName = "skincare"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = beauty.Id,
                    Name = "макияж",
                    IconName = "makeup"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = beauty.Id,
                    Name = "массаж",
                    IconName = "massage"
                },
            };
            beauty.Activities = beautyActivities.Select(x => x.Id).ToList()!;

            MoodGroup events = new()
            {
                Id = _moodGroupRepository.GenerateObjectId(),
                AccountId = accountId,
                Name = "События",
                Visible = false,
                Order = 7
            };
            List<MoodActivity> eventsActivities = new()
            {
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = events.Id,
                    Name = "кинотеатр",
                    IconName = "cinema"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = events.Id,
                    Name = "парк аттракционов",
                    IconName = "amusementPark"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = events.Id,
                    Name = "шопинг",
                    IconName = "shopping"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = events.Id,
                    Name = "пикник",
                    IconName = "picnic"
                },
                new()
                {
                    Id = _moodActivityRepository.GenerateObjectId(),
                    GroupId = events.Id,
                    Name = "поездка",
                    IconName = "travel"
                },
            };
            events.Activities = eventsActivities.Select(x => x.Id).ToList()!;

            List<MoodGroup> groups = new() {social, emotions, hobbies, weather, food, chores, beauty, events };
            
            List<MoodActivity> activities = new();
            activities.AddRange(socialActivities);
            activities.AddRange(emotionsActivities);
            activities.AddRange(hobbiesActivities);
            activities.AddRange(weatherActivities);
            activities.AddRange(foodActivities);
            activities.AddRange(choresActivities);
            activities.AddRange(beautyActivities);
            activities.AddRange(eventsActivities);

            await _moodActivityRepository.InsertManyAsync(activities);
            await _moodGroupRepository.InsertManyAsync(groups);

            return groups.Select(x => x.Id).ToList()!;
        }
    }
}
