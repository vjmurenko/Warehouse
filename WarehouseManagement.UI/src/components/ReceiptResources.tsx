import React, {useState, useEffect, useMemo} from 'react';
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
}

const ReceiptResources: React.FC<ReceiptResourcesProps> = ({ 
  resources, 
  onResourcesChange, 
  disabled = false
}) => {
  const [activeResources, setActiveResources] = useState<ResourceDto[]>([]);
  const [activeUnits, setActiveUnits] = useState<UnitOfMeasureDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [initialResourcesSnapshot] = useState(resources);
  useEffect(() => {
    const loadActiveData = async () => {
      try {
        const [resourcesData, unitsData] = await Promise.all([
          apiService.getActiveResources(),
          apiService.getActiveUnitsOfMeasure(),
        ]);
        setActiveResources(resourcesData);
        setActiveUnits(unitsData);
      } catch (error) {
      } finally {
        setLoading(false);
      }
    };

    loadActiveData();
  }, []);

  const unitsOptions = useMemo(() => {
      const options =  activeUnits.map(c => ({value: c.id, label: c.name || ''}));
      initialResourcesSnapshot.forEach(s => {
        if (!options.some(o => o.value === s.unitId)){
          options.push({value: s.unitId, label: s.unitName || ''})
        }
      })
    return options;
  }, [activeUnits, initialResourcesSnapshot]);

  const resourcesOptions = useMemo(() => {
    const options =  activeResources.map(c => ({value: c.id, label: c.name || ''}));
    initialResourcesSnapshot.forEach(s => {
      if (!options.some(o => o.value === s.resourceId)){
        options.push({value: s.resourceId, label: s.resourceName || ''});
      }
    })
    return options;
  }, [activeResources, initialResourcesSnapshot])

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

  const handleResourceChange = (index: number, field: keyof ReceiptResourceItem, value: any, label?: any) => {
    const updated = [...resources];
    if (field === 'resourceId') {
      updated[index] = {
        ...updated[index],
        resourceId: value,
        resourceName: label
      };
    } else if (field === 'unitId') {
      updated[index] = {
        ...updated[index],
        unitId: value,
        unitName: label
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
              return (
              <tr key={index}>
                <td>
                  <Select
                    options={resourcesOptions}
                    value={resourcesOptions.find(opt => opt.value === item.resourceId)}
                    onChange={(selected) => handleResourceChange(index, 'resourceId', selected?.value || '', selected?.label || '')}
                    isDisabled={disabled}
                    placeholder="Выберите ресурс..."
                  />
                </td>
                <td>
                  <Select
                    options={unitsOptions}
                    value={unitsOptions.find(opt => opt.value === item.unitId)}
                    onChange={(selected) => handleResourceChange(index, 'unitId', selected?.value || '', selected?.label || '')}
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