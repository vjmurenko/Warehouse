import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Form, Button, Alert, Spinner } from 'react-bootstrap';
import { useNavigate, useParams } from 'react-router-dom';
import apiService from '../../services/api';
import { ClientDto } from '../../types/api';

const EditClientPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const [client, setClient] = useState<ClientDto | null>(null);
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const loadClient = async (clientId: string) => {
      try {
        setIsLoading(true);
        setError(null);
        const data = await apiService.getClientById(clientId);
        setClient(data);
        setName(data.name);
        setAddress(data.address);

      } catch (err) {
        setError('Не удалось загрузить данные клиента');
        console.error('Error loading client:', err);
      } finally {
        setIsLoading(false);
      }
    };

    if (id) {
      loadClient(id);
    } else {
      setIsLoading(false);
      setError('Требуется указать ID клиента');
    }
  }, [id]);



  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!id || !client) {
      return;
    }

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
      
      await apiService.updateClient(id, {
        name: trimmedName,
        address: trimmedAddress
      });
      
      navigate('/clients');
    } catch (err: any) {
      setError(err.message || 'Не удалось обновить клиента');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleArchiveToggle = async () => {
    if (!id || !client) {
      return;
    }
    
    try {
      setIsSubmitting(true);
      setError(null);
      
      if (client.isActive) {
        await apiService.archiveClient(id);
      } else {
        await apiService.activateClient(id);
      }
      
      navigate('/clients');
    } catch (err: any) {
      setError(err.message || `Не удалось ${client.isActive ? 'архивировать' : 'активировать'} клиента`);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    navigate('/clients');
  };

  const handleDelete = async () => {
    if (!client) return;
    if (!window.confirm(`Вы уверены, что хотить удалить клиента ${client.name} ? `)) return;

    try {
      setIsSubmitting(true);
      setError(null);

      await apiService.deleteClient(client.id);
      navigate('/Clients');
    }
    catch (err){
      console.log("Ошибка при удалении клиента", err);
      setError("Ошибка при удалении клиента")
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

  if (!client) {
    return (
      <Container fluid className="p-4">
        <Alert variant="danger">
          {error || 'Клиент не найден'}
        </Alert>
        <Button variant="primary" onClick={() => navigate('/clients')}>
          Назад к клиентам
        </Button>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Редактирование клиента</h2>
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

              <Button 
                variant={client.isActive ? "warning" : "success"} 
                onClick={handleArchiveToggle} 
                disabled={isSubmitting}
              >
                {client.isActive ? 'Архивировать' : 'Активировать'}
              </Button>

              <Button variant="danger" onClick={handleDelete}>
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

export default EditClientPage;