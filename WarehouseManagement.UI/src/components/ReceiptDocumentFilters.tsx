import React, { useState, useEffect } from 'react';
import { Form, Row, Col, Button, Card } from 'react-bootstrap';
import Select from 'react-select';
import {
  DocumentFilters,
  SelectOption,
  ResourceDto,
  UnitOfMeasureDto,
  ReceiptDocumentDto
} from '../types/api'
import apiService from '../services/api';

interface DocumentFiltersProps {
  onFiltersChange: (filters: DocumentFilters) => void;
}

const ReceiptDocumentFilterComponent: React.FC<DocumentFiltersProps> = ({ onFiltersChange }) => {
  const [fromDate, setFromDate] = useState<string>('');
  const [toDate, setToDate] = useState<string>('');
  const [selectedResources, setSelectedResources] = useState<SelectOption[]>([]);
  const [selectedUnits, setSelectedUnits] = useState<SelectOption[]>([]);
  const [selectedReceiptDocuments, setSelectedDocumentNumbers] = useState<SelectOption[]>([]);
  
  const [resources, setResources] = useState<ResourceDto[]>([]);
  const [units, setUnits] = useState<UnitOfMeasureDto[]>([]);
  const [receiptDocuments, setReceiptDocuments] = useState<ReceiptDocumentDto[]>([]);
  
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const fetchFilterData = async () => {
      setIsLoading(true);
      
      try {
        const [resourcesData, unitsData, receiptDocumentsData] = await Promise.all([
          apiService.getResources(),
          apiService.getUnitsOfMeasure(),
          apiService.getReceiptDocuments()
        ]);
        
        setResources(resourcesData);
        setUnits(unitsData);
        setReceiptDocuments(receiptDocumentsData);
      } catch (err) {
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
      documentNumbers: selectedReceiptDocuments.map(c => c.value),
      resourceIds: selectedResources.map(r => r.value),
      unitIds: selectedUnits.map(u => u.value)
    };
    
    onFiltersChange(filters);
  };

  const handleClearFilters = () => {
    setFromDate('');
    setToDate('');
    setSelectedResources([]);
    setSelectedUnits([]);
    setSelectedDocumentNumbers([]);
    
    onFiltersChange({});
  };

  const receiptNumbersOptions: SelectOption[] = receiptDocuments.map(receipt => ({
    value: receipt.number,
    label: receipt.number
  }))

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
      <Card.Body>
        <Form>
          <Row className="mb-3">
            <Col>
              <Form.Group className="mb-3">
                <Form.Label>Период</Form.Label>
                <div className="d-flex">
                  <Form.Control
                    type="date"
                    className="me-3"
                    value={fromDate}
                    onChange={(e) => setFromDate(e.target.value)}
                    disabled={isLoading}
                  />
                  <Form.Control
                    type="date"
                    value={toDate}
                    onChange={(e) => setToDate(e.target.value)}
                    disabled={isLoading}
                  />
                </div>
              </Form.Group>
            </Col>
            <Col>
              <Form.Group className="mb-3">
                <Form.Label>Номер поступления</Form.Label>
                <Select
                  isMulti
                  options={receiptNumbersOptions}
                  value={selectedReceiptDocuments}
                  onChange={(selected) => setSelectedDocumentNumbers(selected as SelectOption[])}
                  placeholder="Выберите номер поступления"
                  isDisabled={isLoading}
                  className="basic-multi-select"
                  classNamePrefix="select"
                />
              </Form.Group>
            </Col>
            <Col>
              <Form.Group>
                <Form.Label>Ресурс</Form.Label>
                <Select
                  isMulti
                  options={resourceOptions}
                  value={selectedResources}
                  onChange={(selected) => setSelectedResources(selected as SelectOption[])}
                  placeholder="Выберите ресурс"
                  isDisabled={isLoading}
                  className="basic-multi-select"
                  classNamePrefix="select"
                />
              </Form.Group>
            </Col>
            <Col>
              <Form.Group>
                <Form.Label>Единица измерения</Form.Label>
                <Select
                  isMulti
                  options={unitOptions}
                  value={selectedUnits}
                  onChange={(selected) => setSelectedUnits(selected as SelectOption[])}
                  placeholder="Выберите единицу измерения"
                  isDisabled={isLoading}
                  className="basic-multi-select"
                  classNamePrefix="select"
                />
              </Form.Group>
            </Col>
          </Row>

          <div className="d-flex gap-2 justify-content-end">
            <Button variant="secondary" onClick={handleClearFilters} disabled={isLoading}>
              Очистить
            </Button>
            <Button variant="primary" onClick={handleApplyFilters} disabled={isLoading}>
              Применить
            </Button>
          </div>
        </Form>
      </Card.Body>
    </Card>
  );
};

export default ReceiptDocumentFilterComponent;