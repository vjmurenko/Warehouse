import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Button, Form, Alert, Spinner } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import apiService from '../../services/api';
import { ResourceDto, UpdateResourceDto } from '../../types/api';
import { getErrorMessage, isEntityInUseError, isDuplicateEntityError } from '../../utils/errorUtils';

const EditResourcePage: React.FC = () => {
  const [resource, setResource] = useState<ResourceDto | null>(null);
  const [formData, setFormData] = useState<UpdateResourceDto>({ name: '' });
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  useEffect(() => {

    const loadResource = async (resourceId: string) => {
      try {
        setLoading(true);
        setError(null);
        const foundResource = await apiService.getResourceById(resourceId);

        setResource(foundResource);
        setFormData({ name: foundResource.name });
      } catch (err) {
        setError(getErrorMessage(err));
        console.error('Error loading resource:', err);
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      loadResource(id);
    }
  }, [id]);

  const handleSave = async () => {
    if (!resource || !formData.name.trim()) {
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.updateResource(resource.id, { name: formData.name.trim() });
      navigate('/resources');
    } catch (err) {

      if (isDuplicateEntityError(err)) {
        setError('Ресурс с таким названием уже существует.');
      } else {
        setError(getErrorMessage(err));
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!resource) return;

    if (!window.confirm(`Вы уверены, что хотите удалить ресурс "${resource.name}"?`)) {
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      
     await apiService.deleteResource(resource.id);
     navigate('/resources');
    } catch (err) {

      if (isEntityInUseError(err)) {
        setError('Невозможно удалить этот ресурс, поскольку он используется в документах.');
      } else {
        setError(getErrorMessage(err));
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleArchive = async () => {
    if (!resource) return;

    try {
      setIsSubmitting(true);
      setError(null);

      if(resource.isActive){
        await apiService.archiveResource(resource.id);
      }
      else  {
        await apiService.activateResource(resource.id);
      }

      navigate('/resources');
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/resources');
  };

  if (loading) {
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

  if (!resource) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          Ресурс не найден
        </Alert>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Редактирование ресурса</h2>
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
          <Form>
            <Form.Group className="mb-3">
              <Form.Label>Наименование</Form.Label>
              <Form.Control
                type="text"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                placeholder="Введите наименование ресурса"
                disabled={isSubmitting}
              />
            </Form.Group>
          </Form>
        </Col>
      </Row>

      <Row className="mb-3">
        <Col>
          <div className="d-flex gap-2">
            <Button
              variant="success"
              onClick={handleSave}
              disabled={isSubmitting || !formData.name.trim()}
            >
              {isSubmitting ? (
                <>
                  <Spinner animation="border" size="sm" className="me-2" />
                  Сохранение...
                </>
              ) : (
                'Сохранить'
              )}
            </Button>

            <Button
              variant="danger"
              onClick={handleDelete}
              disabled={isSubmitting}
            >
              Удалить
            </Button>
            <Button
              variant="warning"
              onClick={handleArchive}
              disabled={isSubmitting}
            >
              {resource.isActive ? 'Архивировать' : 'Активировать'}
            </Button>
            <Button variant="secondary" onClick={handleCancel} disabled={isSubmitting}>
              Отмена
            </Button>

          </div>
        </Col>
      </Row>
    </Container>
  );
};

export default EditResourcePage;
