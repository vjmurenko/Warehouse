import React, { useState, useEffect } from 'react';
import { Button, Table, FormControl } from 'react-bootstrap';
import Select from 'react-select';
import { ResourceDto, UnitOfMeasureDto, BalanceDto } from '../types/api';
import apiService from '../services/api';

export interface DocumentResourceItem {
  id?: string;
  resourceId: string;
  resourceName?: string;
  unitId: string;
  unitName?: string;
  quantity: number;
  availableQuantity?: number; // For shipments - shows available balance
}

interface DocumentResourcesSelectorProps {
  resources: DocumentResourceItem[];
  onResourcesChange: (resources: DocumentResourceItem[]) => void;
  disabled?: boolean;
  mode: 'receipt' | 'shipment'; // receipt = empty start, shipment = only resources with balance > 0
}

// Component for shipment mode - shows all balance resources in a table
const ShipmentResourcesTable: React.FC<{
  balances: BalanceDto[];
  resources: DocumentResourceItem[];
  onResourcesChange: (resources: DocumentResourceItem[]) => void;
  disabled: boolean;
}> = ({ balances, resources, onResourcesChange, disabled }) => {
  
  const handleQuantityChange = (resourceId: string, unitId: string, quantity: number) => {
    const updatedResources = [...resources];
    const existingIndex = updatedResources.findIndex(r => r.resourceId === resourceId && r.unitId === unitId);
    
    if (quantity > 0) {
      const balance = balances.find(b => b.resourceId === resourceId && b.unitOfMeasureId === unitId);
      
      if (existingIndex >= 0) {
        // Update existing resource
        updatedResources[existingIndex] = {
          ...updatedResources[existingIndex],
          quantity
        };
      } else {
        // Add new resource
        updatedResources.push({
          resourceId,
          resourceName: balance?.resourceName,
          unitId,
          unitName: balance?.unitOfMeasureName,
          quantity,
          availableQuantity: balance?.quantity || 0
        });
      }
    } else {
      // Remove resource if quantity is 0
      if (existingIndex >= 0) {
        updatedResources.splice(existingIndex, 1);
      }
    }
    
    onResourcesChange(updatedResources);
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
  
  return (
    <div className="table-responsive">
      <Table striped hover>
        <thead>
          <tr>
            <th>Resource</th>
            <th>Unit of Measure</th>
            <th>Quantity</th>
            <th>Available</th>
          </tr>
        </thead>
        <tbody>
          {balances.map((balance) => {
            const currentQuantity = getResourceQuantity(balance.resourceId, balance.unitOfMeasureId);
            const maxQuantity = balance.quantity;
            
            return (
              <tr key={`${balance.resourceId}-${balance.unitOfMeasureId}`}>
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
                  />
                  {currentQuantity > maxQuantity && (
                    <div className="text-danger small mt-1">
                      Exceeds available quantity ({formatQuantity(maxQuantity)})
                    </div>
                  )}
                </td>
                <td className="text-center">
                  <span className="badge bg-info fs-6">
                    {formatQuantity(balance.quantity)}
                  </span>
                </td>
              </tr>
            );
          })}
        </tbody>
      </Table>
      {balances.length === 0 && (
        <div className="text-center text-muted py-3">
          No resources with positive balance available
        </div>
      )}
    </div>
  );
};

// Original component for receipt mode - allows adding/removing resources
const ReceiptResourcesTable: React.FC<{
  resources: DocumentResourceItem[];
  onResourcesChange: (resources: DocumentResourceItem[]) => void;
  disabled: boolean;
  availableResources: ResourceDto[];
  availableUnits: UnitOfMeasureDto[];
}> = ({ resources, onResourcesChange, disabled, availableResources, availableUnits }) => {
  
  // Create a new empty resource item
  const emptyResource: DocumentResourceItem = {
    resourceId: '',
    unitId: '',
    quantity: 0
  };
  
  const handleAddResource = () => {
    onResourcesChange([...resources, { ...emptyResource }]);
  };

  const handleRemoveResource = (index: number) => {
    const updatedResources = [...resources];
    updatedResources.splice(index, 1);
    onResourcesChange(updatedResources);
  };

  const handleResourceChange = (index: number, field: keyof DocumentResourceItem, value: any) => {
    const updatedResources = [...resources];
    
    if (field === 'resourceId') {
      const selectedResource = availableResources.find(r => r.id === value);
      
      updatedResources[index] = {
        ...updatedResources[index],
        resourceId: value,
        resourceName: selectedResource?.name,
        unitId: '', // Reset unit when resource changes
        unitName: ''
      };
    } else if (field === 'unitId') {
      const selectedUnit = availableUnits.find(u => u.id === value);
      
      updatedResources[index] = {
        ...updatedResources[index],
        unitId: value,
        unitName: selectedUnit?.name
      };
    } else {
      updatedResources[index] = {
        ...updatedResources[index],
        [field]: value
      };
    }

    onResourcesChange(updatedResources);
  };
  
  const resourceOptions = availableResources.map(resource => ({
    value: resource.id,
    label: resource.name
  }));
  
  const unitOptions = availableUnits.map(unit => ({
    value: unit.id,
    label: unit.name
  }));

  return (
    <div>
      <Table bordered>
        <thead>
          <tr>
            <th style={{ width: '35%' }}>Resource</th>
            <th style={{ width: '25%' }}>Unit</th>
            <th style={{ width: '25%' }}>Quantity</th>
            <th style={{ width: '15%' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {resources.length === 0 ? (
            <tr>
              <td colSpan={4} className="text-center text-muted py-3">
                No resources added yet
              </td>
            </tr>
          ) : (
            resources.map((item, index) => (
              <tr key={index}>
                <td>
                  <Select
                    options={resourceOptions}
                    value={resourceOptions.find(option => option.value === item.resourceId)}
                    onChange={(selected) => handleResourceChange(index, 'resourceId', selected?.value || '')}
                    isDisabled={disabled}
                    placeholder="Select resource..."
                  />
                </td>
                <td>
                  <Select
                    options={unitOptions}
                    value={unitOptions.find(option => option.value === item.unitId)}
                    onChange={(selected) => handleResourceChange(index, 'unitId', selected?.value || '')}
                    isDisabled={disabled || !item.resourceId}
                    placeholder="Select unit..."
                  />
                </td>
                <td>
                  <FormControl
                    type="number"
                    min="0"
                    step="0.001"
                    value={item.quantity}
                    onChange={(e) => handleResourceChange(index, 'quantity', parseFloat(e.target.value) || 0)}
                    disabled={disabled}
                  />
                </td>
                <td className="text-center">
                  <Button
                    variant="danger"
                    size="sm"
                    onClick={() => handleRemoveResource(index)}
                    disabled={disabled}
                  >
                    Remove
                  </Button>
                </td>
              </tr>
            ))
          )}
        </tbody>
      </Table>
      <div className="d-flex justify-content-end">
        <Button
          variant="secondary"
          onClick={handleAddResource}
          disabled={disabled}
        >
          Add Resource
        </Button>
      </div>
    </div>
  );
};

const DocumentResourcesSelector: React.FC<DocumentResourcesSelectorProps> = ({ 
  resources, 
  onResourcesChange, 
  disabled = false,
  mode = 'receipt'
}) => {
  const [availableResources, setAvailableResources] = useState<ResourceDto[]>([]);
  const [availableUnits, setAvailableUnits] = useState<UnitOfMeasureDto[]>([]);
  const [balances, setBalances] = useState<BalanceDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadReferenceData = async () => {
      setLoading(true);
      setError(null);

      try {
        if (mode === 'shipment') {
          // For shipments, only load balances (which include resource and unit names)
          const balancesData = await apiService.getBalances();
          // Filter balances to only show resources with positive quantities
          const positiveBalances = balancesData.filter(b => b.quantity > 0);
          setBalances(positiveBalances);
        } else {
          // For receipts, load resources and units for selection
          const [resourcesData, unitsData] = await Promise.all([
            apiService.getActiveResources(),
            apiService.getActiveUnitsOfMeasure()
          ]);
          setAvailableResources(resourcesData);
          setAvailableUnits(unitsData);
        }
      } catch (err) {
        setError('Failed to load reference data');
        console.error('Error loading reference data:', err);
      } finally {
        setLoading(false);
      }
    };

    loadReferenceData();
  }, [mode]);

  if (loading) {
    return <div>Loading resources...</div>;
  }

  if (error) {
    return <div className="text-danger">{error}</div>;
  }

  if (mode === 'shipment') {
    return (
      <ShipmentResourcesTable
        balances={balances}
        resources={resources}
        onResourcesChange={onResourcesChange}
        disabled={disabled}
      />
    );
  } else {
    return (
      <ReceiptResourcesTable
        resources={resources}
        onResourcesChange={onResourcesChange}
        disabled={disabled}
        availableResources={availableResources}
        availableUnits={availableUnits}
      />
    );
  }
};

export default DocumentResourcesSelector;