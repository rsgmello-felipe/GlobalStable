-- Inserindo os status de ordem
INSERT INTO ref_order_status (id, status)
VALUES (1, 'CREATED'),
       (2, 'PENDING_DEPOSIT'),
       (3, 'PENDING_APPROVAL'),
       (4, 'SENT_TO_CONNECTOR'),
       (5, 'PENDING_IN_BANK'),
       (6, 'PROCESSING'),
       (7, 'COMPLETED'),
       (8, 'FAILED'),
       (9, 'CANCELLED'),
       (10, 'PROCESSING_REFUND'),
       (11, 'RETURNED'),
       (12, 'EXPIRED'),
       (13, 'BLOCKED'),
       (14, 'PENDING_TREASURY'),
       (15, 'REJECTED') ON CONFLICT (id) DO NOTHING;
