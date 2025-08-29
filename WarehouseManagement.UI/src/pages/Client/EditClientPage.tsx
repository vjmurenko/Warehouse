import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Spinner } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import apiService from '../../services/api';
import { ClientDto } from '../../types/api';

const EditClientPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [client, setClient] = useState<ClientDto | null>(null);
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (id) {
      loadClient(id);
    } else {
      setIsLoading(false);
      setError('Client ID is required');
    }
  }, [id]);

  const loadClient = async (clientId: string) => {
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await apiService.getClientById(clientId);
      setClient(data);
      setName(data.name);
      setAddress(data.address);
    } catch (err) {
      setError('Failed to load client data');
      console.error('Error loading client:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!id || !client) {
      return;
    }
    
    if (!name.trim()) {
      setError('Name is required');
      return;
    }
    
    if (!address.trim()) {
      setError('Address is required');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.updateClient(id, {
        name: name.trim(),
        address: address.trim()
      });
      
      navigate('/clients');
    } catch (err: any) {
      setError(err.message || 'Failed to update client');
      console.error('Error updating client:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleArchiveToggle = async () => {
    if (!id || !client) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      if (client.isActive) {
        await apiService.archiveClient(id);
      } else {
        await apiService.activateClient(id);
      }
      
      navigate('/clients');
    } catch (err: any) {
      setError(err.message || `Failed to ${client.isActive ? 'archive' : 'activate'} client`);
      console.error(`Error ${client.isActive ? 'archiving' : 'activating'} client:`, err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/clients');
  };

  const handleDelete = async () => {
    if (!client) return;
    if (!window.confirm(`Вы уверены, что хотить удалить клиента ${client.name} ? `)) return;

    try {
      setIsSubmitting(true);
      setError(null);

      await apiService.deleteClient(client.id);
      navigate('/Clients');
    }
    catch (err){
      console.log("Ошибка при удалении клиента", err);
      setError("Ошибка при удалении клиента")
    }
    finally {
      setIsSubmitting(false);
    }
  }

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

  if (!client) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          {error || 'Client not found'}
        </Alert>
        <Button variant="primary" onClick={() => navigate('/clients')}>
          Back to Clients
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Edit Client</h2>
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
        <Col md={6}>
          <Form onSubmit={handleSubmit}>
            <Form.Group className="mb-3">
              <Form.Label>Name</Form.Label>
              <Form.Control
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter client name"
                disabled={isSubmitting}
                required
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Address</Form.Label>
              <Form.Control
                type="text"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                placeholder="Enter client address"
                disabled={isSubmitting}
                required
              />
            </Form.Group>
            
            <div className="d-flex gap-2">
              <Button variant="primary" type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Saving...' : 'Save'}
              </Button>

              <Button variant="danger" onClick={handleDelete}>
                Delete
              </Button>

              <Button 
                variant={client.isActive ? "warning" : "success"} 
                onClick={handleArchiveToggle} 
                disabled={isSubmitting}
              >
                {client.isActive ? 'Archive' : 'Activate'}
              </Button>
              
              <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
                Cancel
              </Button>
            </div>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default EditClientPage;