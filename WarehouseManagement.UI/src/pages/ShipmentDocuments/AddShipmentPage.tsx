import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Card } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import Select from 'react-select';
import apiService from '../../services/api';
import { SelectOption, ClientDto } from '../../types/api';
import ShipmentResourcesTable, { ShipmentResourceItem } from '../../components/ShipmentResourcesTable'

const AddShipmentPage: React.FC = () => {
  const [number, setNumber] = useState<string>('');
  const [date, setDate] = useState<string>(new Date().toISOString().split('T')[0]);
  const [selectedClient, setSelectedClient] = useState<SelectOption | null>(null);
  const [resources, setResources] = useState<ShipmentResourceItem[]>([]);
  
  const [clients, setClients] = useState<ClientDto[]>([]);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const navigate = useNavigate();

  useEffect(() => {
    const loadClients = async () => {
      try {
        setIsLoading(true);
        setError(null);
        
        const data = await apiService.getActiveClients();
        setClients(data);
      } catch (err) {
        setError('Не удалось загрузить клиентов');
        console.error('Error loading clients:', err);
      } finally {
        setIsLoading(false);
      }
    };
    
    loadClients();
  }, []);

  const handleSubmit = async (e: React.FormEvent, signOption: boolean = false) => {
    e.preventDefault();
    
    // Validation
    if (!number.trim()) {
      setError('Необходимо указать номер документа');
      return;
    }
    
    if (!date) {
      setError('Необходимо указать дату');
      return;
    }
    
    if (!selectedClient) {
      setError('Необходимо выбрать клиента');
      return;
    }
    
    if (resources.length === 0) {
      setError('Необходимо добавить хотя бы один ресурс');
      return;
    }
    
    for (const resource of resources) {
      if (!resource.resourceId) {
        setError('Необходимо выбрать ресурс');
        return;
      }
      
      if (!resource.unitId) {
        setError('Необходимо выбрать единицу измерения');
        return;
      }
      
      if (resource.quantity <= 0) {
        setError('Количество должно быть больше нуля');
        return;
      }
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.createShipmentDocument({
        number: number.trim(),
        clientId: selectedClient.value,
        date: new Date(date).toISOString(),
        sign: signOption,
        resources: resources.map(r => ({
          resourceId: r.resourceId,
          unitId: r.unitId,
          quantity: r.quantity
        }))
      });
      
      navigate('/shipments');
    } catch (err: any) {
      setError(err.message || 'Не удалось создать документ отгрузки');
      console.error('Error creating shipment document:', err);
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

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Создание документа отгрузки</h2>
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
      
      <Form>
        <Card className="mb-4">
          <Card.Header>Детали документа</Card.Header>
          <Card.Body>
            <Form.Group className="mb-3 col-2">
              <Form.Label>Номер документа</Form.Label>
              <Form.Control
                type="text"
                value={number}
                onChange={(e) => setNumber(e.target.value)}
                placeholder="Введите номер документа"
                disabled={isSubmitting}
                required
              />
            </Form.Group>
            
            <Form.Group className="mb-3 col-2">
              <Form.Label>Клиент</Form.Label>
              <Select
                options={clientOptions}
                value={selectedClient}
                onChange={(selected) => setSelectedClient(selected as SelectOption)}
                isDisabled={isSubmitting || isLoading}
                placeholder="Выберите клиента..."
              />
            </Form.Group>
            
            <Form.Group className="mb-3 col-2">
              <Form.Label>Дата</Form.Label>
              <Form.Control
                type="date"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                disabled={isSubmitting}
                required
              />
            </Form.Group>
          </Card.Body>
        </Card>
        
        <Card className="mb-4">
          <Card.Header>Ресурсы</Card.Header>
          <Card.Body>
            <ShipmentResourcesTable
              resources={resources}
              onResourcesChange={setResources}
              disabled={isSubmitting}
            />
          </Card.Body>
        </Card>
        
        <div className="d-flex gap-2">
          <Button 
            variant="primary" 
            onClick={(e) => handleSubmit(e, false)} 
            disabled={isSubmitting}
          >
            {isSubmitting ? 'Создание...' : 'Сохранить'}
          </Button>
          <Button 
            variant="success" 
            onClick={(e) => handleSubmit(e, true)} 
            disabled={isSubmitting}
          >
            {isSubmitting ? 'Создание...' : 'Сохранить и подписать'}
          </Button>
          <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
            Отмена
          </Button>
        </div>
      </Form>
    </Container>
  );
};

export default AddShipmentPage;