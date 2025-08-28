import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { Container, Row, Col } from 'react-bootstrap';
import Sidebar from './components/Sidebar';
import BalancePage from './pages/BalancePage';
import ReceiptsPage from './pages/ReceiptsPage';
import AddReceiptPage from './pages/AddReceiptPage';
import EditReceiptPage from './pages/EditReceiptPage';
import ShipmentsPage from './pages/ShipmentsPage';
import AddShipmentPage from './pages/AddShipmentPage';
import EditShipmentPage from './pages/EditShipmentPage';
import ResourcesPage from './pages/ResourcesPage';
import AddResourcePage from './pages/AddResourcePage';
import EditResourcePage from './pages/EditResourcePage';
import ClientsPage from './pages/ClientsPage';
import AddClientPage from './pages/AddClientPage';
import EditClientPage from './pages/EditClientPage';
import UnitsOfMeasurePage from './pages/UnitsOfMeasurePage';
import AddUnitOfMeasurePage from './pages/AddUnitOfMeasurePage';
import EditUnitOfMeasurePage from './pages/EditUnitOfMeasurePage';

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
