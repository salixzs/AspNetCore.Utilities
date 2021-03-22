namespace Salix.AspNetCore.Utilities
{
    /// <summary>
    /// Enforcing method for strongly typed Configuration objects validations.
    /// </summary>
    public interface IValidatableConfiguration
    {
        /// <summary>
        /// Performs the validation of this configuration object.
        /// </summary>
        void Validate();
    }
}
