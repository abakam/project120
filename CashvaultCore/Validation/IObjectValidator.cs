using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashvaultCore.Validation
{
    public interface IObjectValidator
    {
        IEnumerable<ValidationResult> Validate(object value);
    }
}
