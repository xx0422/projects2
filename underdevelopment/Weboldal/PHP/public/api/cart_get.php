<?php
session_start();
require_once __DIR__ . "/../../config/db.php";

header("Content-Type: application/json");

if (!isset($_SESSION["user_id"])) {
    echo json_encode(["error" => "Not logged in"]);
    exit;
}

$userId = $_SESSION["user_id"];

// Kosár ID
$stmt = $pdo->prepare("SELECT Id FROM Cart WHERE UserId = ?");
$stmt->execute([$userId]);
$cartId = $stmt->fetchColumn();

if (!$cartId) {
    echo json_encode(["items" => []]);
    exit;
}

$stmt = $pdo->prepare("
    SELECT ci.Id AS CartItemId, p.Name, p.Price, ci.Quantity 
    FROM CartItems ci
    JOIN Products p ON p.Id = ci.ProductId
    WHERE ci.CartId = ?
");
$stmt->execute([$cartId]);

echo json_encode(["items" => $stmt->fetchAll()]);
