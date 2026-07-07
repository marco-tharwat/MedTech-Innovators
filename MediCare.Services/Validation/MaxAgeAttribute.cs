using System.ComponentModel.DataAnnotations;

namespace MediCare.Services.Validation
{
    // Validates that a date of birth is not more than MaxYears in the past, i.e. the person
    // is not older than MaxYears. The minimum allowed date is (today - MaxYears years),
    // evaluated at request time so it always tracks the current date.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class MaxAgeAttribute : ValidationAttribute
    {
        public int MaxYears { get; }

        public MaxAgeAttribute(int maxYears)
        {
            MaxYears = maxYears;
        }

        public override bool IsValid(object? value)
        {
            // Leave emptiness to [Required]; a missing date is not an "age" error.
            if (value is null) return true;
            if (value is not DateTime date) return false;

            var minDate = DateTime.Today.AddYears(-MaxYears);
            return date.Date >= minDate;
        }

        public override string FormatErrorMessage(string name)
        {
            return ErrorMessage ?? $"{name} cannot make the patient older than {MaxYears} years.";
        }
    }
}
