<?php
session_start();

if (!isset($_SESSION["user_id"]) || ($_SESSION["role"] ?? "customer") !== "admin") {
    header("Location: index.php");
    exit;
}

require_once __DIR__ . "/../../config/db.php";

$stmt = $pdo->query("
    SELECT 
        o.Id,
        o.Total,
        o.Status,
        o.CreatedAt,
        o.ShippingMethod,
        o.PaymentMethod,
        u.Name AS UserName,
        u.Email AS UserEmail
    FROM Orders o
    LEFT JOIN Users u ON u.Id = o.UserId
    ORDER BY o.CreatedAt DESC
");

$orders = $stmt->fetchAll();
?>
<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="UTF-8">
    <title>Rendelések</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body class="bg-light">

<div class="container mt-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h1 class="mb-0">Rendelések</h1>
            <small>Admin rendeléskezelő</small>
        </div>
        <div>
            <a href="/admin.php" class="btn btn-outline-secondary me-2">Vissza az adminhoz</a>
            <a href="/" class="btn btn-outline-primary">Felhasználói oldal</a>
        </div>
    </div>

    <hr>

    <?php if (!$orders): ?>
        <div class="alert alert-info">Még nincs rendelés.</div>
    <?php else: ?>
        <table class="table table-bordered table-striped align-middle">
            <thead class="table-dark">
                <tr>
                    <th>Rendelés ID</th>
                    <th>Vevő</th>
                    <th>Email</th>
                    <th>Összeg</th>
                    <th>Szállítás</th>
                    <th>Fizetés</th>
                    <th>Státusz</th>
                    <th>Dátum</th>
                    <th>Műveletek</th>
                </tr>
            </thead>
            <tbody>
                <?php foreach ($orders as $o): ?>
                    <tr>
                        <td>#<?= $o["Id"] ?></td>
                        <td><?= htmlspecialchars($o["UserName"] ?? "Ismeretlen") ?></td>
                        <td><?= htmlspecialchars($o["UserEmail"] ?? "-") ?></td>
                        <td><?= number_format((float)$o["Total"], 0, ",", " ") ?> Ft</td>
                        <td><?= htmlspecialchars($o["ShippingMethod"] ?? "-") ?></td>
                        <td><?= htmlspecialchars($o["PaymentMethod"] ?? "-") ?></td>
                        <td>
                            <span class="badge bg-secondary"><?= htmlspecialchars($o["Status"]) ?></span>
                        </td>
                        <td><?= htmlspecialchars($o["CreatedAt"]) ?></td>
                        <td>
                                    <a href="/admin/admin_order_view.php?id=<?= $o["Id"] ?>" class="btn btn-sm btn-primary">
                                    Részletek
                            </a>
                        </td>
                    </tr>
                <?php endforeach; ?>
            </tbody>
        </table>
    <?php endif; ?>
</div>

</body>
</html>