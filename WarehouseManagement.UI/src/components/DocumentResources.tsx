import React from 'react';
import DocumentResourcesSelector from './DocumentResourcesSelector';
import type { DocumentResourceItem } from './DocumentResourcesSelector';

interface DocumentResourcesProps {
  resources: DocumentResourceItem[];
  onResourcesChange: (resources: DocumentResourceItem[]) => void;
  disabled?: boolean;
  mode?: 'receipt' | 'shipment';
}

const DocumentResources: React.FC<DocumentResourcesProps> = ({ 
  resources, 
  onResourcesChange, 
  disabled = false,
  mode = 'receipt'
}) => {
  return (
    <DocumentResourcesSelector
      resources={resources}
      onResourcesChange={onResourcesChange}
      disabled={disabled}
      mode={mode}
    />
  );
};

export default DocumentResources;
export type { DocumentResourceItem };