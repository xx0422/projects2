<?php
// PHP/api/products.php
require_once __DIR__ . "/../config/db.php";

header('Content-Type: application/json; charset=utf-8');

$stmt = $pdo->query("
    SELECT p.Id, p.Name, p.Price, p.Stock, p.CategoryId,
           c.Name AS Category
    FROM Products p
    LEFT JOIN Categories c ON p.CategoryId = c.Id
");

echo json_encode($stmt->fetchAll());
