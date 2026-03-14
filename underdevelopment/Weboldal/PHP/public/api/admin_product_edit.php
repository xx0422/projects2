<?php
// PHP/api/admin_product_edit.php
header("Content-Type: application/json; charset=utf-8");

require_once __DIR__ . "/../../includes/auth.php";
require_once __DIR__ . "/../../config/db.php";

// Csak admin
if (!isset($_SESSION["role"]) || $_SESSION["role"] !== "admin") {
    echo json_encode(["error" => "Nincs jogosultság"]);
    exit;
}

// POST adatok
$id          = $_POST["id"]          ?? null;
$name        = $_POST["name"]        ?? null;
$description = $_POST["description"] ?? null;
$price       = $_POST["price"]       ?? null;
$stock       = $_POST["stock"]       ?? null;
$categoryId  = $_POST["categoryId"]  ?? null;

if (!$id || !$name || !$price || !$stock) {
    echo json_encode(["error" => "Hiányzó adatok"]);
    exit;
}

try {
    // Termék frissítése
    $stmt = $pdo->prepare("
        UPDATE Products
        SET Name = ?, Description = ?, Price = ?, Stock = ?, CategoryId = ?
        WHERE Id = ?
    ");
    $stmt->execute([$name, $description, $price, $stock, $categoryId, $id]);

    // Ha van új kép
    if (!empty($_FILES["image"]["name"])) {

        // Régi képek törlése fájlból + adatbázisból
        $stmtImg = $pdo->prepare("SELECT ImageUrl FROM ProductImages WHERE ProductId = ?");
        $stmtImg->execute([$id]);

        foreach ($stmtImg->fetchAll() as $img) {
            $path = __DIR__ . "/../../public/" . $img["ImageUrl"];
            if (is_file($path)) {
                @unlink($path);
            }
        }

        // DB törlés
        $pdo->prepare("DELETE FROM ProductImages WHERE ProductId = ?")->execute([$id]);

        // Új kép mentése
        $image = $_FILES["image"];
        $ext = strtolower(pathinfo($image["name"], PATHINFO_EXTENSION));
        $allowed = ["jpg", "jpeg", "png", "webp"];

        if (!in_array($ext, $allowed)) {
            echo json_encode(["error" => "Csak JPG, PNG vagy WEBP engedélyezett"]);
            exit;
        }

        // új fájlnév
        $newName = "p_" . $id . "_" . time() . "." . $ext;
        $relPath = "uploads/" . $newName;
        $dest = __DIR__ . "/../../public/" . $relPath;

        if (!move_uploaded_file($image["tmp_name"], $dest)) {
            echo json_encode(["error" => "A kép mentése nem sikerült"]);
            exit;
        }

        // Kép adatbázisba
        $stmt = $pdo->prepare("
            INSERT INTO ProductImages (ProductId, ImageUrl, IsMain)
            VALUES (?, ?, 1)
        ");
        $stmt->execute([$id, $relPath]);
    }

    echo json_encode(["success" => true]);

} catch (PDOException $e) {
    echo json_encode(["error" => $e->getMessage()]);
}
