export interface ErrorResponse {
  code: string;
  message: string;
  details?: string;
  parameters?: Record<string, any>;
  timestamp: string;
  traceId?: string;
}

export interface ValidationErrorResponse extends ErrorResponse {
  errors: Record<string, string[]>;
}

export class ApiError extends Error {
  public readonly code: string;
  public readonly statusCode: number;
  public readonly details?: string;
  public readonly parameters?: Record<string, any>;
  public readonly traceId?: string;

  constructor(
    statusCode: number,
    errorResponse: ErrorResponse,
    originalMessage?: string
  ) {
    super(errorResponse.message || originalMessage || 'An error occurred');
    this.name = 'ApiError';
    this.code = errorResponse.code;
    this.statusCode = statusCode;
    this.details = errorResponse.details;
    this.parameters = errorResponse.parameters;
    this.traceId = errorResponse.traceId;
  }

  // Helper methods for common error types
  isEntityNotFound(): boolean {
    return this.code === 'ENTITY_NOT_FOUND';
  }

  isEntityInUse(): boolean {
    return this.code === 'ENTITY_IN_USE';
  }

  isDuplicateEntity(): boolean {
    return this.code === 'DUPLICATE_ENTITY';
  }

  isInsufficientBalance(): boolean {
    return this.code === 'INSUFFICIENT_BALANCE';
  }

  isSignedDocumentError(): boolean {
    return this.code === 'SIGNED_DOCUMENT_OPERATION';
  }

  isValidationError(): boolean {
    return this.code === 'VALIDATION_ERROR';
  }
}

export interface BalanceDto {
  id: string;
  resourceId: string;
  resourceName: string;
  unitOfMeasureId: string;
  unitOfMeasureName: string;
  quantity: number;
  createdAt: string;
  updatedAt?: string;
}

export interface ResourceDto {
  id: string;
  name: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface UnitOfMeasureDto {
  id: string;
  name: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface ReceiptDocumentDto {
  id: string;
  number: string;
  date: string;
  resources: ReceiptResourceDetailDto[];
}

export interface ReceiptDocumentSummaryDto {
  id: string;
  number: string;
  date: string;
  resourceCount: number;
}

export interface ReceiptResourceDetailDto {
  id: string;
  resourceId: string;
  resourceName: string;
  unitId: string;
  unitName: string;
  quantity: number;
}

export interface ShipmentDocumentDto {
  id: string;
  number: string;
  clientId: string;
  clientName: string;
  date: string;
  isSigned: boolean;
  resources: ShipmentResourceDetailDto[];
}

export interface ShipmentDocumentSummaryDto {
  id: string;
  number: string;
  clientId: string;
  clientName: string;
  date: string;
  isSigned: boolean;
  resourceCount: number;
}

export interface ShipmentResourceDetailDto {
  id: string;
  resourceId: string;
  resourceName: string;
  unitId: string;
  unitName: string;
  quantity: number;
}

export enum EntityState {
  Active = 1,
  Inactive = 0
}

export interface BalanceFilters {
  resourceIds: string[];
  unitIds: string[];
}

export interface DocumentFilters {
  fromDate?: string;
  toDate?: string;
  documentNumbers?: string[];
  resourceIds?: string[];
  unitIds?: string[];

  // Legacy fields (keeping for backward compatibility while transitioning)
  startDate?: string;
  endDate?: string;
  numbers?: string[];
}

export interface SelectOption {
  value: string;
  label: string;
}

export interface ClientDto {
  id: string;
  name: string;
  address: string;
  isActive: boolean;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateClientDto {
  name: string;
  address: string;
}

export interface UpdateClientDto {
  name: string;
  address: string;
}

export interface CreateResourceDto {
  name: string;
}

export interface UpdateResourceDto {
  name: string;
}
