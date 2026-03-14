<?php
session_start();
require_once "../../api/admin_check.php";
require_once "../../config/db.php";

$id = $_POST["id"] ?? null;
$name = $_POST["name"] ?? "";
$desc = $_POST["description"] ?? "";
$price = $_POST["price"] ?? 0;
$stock = $_POST["stock"] ?? 0;
$cat = $_POST["categoryId"] ?? null;

if (!$id) {
    die("Hiányzó termék ID.");
}

$stmt = $pdo->prepare("
    UPDATE Products
    SET Name = ?, Description = ?, Price = ?, Stock = ?, CategoryId = ?
    WHERE Id = ?
");

$stmt->execute([$name, $desc, $price, $stock, $cat, $id]);

header("Location: ../admin/products.php");
exit;
