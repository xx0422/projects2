<?php
header("Content-Type: application/json; charset=utf-8");

session_start();
require_once __DIR__ . "/../../config/db.php";

if (!isset($_SESSION["role"]) || $_SESSION["role"] !== "admin") {
    echo json_encode(["error" => "Nincs jogosultság"]);
    exit;
}

$data = json_decode(file_get_contents("php://input"), true);

$id = $data["id"] ?? null;

if (!$id) {
    echo json_encode(["error" => "Hiányzó review ID"]);
    exit;
}

try {

    $stmt = $pdo->prepare("
        DELETE FROM Reviews
        WHERE Id = ?
    ");

    $stmt->execute([$id]);

    echo json_encode(["success" => true]);

} catch(PDOException $e){

    echo json_encode(["error" => $e->getMessage()]);

}