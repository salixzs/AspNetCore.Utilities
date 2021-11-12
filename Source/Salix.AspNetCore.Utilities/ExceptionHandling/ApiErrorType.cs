namespace Salix.AspNetCore.Utilities;

/// <summary>
/// Lists types of errors that API may return for fast processing on client side.
/// </summary>
public enum ApiErrorType
{
    /// <summary>
    /// Default value - fall-back if error type could not be set.
    /// </summary>
    Undetermined = 0,

    /// <summary>
    /// Exception occurred in API during processing of a request.
    /// </summary>
    ServerError = 1,

    /// <summary>
    /// The request is malformed - missing or default values for key request elements.
    /// </summary>
    RequestError = 2,

    /// <summary>
    /// Data in request has validation errors, which should be listed in Validation Error list.
    /// </summary>
    DataValidationError = 3,

    /// <summary>
    /// There are problems with configuration of application.
    /// </summary>
    ConfigurationError = 4,

    /// <summary>
    /// There are problems with external dependency.
    /// </summary>
    ExternalError = 5,

    /// <summary>
    /// General security related exception / error.
    /// </summary>
    SecurityError = 10,

    /// <summary>
    /// Authorization error.
    /// </summary>
    AccessRestrictedError = 11,

    /// <summary>
    /// Any general storage/database error.
    /// </summary>
    StorageError = 20,

    /// <summary>
    /// Mainly for update operations when already modified data is found.
    /// </summary>
    StorageConcurrencyError = 21,
}
