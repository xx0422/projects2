<?php
session_start();
require_once "../../api/admin_check.php";
require_once "../../config/db.php";

$name = $_POST["name"] ?? "";
$desc = $_POST["description"] ?? "";
$price = $_POST["price"] ?? 0;
$stock = $_POST["stock"] ?? 0;
$cat = $_POST["categoryId"] ?? null;

// Beszúrás
$stmt = $pdo->prepare("
    INSERT INTO Products (Name, Description, Price, Stock, CategoryId)
    VALUES (?, ?, ?, ?, ?)
");

$stmt->execute([$name, $desc, $price, $stock, $cat]);

header("Location: ../admin/products.php");
exit;
