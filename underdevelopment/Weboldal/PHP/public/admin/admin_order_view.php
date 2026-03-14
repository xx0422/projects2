<?php
session_start();

if (!isset($_SESSION["user_id"]) || ($_SESSION["role"] ?? "customer") !== "admin") {
    header("Location: index.php");
    exit;
}

require_once __DIR__ . "/../../config/db.php";

$id = $_GET["id"] ?? null;

if (!$id) {
    die("Hiányzó rendelés ID.");
}

$stmt = $pdo->prepare("
    SELECT 
        o.*,
        u.Name AS UserName,
        u.Email AS UserEmail
    FROM Orders o
    LEFT JOIN Users u ON u.Id = o.UserId
    WHERE o.Id = ?
");
$stmt->execute([$id]);
$order = $stmt->fetch();

if (!$order) {
    die("A rendelés nem található.");
}

$stmtItems = $pdo->prepare("
    SELECT 
        oi.*,
        p.Name AS ProductName
    FROM OrderItems oi
    LEFT JOIN Products p ON p.Id = oi.ProductId
    WHERE oi.OrderId = ?
");
$stmtItems->execute([$id]);
$items = $stmtItems->fetchAll();
?>
<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="UTF-8">
    <title>Rendelés #<?= $order["Id"] ?></title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body class="bg-light">

<div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h1>Rendelés #<?= $order["Id"] ?></h1>
        <a href="/admin/admin_orders.php" class="btn btn-outline-secondary">Vissza a rendelésekhez</a>
    </div>

    <div class="row">
        <div class="col-md-6">
            <div class="card p-3 mb-3">
                <h4>Vevő</h4>
                <p><b>Név:</b> <?= htmlspecialchars($order["UserName"] ?? "Ismeretlen") ?></p>
                <p><b>Email:</b> <?= htmlspecialchars($order["UserEmail"] ?? "-") ?></p>
                <p><b>Dátum:</b> <?= htmlspecialchars($order["CreatedAt"]) ?></p>
                <p><b>Státusz:</b> <?= htmlspecialchars($order["Status"]) ?></p>
                <p><b>Végösszeg:</b> <?= number_format((float)$order["Total"], 0, ",", " ") ?> Ft</p>
            </div>

            <div class="card p-3 mb-3">
                <h4>Szállítás / fizetés</h4>
                <p><b>Szállítási mód:</b> <?= htmlspecialchars($order["ShippingMethod"] ?? "-") ?></p>
                <p><b>Fizetési mód:</b> <?= htmlspecialchars($order["PaymentMethod"] ?? "-") ?></p>
            </div>

            <div class="card p-3 mb-3">
                <h4>Számlázási cím</h4>
                <p><?= htmlspecialchars($order["BillingName"] ?? "") ?></p>
                <p><?= htmlspecialchars($order["BillingZip"] ?? "") ?> <?= htmlspecialchars($order["BillingCity"] ?? "") ?></p>
                <p><?= htmlspecialchars($order["BillingAddress"] ?? "") ?></p>
            </div>

            <div class="card p-3 mb-3">
                <h4>Szállítási cím</h4>
                <p><?= htmlspecialchars($order["ShippingName"] ?? "") ?></p>
                <p><?= htmlspecialchars($order["ShippingZip"] ?? "") ?> <?= htmlspecialchars($order["ShippingCity"] ?? "") ?></p>
                <p><?= htmlspecialchars($order["ShippingAddress"] ?? "") ?></p>
            </div>
        </div>

        <div class="col-md-6">
            <div class="card p-3 mb-3">
                <h4>Rendelt tételek</h4>

                <?php if (!$items): ?>
                    <div class="alert alert-warning">Nincs rendelési tétel.</div>
                <?php else: ?>
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <th>Termék</th>
                                <th>Ár</th>
                                <th>Mennyiség</th>
                                <th>Összesen</th>
                            </tr>
                        </thead>
                        <tbody>
                            <?php foreach ($items as $item): ?>
                                <tr>
                                    <td><?= htmlspecialchars($item["ProductName"] ?? "Törölt termék") ?></td>
                                    <td><?= number_format((float)$item["Price"], 0, ",", " ") ?> Ft</td>
                                    <td><?= (int)$item["Quantity"] ?></td>
                                    <td><?= number_format((float)$item["Price"] * (int)$item["Quantity"], 0, ",", " ") ?> Ft</td>
                                </tr>
                            <?php endforeach; ?>
                        </tbody>
                    </table>
                <?php endif; ?>
            </div>

            <div class="card p-3">
                <h4>Státusz módosítása</h4>

                <form action="/api/admin_order_status_update.php" method="POST">
                    <input type="hidden" name="order_id" value="<?= $order["Id"] ?>">

                    <select name="status" class="form-select mb-3">
                        <option value="pending" <?= $order["Status"] === "pending" ? "selected" : "" ?>>pending</option>
                        <option value="paid" <?= $order["Status"] === "paid" ? "selected" : "" ?>>paid</option>
                        <option value="processing" <?= $order["Status"] === "processing" ? "selected" : "" ?>>processing</option>
                        <option value="shipped" <?= $order["Status"] === "shipped" ? "selected" : "" ?>>shipped</option>
                        <option value="completed" <?= $order["Status"] === "completed" ? "selected" : "" ?>>completed</option>
                        <option value="cancelled" <?= $order["Status"] === "cancelled" ? "selected" : "" ?>>cancelled</option>
                    </select>

                    <button class="btn btn-success">Mentés</button>
                </form>
            </div>
        </div>
    </div>
</div>

</body>
</html>