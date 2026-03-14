<?php
session_start();
require_once "../../api/admin_check.php";
?>

<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Új kategória</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body class="bg-light">

<nav class="navbar navbar-dark bg-dark mb-4">
    <div class="container">
        <a class="navbar-brand" href="categories.php">← Kategóriák</a>
    </div>
</nav>

<div class="container">
    <h2 class="mb-3">Új kategória hozzáadása</h2>

    <form action="../../api/admin_category_save.php" method="POST" enctype="multipart/form-data">

    <div class="mb-3">
        <label class="form-label">Név</label>
        <input name="name" class="form-control" required>
    </div>

    <div class="mb-3">
        <label class="form-label">Leírás</label>
        <textarea name="description" class="form-control"></textarea>
    </div>

    <div class="mb-3">
        <label class="form-label">Kép</label>
        <input type="file" name="image" class="form-control" accept="image/*">
    </div>

    <button class="btn btn-success">Mentés</button>

</form>
</div>

</body>
</html>
