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
  const [balances, setBalances] = useState<BalanceDto[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadData = async () => {
      try {
        const [activeResourceData, activeUnitsData, balancesData] = await Promise.all([
          apiService.getActiveResources(),
          apiService.getActiveUnitsOfMeasure(),
          apiService.getBalances()
        ]);

        // фильтруем активные + ресурсы текущего документа
        const filteredBalances = balancesData.filter(
          b => existingDocumentResources.some(r => ((r.resourceId === b.resourceId) && (r.unitId === b.unitOfMeasureId)))
            || (activeResourceData.some(r => r.id === b.resourceId) && activeUnitsData.some(u => u.id === b.unitOfMeasureId))
        )

        setBalances(filteredBalances);
      } catch (err) {
        console.error('Error loading resources:', err);
      } finally {
        setLoading(false);
      }
    };

    loadData();
  }, []);

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
        {balances.map(balance => {
          const currentQuantity = getQuantity(balance.resourceId, balance.unitOfMeasureId);
          const maxQuantity = balance.quantity;
          const hasError = !isSigned && (currentQuantity > maxQuantity);

          return(
            <tr>
              <td>{balance.resourceName}</td>
              <td>{balance.unitOfMeasureName}</td>
              <td>
                <FormControl
                  type="number"
                  min={0}
                  step={0.001}
                  value={currentQuantity}
                  onChange={e =>
                handleQuantityChange(balance.resourceId, balance.unitOfMeasureId, parseFloat(e.target.value) || 0)
                }
                  disabled={disabled}
                  className={hasError ? 'is-invalid' : ''}
                >
                </FormControl>
                {hasError && (
                  <div className="invalid-feedback">
                    Exceeds available quantity ({balance?.quantity ?? 0})
                  </div>
                )}
              </td>
              <td>{balance.quantity ?? 0}</td>
            </tr>
            )
        })}
        </tbody>
      </Table>
      {balances.length === 0 && <div className="text-center text-muted py-4">No resources available</div>}
    </div>
  );
};

export default ShipmentResourcesTable;
