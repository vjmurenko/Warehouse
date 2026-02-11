import React, { useState } from 'react';
import { Container, Row, Col, Form, Button, Alert } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';

const AddClientPage: React.FC = () => {
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedName = name.trim();
    const trimmedAddress = address.trim();

    if (!trimmedName) {
      setError('Название обязательно');
      return;
    }
    
    if (!trimmedAddress) {
      setError('Адрес обязателен');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.createClient({
        name: trimmedName,
        address: trimmedAddress,
      });
      
      navigate('/clients');
    } catch (err: any) {
      setError(err.message || 'Не удалось создать клиента');
      console.error('Error creating client:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/clients');
  };

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Добавить нового клиента</h2>
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
              <Form.Label>Название</Form.Label>
              <Form.Control
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Введите название клиента"
                disabled={isSubmitting}
                required
              />
            </Form.Group>
            
            <Form.Group className="mb-3">
              <Form.Label>Адрес</Form.Label>
              <Form.Control
                type="text"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                placeholder="Введите адрес клиента"
                disabled={isSubmitting}
                required
              />
            </Form.Group>
            
            <div className="d-flex gap-2">
              <Button variant="primary" type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Сохранение...' : 'Сохранить'}
              </Button>
              <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
                Отмена
              </Button>
            </div>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default AddClientPage;