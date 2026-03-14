<?php
// FONTOS: első sor! NEM LEHET ELŐTTE ÜRES SOR!
ob_clean();
header("Content-Type: application/json; charset=utf-8");

ini_set('session.use_cookies', 1);
ini_set('session.use_only_cookies', 1);
ini_set('session.cookie_samesite', 'Lax');

session_start();
require_once __DIR__ . "/../../config/db.php";

$data = json_decode(file_get_contents("php://input"), true);

$email = $data["email"] ?? null;
$password = $data["password"] ?? null;

if (!$email || !$password) {
    echo json_encode(["error" => "Email and password required"]);
    exit;
}

// User lookup
$stmt = $pdo->prepare("SELECT * FROM Users WHERE Email = ?");
$stmt->execute([$email]);
$user = $stmt->fetch();

if (!$user || !password_verify($password, $user["PasswordHash"])) {
    echo json_encode(["error" => "Invalid email or password"]);
    exit;
}

// Login ok → session mentés
$_SESSION["user_id"] = $user["Id"];
$_SESSION["role"] = $user["Role"];   // *** EZ KELL HOZZÁ !


// COOKIE KIKÜLDÉSE
header("Set-Cookie: PHPSESSID=" . session_id() . "; Path=/; HttpOnly");

// JSON válasz
echo json_encode([
    "success" => true,
    "message" => "Login successful",
    "user" => [
        "id" => $user["Id"],
        "name" => $user["Name"],
        "email" => $user["Email"],
        "role" => $user["Role"]   // <<< EZ HIÁNYZOTT
    ]
]);


session_write_close();
