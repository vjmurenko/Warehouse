import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Table, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';
import { ClientDto } from '../../types/api';

const ClientsPage: React.FC = () => {
  const [clients, setClients] = useState<ClientDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showArchived, setShowArchived] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    loadClients();
  }, [showArchived]);

  const loadClients = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getClients();

      // Filter clients based on view mode
      const filteredData = showArchived
        ? data.filter(client => !client.isActive)
        : data.filter(client => client.isActive);

      setClients(filteredData);
    } catch (err) {
      setError('Ошибка при загрузке клиентов');
      console.error('Error loading clients:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddClient = () => {
    navigate('/clients/add');
  };

  const handleToggleArchived = () => {
    setShowArchived(!showArchived);
  };

  const handleClientClick = (clientId: string) => {
    navigate(`/clients/edit/${clientId}`);
  };

  if (loading) {
    return (
      <Container fluid className="p-4">
        <div className="text-center">
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Загрузка</span>
          </Spinner>
        </div>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Клиенты</h2>
        </Col>
      </Row>

      <Row className="mb-3">
        <Col>
          <div className="d-flex gap-2">
            {!showArchived ? (
              <>
                <Button variant="success" onClick={handleAddClient}>
                  Добавить
                </Button>
                <Button variant="warning" onClick={handleToggleArchived}>
                  К архиву
                </Button>
              </>
            ) : (
              <Button variant="primary" onClick={handleToggleArchived}>
               К рабочим
              </Button>
            )}
          </div>
        </Col>
      </Row>

      {error && (
        <Row className="mb-3">
          <Col>
            <Alert variant="danger" dismissible onClose={() => setError(null)}>
              {error}
            </Alert>
          </Col>
        </Row>
      )}

      <Row>
        <Col>
          <Table bordered className="mb-0">
            <thead className="table-secondary">
              <tr>
                <th>Имя</th>
                <th>Адрес</th>
              </tr>
            </thead>
            <tbody>
              {clients.length === 0 ? (
                <tr>
                  <td colSpan={2} className="text-center text-muted py-4">
                    {showArchived ? 'Архивных клиентов не найдено' : 'Клиентов не найдено'}
                  </td>
                </tr>
              ) : (
                clients.map((client) => (
                  <tr
                    key={client.id}
                    onClick={() => handleClientClick(client.id)}
                    style={{ cursor: 'pointer' }}
                  >
                    <td>{client.name}</td>
                    <td>{client.address}</td>
                  </tr>
                ))
              )}
            </tbody>
          </Table>
        </Col>
      </Row>
    </Container>
  );
};

export default ClientsPage;