import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Card, Spinner } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import apiService from '../../services/api';
import DocumentResources, { DocumentResourceItem } from '../../components/DocumentResources';
import { ReceiptDocumentDto } from '../../types/api';

const EditReceiptPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [receipt, setReceipt] = useState<ReceiptDocumentDto | null>(null);
  const [number, setNumber] = useState<string>('');
  const [date, setDate] = useState<string>('');
  const [resources, setResources] = useState<DocumentResourceItem[]>([]);
  
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const navigate = useNavigate();

  useEffect(() => {
    if (id) {
      loadReceiptDocument(id);
    } else {
      setIsLoading(false);
      setError('Receipt document ID is required');
    }
  }, [id]);

  const loadReceiptDocument = async (receiptId: string) => {
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await apiService.getReceiptDocumentById(receiptId);
      setReceipt(data);
      setNumber(data.number);
      setDate(new Date(data.date).toISOString().split('T')[0]);
      setResources(data.resources.map(item => ({
        id: item.id,
        resourceId: item.resourceId,
        resourceName: item.resourceName,
        unitId: item.unitId,
        unitName: item.unitName,
        quantity: item.quantity
      })));
    } catch (err) {
      setError('Failed to load receipt document');
      console.error('Error loading receipt document:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!id || !receipt) {
      return;
    }
    
    // Validation
    if (!number.trim()) {
      setError('Document number is required');
      return;
    }
    
    if (!date) {
      setError('Date is required');
      return;
    }
    
    if (resources.length === 0) {
      setError('At least one resource must be added');
      return;
    }
    
    for (const resource of resources) {
      if (!resource.resourceId) {
        setError('All resources must have a resource type selected');
        return;
      }
      
      if (!resource.unitId) {
        setError('All resources must have a unit of measure selected');
        return;
      }
      
      if (resource.quantity <= 0) {
        setError('All resources must have a positive quantity');
        return;
      }
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.updateReceiptDocument(id, {
        id: id,
        number: number.trim(),
        date: new Date(date).toISOString(),
        resources: resources.map(r => ({
          id: r.id,
          resourceId: r.resourceId,
          unitId: r.unitId,
          quantity: r.quantity
        }))
      });
      
      navigate('/receipts');
    } catch (err: any) {
      setError(err.message || 'Failed to update receipt document');
      console.error('Error updating receipt document:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!id || !receipt) {
      return;
    }
    
    if (!window.confirm('Are you sure you want to delete this receipt document?')) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.deleteReceiptDocument(id);
      navigate('/receipts');
    } catch (err: any) {
      setError(err.message || 'Failed to delete receipt document');
      console.error('Error deleting receipt document:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/receipts');
  };

  if (isLoading) {
    return (
      <Container fluid className="p-4">
        <div className="text-center">
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Loading...</span>
          </Spinner>
        </div>
      </Container>
    );
  }

  if (!receipt) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          {error || 'Receipt document not found'}
        </Alert>
        <Button variant="primary" onClick={() => navigate('/receipts')}>
          Back to Receipts
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Edit Receipt Document</h2>
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
      
      <Form onSubmit={handleSubmit}>
        <Card className="mb-4">
          <Card.Header>Document Details</Card.Header>
          <Card.Body>
            <Row className="mb-3">
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Document Number</Form.Label>
                  <Form.Control
                    type="text"
                    value={number}
                    onChange={(e) => setNumber(e.target.value)}
                    placeholder="Enter document number"
                    disabled={isSubmitting}
                    required
                  />
                </Form.Group>
              </Col>
              <Col md={6}>
                <Form.Group className="mb-3">
                  <Form.Label>Date</Form.Label>
                  <Form.Control
                    type="date"
                    value={date}
                    onChange={(e) => setDate(e.target.value)}
                    disabled={isSubmitting}
                    required
                  />
                </Form.Group>
              </Col>
            </Row>
          </Card.Body>
        </Card>
        
        <Card className="mb-4">
          <Card.Header>Resources</Card.Header>
          <Card.Body>
            <DocumentResources
              resources={resources}
              onResourcesChange={setResources}
              disabled={isSubmitting}
              mode="receipt"
            />
          </Card.Body>
        </Card>
        
        <div className="d-flex gap-2">
          <Button variant="primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Save Changes'}
          </Button>
          <Button variant="danger" onClick={handleDelete} disabled={isSubmitting}>
            Delete
          </Button>
          <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
            Cancel
          </Button>
        </div>
      </Form>
    </Container>
  );
};

export default EditReceiptPage;