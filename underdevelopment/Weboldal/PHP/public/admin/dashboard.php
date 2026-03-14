<?php
session_start();
require_once "../../api/admin_check.php";
?>

<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Admin dashboard</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body class="bg-light">

<nav class="navbar navbar-dark bg-dark mb-4">
    <div class="container">
        <span class="navbar-brand">Admin vezérlőpult</span>
        <a href="logout.php" class="btn btn-danger">Kijelentkezés</a>
    </div>
</nav>

<div class="container">
    <h2>Admin lehetőségek</h2>

    <ul class="list-group mt-4">
        <a href="products.php" class="list-group-item list-group-item-action">Termékek kezelése</a>
        <a href="categories.php" class="list-group-item list-group-item-action">Kategóriák kezelése</a>
    </ul>
</div>

</body>
</html>
