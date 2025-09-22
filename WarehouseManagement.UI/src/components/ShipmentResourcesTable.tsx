import React, { useState, useEffect } from 'react';
import { Table, FormControl } from 'react-bootstrap';
import { BalanceDto, ResourceDto, UnitOfMeasureDto } from '../types/api';
import apiService from '../services/api';

export interface ShipmentResourceItem {
  id?: string;
  resourceId: string;
  resourceName?: string;
  unitId: string;
  unitName?: string;
  quantity: number;
}

interface ShipmentResourcesTableProps {
  resources: ShipmentResourceItem[];
  onResourcesChange: (resources: ShipmentResourceItem[]) => void;
  disabled?: boolean;
  isSigned?: boolean;
  existingDocumentResources?: ShipmentResourceItem[];
}

const ShipmentResourcesTable: React.FC<ShipmentResourcesTableProps> = ({
  resources,
  onResourcesChange,
  disabled = false,
  isSigned = false,
  existingDocumentResources = []
}) => {
  const [allResources, setAllResources] = useState<ResourceDto[]>([]);
  const [allUnits, setAllUnits] = useState<UnitOfMeasureDto[]>([]);
  const [balances, setBalances] = useState<BalanceDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [resourcesData, unitsData, balancesData] = await Promise.all([
          apiService.getResources(),
          apiService.getUnitsOfMeasure(),
          apiService.getBalances()
        ]);
        
        setAllResources(resourcesData);
        setAllUnits(unitsData);
        setBalances(balancesData);
      } catch (err) {
        console.error('Error loading data:', err);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

  // Получить имя ресурса по ID
  const getResourceName = (resourceId: string) => {
    const resource = allResources.find(r => r.id === resourceId);
    return resource ? resource.name : '';
  };

  // Получить имя единицы измерения по ID
  const getUnitName = (unitId: string) => {
    const unit = allUnits.find(u => u.id === unitId);
    return unit ? unit.name : '';
  };

  // Получить баланс для комбинации ресурса и единицы измерения
  const getBalance = (resourceId: string, unitId: string) => {
    const balance = balances.find(b => b.resourceId === resourceId && b.unitOfMeasureId === unitId);
    return balance ? balance.quantity : 0;
  };

  // Проверить, активен ли ресурс
  const isResourceActive = (resourceId: string) => {
    const resource = allResources.find(r => r.id === resourceId);
    return resource ? resource.isActive : false;
  };

  // Проверить, активна ли единица измерения
  const isUnitActive = (unitId: string) => {
    const unit = allUnits.find(u => u.id === unitId);
    return unit ? unit.isActive : false;
  };

  // Получить количество для ресурса
  const getQuantity = (resourceId: string, unitId: string) =>
    resources.find(r => r.resourceId === resourceId && r.unitId === unitId)?.quantity || 0;

  // Обработчик изменения количества
  const handleQuantityChange = (resourceId: string, unitId: string, quantity: number) => {
    const updated = [...resources];
    const index = updated.findIndex(r => r.resourceId === resourceId && r.unitId === unitId);

    if (quantity > 0) {
      if (index >= 0) updated[index].quantity = quantity;
      else updated.push({ resourceId, unitId, quantity });
    } else if (index >= 0) {
      updated.splice(index, 1);
    }

    onResourcesChange(updated);
  };

 // Создать уникальный ключ для ресурса
  const getResourceKey = (resourceId: string, unitId: string) => `${resourceId}-${unitId}`;

  // Подготовить список ресурсов для отображения
  const prepareResourcesForDisplay = () => {
    const resourceMap = new Map<string, {
      resourceId: string;
      resourceName: string;
      unitId: string;
      unitName: string;
      availableQuantity: number;
    }>();
    
    // Добавляем ресурсы из existingDocumentResources
    existingDocumentResources.forEach(resource => {
      const key = getResourceKey(resource.resourceId, resource.unitId);
      if (!resourceMap.has(key)) {
        resourceMap.set(key, {
          resourceId: resource.resourceId,
          resourceName: resource.resourceName || getResourceName(resource.resourceId),
          unitId: resource.unitId,
          unitName: resource.unitName || getUnitName(resource.unitId),
          availableQuantity: getBalance(resource.resourceId, resource.unitId)
        });
      }
    });
    
    // Добавляем активные балансы
    balances.forEach(balance => {
      // Проверяем, что ресурс и единица измерения активны
      if (isResourceActive(balance.resourceId) && isUnitActive(balance.unitOfMeasureId)) {
        const key = getResourceKey(balance.resourceId, balance.unitOfMeasureId);
        // Добавляем только если еще не добавили
        if (!resourceMap.has(key)) {
          resourceMap.set(key, {
            resourceId: balance.resourceId,
            resourceName: balance.resourceName,
            unitId: balance.unitOfMeasureId,
            unitName: balance.unitOfMeasureName,
            availableQuantity: balance.quantity
          });
        }
      }
    });
    
    return Array.from(resourceMap.values());
  };

  if (loading) return <div>Loading resources...</div>;

  const displayResources = prepareResourcesForDisplay();

  return (
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
        {displayResources.map(resource => {
          const currentQuantity = getQuantity(resource.resourceId, resource.unitId);
          const maxQuantity = resource.availableQuantity;
          const hasError = !isSigned && (currentQuantity > maxQuantity);

          return(
            <tr key={`${resource.resourceId}-${resource.unitId}`}>
              <td>{resource.resourceName}</td>
              <td>{resource.unitName}</td>
              <td>
                <FormControl
                  type="number"
                  min={0}
                  step={0.001}
                  value={currentQuantity}
                  onChange={e =>
                handleQuantityChange(resource.resourceId, resource.unitId, parseFloat(e.target.value) || 0)
                }
                  disabled={disabled}
                  className={hasError ? 'is-invalid' : ''}
                >
                </FormControl>
                {hasError && (
                  <div className="invalid-feedback">
                    Exceeds available quantity ({resource.availableQuantity})
                  </div>
                )}
              </td>
              <td>{resource.availableQuantity}</td>
            </tr>
            )
        })}
        </tbody>
      </Table>
      {displayResources.length === 0 && <div className="text-center text-muted py-4">No resources available</div>}
    </div>
  );
};

export default ShipmentResourcesTable;
