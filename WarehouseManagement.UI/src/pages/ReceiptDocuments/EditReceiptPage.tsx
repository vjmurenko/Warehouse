import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Card, Spinner } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import apiService from '../../services/api';
import { ReceiptDocumentDto } from '../../types/api';
import ReceiptResources, { ReceiptResourceItem } from '../../components/ReceiptResources'

const EditReceiptPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [receipt, setReceipt] = useState<ReceiptDocumentDto | null>(null);
  const [number, setNumber] = useState<string>('');
  const [date, setDate] = useState<string>('');
  const [resources, setResources] = useState<ReceiptResourceItem[]>([]);
  const [originalResources, setOriginalResources] = useState<ReceiptResourceItem[]>([]); // Store original resources for conditional validation
  
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);
  
  const navigate = useNavigate();

  useEffect(() => {
    if (id) {
      loadReceiptDocument(id);
    } else {
      setIsLoading(false);
      setError('Требуется указать ID документа поступления');
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
      const loadedResources = data.resources.map(item => ({
        id: item.id,
        resourceId: item.resourceId,
        resourceName: item.resourceName,
        unitId: item.unitId,
        unitName: item.unitName,
        quantity: item.quantity
      }));
      setResources(loadedResources);
      setOriginalResources(loadedResources); // Store original resources for conditional validation
    } catch (err) {
      setError('Не удалось загрузить документ поступления');
      console.error('Ошибка при загрузке документа поступления:', err);
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
      setError('Необходимо указать номер документа');
      return;
    }
    
    if (!date) {
        setError('Необходимо указать дату');
      return;
    }

    // Only validate resources that have any data entered
    const resourcesWithData = resources.filter(r => r.resourceId || r.unitId || r.quantity > 0);
    
    if (resourcesWithData.length === 0) {
      setError('Необходимо добавить хотя бы один ресурс');
      return;
    }

    for (const resource of resourcesWithData) {
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
      
      await apiService.updateReceiptDocument(id, {
        id: id,
        number: number.trim(),
        date: new Date(date).toISOString(),
        resources: resourcesWithData.map(r => ({
          id: r.id,
          resourceId: r.resourceId,
          unitId: r.unitId,
          quantity: r.quantity
        }))
      });
      
      navigate('/receipts');
    } catch (err: any) {
      setError(err.message || 'Не удалось обновить документ поступления');
      console.error('Ошибка при обновлении документа поступления:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!id || !receipt) {
      return;
    }
    
    if (!window.confirm('Вы уверены, что хотите удалить этот документ поступления?')) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.deleteReceiptDocument(id);
      navigate('/receipts');
    } catch (err: any) {
      setError(err.message || 'Не удалось удалить документ поступления');
      console.error('Ошибка при удалении документа поступления:', err);
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
            <span className="visually-hidden">Загрузка...</span>
          </Spinner>
        </div>
      </Container>
    );
  }

  if (!receipt) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          {error || 'Документ поступления не найден'}
        </Alert>
        <Button variant="primary" onClick={() => navigate('/receipts')}>
          Назад к поступлениям
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Редактирование документа поступления</h2>
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
          <Card.Header>Детали документа</Card.Header>
          <Card.Body>
            <Row className="mb-3">
              <Col className="col-2">
                <Form.Group>
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
              </Col>
            </Row>
              <Row className="mb-3">
                <Col className="col-2">
                  <Form.Group>
                    <Form.Label>Дата</Form.Label>
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
          <Card.Header>Ресурсы</Card.Header>
          <Card.Body>
            <ReceiptResources
              resources={resources}
              onResourcesChange={setResources}
              disabled={isSubmitting}
              existingResources={originalResources}
            />
          </Card.Body>
        </Card>
        
        <div className="d-flex gap-2">
          <Button variant="primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Сохранение...' : 'Сохранить изменения'}
          </Button>
          <Button variant="danger" onClick={handleDelete} disabled={isSubmitting}>
            Удалить
          </Button>
          <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
            Отмена
          </Button>
        </div>
      </Form>
    </Container>
  );
};

export default EditReceiptPage;