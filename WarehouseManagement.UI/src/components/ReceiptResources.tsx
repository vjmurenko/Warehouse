import React, { useState, useEffect } from 'react';
import { Button, Table, FormControl } from 'react-bootstrap';
import Select from 'react-select';
import { ResourceDto, UnitOfMeasureDto } from '../types/api';
import apiService from '../services/api';

export interface ReceiptResourceItem {
  id?: string;
  resourceId: string;
  resourceName?: string;
  unitId: string;
  unitName?: string;
  quantity: number;
}

interface ReceiptResourcesProps {
  resources: ReceiptResourceItem[];
  onResourcesChange: (resources: ReceiptResourceItem[]) => void;
  disabled?: boolean;
  existingResources?: ReceiptResourceItem[]; // Resources that were in the original document (for edit mode)
}

const ReceiptResources: React.FC<ReceiptResourcesProps> = ({ 
  resources, 
  onResourcesChange, 
  disabled = false,
  existingResources = []
}) => {
  const [availableResources, setAvailableResources] = useState<ResourceDto[]>([]);
  const [availableUnits, setAvailableUnits] = useState<UnitOfMeasureDto[]>([]);
  const [allResources, setAllResources] = useState<ResourceDto[]>([]); // Include archived resources for existing items
  const [allUnits, setAllUnits] = useState<UnitOfMeasureDto[]>([]); // Include archived units for existing items
  const [loading, setLoading] = useState<boolean>(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [activeResourcesData, allResourcesData, activeUnitsData, allUnitsData] = await Promise.all([
          apiService.getActiveResources(),
          apiService.getResources(),
          apiService.getActiveUnitsOfMeasure(),
          apiService.getUnitsOfMeasure()
        ]);
        setAvailableResources(activeResourcesData);
        setAllResources(allResourcesData);
        setAvailableUnits(activeUnitsData);
        setAllUnits(allUnitsData);
      } catch (error) {
        console.error('Error loading reference data:', error);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  const handleAddResource = () => {
    onResourcesChange([...resources, {
      resourceId: '',
      unitId: '',
      quantity: 0
    }]);
  };

  const handleRemoveResource = (index: number) => {
    const updated = [...resources];
    updated.splice(index, 1);
    onResourcesChange(updated);
  };

  const handleResourceChange = (index: number, field: keyof ReceiptResourceItem, value: any) => {
    const updated = [...resources];
    
    if (field === 'resourceId') {
      const resource = availableResources.find(r => r.id === value);
      updated[index] = {
        ...updated[index],
        resourceId: value,
        resourceName: resource?.name,
        unitId: '',
        unitName: ''
      };
    } else if (field === 'unitId') {
      const unit = availableUnits.find(u => u.id === value);
      updated[index] = {
        ...updated[index],
        unitId: value,
        unitName: unit?.name
      };
    } else {
      updated[index] = {
        ...updated[index],
        [field]: value
      };
    }

    onResourcesChange(updated);
  };

  if (loading) {
    return <div>Loading resources...</div>;
  }

  const isExistingResourceUnit = (resourceId: string, unitId: string): boolean => {
    return existingResources.some(er => er.resourceId === resourceId && er.unitId === unitId);
  };

  const getResourceOptionsForRow = (currentResourceId: string, currentUnitId: string) => {
    const shouldIncludeArchived = currentResourceId && currentUnitId && isExistingResourceUnit(currentResourceId, currentUnitId);
    const resourcesToUse = shouldIncludeArchived ? allResources : availableResources;
    
    return resourcesToUse.map(r => ({
      value: r.id,
      label: r.name + (!r.isActive ? ' (Archived)' : '')
    }));
  };

  const getUnitOptionsForRow = (currentResourceId: string, currentUnitId: string) => {
    const shouldIncludeArchived = currentResourceId && currentUnitId && isExistingResourceUnit(currentResourceId, currentUnitId);
    const unitsToUse = shouldIncludeArchived ? allUnits : availableUnits;

    return unitsToUse.map(u => ({
      value: u.id,
      label: u.name + (!u.isActive ? ' (Archived)' : '')
    }));
  };

  return (
    <div>
      <div className="mb-3">
        <h6>Receipt Resources</h6>
      </div>
      
      <Table bordered>
        <thead>
          <tr>
            <th>Resource</th>
            <th>Unit</th>
            <th>Quantity</th>
            <th>Actions</th>
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
            resources.map((item, index) => {
              const resourceOptionsForRow = getResourceOptionsForRow(item.resourceId, item.unitId);
              const unitOptionsForRow = getUnitOptionsForRow(item.resourceId, item.unitId);
              
              return (
              <tr key={index}>
                <td>
                  <Select
                    options={resourceOptionsForRow}
                    value={resourceOptionsForRow.find(opt => opt.value === item.resourceId)}
                    onChange={(selected) => handleResourceChange(index, 'resourceId', selected?.value || '')}
                    isDisabled={disabled}
                    placeholder="Select resource..."
                  />
                </td>
                <td>
                  <Select
                    options={unitOptionsForRow}
                    value={unitOptionsForRow.find(opt => opt.value === item.unitId)}
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
                    placeholder="0"
                  />
                </td>
                <td className="text-center">
                  <Button
                    variant="outline-danger"
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
          variant="outline-primary"
          onClick={handleAddResource}
          disabled={disabled}
        >
          Add Resource
        </Button>
      </div>
    </div>
  );
};

export default ReceiptResources;