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
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();

  useEffect(() => {
    if (id) {
      loadResource(id);
    }
  }, [id]);

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

  const handleSave = async () => {
    if (!resource || !formData.name.trim()) {
      return;
    }

    try {
      setSubmitting(true);
      setError(null);
      
      await apiService.updateResource(resource.id, { name: formData.name.trim() });
      navigate('/resources');
    } catch (err) {
      console.error('Error updating resource:', err);
      
      if (isDuplicateEntityError(err)) {
        setError('A resource with this name already exists.');
      } else {
        setError(getErrorMessage(err));
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async () => {
    if (!resource) return;

    if (!window.confirm(`Вы уверены, что хотите удалить ресурс "${resource.name}"?`)) {
      return;
    }

    try {
      setSubmitting(true);
      setError(null);
      
     await apiService.deleteResource(resource.id);
     navigate('/resources');
    } catch (err) {
      console.error('Error deleting resource:', err);
      
      if (isEntityInUseError(err)) {
        setError('Cannot delete this resource because it is currently being used in documents.');
      } else {
        setError(getErrorMessage(err));
      }
    } finally {
      setSubmitting(false);
    }
  };

  const handleArchive = async () => {
    if (!resource) return;

    try {
      setSubmitting(true);
      setError(null);

      if(resource.isActive){
        await apiService.archiveResource(resource.id);
      }
      else  {
        await apiService.activateResource(resource.id);
      }

      navigate('/resources');
    } catch (err) {
      console.error('Error archiving resource:', err);
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
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
          <h2>Ресурс</h2>
        </Col>
      </Row>

      <Row className="mb-3">
        <Col>
          <div className="d-flex gap-2">
            <Button 
              variant="success" 
              onClick={handleSave}
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
            
            <Button 
              variant="danger" 
              onClick={handleDelete}
              disabled={submitting}
            >
              Удалить
            </Button>
            <Button
                variant="warning" 
                onClick={handleArchive}
                disabled={submitting}
              >
                {resource.isActive ? 'Archive' : 'Activate'}
              </Button>

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

      <Row>
        <Col md={6}>
          <Form>
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

export default EditResourcePage;
