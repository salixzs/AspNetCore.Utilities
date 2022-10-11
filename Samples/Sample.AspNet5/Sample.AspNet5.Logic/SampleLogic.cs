using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sample.AspNet5.Logic
{
    public class SampleLogic : ISampleLogic
    {
        /// <summary>
        /// Dummy method in business domain logic, which ends up throwing exception.
        /// </summary>
        public async Task FaultyLogic()
        {
            // mimic some work in logic.
            await Task.Delay(100).ConfigureAwait(false);

            // Something bad happened in logic.
#pragma warning disable CA2201 // Do not raise reserved exception types
            throw new ApplicationException("This is thrown on purpose.");
#pragma warning restore CA2201 // Do not raise reserved exception types
        }

        /// <summary>
        /// Dummy method in business domain logic, which ends up throwing database exception.
        /// </summary>
        public async Task DatabaseProblemLogic()
        {
            // mimic some work in logic.
            await Task.Delay(100).ConfigureAwait(false);

            // Something bad happened in logic.
            throw new SampleDatabaseException("SQL Statement is not correct.") { ErrorType = DatabaseProblemType.WrongSyntax };
        }

        /// <summary>
        /// Dummy method in business domain logic, which ends up throwing data validation exception (imagine data passed from controler POST/PUT/PATCH methods here).
        /// </summary>
        public async Task ValidationError()
        {
            // mimic some work in logic.
            await Task.Delay(100).ConfigureAwait(false);

            // Compose failed validations.
            var validationErrors = new List<ValidatedProperty>
            {
                new ValidatedProperty { PropertyName = "Name", ValidationMessage = "Missing/Empty", AppliedValue = string.Empty },
                new ValidatedProperty { PropertyName = "Id", ValidationMessage = "Cannot be null", AppliedValue = null },
                new ValidatedProperty { PropertyName = "Description", ValidationMessage = "Text is too long", AppliedValue = "Lorem Ipsum very long..." },
                new ValidatedProperty { PropertyName = "Birthday", ValidationMessage = "Cannot be in future", AppliedValue = DateTime.Now.AddYears(2).AddMonths(3) },
            };

            throw new SampleDataValidationException("There are validation errors.", validationErrors);
        }

        public async Task OperationCancelled()
        {
            // mimic some work in logic.
            await Task.Delay(100).ConfigureAwait(false);

            // Assume it was thrown by dependency via CancellationToken
            throw new OperationCanceledException("User cancelled.");
        }
    }
}
