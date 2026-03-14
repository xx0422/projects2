<?php
session_start();

if (!isset($_SESSION["user_id"]) || ($_SESSION["role"] ?? "customer") !== "admin") {
    header("Location: /index.php");
    exit;
}

require_once __DIR__ . "/../../config/db.php";

$orderId = $_POST["order_id"] ?? null;
$status = $_POST["status"] ?? null;

$allowed = ["pending", "paid", "processing", "shipped", "completed", "cancelled"];

if (!$orderId || !$status || !in_array($status, $allowed, true)) {
    die("Hibás adatok.");
}

$stmt = $pdo->prepare("
    UPDATE Orders
    SET Status = ?
    WHERE Id = ?
");
$stmt->execute([$status, $orderId]);

header("Location: /admin/admin_order_view.php?id=" . $orderId);
exit;