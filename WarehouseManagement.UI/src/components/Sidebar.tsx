import React from 'react';
import { Nav } from 'react-bootstrap';
import { LinkContainer } from 'react-router-bootstrap';

const Sidebar: React.FC = () => {
  return (
    <div className="p-3">
      <h4 className="mb-4">Управлене складом</h4>
      
      <Nav className="flex-column">
        <h6 className="text-light mb-2">Склад</h6>
        <LinkContainer to="/balance">
          <Nav.Link>Баланс</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/receipts">
          <Nav.Link>Поступления</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/shipments">
          <Nav.Link>Отгрузки</Nav.Link>
        </LinkContainer>
        
        <hr className="my-3" style={{ borderColor: 'rgba(255,255,255,0.3)' }} />
        
        <h6 className="text-light mb-2">Справочники</h6>
        <LinkContainer to="/clients">
          <Nav.Link>Клиенты</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/units">
          <Nav.Link>Единицы измерения</Nav.Link>
        </LinkContainer>
        <LinkContainer to="/resources">
          <Nav.Link>Ресурсы</Nav.Link>
        </LinkContainer>
      </Nav>
    </div>
  );
};

export default Sidebar;
