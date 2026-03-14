<?php
session_start();
require_once "../../api/admin_check.php";
require_once "../../config/db.php";

$stmt = $pdo->query("
    SELECT c.Id, c.Name, c.Description,
           (SELECT COUNT(*) FROM Products p WHERE p.CategoryId = c.Id) AS ProductCount
    FROM Categories c
    ORDER BY c.Name
");
$cats = $stmt->fetchAll();
?>

<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="UTF-8">
    <title>Kategóriák</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body class="bg-light">

<nav class="navbar navbar-dark bg-dark mb-4">
    <div class="container">
        <a class="navbar-brand" href="dashboard.php">Admin panel</a>
        <a href="logout.php" class="btn btn-danger">Kijelentkezés</a>
    </div>
</nav>

<div class="container">
    <h2 class="mb-3">Kategóriák</h2>

    <a href="category_new.php" class="btn btn-primary mb-3">+ Új kategória</a>

    <table class="table table-striped table-bordered">
        <thead>
            <tr>
                <th>ID</th>
                <th>Név</th>
                <th>Leírás</th>
                <th>Termékek száma</th>
                <th>Műveletek</th>
                <th>Kép</th>
            </tr>
        </thead>
        <tbody>
            <?php foreach ($cats as $c): ?>
            <tr>
                <td><?= $c["Id"] ?></td>
                <td><?= $c["Name"] ?></td>
                <td><?= $c["Description"] ?></td>
                <td><?= $c["ProductCount"] ?></td>
                <td>
                    <a href="category_edit.php?id=<?= $c["Id"] ?>" class="btn btn-sm btn-warning">Szerkesztés</a>
                    <a href="category_delete.php?id=<?= $c["Id"] ?>" class="btn btn-sm btn-danger"
                       onclick="return confirm('Biztos törlöd a kategóriát?')">Törlés</a>
                </td>
                <td>
                    <?php if($c["ImageUrl"]): ?>
                    <img src="/<?= $c["ImageUrl"] ?>" width="60">
                    <?php endif; ?>
                </td>
            </tr>
            <?php endforeach; ?>
        </tbody>
    </table>

</div>
</body>
</html>
