<?php
// PHP/api/admin_product_delete.php
header("Content-Type: application/json; charset=utf-8");

require_once __DIR__ . "/../../includes/auth.php";
require_once __DIR__ . "/../../config/db.php";


if (!isset($_SESSION["role"]) || $_SESSION["role"] !== "admin") {
    echo json_encode(["error" => "Nincs jogosultság"]);
    exit;
}

$data = json_decode(file_get_contents("php://input"), true);
$id = $data["id"] ?? null;

if (!$id) {
    echo json_encode(["error" => "Hiányzó termék ID"]);
    exit;
}

try {
    // régi képek törlése
    $stmt = $pdo->prepare("SELECT ImageUrl FROM ProductImages WHERE ProductId = ?");
    $stmt->execute([$id]);
    $imgs = $stmt->fetchAll();

    foreach ($imgs as $img) {
        $path = __DIR__ . "/../public/" . $img["ImageUrl"];
        if (is_file($path)) {
            @unlink($path);
        }
    }

    // képek törlése DB-ből
    $pdo->prepare("DELETE FROM ProductImages WHERE ProductId = ?")->execute([$id]);

    // termék törlése
    $pdo->prepare("DELETE FROM Products WHERE Id = ?")->execute([$id]);

    echo json_encode(["success" => true]);

} catch (PDOException $e) {
    echo json_encode(["error" => $e->getMessage()]);
}
