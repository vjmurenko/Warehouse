import React, { useState, useEffect } from 'react';
import { Table, FormControl } from 'react-bootstrap';
import { BalanceDto, ResourceDto, UnitOfMeasureDto } from '../types/api';
import apiService from '../services/api';

export interface ShipmentResourceItem {
  resourceId: string;
  unitId: string;
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

        // фильтруем активные + ресурсы текущего документа
        const filteredResources = resourcesData.filter(
          r => r.isActive || existingDocumentResources.some(er => er.resourceId === r.id)
        );
        const filteredUnits = unitsData.filter(
          u => u.isActive || existingDocumentResources.some(er => er.unitId === u.id)
        );

        setAllResources(filteredResources);
        setAllUnits(filteredUnits);
        setBalances(balancesData);
      } catch (err) {
        console.error('Error loading resources:', err);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, [existingDocumentResources]);

  const getQuantity = (resourceId: string, unitId: string) =>
    resources.find(r => r.resourceId === resourceId && r.unitId === unitId)?.quantity || 0;

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

  if (loading) return <div>Loading resources...</div>;

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
        {allResources.map(resource =>
          allUnits.map(unit => {
            const balance = balances.find(b => b.resourceId === resource.id && b.unitOfMeasureId === unit.id);
            const currentQuantity = getQuantity(resource.id, unit.id);
            const maxQuantity = balance?.quantity ?? 0;
            const hasError = !isSigned && (currentQuantity > maxQuantity);

            return (
              <tr key={`${resource.id}-${unit.id}`} className={currentQuantity > 0 ? 'table-warning' : ''}>
                <td>{resource.name}</td>
                <td>{unit.name}</td>
                <td>
                  <FormControl
                    type="number"
                    min={0}
                    step={0.001}
                    value={currentQuantity}
                    onChange={e =>
                      handleQuantityChange(resource.id, unit.id, parseFloat(e.target.value) || 0)
                    }
                    disabled={disabled}
                    className={hasError ? 'is-invalid' : ''}
                  />
                  {hasError && (
                    <div className="invalid-feedback">
                      Exceeds available quantity ({balance?.quantity ?? 0})
                    </div>
                  )}
                </td>
                <td>{balance?.quantity ?? 0}</td>
              </tr>
            );
          })
        )}
        </tbody>
      </Table>
      {allResources.length === 0 && <div className="text-center text-muted py-4">No resources available</div>}
    </div>
  );
};

export default ShipmentResourcesTable;
