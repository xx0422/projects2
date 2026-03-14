<?php
session_start();
require_once "../includes/auth.php";
require_once "../config/db.php";

if ($_SESSION["role"] !== "admin") {
    die("Nincs jogosultság");
}

$name = $_POST["name"];
$description = $_POST["description"] ?? null;

$imagePath = null;

if (!empty($_FILES["image"]["name"])) {

    $ext = strtolower(pathinfo($_FILES["image"]["name"], PATHINFO_EXTENSION));
    $allowed = ["jpg","jpeg","png","webp"];

    if (!in_array($ext, $allowed)) {
        die("Csak JPG/PNG/WEBP kép tölthető fel");
    }

    $newName = "cat_" . time() . "." . $ext;
    $relPath = "uploads/categories/" . $newName;
    $dest = __DIR__ . "/../public/" . $relPath;

    if (!move_uploaded_file($_FILES["image"]["tmp_name"], $dest)) {
        die("Kép mentése nem sikerült");
    }

    $imagePath = $relPath;
}

$stmt = $pdo->prepare("
    INSERT INTO Categories (Name, Description, ImageUrl)
    VALUES (?, ?, ?)
");

$stmt->execute([$name, $description, $imagePath]);

header("Location: ../public/admin/categories.php");
exit;