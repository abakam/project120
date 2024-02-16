using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashvaultCore.Utilities;

namespace CashvaultCore.Validation
{
    public class ErrorMessageGenerator: IErrorMessageGenerator
    {
        public Error GenerateErrorMessage(string operationName, IEnumerable<ValidationResult> validationResults)
        {
            if (validationResults == null)
            {
                throw new ArgumentNullException(nameof(validationResults));
            }

            var enumerable = validationResults as ValidationResult[] ?? validationResults.ToArray();
            if (!enumerable.Any())
            {
                throw new ArgumentException("At lease one validationResult is required");
            }

            var error = new Error
            {
                ResponseCode = Constants.InputValidationResponseCode,
                ResponseMessages = new List<string>()
            };

            foreach (var validationResult in enumerable)
            {
                error.ResponseMessages.Add(validationResult.ErrorMessage);
            }

            return error;
        }
    }
}
