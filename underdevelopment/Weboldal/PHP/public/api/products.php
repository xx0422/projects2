<?php
require_once __DIR__ . "/../../config/db.php";
header('Content-Type: application/json; charset=utf-8');

$stmt = $pdo->query("
    SELECT 
        p.Id, p.Name, p.Description, p.Price, p.Stock, p.CategoryId,
        c.Name AS Category,
        (SELECT TOP 1 ImageUrl FROM ProductImages WHERE ProductId = p.Id ORDER BY IsMain DESC, Id DESC) AS ImageUrl
    FROM Products p
    LEFT JOIN Categories c ON p.CategoryId = c.Id
");

echo json_encode($stmt->fetchAll());