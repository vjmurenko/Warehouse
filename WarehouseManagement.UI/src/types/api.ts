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
