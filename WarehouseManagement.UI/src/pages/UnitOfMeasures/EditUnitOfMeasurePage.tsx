import React, { useState, useEffect } from 'react';
import { getErrorMessage, isEntityInUseError } from '../../utils/errorUtils';
import { Container, Row, Col, Form, Button, Alert, Spinner } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import apiService from '../../services/api';
import { UnitOfMeasureDto } from '../../types/api';

const EditUnitOfMeasurePage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [unit, setUnit] = useState<UnitOfMeasureDto | null>(null);
  const [name, setName] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    if (id) {
      loadUnit(id);
    } else {
      setIsLoading(false);
      setError('Unit ID is required');
    }
  }, [id]);

  const loadUnit = async (unitId: string) => {
    try {
      setIsLoading(true);
      setError(null);
      
      const data = await apiService.getUnitOfMeasureById(unitId);
      setUnit(data);
      setName(data.name);
    } catch (err) {
      setError('Не удалось загрузить данные единицы измерения');
      console.error('Error loading unit:', err);
    } finally {
      setIsLoading(false);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!id || !unit) {
      return;
    }
    
    if (!name.trim()) {
      setError('Название обязательно');
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);
      
      await apiService.updateUnitOfMeasure(id, {
        name: name.trim()
      });
      
      navigate('/units');
    } catch (err: any) {
      setError(err.message || 'Не удалось обновить единицу измерения');
      console.error('Error updating unit of measure:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleArchiveToggle = async () => {
    if (!id || !unit) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      if (unit.isActive) {
        await apiService.archiveUnitOfMeasure(id);
      } else {
        await apiService.activateUnitOfMeasure(id);
      }
      
      navigate('/units');
    } catch (err: any) {
      setError(err.message || `Не удалось ${unit.isActive ? 'архивировать' : 'активировать'} единицу измерения`);
      console.error(`Error ${unit.isActive ? 'archiving' : 'activating'} unit:`, err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/units');
  };

  const handleDelete = async () => {
    if (!unit) return;
    if (!window.confirm(`Вы уверены, что хотите удалить единицу измеререния ${unit?.name}?`)) return

    try {
      setIsSubmitting(true);
      setError(null);

      await apiService.deleteUnitOfMeasure(unit.id);
      navigate('/units')
    }
    catch (err){
      console.error('Error deleting unit of measure:', err);
      
      if (isEntityInUseError(err)) {
        setError('Невозможно удалить эту единицу измерения, поскольку она используется в документах.');
      } else {
        setError(getErrorMessage(err));
      }
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
            <span className="visually-hidden">Загрузка...</span>
          </Spinner>
        </div>
      </Container>
    );
  }

  if (!unit) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          {error || 'Единица измерения не найдена'}
        </Alert>
        <Button variant="primary" onClick={() => navigate('/units')}>
          Назад к списку единиц
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Редактирование единицы измерения</h2>
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
                placeholder="Введите название единицы"
                disabled={isSubmitting}
                required
              />
            </Form.Group>
            
            <div className="d-flex gap-2">
              <Button variant="primary" type="submit" disabled={isSubmitting}>
                {isSubmitting ? 'Сохранение...' : 'Сохранить'}
              </Button>
              
              <Button 
                variant={unit.isActive ? "warning" : "success"} 
                onClick={handleArchiveToggle} 
                disabled={isSubmitting}
              >
                {unit.isActive ? 'Архивировать' : 'Активировать'}
              </Button>

              <Button variant="danger" onClick={handleDelete} disabled={isSubmitting}>
                Удалить
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

export default EditUnitOfMeasurePage;