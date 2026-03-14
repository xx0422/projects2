<?php
header("Content-Type: application/json; charset=utf-8");

require_once __DIR__ . "/../../config/db.php";
require_once __DIR__ . "/../../includes/mailer.php";

$data = json_decode(file_get_contents("php://input"), true);

$name = trim($data["name"] ?? "");
$email = trim($data["email"] ?? "");
$password = $data["password"] ?? "";

if ($name === "" || $email === "" || $password === "") {
    echo json_encode(["error" => "Minden mező kötelező!"]);
    exit;
}

try {
    $passwordHash = password_hash($password, PASSWORD_DEFAULT);

    $stmt = $pdo->prepare("
        INSERT INTO Users (Name, Email, PasswordHash, Role)
        VALUES (?, ?, ?, 'customer')
    ");
    $stmt->execute([$name, $email, $passwordHash]);

    $mailSent = sendRegistrationSuccessEmail($email, $name);

    echo json_encode([
        "success" => true,
        "mailSent" => $mailSent
    ]);

} catch (PDOException $e) {
    echo json_encode(["error" => $e->getMessage()]);
}
