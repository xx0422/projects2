<?php
session_start();
require_once "../../config/db.php";

if ($_SERVER["REQUEST_METHOD"] === "POST") {
    $email = $_POST["email"] ?? "";
    $pass = $_POST["password"] ?? "";

    // Felhasználó lekérdezése
    $stmt = $pdo->prepare("SELECT * FROM Users WHERE Email = ?");
    $stmt->execute([$email]);
    $user = $stmt->fetch();

    if ($user && password_verify($pass, $user["PasswordHash"])) {

        if ($user["Role"] !== "admin") {
            $error = "Nincs jogosultságod az admin felülethez.";
        } else {
            $_SESSION["admin_id"] = $user["Id"];
            header("Location: dashboard.php");
            exit;
        }
    } else {
        $error = "Hibás email vagy jelszó.";
    }
}
?>

<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="UTF-8">
    <title>Admin login</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body class="bg-light">
<div class="container mt-5" style="max-width: 400px;">
    <div class="card p-4 shadow">
        <h3 class="mb-3">Admin belépés</h3>

        <?php if (isset($error)): ?>
            <div class="alert alert-danger"><?= $error ?></div>
        <?php endif; ?>

        <form method="POST">
            <input name="email" class="form-control mb-2" placeholder="Email">
            <input name="password" type="password" class="form-control mb-2" placeholder="Jelszó">
            <button class="btn btn-primary w-100">Belépés</button>
        </form>
    </div>
</div>
</body>
</html>
