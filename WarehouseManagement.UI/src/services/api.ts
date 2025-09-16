import {
  BalanceDto,
  ResourceDto,
  UnitOfMeasureDto,
  ReceiptDocumentDto,
  ShipmentDocumentDto,
  DocumentFilters,
  CreateResourceDto,
  UpdateResourceDto,
  ClientDto,
  CreateClientDto,
  UpdateClientDto,
  ErrorResponse,
  ApiError
} from '../types/api';

// For development with ASP.NET Core, make sure it matches launchSettings.json
const API_BASE_URL = 'https://localhost:7230/api';

class ApiService {
  private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    const url = `${API_BASE_URL}${endpoint}`;

    const config: RequestInit = {
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
      ...options,
      mode: 'cors',
      credentials: 'same-origin',
    };

    const response = await fetch(url, config);

    if (!response.ok) {
      let errorResponse: ErrorResponse;
      
      try {
        errorResponse = await response.json() as ErrorResponse;
      } catch {
        // Fallback for non-JSON error responses
        const errorText = await response.text().catch(() => 'Unknown error');
        errorResponse = {
          code: 'UNKNOWN_ERROR',
          message: errorText || `HTTP ${response.status} error`,
          timestamp: new Date().toISOString()
        };
      }
      
      throw new ApiError(response.status, errorResponse);
    }

    // Если 204 No Content → возвращаем "пустое значение"
    if (response.status === 204) {
      return null as unknown as T;
    }

    return (await response.json()) as T;
  }

  // Balance API
  async getBalances(): Promise<BalanceDto[]> {
    return this.request<BalanceDto[]>('/Balance');
  }

  async getFilteredBalances(resourceIds: string[] = [], unitIds: string[] = []): Promise<BalanceDto[]> {
    const params = new URLSearchParams();
    
    resourceIds.forEach(id => params.append('resourceIds', id));
    unitIds.forEach(id => params.append('unitIds', id));
    
    const queryString = params.toString();
    const endpoint = queryString ? `/Balance?${queryString}` : '/Balance';
    
    return this.request<BalanceDto[]>(endpoint);
  }

  // Resources API
  async getResources(): Promise<ResourceDto[]> {
    return this.request<ResourceDto[]>('/Resources');
  }

  async getActiveResources(): Promise<ResourceDto[]> {
    return this.request<ResourceDto[]>('/Resources/active');
  }

  async getResourceById(id: string): Promise<ResourceDto> {
    return this.request<ResourceDto>(`/Resources/${id}`);
  }

  async createResource(dto: CreateResourceDto): Promise<ResourceDto> {
    return this.request<ResourceDto>('/Resources', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async updateResource(id: string, dto: UpdateResourceDto): Promise<ResourceDto> {
    return this.request<ResourceDto>(`/Resources/${id}`, {
      method: 'PUT',
      body: JSON.stringify(dto),
    });
  }

  async archiveResource(id: string): Promise<void> {
    return this.request<void>(`/Resources/${id}/archive`, {
      method: 'POST',
    });
  }

  async activateResource(id: string): Promise<void> {
    return this.request<void>(`/Resources/${id}/activate`, {
      method: 'POST',
    });
  }
  async  deleteResource(id: string): Promise<void> {
    return  this.request<void>(`/Resources/${id}`, {
      method: 'DELETE',
    })
  }
  
  // Clients API
  async getClients(): Promise<ClientDto[]> {
    return this.request<ClientDto[]>('/Clients');
  }

  async getActiveClients(): Promise<ClientDto[]> {
    return this.request<ClientDto[]>('/Clients/active');
  }

  async getClientById(id: string): Promise<ClientDto> {
    return this.request<ClientDto>(`/Clients/${id}`);
  }

  async createClient(dto: CreateClientDto): Promise<ClientDto> {
    return this.request<ClientDto>('/Clients', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async updateClient(id: string, dto: UpdateClientDto): Promise<ClientDto> {
    return this.request<ClientDto>(`/Clients/${id}`, {
      method: 'PUT',
      body: JSON.stringify(dto),
    });
  }

  async archiveClient(id: string): Promise<void> {
    return this.request<void>(`/Clients/${id}/archive`, {
      method: 'POST',
    });
  }

  async activateClient(id: string): Promise<void> {
    return this.request<void>(`/Clients/${id}/activate`, {
      method: 'POST',
    });
  }

  async deleteClient(id: string): Promise<void> {
    return this.request<void>(`/Clients/${id}`, {
      method: 'DELETE'
    })
  }

  // Units of Measure API
  async getUnitsOfMeasure(): Promise<UnitOfMeasureDto[]> {
    console.log('Calling getUnitsOfMeasure API method');
    try {
      const result = await this.request<UnitOfMeasureDto[]>('/UnitOfMeasure');
      console.log('Units of measure fetched successfully:', result);
      return result;
    } catch (error) {
      console.error('Failed to fetch units of measure:', error);
      throw error;
    }
  }

  async getActiveUnitsOfMeasure(): Promise<UnitOfMeasureDto[]> {
    return this.request<UnitOfMeasureDto[]>('/UnitOfMeasure/active');
  }
  
  async getUnitOfMeasureById(id: string): Promise<UnitOfMeasureDto> {
    return this.request<UnitOfMeasureDto>(`/UnitOfMeasure/${id}`);
  }

  async createUnitOfMeasure(dto: { name: string }): Promise<UnitOfMeasureDto> {
    return this.request<UnitOfMeasureDto>('/UnitOfMeasure', {
      method: 'POST',
      body: JSON.stringify(dto),
    });
  }

  async updateUnitOfMeasure(id: string, dto: { name: string }): Promise<UnitOfMeasureDto> {
    return this.request<UnitOfMeasureDto>(`/UnitOfMeasure/${id}`, {
      method: 'PUT',
      body: JSON.stringify(dto),
    });
  }

  async archiveUnitOfMeasure(id: string): Promise<void> {
    return this.request<void>(`/UnitOfMeasure/${id}/archive`, {
      method: 'POST',
    });
  }

  async activateUnitOfMeasure(id: string): Promise<void> {
    return this.request<void>(`/UnitOfMeasure/${id}/activate`, {
      method: 'POST',
    });
  }
  async deleteUnitOfMeasure(id: string): Promise<void> {
    return this.request<void>(`/UnitOfMeasure/${id}`, {
      method: 'DELETE'
    })
  }

  // Receipt Documents API
  async getReceiptDocuments(): Promise<ReceiptDocumentDto[]> {
    return this.request<ReceiptDocumentDto[]>('/ReceiptDocuments');
  }

  async getFilteredReceiptDocuments(filters: DocumentFilters = {}): Promise<ReceiptDocumentDto[]> {
    const params = new URLSearchParams();
    
    if (filters.fromDate) params.append('fromDate', filters.fromDate);
    if (filters.toDate) params.append('toDate', filters.toDate);
    if (filters.documentNumbers) filters.documentNumbers.forEach(num => params.append('documentNumbers', num));
    if (filters.resourceIds) filters.resourceIds.forEach(id => params.append('resourceIds', id));
    if (filters.unitIds) filters.unitIds.forEach(id => params.append('unitIds', id));
    
    const queryString = params.toString();
    const endpoint = queryString ? `/ReceiptDocuments?${queryString}` : '/ReceiptDocuments';
    
    return this.request<ReceiptDocumentDto[]>(endpoint);
  }

  async getReceiptDocumentById(id: string): Promise<ReceiptDocumentDto> {
    return this.request<ReceiptDocumentDto>(`/ReceiptDocuments/${id}`);
  }

  async createReceiptDocument(data: { number: string; date: string; resources: Array<{ resourceId: string; unitId: string; quantity: number }> }): Promise<string> {
    // Convert to the exact format expected by the backend CreateReceiptCommand
    const command = {
      number: data.number,
      date: data.date,
      resources: data.resources.map(r => ({
        resourceId: r.resourceId,
        unitId: r.unitId,
        quantity: r.quantity
      }))
    };
    
    const response = await this.request<string>('/ReceiptDocuments', {
      method: 'POST',
      body: JSON.stringify(command),
    });
    return response;
  }

  async updateReceiptDocument(id: string, data: { id: string; number: string; date: string; resources: Array<{ id?: string; resourceId: string; unitId: string; quantity: number }> }): Promise<void> {
    // Convert to the exact format expected by the backend UpdateReceiptCommand
    const command = {
      id: data.id,
      number: data.number,
      date: data.date,
      resources: data.resources.map(r => ({
        resourceId: r.resourceId,
        unitId: r.unitId,
        quantity: r.quantity
      }))
    };
    
    return this.request<void>(`/ReceiptDocuments/${id}`, {
      method: 'PUT',
      body: JSON.stringify(command),
    });
  }

  async deleteReceiptDocument(id: string): Promise<void> {
    return this.request<void>(`/ReceiptDocuments/${id}`, {
      method: 'DELETE',
    });
  }

  // Shipment Documents API
  async getShipmentDocuments(): Promise<ShipmentDocumentDto[]> {
    return this.request<ShipmentDocumentDto[]>('/ShipmentDocuments');
  }

  async getFilteredShipmentDocuments(filters: DocumentFilters = {}): Promise<ShipmentDocumentDto[]> {
    const params = new URLSearchParams();
    
    if (filters.fromDate) params.append('fromDate', filters.fromDate);
    if (filters.toDate) params.append('toDate', filters.toDate);
    if (filters.documentNumbers) filters.documentNumbers.forEach(num => params.append('documentNumbers', num));
    if (filters.resourceIds) filters.resourceIds.forEach(id => params.append('resourceIds', id));
    if (filters.unitIds) filters.unitIds.forEach(id => params.append('unitIds', id));
    if (filters.clientIds) filters.clientIds.forEach(id => params.append('clientIds', id));
    
    const queryString = params.toString();
    const endpoint = queryString ? `/ShipmentDocuments?${queryString}` : '/ShipmentDocuments';
    
    return this.request<ShipmentDocumentDto[]>(endpoint);
  }

  async getShipmentDocumentById(id: string): Promise<ShipmentDocumentDto> {
    return this.request<ShipmentDocumentDto>(`/ShipmentDocuments/${id}`);
  }

  async createShipmentDocument(data: { number: string; clientId: string; date: string; sign: boolean; resources: Array<{ resourceId: string; unitId: string; quantity: number }> }): Promise<string> {
    // Convert to the exact format expected by the backend CreateShipmentCommand
    const command = {
      number: data.number,
      clientId: data.clientId,
      date: data.date,
      sign: data.sign,
      resources: data.resources.map(r => ({
        resourceId: r.resourceId,
        unitId: r.unitId,
        quantity: r.quantity
      }))
    };
    
    const response = await this.request<string>('/ShipmentDocuments', {
      method: 'POST',
      body: JSON.stringify(command),
    });
    return response;
  }

  async updateShipmentDocument(id: string, data: { id: string; number: string; clientId: string; date: string; sign: boolean; resources: Array<{ id?: string; resourceId: string; unitId: string; quantity: number }> }): Promise<void> {
    // Convert to the exact format expected by the backend UpdateShipmentCommand
    const command = {
      id: data.id,
      number: data.number,
      clientId: data.clientId,
      date: data.date,
      sign: data.sign,
      resources: data.resources.map(r => ({
        resourceId: r.resourceId,
        unitId: r.unitId,
        quantity: r.quantity
      }))
    };
    
    return this.request<void>(`/ShipmentDocuments/${id}`, {
      method: 'PUT',
      body: JSON.stringify(command),
    });
  }

  async deleteShipmentDocument(id: string): Promise<void> {
    return this.request<void>(`/ShipmentDocuments/${id}`, {
      method: 'DELETE',
    });
  }

  async revokeShipmentDocument(id: string): Promise<void> {
    return this.request<void>(`/ShipmentDocuments/${id}/revoke`, {
      method: 'POST',
    });
  }
}

export default new ApiService();
