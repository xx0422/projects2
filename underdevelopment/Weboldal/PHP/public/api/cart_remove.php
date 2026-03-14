<?php
session_start();
require_once __DIR__ . "/../../config/db.php";

header("Content-Type: application/json");

if (!isset($_SESSION["user_id"])) {
    echo json_encode(["error" => "Not logged in"]);
    exit;
}

$data = json_decode(file_get_contents("php://input"), true);
$itemId = $data["cart_item_id"] ?? null;

$stmt = $pdo->prepare("DELETE FROM CartItems WHERE Id = ?");
$stmt->execute([$itemId]);

echo json_encode(["success" => true]);
