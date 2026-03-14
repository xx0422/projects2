
ALTER TABLE Orders
ADD ShippingMethod NVARCHAR(50) NULL,
    PaymentMethod NVARCHAR(50) NULL,
    BillingName NVARCHAR(100) NULL,
    BillingZip NVARCHAR(20) NULL,
    BillingCity NVARCHAR(100) NULL,
    BillingAddress NVARCHAR(255) NULL,
    ShippingName NVARCHAR(100) NULL,
    ShippingZip NVARCHAR(20) NULL,
    ShippingCity NVARCHAR(100) NULL,
    ShippingAddress NVARCHAR(255) NULL;





