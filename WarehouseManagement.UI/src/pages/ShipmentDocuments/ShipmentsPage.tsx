import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Table, Alert, Spinner, Card, Badge } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';
import { DocumentFilters as DocumentFiltersType, ShipmentDocumentDto } from '../../types/api';
import ShipmentDocumentFilterComponent from '../../components/ShipmentDocumentFilters';

const ShipmentsPage: React.FC = () => {
  const [shipments, setShipments] = useState<ShipmentDocumentDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<DocumentFiltersType>({});
  const navigate = useNavigate();

  useEffect(() => {
    loadShipments();
  }, []);

  const loadShipments = async (appliedFilters?: DocumentFiltersType) => {
    try {
      setLoading(true);
      setError(null);
      
      const filtersToUse = appliedFilters || filters;
      const data = filtersToUse.fromDate || filtersToUse.toDate || 
                  filtersToUse.documentNumbers?.length || 
                  filtersToUse.resourceIds?.length || 
                  filtersToUse.unitIds?.length
        ? await apiService.getFilteredShipmentDocuments(filtersToUse)
        : await apiService.getShipmentDocuments();
      
      setShipments(data);
    } catch (err) {
      setError('Ошибка при загрузке документов отгрузки');
      console.error('Error loading shipments:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddShipment = () => {
    navigate('/shipments/add');
  };

  const handleShipmentClick = (shipmentId: string) => {
    navigate(`/shipments/edit/${shipmentId}`);
  };

  const handleFiltersChange = (newFilters: DocumentFiltersType) => {
    setFilters(newFilters);
    loadShipments(newFilters);
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  };

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <h2 className="mb-0">Отгрузки</h2>
            <Button variant="success" onClick={handleAddShipment}>
              Добавить
            </Button>
          </div>
        </Col>
      </Row>

      <Row className="mb-4">
        <Col>
        <ShipmentDocumentFilterComponent onFiltersChange={handleFiltersChange} />
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
          <Card>
            <Card.Body className="p-0">
              {loading ? (
                <div className="text-center py-5">
                  <Spinner animation="border" role="status">
                    <span className="visually-hidden">Загрузка</span>
                  </Spinner>
                </div>
              ) : (
                <Table bordered hover className="mb-0">
                  <thead className="table-secondary">
                    <tr>
                      <th>Номер</th>
                      <th>Дата</th>
                      <th>Клиент</th>
                      <th>Статус</th>
                      <th>Ресурс</th>
                      <th>Единицы измерения</th>
                      <th>Количество</th>
                    </tr>
                  </thead>
                  <tbody>
                    {shipments.length === 0 ? (
                      <tr>
                        <td colSpan={6} className="text-center text-muted py-4">
                          Отгрузок не найдено
                        </td>
                      </tr>
                    ) : (
                      shipments.map((shipment) => (
                        <tr
                          key={shipment.id}
                          onClick={() => handleShipmentClick(shipment.id)}
                          style={{ cursor: 'pointer' }}
                        >
                          <td>{shipment.number}</td>
                          <td>{formatDate(shipment.date)}</td>
                          <td>{shipment.clientName}</td>
                          <td>
                            <Badge bg={shipment.isSigned ? "success" : "warning"}>  
                              {shipment.isSigned ? "Подписан" : "Черновик"}
                            </Badge>
                          </td>
                          <td>
                            {shipment.resources.map((resource, index) => (
                              <div key={resource.id}>
                                {index > 0 && <hr className="my-1" />}
                                {resource.resourceName}
                              </div>
                            ))}
                          </td>
                          <td>
                            {shipment.resources.map((resource, index) => (
                              <div key={resource.id}>
                                {index > 0 && <hr className="my-1" />}
                                {resource.unitName}
                              </div>
                            ))}
                          </td>
                          <td>
                            {shipment.resources.map((resource, index) =>
                              (<div key={resource.id}>
                                {index > 0 && <hr className="my-1" />}
                                {resource.quantity}
                              </div>))}
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </Table>
              )}
            </Card.Body>
          </Card>
        </Col>
      </Row>
    </Container>
  );
};

export default ShipmentsPage;
