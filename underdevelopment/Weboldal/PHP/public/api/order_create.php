<?php
header('Content-Type: application/json; charset=utf-8');

require_once __DIR__ . "/../../includes/auth.php";
require_once __DIR__ . "/../../config/db.php";

if (!isset($_SESSION["user_id"])) {
    http_response_code(401);
    echo json_encode(["error" => "Bejelentkezés szükséges"]);
    exit;
}

$userId = $_SESSION["user_id"];

$data = json_decode(file_get_contents("php://input"), true);

$shippingMethod = $data["shipping_method"] ?? null;
$paymentMethod  = $data["payment_method"] ?? null;

$billingName    = trim($data["billing_name"] ?? "");
$billingZip     = trim($data["billing_zip"] ?? "");
$billingCity    = trim($data["billing_city"] ?? "");
$billingAddress = trim($data["billing_address"] ?? "");

$shippingName    = trim($data["shipping_name"] ?? "");
$shippingZip     = trim($data["shipping_zip"] ?? "");
$shippingCity    = trim($data["shipping_city"] ?? "");
$shippingAddress = trim($data["shipping_address"] ?? "");

if (
    !$shippingMethod || !$paymentMethod ||
    $billingName === "" || $billingZip === "" || $billingCity === "" || $billingAddress === "" ||
    $shippingName === "" || $shippingZip === "" || $shippingCity === "" || $shippingAddress === ""
) {
    http_response_code(400);
    echo json_encode(["error" => "Hiányzó adatok"]);
    exit;
}

// kosár
$stmt = $pdo->prepare("SELECT Id FROM Cart WHERE UserId = ?");
$stmt->execute([$userId]);
$cart = $stmt->fetch();

if (!$cart) {
    http_response_code(400);
    echo json_encode(['error' => 'A kosár üres']);
    exit;
}

$cartId = (int)$cart['Id'];

$stmt = $pdo->prepare("
    SELECT ci.ProductId, ci.Quantity, p.Price, p.Stock, p.Name
    FROM CartItems ci
    JOIN Products p ON ci.ProductId = p.Id
    WHERE ci.CartId = ?
");
$stmt->execute([$cartId]);
$items = $stmt->fetchAll();

if (!$items) {
    http_response_code(400);
    echo json_encode(['error' => 'A kosár üres']);
    exit;
}

// összeg + készlet ellenőrzés
$total = 0;
foreach ($items as $item) {
    if ($item['Stock'] < $item['Quantity']) {
        http_response_code(400);
        echo json_encode([
            'error' => 'Nincs elég készlet ebből: ' . $item['Name']
        ]);
        exit;
    }
    $total += $item['Price'] * $item['Quantity'];
}

try {
    $pdo->beginTransaction();

    // rendelés beszúrása
    $stmt = $pdo->prepare("
        INSERT INTO Orders (
            UserId, Total, Status, CreatedAt,
            ShippingMethod, PaymentMethod,
            BillingName, BillingZip, BillingCity, BillingAddress,
            ShippingName, ShippingZip, ShippingCity, ShippingAddress
        )
        VALUES (
            ?, ?, 'pending', GETDATE(),
            ?, ?,
            ?, ?, ?, ?,
            ?, ?, ?, ?
        )
    ");

    $stmt->execute([
        $userId,
        $total,

        $shippingMethod,
        $paymentMethod,

        $billingName,
        $billingZip,
        $billingCity,
        $billingAddress,

        $shippingName,
        $shippingZip,
        $shippingCity,
        $shippingAddress
    ]);

    $orderId = (int)$pdo->lastInsertId();

    // order_items + készlet csökkentése
    $stmtItem = $pdo->prepare("
        INSERT INTO OrderItems (OrderId, ProductId, Price, Quantity)
        VALUES (?, ?, ?, ?)
    ");

    $stmtStock = $pdo->prepare("
        UPDATE Products
        SET Stock = Stock - ?
        WHERE Id = ?
    ");

    foreach ($items as $item) {
        $stmtItem->execute([
            $orderId,
            $item['ProductId'],
            $item['Price'],
            $item['Quantity']
        ]);

        $stmtStock->execute([
            $item['Quantity'],
            $item['ProductId']
        ]);
    }

    // kosár ürítése
    $del = $pdo->prepare("DELETE FROM CartItems WHERE CartId = ?");
    $del->execute([$cartId]);

    $pdo->commit();

    echo json_encode([
        'success' => true,
        'order_id' => $orderId
    ]);

} catch (Exception $e) {
    if ($pdo->inTransaction()) {
        $pdo->rollBack();
    }

    http_response_code(500);
    echo json_encode([
        'error' => 'Hiba a rendelés mentésekor',
        'details' => $e->getMessage()
    ]);
}