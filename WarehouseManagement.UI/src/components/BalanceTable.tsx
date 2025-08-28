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

  const formatQuantity = (quantity: number): string => {
    return new Intl.NumberFormat('ru-RU', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 3
    }).format(quantity);
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleString('ru-RU', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="table-responsive">
      <Table striped hover className="balance-table mb-0">
        <thead>
          <tr>
            <th>Ресурс</th>
            <th>Единица измерения</th>
            <th className="text-end">Количество</th>
            <th>Создан</th>
            <th>Обновлен</th>
          </tr>
        </thead>
        <tbody>
          {balances.map((balance) => (
            <tr key={balance.id}>
              <td>
                <strong>{balance.resourceName}</strong>
              </td>
              <td>
                <span className="badge bg-secondary">
                  {balance.unitOfMeasureName}
                </span>
              </td>
              <td className="text-end">
                <strong className={balance.quantity > 0 ? 'text-success' : 'text-danger'}>
                  {formatQuantity(balance.quantity)}
                </strong>
              </td>
              <td>
                <small className="text-muted">
                  {formatDate(balance.createdAt)}
                </small>
              </td>
              <td>
                <small className="text-muted">
                  {balance.updatedAt ? formatDate(balance.updatedAt) : '—'}
                </small>
              </td>
            </tr>
          ))}
        </tbody>
      </Table>
    </div>
  );
};

export default BalanceTable;
