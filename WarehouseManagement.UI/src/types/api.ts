export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  [key: string]: any;
}

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
  public readonly detail?: string;
  public readonly parameters?: Record<string, any>;
  public readonly traceId?: string;
  public readonly title?: string;
  public readonly type?: string;
  public readonly instance?: string;

  constructor(
    statusCode: number,
    problemDetails: ProblemDetails
  ) {
    super(problemDetails.detail || problemDetails.title || 'An error occurred');
    this.name = 'ApiError';
    this.statusCode = statusCode;
    this.code = problemDetails.code || 'UNKNOWN_ERROR';
    this.title = problemDetails.title;
    this.detail = problemDetails.detail;
    this.type = problemDetails.type;
    this.instance = problemDetails.instance;
    this.traceId = problemDetails.traceId;
    this.parameters = problemDetails.parameters;
  }

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
  clientIds?: string[]; 
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
