<?php

session_start();
header('Content-Type: application/json; charset=utf-8');

require_once __DIR__ . "/../../config/db.php";

if (!isset($_SESSION["user_id"])) {
    echo json_encode(["error"=>"Bejelentkezés szükséges"]);
    exit;
}

$data = json_decode(file_get_contents("php://input"), true);

$productId = $data["product_id"] ?? null;
$rating    = $data["rating"] ?? null;
$comment   = trim($data["comment"] ?? "");

if (!$productId || !$rating || $comment === "") {
    echo json_encode(["error" => "Hiányzó adatok"]);
    exit;
}

$rating = (int)$rating;
if ($rating < 1 || $rating > 5) {
    echo json_encode(["error" => "A rating 1 és 5 között legyen"]);
    exit;
}

$userId = $_SESSION["user_id"];

try {

    // user név lekérése
    $stmtU = $pdo->prepare("SELECT Name FROM Users WHERE Id = ?");
    $stmtU->execute([$userId]);
    $user = $stmtU->fetch();

    $userName = $user["Name"] ?? "Felhasználó";

    // review mentése
    $stmt = $pdo->prepare("
        INSERT INTO Reviews (ProductId, UserId, UserName, Rating, Comment)
        VALUES (?, ?, ?, ?, ?)
    ");

    $stmt->execute([
        $productId,
        $userId,
        $userName,
        $rating,
        $comment
    ]);

    echo json_encode(["success" => true]);

} catch (PDOException $e) {
    echo json_encode(["error" => $e->getMessage()]);
}