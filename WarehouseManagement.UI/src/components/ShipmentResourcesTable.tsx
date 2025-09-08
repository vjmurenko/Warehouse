import React, { useState, useEffect } from 'react';
import { Table, FormControl } from 'react-bootstrap';
import { BalanceDto, ReceiptDocumentDto } from '../types/api';
import apiService from '../services/api';

export interface ShipmentResourceItem {
  id?: string;
  resourceId: string;
  resourceName?: string;
  unitId: string;
  unitName?: string;
  quantity: number;
  availableQuantity?: number;
}

interface ShipmentResourcesTableProps {
  resources: ShipmentResourceItem[];
  onResourcesChange: (resources: ShipmentResourceItem[]) => void;
  disabled?: boolean;
  existingDocumentResources?: ShipmentResourceItem[]; // Resources already in document (for edit mode)
}

const ShipmentResourcesTable: React.FC<ShipmentResourcesTableProps> = ({ 
  resources, 
  onResourcesChange, 
  disabled = false,
  existingDocumentResources = []
}) => {
  const [balances, setBalances] = useState<BalanceDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [balancesData, receiptDocuments] = await Promise.all([
          apiService.getBalances(),
          apiService.getReceiptDocuments()
        ]);
        
        const positiveBalances = balancesData.filter(b => b.quantity > 0);

        const allowedResourceCombinations = new Set<string>();

        receiptDocuments.forEach(receipt => {
          if (receipt.resources) {
            receipt.resources.forEach(resource => {
              const key = `${resource.resourceId}:${resource.unitId}`;
              allowedResourceCombinations.add(key);
            });
          }
        });
        
        existingDocumentResources.forEach(resource => {
          const key = `${resource.resourceId}:${resource.unitId}`;
          allowedResourceCombinations.add(key);
        });
        
        const filteredBalances = positiveBalances.filter(balance => {
          const key = `${balance.resourceId}:${balance.unitOfMeasureId}`;
          return allowedResourceCombinations.has(key);
        });
        
        setBalances(filteredBalances);
      } catch (error) {
        console.error('Error loading data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const handleQuantityChange = (resourceId: string, unitId: string, quantity: number) => {
    const updated = [...resources];
    const existingIndex = updated.findIndex(r => r.resourceId === resourceId && r.unitId === unitId);
    
    if (quantity > 0) {
      const balance = balances.find(b => b.resourceId === resourceId && b.unitOfMeasureId === unitId);
      
      if (existingIndex >= 0) {
        updated[existingIndex] = {
          ...updated[existingIndex],
          quantity
        };
      } else {
        updated.push({
          resourceId,
          resourceName: balance?.resourceName,
          unitId,
          unitName: balance?.unitOfMeasureName,
          quantity,
          availableQuantity: balance?.quantity || 0
        });
      }
    } else {
      if (existingIndex >= 0) {
        updated.splice(existingIndex, 1);
      }
    }
    
    onResourcesChange(updated);
  };

  const getResourceQuantity = (resourceId: string, unitId: string): number => {
    const resource = resources.find(r => r.resourceId === resourceId && r.unitId === unitId);
    return resource?.quantity || 0;
  };

  const formatQuantity = (quantity: number): string => {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 3
    }).format(quantity);
  };

  if (loading) {
    return <div>Loading available resources...</div>;
  }

  return (
    <div>
      <div className="table-responsive">
        <Table striped hover>
          <thead>
            <tr>
              <th>Ресурс</th>
              <th>Единица измерения</th>
              <th>Количество</th>
              <th>Доступный баланс</th>
            </tr>
          </thead>
          <tbody>
            {balances.map((balance) => {
              const currentQuantity = getResourceQuantity(balance.resourceId, balance.unitOfMeasureId);
              const maxQuantity = balance.quantity;
              const hasError = currentQuantity > maxQuantity;
              
              return (
                <tr key={`${balance.resourceId}-${balance.unitOfMeasureId}`} className={currentQuantity > 0 ? 'table-warning' : ''}>
                  <td>
                    <strong>{balance.resourceName}</strong>
                  </td>
                  <td>
                    {balance.unitOfMeasureName}
                  </td>
                  <td>
                    <FormControl
                      type="number"
                      min="0"
                      max={maxQuantity}
                      step="0.001"
                      value={currentQuantity}
                      onChange={(e) => handleQuantityChange(
                        balance.resourceId, 
                        balance.unitOfMeasureId, 
                        parseFloat(e.target.value) || 0
                      )}
                      disabled={disabled}
                      placeholder="0"
                      className={hasError ? 'is-invalid' : ''}
                    />
                    {hasError && (
                      <div className="invalid-feedback">
                        Exceeds available quantity ({formatQuantity(maxQuantity)})
                      </div>
                    )}
                  </td>
                  <td>
                    <span>
                      {formatQuantity(balance.quantity)}
                    </span>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </Table>
        
        {balances.length === 0 && (
          <div className="text-center text-muted py-4">
            <p>No resources available</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default ShipmentResourcesTable;