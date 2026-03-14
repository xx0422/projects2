<?php
session_start();
require_once "../includes/auth.php";
require_once "../config/db.php";

if ($_SESSION["role"] !== "admin") {
    die("Nincs jogosultság");
}

$id = $_POST["id"];
$name = $_POST["name"];
$description = $_POST["description"];

$imagePath = null;

if (!empty($_FILES["image"]["name"])) {

    $ext = strtolower(pathinfo($_FILES["image"]["name"], PATHINFO_EXTENSION));
    $allowed = ["jpg","jpeg","png","webp"];

    if (!in_array($ext, $allowed)) {
        die("Csak JPG/PNG/WEBP kép tölthető fel");
    }

    $newName = "cat_" . $id . "_" . time() . "." . $ext;
    $relPath = "uploads/categories/" . $newName;
    $dest = __DIR__ . "/../public/" . $relPath;

    move_uploaded_file($_FILES["image"]["tmp_name"], $dest);

    $stmt = $pdo->prepare("
        UPDATE Categories
        SET Name=?, Description=?, ImageUrl=?
        WHERE Id=?
    ");
    $stmt->execute([$name, $description, $relPath, $id]);

} else {

    $stmt = $pdo->prepare("
        UPDATE Categories
        SET Name=?, Description=?
        WHERE Id=?
    ");
    $stmt->execute([$name, $description, $id]);

}

header("Location: ../public/admin/categories.php");
exit;