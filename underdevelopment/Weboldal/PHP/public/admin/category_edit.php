<?php
session_start();
require_once "../../api/admin_check.php";
require_once "../../config/db.php";

$id = $_GET["id"] ?? null;

$stmt = $pdo->prepare("SELECT * FROM Categories WHERE Id = ?");
$stmt->execute([$id]);
$cat = $stmt->fetch();
?>

<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Kategória szerkesztése</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body class="bg-light">

<nav class="navbar navbar-dark bg-dark mb-4">
    <div class="container">
        <a class="navbar-brand" href="categories.php">← Kategóriák</a>
    </div>
</nav>

<div class="container">
    <h2 class="mb-3"><?= $cat["Name"] ?> szerkesztése</h2>

    <form action="../../api/admin_category_update.php" method="POST" enctype="multipart/form-data">        
        <input type="hidden" name="id" value="<?= $cat["Id"] ?>">

        <div class="mb-3">
            <label class="form-label">Név</label>
            <input name="name" class="form-control" value="<?= $cat["Name"] ?>" required>
        </div>

        <div class="mb-3">
            <label class="form-label">Leírás</label>
            <textarea name="description" class="form-control"><?= $cat["Description"] ?></textarea>
        </div>

        <div class="mb-3">
            <label class="form-label">Kép csere</label>
            <input type="file" name="image" class="form-control">
        </div>

        <?php if($cat["ImageUrl"]): ?>
            <img src="/<?= $cat["ImageUrl"] ?>" width="120">
        <?php endif; ?>

        <button class="btn btn-success">Mentés</button>
    </form>
</div>

</body>
</html>
