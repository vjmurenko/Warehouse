import React from 'react';
import { Table, Spinner, Alert } from 'react-bootstrap';
import { BalanceDto } from '../types/api';

interface BalanceTableProps {
  balances: BalanceDto[];
  loading: boolean;
  error: string | null;
}

const BalanceTable: React.FC<BalanceTableProps> = ({ balances, loading, error }) => {
  if (loading) {
    return (
      <div className="text-center py-5">
        <Spinner animation="border" role="status">
          <span className="visually-hidden">Загрузка...</span>
        </Spinner>
        <div className="mt-2">Загрузка данных...</div>
      </div>
    );
  }

  if (error) {
    return (
      <Alert variant="danger">
        <Alert.Heading>Ошибка загрузки данных</Alert.Heading>
        <p>{error}</p>
      </Alert>
    );
  }

  if (!balances || balances.length === 0) {
    return (
      <Alert variant="info">
        <div className="text-center">
          <h5>Нет данных для отображения</h5>
          <p className="mb-0">Попробуйте изменить фильтры или добавить данные в систему.</p>
        </div>
      </Alert>
    );
  }
  
  return (
    <div className="table-responsive">
      <Table hover className="balance-table mb-0">
        <thead className="table-secondary">
          <tr>
            <th>Ресурс</th>
            <th>Единица измерения</th>
            <th>Количество</th>
          </tr>
        </thead>
        <tbody>
          {balances.map((balance) => (
            <tr key={balance.id}>
              <td>
                {balance.resourceName}
              </td>
              <td>
                <span>
                  {balance.unitOfMeasureName}
                </span>
              </td>
              <td>
                  {balance.quantity}
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  );
};

export default BalanceTable;
