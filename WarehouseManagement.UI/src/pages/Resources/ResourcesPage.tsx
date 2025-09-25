import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Table, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';
import { ResourceDto } from '../../types/api';
import { getErrorMessage } from '../../utils/errorUtils';

const ResourcesPage: React.FC = () => {
  const [resources, setResources] = useState<ResourceDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showArchived, setShowArchived] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    loadResources();
  }, [showArchived]);

  const loadResources = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await apiService.getResources();

      // Фильтруем ресурсы в зависимости от режима просмотра
      const filteredData = showArchived
        ? data.filter(resource => !resource.isActive)
        : data.filter(resource => resource.isActive);

      setResources(filteredData);
    } catch (err) {
      setError(getErrorMessage(err));
      console.error('Error loading resources:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddResource = () => {
    navigate('/resources/add');
  };

  const handleToggleArchived = () => {
    setShowArchived(!showArchived);
  };

  const handleResourceClick = (resourceId: string) => {
    navigate(`/resources/edit/${resourceId}`);
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
          <h2>Ресурсы</h2>
        </Col>
      </Row>

      <Row className="mb-3">
        <Col>
          <div className="d-flex gap-2">
            {!showArchived ? (
              <>
                <Button variant="success" onClick={handleAddResource}>
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
                <th>Наименование</th>
              </tr>
            </thead>
            <tbody>
              {resources.length === 0 ? (
                <tr>
                  <td className="text-center text-muted py-4">
                    {showArchived ? 'Архивные ресурсы не найдены' : 'Ресурсы не найдены'}
                  </td>
                </tr>
              ) : (
                resources.map((resource) => (
                  <tr
                    key={resource.id}
                    onClick={() => handleResourceClick(resource.id)}
                    style={{ cursor: 'pointer' }}
                  >
                    <td>{resource.name}</td>
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

export default ResourcesPage;
