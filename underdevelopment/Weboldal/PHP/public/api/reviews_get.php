<?php
header('Content-Type: application/json; charset=utf-8');

require_once __DIR__ . "/../../config/db.php";

$productId = $_GET["product_id"] ?? null;
if (!$productId) {
    echo json_encode(["error" => "Hiányzó product_id"]);
    exit;
}

try {
    // Átlag + darab
    $stmt = $pdo->prepare("
        SELECT 
            CAST(AVG(CAST(Rating AS FLOAT)) AS FLOAT) AS AvgRating,
            COUNT(*) AS CountRating
        FROM Reviews
        WHERE ProductId = ?
    ");
    $stmt->execute([$productId]);
    $stats = $stmt->fetch();

    // Lista
    $stmt2 = $pdo->prepare("
        SELECT TOP 50
            r.Id, r.Rating, r.Comment, r.CreatedAt,
            COALESCE(u.Name, r.UserName, 'Vendég') AS DisplayName
        FROM Reviews r
        LEFT JOIN Users u ON u.Id = r.UserId
        WHERE r.ProductId = ?
        ORDER BY r.CreatedAt DESC
    ");
    $stmt2->execute([$productId]);
    $items = $stmt2->fetchAll();

    echo json_encode([
        "success" => true,
        "stats" => [
            "avg" => $stats["AvgRating"] ? round((float)$stats["AvgRating"], 2) : 0,
            "count" => (int)$stats["CountRating"]
        ],
        "items" => $items
    ]);

} catch (PDOException $e) {
    echo json_encode(["error" => $e->getMessage()]);
}