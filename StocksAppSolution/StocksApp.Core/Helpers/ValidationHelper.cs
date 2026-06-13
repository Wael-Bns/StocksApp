using System.ComponentModel.DataAnnotations;
using StocksApp.Core.Exceptions;

namespace StocksApp.Core.Helpers
{
    public static class ValidationHelper
    {
        public static void ModelValidation(object obj)
        {
            ValidationContext validationContext = new ValidationContext(obj);
            List<ValidationResult> validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);

            if (!isValid)
            {
                var errorMessages = validationResults.Select(vr => vr.ErrorMessage).ToList();
                throw new InvalidPropertyException(string.Join(", ", errorMessages));
            }
        }
    }
}
