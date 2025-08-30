import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Table, Alert, Spinner, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import DocumentFilters from '../../components/ReceiptDocumentFilters';
import apiService from '../../services/api';
import { DocumentFilters as DocumentFiltersType, ReceiptDocumentDto } from '../../types/api';

const ReceiptsPage: React.FC = () => {
  const [receipts, setReceipts] = useState<ReceiptDocumentDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [filters, setFilters] = useState<DocumentFiltersType>({});
  const navigate = useNavigate();

  useEffect(() => {
    loadReceipts();
  }, []);

  const loadReceipts = async (appliedFilters?: DocumentFiltersType) => {
    try {
      setLoading(true);
      setError(null);
      
      const filtersToUse = appliedFilters || filters;
      const data = filtersToUse.fromDate || filtersToUse.toDate || 
                filtersToUse.documentNumbers?.length || 
                filtersToUse.resourceIds?.length || 
                filtersToUse.unitIds?.length
        ? await apiService.getFilteredReceiptDocuments(filtersToUse)
        : await apiService.getReceiptDocuments();
      
      setReceipts(data);
    } catch (err) {
      setError('Error loading receipt documents');
      console.error('Error loading receipts:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddReceipt = () => {
    navigate('/receipts/add');
  };

  const handleReceiptClick = (receiptId: string) => {
    navigate(`/receipts/edit/${receiptId}`);
  };

  const handleFiltersChange = (newFilters: DocumentFiltersType) => {
    setFilters(newFilters);
    loadReceipts(newFilters);
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
            <h2 className="mb-0">Receipt Documents</h2>
            <Button variant="success" onClick={handleAddReceipt}>
              Add Receipt
            </Button>
          </div>
        </Col>
      </Row>

      <Row className="mb-4">
        <Col>
          <DocumentFilters onFiltersChange={handleFiltersChange} />
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
                      <th>Units of Measure</th>
                      <th>Resources</th>
                    </tr>
                  </thead>
                  <tbody>
                    {receipts.length === 0 ? (
                      <tr>
                        <td colSpan={4} className="text-center text-muted py-4">
                          No receipt documents found
                        </td>
                      </tr>
                    ) : (
                      receipts.map((receipt) => (
                        <tr
                          key={receipt.id}
                          onClick={() => handleReceiptClick(receipt.id)}
                          style={{ cursor: 'pointer' }}
                        >
                          <td>{receipt.number}</td>
                          <td>{formatDate(receipt.date)}</td>
                          <td>
                            {receipt.resources.map((resource, index) => (
                              <div key={resource.id}>
                                {index > 0 && <hr className="my-1" />}
                                {resource.unitName}
                              </div>
                            ))}
                          </td>
                          <td>
                            {receipt.resources.map((resource, index) => (
                              <div key={resource.id}>
                                {index > 0 && <hr className="my-1" />}
                                {resource.resourceName}
                              </div>
                            ))}
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

export default ReceiptsPage;
