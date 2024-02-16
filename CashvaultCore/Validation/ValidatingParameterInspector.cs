using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace CashvaultCore.Validation
{
    public class ValidatingParameterInspector: IParameterInspector
    {
        private readonly IErrorMessageGenerator _errorMessageGenerator;
        private readonly IEnumerable<IObjectValidator> _validators;
         
        public ValidatingParameterInspector(IEnumerable<IObjectValidator> validators, IErrorMessageGenerator errorMessageGenerator)
        {
            if (validators == null)
            {
                throw  new ArgumentNullException(nameof(validators));
            }

            var objectValidators = validators as IObjectValidator[] ?? validators.ToArray();
            if (!objectValidators.Any())
            {
                throw new ArgumentException("At least one validator is required");
            }

            if (errorMessageGenerator == null)
            {
                throw new ArgumentNullException(nameof(errorMessageGenerator));
            }

            _validators = objectValidators;
            _errorMessageGenerator = errorMessageGenerator;
        }

        public object BeforeCall(string operationName, object[] inputs)
        {
            var validationResults = new List<ValidationResult>();

            foreach (var results in from input in inputs from validator in _validators select validator.Validate(input))
            {
                validationResults.AddRange(results);
            }

            if (validationResults.Count > 0)
            {
                throw new WebFaultException<Error>(_errorMessageGenerator.GenerateErrorMessage(operationName, validationResults), 
                    HttpStatusCode.BadRequest);
            }

            return null;
        }

        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {
        }
    }
}
