using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashvaultCore.Validation
{
    public interface IErrorMessageGenerator
    {
        Error GenerateErrorMessage(string operationName, IEnumerable<ValidationResult> validationResults);
    }
}
