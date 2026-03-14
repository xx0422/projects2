<?php
session_start();
require_once "../../api/admin_check.php";
require_once "../../config/db.php";

$id = $_GET["id"];

// FIGYELEM → a termékek CategoryId-je NULL lesz
$pdo->prepare("UPDATE Products SET CategoryId = NULL WHERE CategoryId = ?")->execute([$id]);

$pdo->prepare("DELETE FROM Categories WHERE Id = ?")->execute([$id]);

header("Location: categories.php");
exit;
