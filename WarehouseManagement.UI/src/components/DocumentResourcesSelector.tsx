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

  // Create a new empty resource item
  const emptyResource: DocumentResourceItem = {
    resourceId: '',
    unitId: '',
    quantity: 0
  };

  useEffect(() => {
    const loadReferenceData = async () => {
      setLoading(true);
      setError(null);

      try {
        const resourcesData = await apiService.getActiveResources();
        const unitsData = await apiService.getActiveUnitsOfMeasure();

        setAvailableResources(resourcesData);
        setAvailableUnits(unitsData);
        
        if (mode === 'shipment') {
          const balancesData = await apiService.getBalances();
          // Filter balances to only show resources with positive quantities
          const positiveBalances = balancesData.filter(b => b.quantity > 0);
          setBalances(positiveBalances);
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
      const resourceBalance = balances.find(b => b.resourceId === value);
      
      updatedResources[index] = {
        ...updatedResources[index],
        resourceId: value,
        resourceName: selectedResource?.name,
        unitId: '', // Reset unit when resource changes
        unitName: '',
        availableQuantity: resourceBalance?.quantity || 0
      };
    } else if (field === 'unitId') {
      const selectedUnit = availableUnits.find(u => u.id === value);
      const resourceBalance = balances.find(b => 
        b.resourceId === updatedResources[index].resourceId && b.unitOfMeasureId === value
      );
      
      updatedResources[index] = {
        ...updatedResources[index],
        unitId: value,
        unitName: selectedUnit?.name,
        availableQuantity: resourceBalance?.quantity || 0
      };
    } else {
      updatedResources[index] = {
        ...updatedResources[index],
        [field]: value
      };
    }

    onResourcesChange(updatedResources);
  };

  // For shipments, only show resources that have positive balance
  const getAvailableResourceOptions = () => {
    if (mode === 'receipt') {
      return availableResources.map(resource => ({
        value: resource.id,
        label: resource.name
      }));
    } else {
      // For shipments, only show resources with positive balance
      const resourcesWithBalance = availableResources.filter(resource =>
        balances.some(balance => balance.resourceId === resource.id && balance.quantity > 0)
      );
      
      return resourcesWithBalance.map(resource => ({
        value: resource.id,
        label: resource.name
      }));
    }
  };

  // For shipments, only show units that have positive balance for the selected resource
  const getAvailableUnitOptions = (resourceId: string) => {
    if (mode === 'receipt') {
      return availableUnits.map(unit => ({
        value: unit.id,
        label: unit.name
      }));
    } else {
      // For shipments, only show units with positive balance for this resource
      const unitsWithBalance = availableUnits.filter(unit =>
        balances.some(balance => 
          balance.resourceId === resourceId && 
          balance.unitOfMeasureId === unit.id && 
          balance.quantity > 0
        )
      );
      
      return unitsWithBalance.map(unit => ({
        value: unit.id,
        label: unit.name
      }));
    }
  };

  const resourceOptions = getAvailableResourceOptions();

  if (loading) {
    return <div>Loading resources...</div>;
  }

  if (error) {
    return <div className="text-danger">{error}</div>;
  }

  return (
    <div>
      <Table bordered>
        <thead>
          <tr>
            <th style={{ width: '35%' }}>Resource</th>
            <th style={{ width: '20%' }}>Unit</th>
            {mode === 'shipment' && <th style={{ width: '15%' }}>Available</th>}
            <th style={{ width: mode === 'shipment' ? '20%' : '35%' }}>Quantity</th>
            <th style={{ width: '10%' }}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {resources.length === 0 ? (
            <tr>
              <td colSpan={mode === 'shipment' ? 5 : 4} className="text-center text-muted py-3">
                No resources added yet
              </td>
            </tr>
          ) : (
            resources.map((item, index) => {
              const unitOptions = getAvailableUnitOptions(item.resourceId);
              const maxQuantity = mode === 'shipment' ? item.availableQuantity || 0 : undefined;
              
              return (
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
                  {mode === 'shipment' && (
                    <td className="text-center align-middle">
                      <span className="badge bg-info">
                        {item.availableQuantity?.toFixed(3) || '0.000'}
                      </span>
                    </td>
                  )}
                  <td>
                    <FormControl
                      type="number"
                      min="0"
                      max={maxQuantity}
                      step="0.001"
                      value={item.quantity}
                      onChange={(e) => handleResourceChange(index, 'quantity', parseFloat(e.target.value) || 0)}
                      disabled={disabled}
                    />
                    {mode === 'shipment' && maxQuantity !== undefined && item.quantity > maxQuantity && (
                      <div className="text-danger small mt-1">
                        Exceeds available quantity ({maxQuantity.toFixed(3)})
                      </div>
                    )}
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
              );
            })
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

export default DocumentResourcesSelector;