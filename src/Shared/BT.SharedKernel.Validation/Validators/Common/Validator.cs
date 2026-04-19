using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace BT.SharedKernel.Validation.Validators.Common;


public abstract class Validator<T> : AbstractValidator<T>
{
    public Func<object, string, Task<IEnumerable<string>>> ValidateValue =>
        async (model, propertyName) =>
        {
            try
            {
                if (model is not T validModel) return [];

                var result = await ValidateAsync(
                    ValidationContext<T>.CreateWithOptions(validModel,
                    x => x.IncludeProperties(propertyName))).ConfigureAwait(false);

                return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException 
                                        and not StackOverflowException 
                                        and not AccessViolationException
                                        and not AppDomainUnloadedException
                                        and not BadImageFormatException
                                        and not InvalidProgramException)
            {
                // Return a friendly string instead of letting the exception bubble up to the UI
                return ["Validation could not be performed."];
            }

            //var result = await ValidateAsync(ValidationContext<T>.CreateWithOptions((T)model, x => x.IncludeProperties(propertyName)));

            //return result.IsValid ? [] : [] : result.Errors.Select(e => e.ErrorMessage);
                
                
        };
}
