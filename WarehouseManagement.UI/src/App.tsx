import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Container, Row, Col } from 'react-bootstrap';
import Sidebar from './components/Sidebar';
import BalancePage from './pages/Balances/BalancePage';
import ReceiptsPage from './pages/ReceiptDocuments/ReceiptsPage';
import AddReceiptPage from './pages/ReceiptDocuments/AddReceiptPage';
import EditReceiptPage from './pages/ReceiptDocuments/EditReceiptPage';
import ShipmentsPage from './pages/ShipmentDocuments/ShipmentsPage';
import AddShipmentPage from './pages/ShipmentDocuments/AddShipmentPage';
import EditShipmentPage from './pages/ShipmentDocuments/EditShipmentPage';
import ResourcesPage from './pages/Resources/ResourcesPage';
import AddResourcePage from './pages/Resources/AddResourcePage';
import EditResourcePage from './pages/Resources/EditResourcePage';
import ClientsPage from './pages/Client/ClientsPage';
import AddClientPage from './pages/Client/AddClientPage';
import EditClientPage from './pages/Client/EditClientPage';
import UnitsOfMeasurePage from './pages/UnitOfMeasures/UnitsOfMeasurePage';
import AddUnitOfMeasurePage from './pages/UnitOfMeasures/AddUnitOfMeasurePage';
import EditUnitOfMeasurePage from './pages/UnitOfMeasures/EditUnitOfMeasurePage';

function App() {
  return (
    <Router>
      <Container fluid className="p-0">
        <Row className="g-0">
          <Col md={3} lg={2} className="sidebar">
            <Sidebar />
          </Col>
          <Col md={9} lg={10} className="main-content">
            <Routes>
              <Route path="/" element={<BalancePage />} />
              <Route path="/balance" element={<BalancePage />} />
              <Route path="/receipts" element={<ReceiptsPage />} />
              <Route path="/receipts/add" element={<AddReceiptPage />} />
              <Route path="/receipts/edit/:id" element={<EditReceiptPage />} />
              <Route path="/shipments" element={<ShipmentsPage />} />
              <Route path="/shipments/add" element={<AddShipmentPage />} />
              <Route path="/shipments/edit/:id" element={<EditShipmentPage />} />
              <Route path="/resources" element={<ResourcesPage />} />
              <Route path="/resources/add" element={<AddResourcePage />} />
              <Route path="/resources/edit/:id" element={<EditResourcePage />} />
              <Route path="/clients" element={<ClientsPage />} />
              <Route path="/clients/add" element={<AddClientPage />} />
              <Route path="/clients/edit/:id" element={<EditClientPage />} />
              <Route path="/units" element={<UnitsOfMeasurePage />} />
              <Route path="/units/add" element={<AddUnitOfMeasurePage />} />
              <Route path="/units/edit/:id" element={<EditUnitOfMeasurePage />} />
            </Routes>
          </Col>
        </Row>
      </Container>
    </Router>
  );
}

export default App;
