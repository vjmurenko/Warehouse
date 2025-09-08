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
      // Find resource from all resources (including archived)
      const resource = allResources.find(r => r.id === value);
      updated[index] = {
        ...updated[index],
        resourceId: value,
        resourceName: resource?.name
      };
    } else if (field === 'unitId') {
      // Find unit from all units (including archived)
      const unit = allUnits.find(u => u.id === value);
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
    return <div>Загрузка ресурсов...</div>;
  }

  const isExistingResource = (resourceId: string): boolean => {
    return existingResources.some(er => er.resourceId === resourceId);
  };

  const isExistingUnit = (unitId: string): boolean => {
    return existingResources.some(er => er.unitId === unitId);
  };

  const getResourceOptionsForRow = (currentResourceId: string) => {
    // Include archived resources if they are currently selected or were in the original document
    const resourcesToShow = allResources.filter(r => 
      r.isActive || r.id === currentResourceId || isExistingResource(r.id)
    );
    
    return resourcesToShow.map(r => ({
      value: r.id,
      label: r.name
    }));
  };

  const getUnitOptionsForRow = (currentUnitId: string) => {
    // Include archived units if they are currently selected or were in the original document
    const unitsToShow = allUnits.filter(u => 
      u.isActive || u.id === currentUnitId || isExistingUnit(u.id)
    );

    return unitsToShow.map(u => ({
      value: u.id,
      label: u.name
    }));
  };

  return (
    <div>
      <div className="mb-3">
        <h6>Ресурсы поступления</h6>
      </div>
      
      <Table bordered>
        <thead>
          <tr>
            <th>Ресурс</th>
            <th>Единица измерения</th>
            <th>Количество</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {resources.length === 0 ? (
            <tr>
              <td colSpan={4} className="text-center text-muted py-3">
                Ресурсы не добавлены
              </td>
            </tr>
          ) : (
            resources.map((item, index) => {
              const resourceOptionsForRow = getResourceOptionsForRow(item.resourceId);
              const unitOptionsForRow = getUnitOptionsForRow(item.unitId);
              
              return (
              <tr key={index}>
                <td>
                  <Select
                    options={resourceOptionsForRow}
                    value={resourceOptionsForRow.find(opt => opt.value === item.resourceId)}
                    onChange={(selected) => handleResourceChange(index, 'resourceId', selected?.value || '')}
                    isDisabled={disabled}
                    placeholder="Выберите ресурс..."
                  />
                </td>
                <td>
                  <Select
                    options={unitOptionsForRow}
                    value={unitOptionsForRow.find(opt => opt.value === item.unitId)}
                    onChange={(selected) => handleResourceChange(index, 'unitId', selected?.value || '')}
                    isDisabled={disabled}
                    placeholder="Выберите единицу измерения..."
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
                    Удалить
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
          Добавить ресурс
        </Button>
      </div>
    </div>
  );
};

export default ReceiptResources;