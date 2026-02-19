import React, { useEffect, useMemo, useState, useCallback } from 'react';
import { Table, FormControl, Spinner } from 'react-bootstrap';
import { BalanceDto } from '../types/api';
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
}

const ShipmentResourcesTable: React.FC<ShipmentResourcesTableProps> = ({
       resources,
       onResourcesChange,
       disabled = false,
       isSigned = false}) => {
  const [balances, setBalances] = useState<BalanceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const loadData = async () => {
      try {
        const balancesData = await apiService.getBalances();
        if (isMounted) setBalances(balancesData);
      } catch (err) {
        console.error(err);
        if (isMounted) setError('Failed to load balances');
      } finally {
        if (isMounted) setLoading(false);
      }
    };

    loadData();

    return () => {
      isMounted = false;
    };
  }, []);

  const resourcesMap = useMemo(() => {
    const map = new Map<string, ShipmentResourceItem>();
    for (const r of resources) {
      map.set(`${r.resourceId}-${r.unitId}`, r);
    }
    return map;
  }, [resources]);

  const handleQuantityChange = useCallback((balance: BalanceDto, quantity: number) => {
        const key = `${balance.resourceId}-${balance.unitOfMeasureId}`;
        const existing = resourcesMap.get(key);

        let updated: ShipmentResourceItem[];

        if (quantity <= 0) {
          updated = resources.filter(
              r =>
                  !(r.resourceId === balance.resourceId &&
                      r.unitId === balance.unitOfMeasureId)
          );
        } else if (existing) {
          updated = resources.map(r =>
              r.resourceId === balance.resourceId &&
              r.unitId === balance.unitOfMeasureId
                  ? { ...r, quantity }
                  : r
          );
        } else {
          updated = [
            ...resources,
            {
              resourceId: balance.resourceId,
              resourceName: balance.resourceName,
              unitId: balance.unitOfMeasureId,
              unitName: balance.unitOfMeasureName,
              quantity
            }
          ];
        }

        onResourcesChange(updated);
      },
      [resources, resourcesMap, onResourcesChange]
  );

  if (loading) {
    return (
        <div className="text-center py-4">
          <Spinner animation="border" />
        </div>
    );
  }

  if (error) {
    return (
        <div className="text-danger text-center py-4">
          {error}
        </div>
    );
  }

  if (balances.length === 0) {
    return (
        <div className="text-center text-muted py-4">
          No resources available
        </div>
    );
  }

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
            const key = `${balance.resourceId}-${balance.unitOfMeasureId}`;
            const resource = resourcesMap.get(key);

            const currentQuantity = resource?.quantity ?? 0;
            const maxQuantity = balance.quantity;

            const hasError = !isSigned && currentQuantity > maxQuantity;

            return (
                <tr key={key}>
                  <td>{balance.resourceName}</td>
                  <td>{balance.unitOfMeasureName}</td>
                  <td>
                    <FormControl
                        type="number"
                        min={0}
                        step={0.001}
                        value={currentQuantity}
                        disabled={disabled || isSigned}
                        className={hasError ? 'is-invalid' : ''}
                        onChange={e =>
                            handleQuantityChange(
                                balance,
                                parseFloat(e.target.value) || 0
                            )
                        }
                    />
                    {hasError && (
                        <div className="invalid-feedback">
                          Exceeds available quantity ({maxQuantity})
                        </div>
                    )}
                  </td>
                  <td>{maxQuantity}</td>
                </tr>
            );
          })}
          </tbody>
        </Table>
      </div>
  );
};

export default ShipmentResourcesTable;
