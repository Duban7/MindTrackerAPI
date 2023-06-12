using Domain.Models;
using FluentValidation;
using System.Text.RegularExpressions;

namespace MindTrackerServer.Validators
{
    public class AccountValidator: AbstractValidator<Account>
    {
        public AccountValidator() 
        {
            string msg = "Error in property {PropertyName}: value {PropertyValue}";
            string lengthMsg = "Invalid length for {PropertyName}";
            
            RuleFor(acc=> acc.Email)
                .EmailAddress().WithMessage(msg);

            RuleFor(acc=>acc.Password)
                .Length(8,20).WithMessage(lengthMsg)
                .Must(IsPasswordValid).WithMessage(msg);
        }

        public static bool IsPasswordValid(string password) =>
            Regex.IsMatch(password, @"(?=.*[0-9])(?=.*[A-Z])(?=.*[a-z])[0-9a-zA-Z_\-]{8,20}");

        public static bool IsEmailValid(string email) =>
            Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
    }
}
