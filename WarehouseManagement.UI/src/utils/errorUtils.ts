import { ApiError } from '../types/api';

/**
 * Converts API errors to user-friendly messages
 */
export function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    switch (error.code) {
      case 'ENTITY_NOT_FOUND':
        return 'Запрашиваемый элемент не найден.';
      
      case 'ENTITY_IN_USE':
        return 'Невозможно удалить этот элемент, поскольку он используется в документах.';
      
      case 'DUPLICATE_ENTITY':
        return 'Элемент с таким именем уже существует.';
      
      case 'INSUFFICIENT_BALANCE':
        return 'Недостаточный остаток на складе для выполнения операции.';
      
      case 'SIGNED_DOCUMENT_OPERATION':
        return 'Невозможно изменить подписанный документ. Пожалуйста, сначала отзовите документ.';
      
      case 'VALIDATION_ERROR':
        return 'Пожалуйста, проверьте введенные данные и повторите попытку.';
      
      case 'INVALID_ARGUMENT':
        return 'Введены некорректные данные.';
      
      case 'INVALID_OPERATION':
        return 'Эта операция не может быть выполнена в данный момент.';
      
      case 'INTERNAL_SERVER_ERROR':
        return 'Произошла непредвиденная ошибка. Пожалуйста, повторите попытку позже.';
      
      default:
        return error.message || 'Произошла ошибка при обработке вашего запроса.';
    }
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  return 'Произошла непредвиденная ошибка.';
}

/**
 * Checks if the error is a duplicate entity error
 */
export function isDuplicateEntityError(error: unknown): boolean {
  return error instanceof ApiError && error.code === 'DUPLICATE_ENTITY';
}

/**
 * Checks if the error is an entity in use error
 */
export function isEntityInUseError(error: unknown): boolean {
  return error instanceof ApiError && error.code === 'ENTITY_IN_USE';
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