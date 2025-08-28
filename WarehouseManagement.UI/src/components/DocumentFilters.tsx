import React, { useState, useEffect } from 'react';
import { Form, Row, Col, Button, Card } from 'react-bootstrap';
import Select from 'react-select';
import { DocumentFilters, SelectOption, ResourceDto, UnitOfMeasureDto } from '../types/api';
import apiService from '../services/api';

interface DocumentFiltersProps {
  onFiltersChange: (filters: DocumentFilters) => void;
  title?: string;
}

const DocumentFiltersComponent: React.FC<DocumentFiltersProps> = ({ onFiltersChange, title = 'Filter Documents' }) => {
  const [fromDate, setFromDate] = useState<string>('');
  const [toDate, setToDate] = useState<string>('');
  const [documentNumbers, setDocumentNumbers] = useState<string>('');
  const [selectedResources, setSelectedResources] = useState<SelectOption[]>([]);
  const [selectedUnits, setSelectedUnits] = useState<SelectOption[]>([]);
  
  const [resources, setResources] = useState<ResourceDto[]>([]);
  const [units, setUnits] = useState<UnitOfMeasureDto[]>([]);
  
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchFilterData = async () => {
      setIsLoading(true);
      setError(null);
      
      try {
        const [resourcesData, unitsData] = await Promise.all([
          apiService.getActiveResources(),
          apiService.getActiveUnitsOfMeasure()
        ]);
        
        setResources(resourcesData);
        setUnits(unitsData);
      } catch (err) {
        setError('Failed to load filter data');
        console.error('Error loading filter data:', err);
      } finally {
        setIsLoading(false);
      }
    };
    
    fetchFilterData();
  }, []);

  const handleApplyFilters = () => {
    const filters: DocumentFilters = {
      fromDate: fromDate || undefined,
      toDate: toDate || undefined,
      documentNumbers: documentNumbers ? documentNumbers.split(',').map(n => n.trim()) : undefined,
      resourceIds: selectedResources.map(r => r.value),
      unitIds: selectedUnits.map(u => u.value)
    };
    
    onFiltersChange(filters);
  };

  const handleClearFilters = () => {
    setFromDate('');
    setToDate('');
    setDocumentNumbers('');
    setSelectedResources([]);
    setSelectedUnits([]);
    
    onFiltersChange({});
  };

  const resourceOptions: SelectOption[] = resources.map(resource => ({
    value: resource.id,
    label: resource.name
  }));

  const unitOptions: SelectOption[] = units.map(unit => ({
    value: unit.id,
    label: unit.name
  }));

  return (
    <Card className="mb-4">
      <Card.Header>{title}</Card.Header>
      <Card.Body>
        <Form>
          <Row className="mb-3">
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>Start Date</Form.Label>
                <Form.Control
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                  disabled={isLoading}
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group className="mb-3">
                <Form.Label>End Date</Form.Label>
                <Form.Control
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                  disabled={isLoading}
                />
              </Form.Group>
            </Col>
          </Row>

          <Form.Group className="mb-3">
            <Form.Label>Document Numbers (comma-separated)</Form.Label>
            <Form.Control
              type="text"
              value={documentNumbers}
              onChange={(e) => setDocumentNumbers(e.target.value)}
              placeholder="e.g. DOC-001, DOC-002"
              disabled={isLoading}
            />
          </Form.Group>

          <Row className="mb-3">
            <Col md={6}>
              <Form.Group>
                <Form.Label>Resources</Form.Label>
                <Select
                  isMulti
                  options={resourceOptions}
                  value={selectedResources}
                  onChange={(selected) => setSelectedResources(selected as SelectOption[])}
                  placeholder="Select resources..."
                  isDisabled={isLoading}
                  className="basic-multi-select"
                  classNamePrefix="select"
                />
              </Form.Group>
            </Col>
            <Col md={6}>
              <Form.Group>
                <Form.Label>Units of Measure</Form.Label>
                <Select
                  isMulti
                  options={unitOptions}
                  value={selectedUnits}
                  onChange={(selected) => setSelectedUnits(selected as SelectOption[])}
                  placeholder="Select units..."
                  isDisabled={isLoading}
                  className="basic-multi-select"
                  classNamePrefix="select"
                />
              </Form.Group>
            </Col>
          </Row>

          <div className="d-flex gap-2 justify-content-end">
            <Button variant="secondary" onClick={handleClearFilters} disabled={isLoading}>
              Clear
            </Button>
            <Button variant="primary" onClick={handleApplyFilters} disabled={isLoading}>
              Apply Filters
            </Button>
          </div>
        </Form>
      </Card.Body>
    </Card>
  );
};

export default DocumentFiltersComponent;