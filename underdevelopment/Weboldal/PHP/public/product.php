<?php
session_start();
require_once __DIR__ . "/../config/db.php";

$isLogged = isset($_SESSION["user_id"]);

$id = $_GET["id"] ?? null;

if (!$id) {
    die("Termék nem található.");
}

$stmt = $pdo->prepare("
    SELECT p.*, c.Name AS Category
    FROM Products p
    LEFT JOIN Categories c ON p.CategoryId = c.Id
    WHERE p.Id = ?
");
$stmt->execute([$id]);
$product = $stmt->fetch();

if (!$product) {
    die("Termék nem található.");
}

$stmtImg = $pdo->prepare("
    SELECT *
    FROM ProductImages
    WHERE ProductId = ?
    ORDER BY IsMain DESC, Id DESC
");
$stmtImg->execute([$id]);
$images = $stmtImg->fetchAll();
?>
<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="UTF-8">
    <title><?= htmlspecialchars($product["Name"]) ?></title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
    <style>
        .product-img {
            width: 100%;
            border-radius: 8px;
            margin-bottom: 10px;
        }
        .review-card {
            border-radius: 10px;
        }
    </style>
</head>
<body class="bg-light">

<div class="container mt-4">
    <a href="/" class="btn btn-secondary mb-3">← Vissza</a>

    <div class="row">
        <div class="col-md-6">
            <?php if (count($images) > 0): ?>
                <?php foreach ($images as $img): ?>
                    <img src="/<?= htmlspecialchars($img["ImageUrl"]) ?>" class="product-img">
                <?php endforeach; ?>
            <?php else: ?>
                <div class="alert alert-info">Nincs kép ehhez a termékhez.</div>
            <?php endif; ?>
        </div>

        <div class="col-md-6">
            <h2><?= htmlspecialchars($product["Name"]) ?></h2>

            <p class="text-muted">
                Kategória: <?= htmlspecialchars($product["Category"] ?? "-") ?>
            </p>

            <h3 class="text-success">
                <?= number_format((float)$product["Price"], 0, ",", " ") ?> Ft
            </h3>

            <p><?= nl2br(htmlspecialchars($product["Description"] ?? "")) ?></p>

            <p>Készlet: <b><?= (int)$product["Stock"] ?></b></p>

            <button class="btn btn-primary btn-lg" onclick="addToCart(<?= (int)$product["Id"] ?>)">
                Kosárba
            </button>
        </div>
    </div>

    <hr class="my-4">

    <div class="row">
        <div class="col-md-7">
            <h4>Vélemények</h4>
            <div id="reviewStats" class="text-muted mb-2"></div>
            <div id="reviewList"></div>
        </div>

        <div class="col-md-5">
            <h4>Írj véleményt</h4>

            <?php if (!$isLogged): ?>
                <div class="alert alert-warning">
                    Vélemény írásához be kell jelentkezned.
                </div>
            <?php else: ?>

                <div class="mb-2">
                    <label class="form-label">Értékelés</label>
                    <select id="reviewRating" class="form-select">
                        <option value="5">5 - Kiváló</option>
                        <option value="4">4 - Jó</option>
                        <option value="3">3 - Közepes</option>
                        <option value="2">2 - Gyenge</option>
                        <option value="1">1 - Rossz</option>
                    </select>
                </div>

                <div class="mb-3">
                    <label class="form-label">Vélemény</label>
                    <textarea id="reviewComment" class="form-control" rows="4"></textarea>
                </div>

                <button type="button" class="btn btn-success w-100" onclick="submitReview()">
                    Küldés
                </button>

            <?php endif; ?>
        </div>
    </div>
</div>

<script>
const PRODUCT_ID = <?= (int)$product["Id"] ?>;
window.USER_LOGGED = <?= isset($_SESSION["user_id"]) ? "true" : "false" ?>;
window.IS_ADMIN = <?= (isset($_SESSION["role"]) && $_SESSION["role"] === "admin") ? "true" : "false" ?>;

function escHtml(text) {
    return (text ?? "")
        .toString()
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

async function addToCart(id) {
    let res = await fetch("/api/cart_add.php", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            product_id: id,
            qty: 1
        })
    });

    let data = await res.json();

    if (data.success) {
        alert("Kosárba rakva!");
    } else {
        alert(data.error ?? "Hiba történt");
    }
}

async function loadReviews() {
    let res = await fetch("/api/reviews_get.php?product_id=" + PRODUCT_ID);
    let data = await res.json();

    if (!data.success) {
        document.getElementById("reviewStats").innerText = data.error ?? "Hiba a vélemények betöltésénél";
        return;
    }

    document.getElementById("reviewStats").innerText =
        "Átlag: " + data.stats.avg + " / 5 (" + data.stats.count + " vélemény)";

    let html = "";

    if (data.items.length === 0) {
        html = "<div class='alert alert-secondary'>Még nincs vélemény</div>";
    } else {
        data.items.forEach(r => {
            let date = new Date(r.CreatedAt).toLocaleString();

            html += `
                <div class="card review-card mb-2">
                    <div class="card-body">
                        <div class="d-flex justify-content-between">
                            <b>${escHtml(r.DisplayName)}</b>
                            <div>
                                <span class="text-muted">${date}</span>
                                ${window.IS_ADMIN ? `
                                    <button type="button" class="btn btn-sm btn-danger ms-2" onclick="deleteReview(${r.Id})">
                                        Törlés
                                    </button>
                                ` : ""}
                            </div>
                        </div>

                        <div class="mb-2">⭐ ${r.Rating}/5</div>
                        <div>${escHtml(r.Comment)}</div>
                    </div>
                </div>
            `;
        });
    }

    document.getElementById("reviewList").innerHTML = html;
}

async function submitReview() {
    if (!window.USER_LOGGED) {
        alert("Előbb jelentkezz be!");
        return;
    }

    let ratingEl = document.getElementById("reviewRating");
    let commentEl = document.getElementById("reviewComment");

    if (!ratingEl || !commentEl) {
        alert("Hiányzik a vélemény űrlap.");
        return;
    }

    let rating = ratingEl.value;
    let comment = commentEl.value;

    if (!comment.trim()) {
        alert("Írj véleményt is!");
        return;
    }

    let res = await fetch("/api/review_add.php", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            product_id: PRODUCT_ID,
            rating: rating,
            comment: comment
        })
    });

    let data = await res.json();

    if (data.success) {
        alert("Vélemény elküldve");
        document.getElementById("reviewComment").value = "";
        loadReviews();
    } else {
        alert(data.error ?? "Hiba történt");
    }
}

async function deleteReview(id) {
    if (!confirm("Biztos törlöd a véleményt?")) return;

    let res = await fetch("/api/admin_review_delete.php", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ id: id })
    });

    let data = await res.json();

    if (data.success) {
        alert("Vélemény törölve");
        loadReviews();
    } else {
        alert(data.error ?? "Hiba történt");
    }
}

loadReviews();
</script>

</body>
</html>