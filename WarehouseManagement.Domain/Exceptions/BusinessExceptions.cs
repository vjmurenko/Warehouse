namespace WarehouseManagement.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to delete an entity that is referenced by other entities
/// </summary>
public sealed class EntityInUseException : DomainException
{
    public EntityInUseException(string entityType, Guid entityId, string referencingEntities)
        : base("ENTITY_IN_USE", 
               $"Cannot delete {entityType} with ID {entityId} because it is used in {referencingEntities}",
               entityType, entityId, referencingEntities)
    {
    }
}

/// <summary>
/// Exception thrown when attempting to access an entity that does not exist
/// </summary>
public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityType, Guid entityId)
        : base("ENTITY_NOT_FOUND",
               $"{entityType} with ID {entityId} was not found",
               entityType, entityId)
    {
    }

    public EntityNotFoundException(string entityType, string identifier)
        : base("ENTITY_NOT_FOUND",
               $"{entityType} with identifier '{identifier}' was not found",
               entityType, identifier)
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create an entity with a duplicate name
/// </summary>
public sealed class DuplicateEntityException : DomainException
{
    public DuplicateEntityException(string entityType, string name)
        : base("DUPLICATE_ENTITY",
               $"{entityType} with name '{name}' already exists",
               entityType, name)
    {
    }
}

/// <summary>
/// Exception thrown when business rule validation fails
/// </summary>
public sealed class BusinessRuleValidationException : DomainException
{
    public BusinessRuleValidationException(string rule, string message, params object[] parameters)
        : base($"BUSINESS_RULE_{rule}",
               message,
               parameters)
    {
    }
}

/// <summary>
/// Exception thrown when insufficient inventory balance exists
/// </summary>
public sealed class InsufficientBalanceException : DomainException
{
    public InsufficientBalanceException(string resourceName, string unitName, decimal requested, decimal available)
        : base("INSUFFICIENT_BALANCE",
               $"Insufficient balance for {resourceName} ({unitName}). Requested: {requested}, Available: {available}",
               resourceName, unitName, requested, available)
    {
    }
}

/// <summary>
/// Exception thrown when trying to modify a signed document
/// </summary>
public sealed class SignedDocumentException : DomainException
{
    public SignedDocumentException(string operation, string documentType, string documentNumber)
        : base("SIGNED_DOCUMENT_OPERATION",
               $"Cannot {operation} signed {documentType} document '{documentNumber}'. Please revoke the document first.",
               operation, documentType, documentNumber)
    {
    }
}