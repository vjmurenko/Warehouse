import React, {useState, useEffect, useMemo} from 'react';
import { Container, Row, Col, Button, Table, Alert, Spinner } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom';
import apiService from '../../services/api';
import { UnitOfMeasureDto } from '../../types/api';

const UnitsOfMeasurePage: React.FC = () => {
  const [units, setUnits] = useState<UnitOfMeasureDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showArchived, setShowArchived] = useState(false);
  const navigate = useNavigate();

  useEffect(() => {
    const loadUnits = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await apiService.getUnitsOfMeasure();
        setUnits(data);
      } catch (err) {
        setError(`Error loading units of measure: ${err instanceof Error ? err.message : 'Unknown error'}`);
      } finally {
        setLoading(false);
      }
    };
     loadUnits();
  }, []);



  const filteredUnits = useMemo(() => {
    return  showArchived
        ? units.filter(unit => !unit.isActive)
        : units.filter(unit => unit.isActive);
  },[units, showArchived]);

  const handleAddUnit = () => {
    navigate('/units/add');
  };

  const handleToggleArchived = () => {
    setShowArchived(prev => !prev);
  };

  const handleUnitClick = (unitId: string) => {
    navigate(`/units/edit/${unitId}`);
  };

  if (loading) {
    return (
      <Container fluid className="p-4">
        <div className="text-center">
          <Spinner animation="border" role="status">
            <span className="visually-hidden">Загрузка</span>
          </Spinner>
        </div>
      </Container>
    );
  }

  return (
    <Container fluid className="p-4">
      <Row className="mb-3">
        <Col>
          <h2>Единицы измерения</h2>
        </Col>
      </Row>

      <Row className="mb-3">
        <Col>
          <div className="d-flex gap-2">
            {!showArchived ? (
              <>
                <Button variant="success" onClick={handleAddUnit}>
                  Добавить
                </Button>
                <Button variant="warning" onClick={handleToggleArchived}>
                  К архиву
                </Button>
              </>
            ) : (
              <Button variant="primary" onClick={handleToggleArchived}>
                К рабочим
              </Button>
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

      <Row>
        <Col>
          <Table bordered className="mb-0">
            <thead className="table-secondary">
              <tr>
                <th>Наименование</th>
              </tr>
            </thead>
            <tbody>
              {units.length === 0 ? (
                <tr>
                  <td className="text-center text-muted py-4">
                    {showArchived ? 'Архивных единиц измерения не найдено' : 'Единиц измерения не найдено'}
                  </td>
                </tr>
              ) : (
                filteredUnits.map((unit) => (
                  <tr
                    key={unit.id}
                    onClick={() => handleUnitClick(unit.id)}
                    style={{ cursor: 'pointer' }}
                  >
                    <td>{unit.name}</td>
                  </tr>
                ))
              )}
            </tbody>
          </Table>
        </Col>
      </Row>
    </Container>
  );
};

export default UnitsOfMeasurePage;