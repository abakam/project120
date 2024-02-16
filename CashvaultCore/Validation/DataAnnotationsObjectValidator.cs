using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CashvaultCore.Validation
{
    public class DataAnnotationsObjectValidator: IObjectValidator
    {
        public IEnumerable<ValidationResult> Validate(object value)
        {
            if (value == null)
            {
                yield break;
            }

            foreach (var validationResult in GetValidationResults(value.GetType(), value))
            {
                yield return validationResult;
            }
        }

        private IEnumerable<ValidationResult> GetValidationResults(Type properType, object value)
        {
            var enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (var result in enumerable.Cast<object>().SelectMany(Validate))
                {
                    yield return result;
                }
            }

            var properties =
                TypeDescriptor.GetProperties(properType).Cast<PropertyDescriptor>().Where(p => !p.IsReadOnly);
            foreach (var result in properties.SelectMany(property => ValidateProperties(property, value)))
            {
                yield return result;
            }
        }

        private IEnumerable<ValidationResult> ValidateProperties(PropertyDescriptor propertyDescriptor, object container)
        {
            var value = propertyDescriptor.GetValue(container);
            var context = new ValidationContext(container, null, null)
            {
                DisplayName = propertyDescriptor.DisplayName,
                MemberName = propertyDescriptor.Name
            };

            foreach (var result in propertyDescriptor.Attributes.OfType<ValidationAttribute>().Select(validationAttibute => validationAttibute.GetValidationResult(value, context)).Where(result => result != ValidationResult.Success))
            {
                yield return result;
            }

            if (value == null) yield break;
            foreach (var validationResult in GetValidationResults(propertyDescriptor.PropertyType, value))
            {
                yield return validationResult;
            }
        } 
    }
}
