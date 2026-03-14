<?php
session_start();
require_once __DIR__ . "/../../config/db.php";

header("Content-Type: application/json");

if (!isset($_SESSION["user_id"])) {
    echo json_encode(["error" => "Not logged in"]);
    exit;
}

$data = json_decode(file_get_contents("php://input"), true);
$productId = $data["product_id"] ?? null;
$qty = $data["qty"] ?? 1;

$userId = $_SESSION["user_id"];

// Kosár ID lekérése vagy létrehozása
$stmt = $pdo->prepare("SELECT Id FROM Cart WHERE UserId = ?");
$stmt->execute([$userId]);
$cartId = $stmt->fetchColumn();

if (!$cartId) {
    $pdo->prepare("INSERT INTO Cart (UserId) VALUES (?)")->execute([$userId]);
    $cartId = $pdo->lastInsertId();
}

// Van már ilyen tétel?
$stmt = $pdo->prepare("SELECT Id, Quantity FROM CartItems WHERE CartId = ? AND ProductId = ?");
$stmt->execute([$cartId, $productId]);
$item = $stmt->fetch();

if ($item) {
    $newQty = $item["Quantity"] + $qty;
    $pdo->prepare("UPDATE CartItems SET Quantity = ? WHERE Id = ?")
        ->execute([$newQty, $item["Id"]]);
} else {
    $pdo->prepare("INSERT INTO CartItems (CartId, ProductId, Quantity) VALUES (?, ?, ?)")
        ->execute([$cartId, $productId, $qty]);
}

echo json_encode(["success" => true, "message" => "Added to cart"]);
