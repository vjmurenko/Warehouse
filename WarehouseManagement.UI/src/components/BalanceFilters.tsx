import React, { useState, useEffect } from 'react';
import { Row, Col, Form, Button } from 'react-bootstrap';
import Select from 'react-select';
import apiService from '../services/api';
import { SelectOption, BalanceFilters } from '../types/api';

interface BalanceFiltersProps {
  onFiltersChange: (filters: BalanceFilters) => void;
}

const BalanceFiltersComponent: React.FC<BalanceFiltersProps> = ({ onFiltersChange }) => {
  const [resources, setResources] = useState<SelectOption[]>([]);
  const [unitsOfMeasure, setUnitsOfMeasure] = useState<SelectOption[]>([]);
  const [selectedResources, setSelectedResources] = useState<SelectOption[]>([]);
  const [selectedUnits, setSelectedUnits] = useState<SelectOption[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadFilterData();
  }, []);

  const loadFilterData = async () => {
    try {
      setLoading(true);
      const [resourcesData, unitsData] = await Promise.all([
        apiService.getActiveResources(),
        apiService.getActiveUnitsOfMeasure()
      ]);
      
      setResources(resourcesData.map(r => ({ value: r.id, label: r.name })));
      setUnitsOfMeasure(unitsData.map(u => ({ value: u.id, label: u.name })));
    } catch (error) {
      console.error('Failed to load filter data:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleApplyFilters = () => {
    const filters: BalanceFilters = {
      resourceIds: selectedResources.map(r => r.value),
      unitIds: selectedUnits.map(u => u.value)
    };
    onFiltersChange(filters);
  };

  const handleClearFilters = () => {
    setSelectedResources([]);
    setSelectedUnits([]);
    onFiltersChange({ resourceIds: [], unitIds: [] });
  };

  if (loading) {
    return (
      <div className="filter-section p-3 mb-4">
        <div className="text-center">Загрузка фильтров...</div>
      </div>
    );
  }

  return (
    <div className="filter-section p-3 mb-4">
      <Row className="align-items-end">
        <Col md={5}>
          <Form.Group>
            <Form.Label>Ресурс</Form.Label>
            <Select
              isMulti
              value={selectedResources}
              onChange={(newValue) => setSelectedResources(newValue as SelectOption[])}
              options={resources}
              placeholder="Выберите ресурсы..."
              noOptionsMessage={() => "Нет доступных ресурсов"}
              className="react-select-container"
              classNamePrefix="react-select"
            />
          </Form.Group>
        </Col>
        <Col md={5}>
          <Form.Group>
            <Form.Label>Единица измерения</Form.Label>
            <Select
              isMulti
              value={selectedUnits}
              onChange={(newValue) => setSelectedUnits(newValue as SelectOption[])}
              options={unitsOfMeasure}
              placeholder="Выберите единицы измерения..."
              noOptionsMessage={() => "Нет доступных единиц измерения"}
              className="react-select-container"
              classNamePrefix="react-select"
            />
          </Form.Group>
        </Col>
        <Col md={2}>
          <div className="d-flex gap-2">
            <Button 
              variant="primary" 
              onClick={handleApplyFilters}
              className="flex-fill"
            >
              Применить
            </Button>
            <Button 
              variant="outline-secondary" 
              onClick={handleClearFilters}
              className="flex-fill"
            >
              Очистить
            </Button>
          </div>
        </Col>
      </Row>
    </div>
  );
};

export default BalanceFiltersComponent;
