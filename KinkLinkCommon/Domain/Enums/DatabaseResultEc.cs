namespace KinkLinkCommon.Domain.Enums;

/// <summary>
///     Result of Authentication operations
/// </summary>
public enum DBAuthenticationResult
{
    /// <summary>
    ///     An unknown issue occurred. This can only occur if no other results are provided and the default is.
    /// </summary>
    UnknownError,

    /// <summary>
    ///     Authentication failed - invalid credentials or access denied
    /// </summary>
    Unauthorized,

    /// <summary>
    ///     Authentication successful - user is authorized
    /// </summary>
    Authorized
}

/// <summary> 
///     Result of pair results
/// </summary>
public enum DBPairResult
{
    /// <summary>
    ///     An unknown issue occurred. This can only occur if no other results are provided and the default is.
    /// </summary>
    UnknownError,

    /// <summary>
    ///     No operation was performed (such as no rows updated)
    /// </summary>
    NoOp,

    /// <summary>
    ///     PairUID does not exist or is invalid
    /// </summary>
    PairUIDDoesNotExist,

    /// <summary>
    ///     Pair relationship already exists between users
    /// </summary>
    OnesidedPairExists,

    /// <summary>
    ///     Pair relationship successfully created
    /// </summary>
    PairCreated,

    /// <summary>
    ///     Already friends with a target
    /// </summary>
    Paired,

    /// <summary>
    ///     Action was successful
    /// </summary>
    Success
}

/// <summary>
///     Result of database permissions operations
/// </summary>
public enum DBPermissionsResults
{
    /// <summary>
    ///     An unknown issue occurred. This can only occur if no other results are provided and the default is.
    /// </summary>
    UnknownError,

    /// <summary>
    ///     Failed to create permission entry due to database error, constraint violation, or invalid data
    /// </summary>
    CreateFailure,

    /// <summary>
    ///     Permission entry successfully created in the database
    /// </summary>
    CreatedSuccessful,

    /// <summary>
    ///     Permission entry successfully updated in the database
    /// </summary>
    UpdateSuccessful,

    /// <summary>
    ///     Failed to update permission entry due to database error, constraint violation, or invalid data
    /// </summary>
    UpdateFailure,

    /// <summary>
    ///     Permission entry successfully deleted from the database
    /// </summary>
    DeleteSuccessful,

    /// <summary>
    ///     Failed to delete permission entry due to database error, constraint violation, or foreign key dependency
    /// </summary>
    DeleteFailure,

    /// <summary>
    ///     No permission entry found for the specified operation
    /// </summary>
    NotFound,

    /// <summary>
    ///     Permission entry already exists (duplicate create attempt)
    /// </summary>
    AlreadyExists
}
/// <summary>
///     Result of database profile operations
/// </summary>
/// <summary>
///     Result of database profile operations
/// </summary>
public enum DBProfileResult
{
    /// <summary>
    ///     An unknown issue occurred. This can only occur if no other results are provided and the default is.
    /// </summary>
    UnknownError,

    /// <summary>
    ///     Profile title update failed due to database error, constraint violation, or invalid data
    /// </summary>
    TitleUpdateFailed,

    /// <summary>
    ///     Profile title successfully updated in the database
    /// </summary>
    TitleUpdateSuccess,

    /// <summary>
    ///     Profile description update failed due to database error, constraint violation, or invalid data
    /// </summary>
    DescriptionUpdateFailed,

    /// <summary>
    ///     Profile description successfully updated in the database
    /// </summary>
    DescriptionUpdateSuccess,

    /// <summary>
    ///     Profile creation failed due to database error, constraint violation, or invalid data
    /// </summary>
    CreateFailed,

    /// <summary>
    ///     Profile successfully created in the database
    /// </summary>
    CreateSuccess,

    /// <summary>
    ///     Profile deletion failed due to database error, constraint violation, or foreign key dependency
    /// </summary>
    DeleteFailed,

    /// <summary>
    ///     Profile successfully deleted from the database
    /// </summary>
    DeleteSuccess,

    /// <summary>
    ///     No profile found for the specified user or identifier
    /// </summary>
    NotFound,

    /// <summary>
    ///     Profile already exists (duplicate create attempt)
    /// </summary>
    AlreadyExists
}

/// <summary>
///     Result of database connection operations
/// </summary>
public enum DBConnectionResult
{
    /// <summary>
    ///     An unknown issue occurred. This can only occur if no other results are provided and the default is.
    /// </summary>
    UnknownError,

    /// <summary>
    ///     Database connection established successfully
    /// </summary>
    Connected,

    /// <summary>
    ///     Failed to connect to database due to network issue, invalid credentials, or server unavailable
    /// </summary>
    ConnectionFailed,

    /// <summary>
    ///     Database connection timed out
    /// </summary>
    Timeout,

    /// <summary>
    ///     Database server is not responding
    /// </summary>
    ServerUnavailable,

    /// <summary>
    ///     Invalid database credentials provided
    /// </summary>
    InvalidCredentials,

    /// <summary>
    ///     Database does not exist
    /// </summary>
    DatabaseNotFound
}

/// <summary>
///     Result of database transaction operations
/// </summary>
public enum DBTransactionResult
{
    /// <summary>
    ///     An unknown issue occurred. This can only occur if no other results are provided and the default is.
    /// </summary>
    UnknownError,

    /// <summary>
    ///     Transaction completed successfully and committed
    /// </summary>
    Committed,

    /// <summary>
    ///     Transaction was rolled back due to error or explicit rollback
    /// </summary>
    RolledBack,

    /// <summary>
    ///     Transaction failed to start due to database error or resource constraint
    /// </summary>
    StartFailed,

    /// <summary>
    ///     Transaction failed to commit due to deadlock, constraint violation, or database error
    /// </summary>
    CommitFailed,

    /// <summary>
    ///     Transaction was rolled back due to deadlock
    /// </summary>
    Deadlock
}

/// <summary>
///     Result of database validation operations
/// </summary>
public enum DBValidationResult
{
    /// <summary>
    ///     An unknown issue occurred. This can only occur if no other results are provided and the default is.
    /// </summary>
    UnknownError,

    /// <summary>
    ///     Data validation passed successfully
    /// </summary>
    Valid,

    /// <summary>
    ///     Data validation failed due to format constraints
    /// </summary>
    InvalidFormat,

    /// <summary>
    ///     Data validation failed due to constraint violation
    /// </summary>
    ConstraintViolation,

    /// <summary>
    ///     Required field is missing or null
    /// </summary>
    RequiredFieldMissing,

    /// <summary>
    ///     Data validation failed due to exceeded length limits
    /// </summary>
    ExceededLength,

    /// <summary>
    ///     Data validation failed due to invalid data type
    /// </summary>
    InvalidType
}
