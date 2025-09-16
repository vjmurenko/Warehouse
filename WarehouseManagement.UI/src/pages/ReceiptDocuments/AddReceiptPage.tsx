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
      setError('Необходимо указать номер документа');
      return;
    }
    
    if (!date) {
      setError('Необходимо указать дату');
      return;
    }

    // Only validate resources that have any data entered
    const resourcesWithData = resources.filter(r => r.resourceId || r.unitId || r.quantity > 0);
    

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
      
      await apiService.createReceiptDocument({
        number: number.trim(),
        date: new Date(date).toISOString(),
        resources: resourcesWithData.map(r => ({
          resourceId: r.resourceId,
          unitId: r.unitId,
          quantity: r.quantity
        }))
      });
      
      navigate('/receipts');
    } catch (err: any) {
      setError(err.message || 'Не удалось создать документ поступления');
      console.error('Ошибка при создании документа поступления:', err);
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
          <h2>Создание документа поступления</h2>
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
            />
          </Card.Body>
        </Card>
        
        <div className="d-flex gap-2">
          <Button variant="primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Создание...' : 'Создать поступление'}
          </Button>
          <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
            Отмена
          </Button>
        </div>
      </Form>
    </Container>
  );
};

export default AddReceiptPage;