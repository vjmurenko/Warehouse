import React, { useState } from 'react';
import { Container, Row, Col, Form, Button, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';

const AddUnitOfMeasurePage: React.FC = () => {
  const [name, setName] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    const trimmedName = name.trim();
    if (!trimmedName) {
      setError('Name is required');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.createUnitOfMeasure({
        name: trimmedName
      });
      
      navigate('/units');
    } catch (err: any) {
      setError(err.message || 'Failed to create unit of measure');
      console.error('Error creating unit of measure:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/units');
  };

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Добавить единицу измерения</h2>
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
              <Form.Label>Наименование</Form.Label>
              <Form.Control
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Введите наименование единицы измерения"
                disabled={isSubmitting}
                required
              />
            </Form.Group>
            
            <div className="d-flex gap-2">
              <Button variant="success" type="submit" disabled={isSubmitting}>
                {isSubmitting ? (
                  <>
                    <Spinner animation="border" size="sm" className="me-2" />
                    Сохранение...
                  </>
                ) : (
                  'Сохранить'
                )}
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

export default AddUnitOfMeasurePage;