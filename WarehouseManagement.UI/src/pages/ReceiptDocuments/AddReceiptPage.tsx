import React, { useState } from 'react';
import { Container, Row, Col, Form, Button, Alert, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';
import ReceiptResources, { ReceiptResourceItem } from '../../components/ReceiptResources'

const AddReceiptPage: React.FC = () => {
  const [number, setNumber] = useState<string>('');
  const [date, setDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [resources, setResources] = useState<ReceiptResourceItem[]>([]);
  
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
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
      
      await apiService.createReceiptDocument({
        number: number.trim(),
        date: new Date(date).toISOString(),
        resources: resources.map(r => ({
          resourceId: r.resourceId,
          unitId: r.unitId,
          quantity: r.quantity
        }))
      });
      
      navigate('/receipts');
    } catch (err: any) {
      setError(err.message || 'Failed to create receipt document');
      console.error('Error creating receipt document:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/receipts');
  };

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Create Receipt Document</h2>
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
            <ReceiptResources
              resources={resources}
              onResourcesChange={setResources}
              disabled={isSubmitting}
            />
          </Card.Body>
        </Card>
        
        <div className="d-flex gap-2">
          <Button variant="primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Creating...' : 'Create Receipt'}
          </Button>
          <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
            Cancel
          </Button>
        </div>
      </Form>
    </Container>
  );
};

export default AddReceiptPage;