import React from 'react';
import { Nav } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';

const Sidebar: React.FC = () => {
  return (
    <div className="p-3">
      <h4 className="mb-4">Warehouse Management</h4>
      
      <Nav className="flex-column">
        <h6 className="text-light mb-2">Warehouse</h6>
        <LinkContainer to="/balance">
          <Nav.Link>Balance</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/receipts">
          <Nav.Link>Receipts</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/shipments">
          <Nav.Link>Shipments</Nav.Link>
        </LinkContainer>
        
        <hr className="my-3" style={{ borderColor: 'rgba(255,255,255,0.3)' }} />
        
        <h6 className="text-light mb-2">Reference Data</h6>
        <LinkContainer to="/clients">
          <Nav.Link>Clients</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/units">
          <Nav.Link>Units of Measure</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/resources">
          <Nav.Link>Resources</Nav.Link>
        </LinkContainer>
      </Nav>
    </div>
  );
};

export default Sidebar;
