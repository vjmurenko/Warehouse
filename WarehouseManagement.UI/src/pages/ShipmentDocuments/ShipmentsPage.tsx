import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Table, Alert, Spinner, Card, Badge } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import DocumentFilters from '../../components/ReceiptDocumentFilters';
import apiService from '../../services/api';
import { DocumentFilters as DocumentFiltersType, ShipmentDocumentSummaryDto } from '../../types/api';

const ShipmentsPage: React.FC = () => {
  const [shipments, setShipments] = useState<ShipmentDocumentSummaryDto[]>([]);
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
      setError('Error loading shipment documents');
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
            <h2 className="mb-0">Shipment Documents</h2>
            <Button variant="success" onClick={handleAddShipment}>
              Add Shipment
            </Button>
          </div>
        </Col>
      </Row>

      <Row className="mb-4">
        <Col>
          <DocumentFilters onFiltersChange={handleFiltersChange} title="Filter Shipments" />
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
                    <span className="visually-hidden">Loading...</span>
                  </Spinner>
                </div>
              ) : (
                <Table bordered hover className="mb-0">
                  <thead>
                    <tr>
                      <th>Number</th>
                      <th>Date</th>
                      <th>Client</th>
                      <th>Status</th>
                      <th>Resources</th>
                    </tr>
                  </thead>
                  <tbody>
                    {shipments.length === 0 ? (
                      <tr>
                        <td colSpan={5} className="text-center text-muted py-4">
                          No shipment documents found
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
                              {shipment.isSigned ? "Signed" : "Draft"}
                            </Badge>
                          </td>
                          <td>
                            <Badge bg="info">
                              {shipment.resourceCount} items
                            </Badge>
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
