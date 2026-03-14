<?php
// PHP/api/admin_product_new.php
header("Content-Type: application/json; charset=utf-8");

require_once __DIR__ . "/../../includes/auth.php";
require_once __DIR__ . "/../../config/db.php";

// Csak admin
if (!isset($_SESSION["role"]) || $_SESSION["role"] !== "admin") {
    echo json_encode(["error" => "Nincs jogosultság"]);
    exit;
}

// POST adatok
$name        = $_POST["name"]        ?? null;
$description = $_POST["description"] ?? null;
$price       = $_POST["price"]       ?? null;
$stock       = $_POST["stock"]       ?? null;
$categoryId  = $_POST["categoryId"]  ?? null;

if (!$name || !$price || !$stock) {
    echo json_encode(["error" => "Hiányzó adatok"]);
    exit;
}

try {
    // Termék adatainak mentése
    $stmt = $pdo->prepare("
        INSERT INTO Products (Name, Description, Price, Stock, CategoryId)
        VALUES (?, ?, ?, ?, ?)
    ");
    $stmt->execute([$name, $description, $price, $stock, $categoryId]);

    $productId = $pdo->lastInsertId();

    // Ha érkezett kép
    if (!empty($_FILES["image"]["name"])) {

        $image = $_FILES["image"];
        $ext = strtolower(pathinfo($image["name"], PATHINFO_EXTENSION));
        $allowed = ["jpg", "jpeg", "png", "webp"];

        if (!in_array($ext, $allowed)) {
            echo json_encode(["error" => "Csak JPG, PNG vagy WEBP engedélyezett"]);
            exit;
        }

        // fájlnév
        $newName = "p_" . $productId . "_" . time() . "." . $ext;

        // relatív út az adatbázisba
        $relPath = "uploads/" . $newName;

        // abszolút út a mozgatáshoz
        $dest = __DIR__ . "/../../public/" . $relPath;

        // kép mentése
        if (!move_uploaded_file($image["tmp_name"], $dest)) {
            echo json_encode(["error" => "A kép mentése nem sikerült"]);
            exit;
        }

        // adatbázisba mentés
        $stmt = $pdo->prepare("
            INSERT INTO ProductImages (ProductId, ImageUrl, IsMain)
            VALUES (?, ?, 1)
        ");
        $stmt->execute([$productId, $relPath]);
    }

    echo json_encode([
        "success" => true,
        "productId" => $productId
    ]);

} catch (PDOException $e) {
    echo json_encode(["error" => $e->getMessage()]);
}
