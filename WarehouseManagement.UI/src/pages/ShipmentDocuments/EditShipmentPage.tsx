import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Card, Spinner, Badge } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import Select from 'react-select';
import apiService from '../../services/api';
import DocumentResources, { DocumentResourceItem } from '../../components/DocumentResources';
import { SelectOption, ClientDto, ShipmentDocumentDto } from '../../types/api';

const EditShipmentPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [shipment, setShipment] = useState<ShipmentDocumentDto | null>(null);
  const [number, setNumber] = useState<string>('');
  const [date, setDate] = useState<string>('');
  const [selectedClient, setSelectedClient] = useState<SelectOption | null>(null);
  const [resources, setResources] = useState<DocumentResourceItem[]>([]);
  const [isSigned, setIsSigned] = useState<boolean>(false);
  const [signDocument, setSignDocument] = useState<boolean>(false);
  
  const [clients, setClients] = useState<ClientDto[]>([]);
  const [isLoadingClients, setIsLoadingClients] = useState<boolean>(true);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const navigate = useNavigate();

  useEffect(() => {
    const loadClients = async () => {
      try {
        setIsLoadingClients(true);
        const data = await apiService.getActiveClients();
        setClients(data);
      } catch (err) {
        console.error('Error loading clients:', err);
      } finally {
        setIsLoadingClients(false);
      }
    };
    
    loadClients();
  }, []);

  useEffect(() => {
    if (id) {
      loadShipmentDocument(id);
    } else {
      setIsLoading(false);
      setError('Shipment document ID is required');
    }
  }, [id]);

  const loadShipmentDocument = async (shipmentId: string) => {
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await apiService.getShipmentDocumentById(shipmentId);
      setShipment(data);
      setNumber(data.number);
      setDate(new Date(data.date).toISOString().split('T')[0]);
      setSelectedClient({ value: data.clientId, label: data.clientName });
      setIsSigned(data.isSigned);
      setSignDocument(data.isSigned);
      
      setResources(data.resources.map(item => ({
        id: item.id,
        resourceId: item.resourceId,
        resourceName: item.resourceName,
        unitId: item.unitId,
        unitName: item.unitName,
        quantity: item.quantity
      })));
    } catch (err) {
      setError('Failed to load shipment document');
      console.error('Error loading shipment document:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!id || !shipment) {
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
    
    if (!selectedClient) {
      setError('Client is required');
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
      
      await apiService.updateShipmentDocument(id, {
        id: id,
        number: number.trim(),
        clientId: selectedClient.value,
        date: new Date(date).toISOString(),
        sign: signDocument,
        resources: resources.map(r => ({
          id: r.id,
          resourceId: r.resourceId,
          unitId: r.unitId,
          quantity: r.quantity
        }))
      });
      
      navigate('/shipments');
    } catch (err: any) {
      setError(err.message || 'Failed to update shipment document');
      console.error('Error updating shipment document:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!id || !shipment) {
      return;
    }
    
    if (!window.confirm('Are you sure you want to delete this shipment document?')) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.deleteShipmentDocument(id);
      navigate('/shipments');
    } catch (err: any) {
      setError(err.message || 'Failed to delete shipment document');
      console.error('Error deleting shipment document:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/shipments');
  };

  const clientOptions: SelectOption[] = clients.map(client => ({
    value: client.id,
    label: client.name
  }));

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

  if (!shipment) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          {error || 'Shipment document not found'}
        </Alert>
        <Button variant="primary" onClick={() => navigate('/shipments')}>
          Back to Shipments
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <h2>Edit Shipment Document</h2>
            {isSigned && (
              <Badge bg="success" className="fs-6">Signed</Badge>
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
            
            <Form.Group className="mb-3">
              <Form.Label>Client</Form.Label>
              <Select
                options={clientOptions}
                value={selectedClient}
                onChange={(selected) => setSelectedClient(selected as SelectOption)}
                isDisabled={isSubmitting || isLoadingClients}
                placeholder="Select client..."
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Check
                type="checkbox"
                id="sign-document"
                label="Sign document (will affect warehouse balance)"
                checked={signDocument}
                onChange={(e) => setSignDocument(e.target.checked)}
                disabled={isSubmitting}
              />
              {isSigned && !signDocument && (
                <div className="text-danger small mt-1">
                  Warning: Unsigning this document will reverse the balance changes
                </div>
              )}
              {!isSigned && signDocument && (
                <div className="text-success small mt-1">
                  This will update warehouse balances
                </div>
              )}
            </Form.Group>
          </Card.Body>
        </Card>
        
        <Card className="mb-4">
          <Card.Header>Resources</Card.Header>
          <Card.Body>
            <DocumentResources
              resources={resources}
              onResourcesChange={setResources}
              disabled={isSubmitting}
              mode="shipment"
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

export default EditShipmentPage;