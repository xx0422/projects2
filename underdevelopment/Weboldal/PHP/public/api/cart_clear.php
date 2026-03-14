<?php
session_start();
require_once __DIR__ . "/../../config/db.php";

header("Content-Type: application/json");

if (!isset($_SESSION["user_id"])) {
    echo json_encode(["error" => "Not logged in"]);
    exit;
}

$userId = $_SESSION["user_id"];

// kosár ID
$stmt = $pdo->prepare("SELECT Id FROM Cart WHERE UserId = ?");
$stmt->execute([$userId]);
$cartId = $stmt->fetchColumn();

if ($cartId) {
    $pdo->prepare("DELETE FROM CartItems WHERE CartId = ?")->execute([$cartId]);
}

echo json_encode(["success" => true]);
