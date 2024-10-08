﻿namespace WebApp.ApiModels;

public enum EErrorType
{
    /// <summary>
    /// Generic unspecified error
    /// </summary>
    GenericError,
    /// <summary>
    /// Generic error indicating that requested entity wasn't found.
    /// NB! Should be treated as semantically different from "page not found"!
    /// </summary>
    EntityNotFound,
    
    /// <summary>
    /// Provided credentials for logging in were invalid.
    /// Could mean that the user doesn't exist or that the password was wrong.
    /// </summary>
    InvalidLoginCredentials,
    /// <summary>
    /// Provided details for registering an account were invalid.
    /// </summary>
    InvalidRegistrationData,
    /// <summary>
    /// Registering new accounts is currently disabled
    /// </summary>
    RegistrationDisabled,
    /// <summary>
    /// User account must be approved by an administrator before it can be used.
    /// </summary>
    UserNotApproved,
    
    /// <summary>
    /// Provided JWT was invalid.
    /// </summary>
    InvalidJwt,
    /// <summary>
    /// Provided refresh token was invalid.
    /// </summary>
    InvalidRefreshToken,
    
    /// <summary>
    /// Submitted URL is not recognized/supported by the archive.
    /// </summary>
    UnrecognizedUrl,
    /// <summary>
    /// Submitted identifier did not resolve to an archivable entity
    /// </summary>
    SubmissionEntityNotFound,
}