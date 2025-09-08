import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Badge } from 'react-bootstrap';
import BalanceFilters from '../../components/BalanceFilters';
import BalanceTable from '../../components/BalanceTable';
import apiService from '../../services/api';
import { BalanceDto, BalanceFilters as BalanceFiltersType } from '../../types/api';

const BalancePage: React.FC = () => {
  const [balances, setBalances] = useState<BalanceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<BalanceFiltersType>({ resourceIds: [], unitIds: [] });

  useEffect(() => {
    loadBalances();
  }, []);

  const loadBalances = async (appliedFilters?: BalanceFiltersType) => {
    try {
      setLoading(true);
      setError(null);
      
      const filtersToUse = appliedFilters || filters;
      let data: BalanceDto[];
      
      if (filtersToUse.resourceIds.length > 0 || filtersToUse.unitIds.length > 0) {
        data = await apiService.getFilteredBalances(
          filtersToUse.resourceIds,
          filtersToUse.unitIds
        );
      } else {
        data = await apiService.getBalances();
      }
      
      setBalances(data);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : 'Error loading balance data';
      setError(errorMessage);
      console.error('Failed to load balances:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleFiltersChange = (newFilters: BalanceFiltersType) => {
    setFilters(newFilters);
    loadBalances(newFilters);
  };

  const getTotalQuantity = (): number => {
    return balances.reduce((sum, balance) => sum + balance.quantity, 0);
  };

  const getPositiveBalancesCount = (): number => {
    return balances.filter(balance => balance.quantity > 0).length;
  };

  const formatQuantity = (quantity: number): string => {
    return new Intl.NumberFormat('ru-RU', {
      minimumFractionDigits: 0,
      maximumFractionDigits: 3
    }).format(quantity);
  };

  return (
    <Container fluid className="p-4">
      <Row>
        <h2>Баланс</h2>
      </Row>
      <Row>
        <Col>
          <BalanceFilters onFiltersChange={handleFiltersChange} />
          
          <Card>
            <Card.Body className="p-0">
              <BalanceTable 
                balances={balances}
                loading={loading}
                error={error}
              />
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default BalancePage;
