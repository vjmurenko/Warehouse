import React, { useState } from 'react';
import { Container, Row, Col, Button, Form, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';
import { CreateResourceDto } from '../../types/api';

const AddResourcePage: React.FC = () => {
  const [formData, setFormData] = useState<CreateResourceDto>({ name: '' });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!formData.name.trim()) {
      return;
    }

    try {
      setSubmitting(true);
      setError(null);
      
      const createDto: CreateResourceDto = { name: formData.name.trim() };
      await apiService.createResource(createDto);
      
      // Возвращаемся на страницу ресурсов после успешного создания
      navigate('/resources');
    } catch (err) {
      console.error('Error creating resource:', err);
      setError('Ошибка при создании ресурса');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Ресурс</h2>
        </Col>
      </Row>

      <Row className="mb-3">
        <Col>
          <Button 
            variant="success" 
            onClick={handleSubmit}
            disabled={submitting || !formData.name.trim()}
          >
            {submitting ? (
              <>
                <Spinner animation="border" size="sm" className="me-2" />
                Сохранение...
              </>
            ) : (
              'Сохранить'
            )}
          </Button>
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
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder=""
                disabled={submitting}
              />
            </Form.Group>
          </Form>
        </Col>
      </Row>
    </Container>
  );
};

export default AddResourcePage;
