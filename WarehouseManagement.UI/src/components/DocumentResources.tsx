import React, { useState, useEffect } from 'react';
import { Form, Button, Table, InputGroup, FormControl } from 'react-bootstrap';
import Select from 'react-select';
import { SelectOption, ResourceDto, UnitOfMeasureDto } from '../types/api';
import apiService from '../services/api';

export interface DocumentResourceItem {
  id?: string;
  resourceId: string;
  resourceName?: string;
  unitId: string;
  unitName?: string;
  quantity: number;
}

interface DocumentResourcesProps {
  resources: DocumentResourceItem[];
  onResourcesChange: (resources: DocumentResourceItem[]) => void;
  disabled?: boolean;
}

const DocumentResources: React.FC<DocumentResourcesProps> = ({ 
  resources, 
  onResourcesChange, 
  disabled = false 
}) => {
  const [availableResources, setAvailableResources] = useState<ResourceDto[]>([]);
  const [availableUnits, setAvailableUnits] = useState<UnitOfMeasureDto[]>([]);
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
        const [resourcesData, unitsData] = await Promise.all([
          apiService.getActiveResources(),
          apiService.getActiveUnitsOfMeasure()
        ]);

        setAvailableResources(resourcesData);
        setAvailableUnits(unitsData);
      } catch (err) {
        setError('Failed to load reference data');
        console.error('Error loading reference data:', err);
      } finally {
        setLoading(false);
      }
    };

    loadReferenceData();
  }, []);

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
        resourceName: selectedResource?.name
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
            <th style={{ width: '40%' }}>Resource</th>
            <th style={{ width: '25%' }}>Unit</th>
            <th style={{ width: '25%' }}>Quantity</th>
            <th style={{ width: '10%' }}>Actions</th>
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
                    isDisabled={disabled}
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

export default DocumentResources;