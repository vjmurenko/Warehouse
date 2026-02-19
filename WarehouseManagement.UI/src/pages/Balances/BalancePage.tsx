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

      const loadBalances = async () => {
          try {
              setLoading(true);
              setError(null);

              let data: BalanceDto[];

              if (filters.resourceIds.length > 0 || filters.unitIds.length > 0) {
                  data = await apiService.getFilteredBalances(
                      filters.resourceIds,
                      filters.unitIds
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
    loadBalances();
  }, [filters]);

  const handleFiltersChange = (newFilters: BalanceFiltersType) => {
    setFilters(newFilters);
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
