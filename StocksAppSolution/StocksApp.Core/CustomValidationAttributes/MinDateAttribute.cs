using System;
using System.ComponentModel.DataAnnotations;

namespace StocksApp.Core.CustomValidationAttributes
{
    public class MinDateAttribute : ValidationAttribute
    {
        private readonly DateTime _minDate;

        public MinDateAttribute(string minDate)
        {
            _minDate = DateTime.Parse(minDate);
        }
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // if there is no value, we consider it valid (because it is not required)
            if(value == null)
            {
                return ValidationResult.Success;
            }

            if (value is DateTime date)
            {
                if (date < _minDate)
                {
                    return new ValidationResult(
                        ErrorMessage ??
                        $"Date must be on or after {_minDate:yyyy-MM-dd}");
                }

                return ValidationResult.Success;
            }

            return base.IsValid(value, validationContext);
        }
    }
}
