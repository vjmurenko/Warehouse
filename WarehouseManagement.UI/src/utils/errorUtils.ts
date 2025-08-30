import { ApiError } from '../types/api';

/**
 * Converts API errors to user-friendly messages
 */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    switch (error.code) {
      case 'ENTITY_NOT_FOUND':
        return 'The requested item was not found.';
      
      case 'ENTITY_IN_USE':
        return 'Cannot delete this item because it is currently being used in documents.';
      
      case 'DUPLICATE_ENTITY':
        return 'An item with this name already exists.';
      
      case 'INSUFFICIENT_BALANCE':
        return 'Insufficient inventory balance for this operation.';
      
      case 'SIGNED_DOCUMENT_OPERATION':
        return 'Cannot modify a signed document. Please revoke the document first.';
      
      case 'VALIDATION_ERROR':
        return 'Please check your input and try again.';
      
      case 'INVALID_ARGUMENT':
        return 'Invalid input provided.';
      
      case 'INVALID_OPERATION':
        return 'This operation cannot be performed at this time.';
      
      case 'INTERNAL_SERVER_ERROR':
        return 'An unexpected error occurred. Please try again later.';
      
      default:
        return error.message || 'An error occurred while processing your request.';
    }
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  return 'An unexpected error occurred.';
}

/**
 * Gets detailed error information for debugging
 */
export function getErrorDetails(error: unknown): {
  message: string;
  code?: string;
  details?: string;
  traceId?: string;
} {
  if (error instanceof ApiError) {
    return {
      message: getErrorMessage(error),
      code: error.code,
      details: error.details,
      traceId: error.traceId
    };
  }
  
  return {
    message: getErrorMessage(error)
  };
}

/**
 * Checks if error is a specific type for conditional handling
 */
export function isSpecificError(error: unknown, errorCode: string): boolean {
  return error instanceof ApiError && error.code === errorCode;
}

/**
 * Checks if error indicates the entity is being used and cannot be deleted
 */
export function isEntityInUseError(error: unknown): boolean {
  return isSpecificError(error, 'ENTITY_IN_USE');
}

/**
 * Checks if error indicates duplicate entity name
 */
export function isDuplicateEntityError(error: unknown): boolean {
  return isSpecificError(error, 'DUPLICATE_ENTITY');
}

/**
 * Checks if error indicates insufficient balance
 */
export function isInsufficientBalanceError(error: unknown): boolean {
  return isSpecificError(error, 'INSUFFICIENT_BALANCE');
}