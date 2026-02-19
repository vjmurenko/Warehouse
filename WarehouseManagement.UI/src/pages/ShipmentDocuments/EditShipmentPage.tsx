import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Card, Spinner, Badge } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import Select from 'react-select';
import apiService from '../../services/api';
import { SelectOption, ClientDto, ShipmentDocumentDto } from '../../types/api';
import ShipmentResourcesTable, { ShipmentResourceItem } from '../../components/ShipmentResourcesTable'

const EditShipmentPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [shipment, setShipment] = useState<ShipmentDocumentDto | null>(null);
  const [number, setNumber] = useState<string>('');
  const [date, setDate] = useState<string>('');
  const [selectedClient, setSelectedClient] = useState<SelectOption | null>(null);
  const [resources, setResources] = useState<ShipmentResourceItem[]>([]);
  const [isSigned, setIsSigned] = useState<boolean>(false);
  const [signDocument, setSignDocument] = useState<boolean>(false);
  
  const [clients, setClients] = useState<ClientDto[]>([]);
  const [isLoadingClients, setIsLoadingClients] = useState<boolean>(true);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const navigate = useNavigate();

  useEffect(() => {
    if (id) {
      loadData(id);
    } else {
      setIsLoading(false);
      setError('Не удалось загрузить документ отгрузки');
    }
  }, [id]);

  const loadData = async (shipmentId: string) => {
    try {
      setIsLoading(true);
      setIsLoadingClients(true);
      setError(null);
      
      const [activeClientsData, shipmentData] = await Promise.all([
        apiService.getActiveClients(),
        apiService.getShipmentDocumentById(shipmentId)
      ]);
      
      const isClientActive = activeClientsData.find(c => c.id === shipmentData.clientId);
      let allClients = [...activeClientsData];
      
      if (!isClientActive) {
        try {
          const documentClient = await apiService.getClientById(shipmentData.clientId);
          allClients = [...activeClientsData, documentClient];
        } catch (err) {
          console.error('Error loading document client:', err);
        }
      }

      setClients(allClients);
      setShipment(shipmentData);
      setNumber(shipmentData.number);
      setDate(new Date(shipmentData.date).toISOString().split('T')[0]);
      setSelectedClient({ value: shipmentData.clientId, label: shipmentData.clientName });
      setIsSigned(shipmentData.isSigned);
      setSignDocument(shipmentData.isSigned);
      
      setResources(shipmentData.resources.map(item => ({
        id: item.id,
        resourceId: item.resourceId,
        resourceName: item.resourceName,
        unitId: item.unitId,
        unitName: item.unitName,
        quantity: item.quantity
      })));
    } catch (err) {
      setError('Не удалось загрузить документ отгрузки');
      console.error('Error loading shipment document:', err);
    } finally {
      setIsLoading(false);
      setIsLoadingClients(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent, signOption?: boolean) => {
    e.preventDefault();
    
    if (!id || !shipment) {
      return;
    }
    const shouldSign = signOption !== undefined ? signOption : signDocument;

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
      
      await apiService.updateShipmentDocument(id, {
        id: id,
        number: number.trim(),
        clientId: selectedClient.value,
        date: new Date(date).toISOString(),
        sign: shouldSign,
        resources: resources.map(r => ({
          resourceId: r.resourceId,
          unitId: r.unitId,
          quantity: r.quantity
        }))
      });
      
      navigate('/shipments');
    } catch (err: any) {
      setError(err.message || 'Не удалось обновить документ отгрузки');
      console.error('Error updating shipment document:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRevoke = async () => {
    if (!id || !shipment) {
      return;
    }
    
    if (!window.confirm('Вы уверены, что хотите отозвать этот документ отгрузки? Это восстановит балансы на складе.')) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.revokeShipmentDocument(id);
      navigate('/shipments');
    } catch (err: any) {
      setError(err.message || 'Не удалось отозвать документ отгрузки');
      console.error('Error revoking shipment document:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!id || !shipment) {
      return;
    }
    
    if (!window.confirm('Вы уверены, что хотите удалить этот документ отгрузки?')) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.deleteShipmentDocument(id);
      navigate('/shipments');
    } catch (err: any) {
      setError(err.message || 'Не удалось удалить документ отгрузки');
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
            <span className="visually-hidden">Загрузка...</span>
          </Spinner>
        </div>
      </Container>
    );
  }

  if (!shipment) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          {error || 'Документ отгрузки не найден'}
        </Alert>
        <Button variant="primary" onClick={() => navigate('/shipments')}>
          Назад к отгрузкам
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <div className="d-flex justify-content-between align-items-center">
            <h2>Редактирование документа отгрузки</h2>
            {isSigned && (
              <Badge bg="success" className="fs-6">Подписан</Badge>
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
                disabled={isSubmitting || isSigned}
                required
              />
            </Form.Group>
            
            <Form.Group className="mb-3 col-2">
              <Form.Label>Клиент</Form.Label>
              <Select
                options={clientOptions}
                value={selectedClient}
                onChange={(selected) => setSelectedClient(selected as SelectOption)}
                isDisabled={isSubmitting || isLoadingClients || isSigned}
                placeholder="Выберите клиента..."
              />
            </Form.Group>
            
            <Form.Group className="mb-3 col-2">
              <Form.Label>Дата</Form.Label>
              <Form.Control
                type="date"
                value={date}
                onChange={(e) => setDate(e.target.value)}
                disabled={isSubmitting || isSigned}
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
              disabled={isSubmitting || isSigned}
              isSigned={isSigned}
            />
          </Card.Body>
        </Card>
        <div className="d-flex gap-2">
          {isSigned ? (
            <>
              <Button variant="warning" onClick={handleRevoke} disabled={isSubmitting}>
                {isSubmitting ? 'Отзыв...' : 'Отозвать'}
              </Button>
              <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
                Отмена
              </Button>
            </>
          ) : (
            <>
              <Button 
                variant="primary" 
                onClick={(e) => handleSubmit(e, false)} 
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Сохранение...' : 'Сохранить'}
              </Button>
              <Button 
                variant="success" 
                onClick={(e) => handleSubmit(e, true)} 
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Сохранение...' : 'Сохранить и подписать'}
              </Button>
              <Button variant="danger" onClick={handleDelete} disabled={isSubmitting}>
                {isSubmitting ? 'Удаление...' : 'Удалить'}
              </Button>
              <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
                Отмена
              </Button>
            </>
          )}
        </div>
      </Form>
    </Container>
  );
};

export default EditShipmentPage;