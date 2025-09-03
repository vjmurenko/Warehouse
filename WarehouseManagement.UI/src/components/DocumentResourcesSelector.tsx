import React from 'react';
import ReceiptResources, { ReceiptResourceItem } from './ReceiptResources';
import ShipmentResourcesTable, { ShipmentResourceItem } from './ShipmentResourcesTable';

// Legacy interface for backward compatibility
export interface DocumentResourceItem {
  id?: string;
  resourceId: string;
  resourceName?: string;
  unitId: string;
  unitName?: string;
  quantity: number;
  availableQuantity?: number;
}

interface DocumentResourcesSelectorProps {
  resources: DocumentResourceItem[];
  onResourcesChange: (resources: DocumentResourceItem[]) => void;
  disabled?: boolean;
  mode: 'receipt' | 'shipment';
}

/**
 * Legacy wrapper component - use ReceiptResources or ShipmentResourcesTable directly instead
 * @deprecated Use ReceiptResources or ShipmentResourcesTable components directly
 */
const DocumentResourcesSelector: React.FC<DocumentResourcesSelectorProps> = ({ 
  resources, 
  onResourcesChange, 
  disabled = false,
  mode = 'receipt'
}) => {
  if (mode === 'shipment') {
    return (
      <ShipmentResourcesTable
        resources={resources as ShipmentResourceItem[]}
        onResourcesChange={onResourcesChange as (resources: ShipmentResourceItem[]) => void}
        disabled={disabled}
      />
    );
  } else {
    return (
      <ReceiptResources
        resources={resources as ReceiptResourceItem[]}
        onResourcesChange={onResourcesChange as (resources: ReceiptResourceItem[]) => void}
        disabled={disabled}
      />
    );
  }
};

export default DocumentResourcesSelector;