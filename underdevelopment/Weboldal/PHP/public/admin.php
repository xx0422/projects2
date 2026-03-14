<?php
// PHP/public/admin.php
session_start();

// csak admin
if (!isset($_SESSION["user_id"]) || ($_SESSION["role"] ?? "customer") !== "admin") {
    header("Location: index.php");
    exit;
}
?>
<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="UTF-8">
    <title>Admin felület</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body class="bg-light">

<div class="d-flex justify-content-between align-items-center mb-3">
    <div>
        <h1 class="mb-0">Admin felület</h1>
        <small>Üdv, admin! 👑</small>
    </div>

    <div>
        <a href="/" class="btn btn-outline-secondary me-2">Felhasználói oldal</a>
        <a href="/api/logout.php" class="btn btn-outline-danger">Logout</a>
        <a href="/admin/admin_orders.php" class="btn btn-outline-primary me-2">Rendelések</a>    
    </div>
</div>

<hr>

<!-- Terméklista -->
<h3 class="mt-4">Termékek kezelése</h3>
<button class="btn btn-success mb-3" onclick="showAddProduct()">Új termék hozzáadása</button>

<table class="table table-bordered table-hover">
    <thead class="table-dark">
        <tr>
            <th>ID</th>
            <th>Név</th>
            <th>Ár</th>
            <th>Készlet</th>
            <th>Kategória</th>
            <th>Műveletek</th>
            <th>Kép</th>
        </tr>
    </thead>
    <tbody id="productTable"></tbody>
</table>


<!-- Modal: Új termék -->
<div class="modal fade" id="addModal">
  <div class="modal-dialog">
    <div class="modal-content">
      <div class="modal-header"><h5 class="modal-title">Új termék</h5></div>
      <div class="modal-body">
        <input id="addName" class="form-control mb-2" placeholder="Név">
        <textarea id="addDescription" class="form-control mb-2" placeholder="Leírás"></textarea>
        <input id="addPrice" class="form-control mb-2" placeholder="Ár">
        <input id="addStock" class="form-control mb-2" placeholder="Készlet">
        <input id="addCategory" class="form-control mb-3" placeholder="Kategória ID">

        <label class="form-label">Kép (opcionális)</label>
        <input id="addImage" type="file" class="form-control mb-3" accept="image/*">

        <button class="btn btn-primary w-100" onclick="addProduct()">Hozzáadás</button>
      </div>
    </div>
  </div>
</div>


<!-- Modal: Termék szerkesztése -->
<div class="modal fade" id="editModal">
  <div class="modal-dialog">
    <div class="modal-content">
      <div class="modal-header"><h5 class="modal-title">Termék szerkesztése</h5></div>
      <div class="modal-body">
        <input id="editId" type="hidden">
        <input id="editName" class="form-control mb-2" placeholder="Név">
        <textarea id="editDescription" class="form-control mb-2" placeholder="Leírás"></textarea>
        <input id="editPrice" class="form-control mb-2" placeholder="Ár">
        <input id="editStock" class="form-control mb-2" placeholder="Készlet">
        <input id="editCategory" class="form-control mb-2" placeholder="Kategória ID">

        <label class="form-label">Új kép (opcionális, felülírja a régit)</label>
        <input id="editImage" type="file" class="form-control mb-3" accept="image/*">

        <button class="btn btn-primary w-100" onclick="saveEdit()">Mentés</button>
      </div>
    </div>
  </div>
</div>


<script>
// ---------------------------------------------------
// Terméklista betöltése
// ---------------------------------------------------
async function loadProducts() {
    let res = await fetch("/api/products.php");
    let data = await res.json();

    let html = "";
    data.forEach(p => {
        html += `
        <tr>
            <td>${p.Id}</td>
            <td>${p.Name}</td>
            <td>${p.Price}</td>
            <td>${p.Stock}</td>
            <td>${p.Category ?? "-"}</td>
            <td>
                <button class="btn btn-warning btn-sm"
                        onclick="openEdit(
                            ${p.Id},
                            '${p.Name.replace(/'/g, "\\'")}',
                            '${(p.Description ?? "").replace(/'/g, "\\'")}',
                            ${p.Price},
                            ${p.Stock},
                            ${p.CategoryId ?? "null"}
                        )">
                    Szerkesztés
                </button>
                <button class="btn btn-danger btn-sm" onclick="deleteProduct(${p.Id})">
                    Törlés
                </button>
            </td>
            <td>
                ${p.ImageUrl ? `<img src="/${p.ImageUrl}" style="width:60px;height:60px;object-fit:cover;border-radius:8px;">` : "-"}
            </td>
        </tr>`;
    });

    document.getElementById("productTable").innerHTML = html;
}


// ---------------------------------------------------
// Új termék modal
// ---------------------------------------------------
function showAddProduct() {
    document.getElementById("addName").value = "";
    document.getElementById("addDescription").value = "";
    document.getElementById("addPrice").value = "";
    document.getElementById("addStock").value = "";
    document.getElementById("addCategory").value = "";
    document.getElementById("addImage").value = "";

    new bootstrap.Modal(document.getElementById("addModal")).show();
}


// ---------------------------------------------------
// Termék hozzáadása (FormData + kép)
// ---------------------------------------------------
async function addProduct() {

    // FormData létrehozása
    let form = new FormData();

    form.append("name", document.getElementById("addName").value);
    form.append("description", document.getElementById("addDescription").value);
    form.append("price", document.getElementById("addPrice").value);
    form.append("stock", document.getElementById("addStock").value);
    form.append("categoryId", document.getElementById("addCategory").value);

    // Kép hozzáadása ha van
    let imageInput = document.getElementById("addImage");
    if (imageInput.files.length > 0) {
        form.append("image", imageInput.files[0]);
    }

    try {

        let res = await fetch("/api/admin_product_new.php", {
            method: "POST",
            body: form
        });

        let result = await res.json();

        if (result.success) {

            alert("Termék hozzáadva!");

            // Modal bezárása
            let modal = bootstrap.Modal.getInstance(
                document.getElementById("addModal")
            );
            modal.hide();

            // Mezők törlése
            document.getElementById("addName").value = "";
            document.getElementById("addDescription").value = "";
            document.getElementById("addPrice").value = "";
            document.getElementById("addStock").value = "";
            document.getElementById("addCategory").value = "";
            document.getElementById("addImage").value = "";

            // Lista frissítése
            loadProducts();

        } else {

            alert(result.error ?? "Ismeretlen hiba");

        }

    } catch (err) {

        console.error(err);
        alert("Szerver hiba történt");

    }
}


// ---------------------------------------------------
// Szerkesztés modal megnyitása
// ---------------------------------------------------
function openEdit(id, name, description, price, stock, catId) {
    document.getElementById("editId").value = id;
    document.getElementById("editName").value = name;
    document.getElementById("editDescription").value = description;
    document.getElementById("editPrice").value = price;
    document.getElementById("editStock").value = stock;
    document.getElementById("editCategory").value = catId ?? "";

    document.getElementById("editImage").value = "";

    new bootstrap.Modal(document.getElementById("editModal")).show();
}


// ---------------------------------------------------
// Termék mentése (szerkesztés + opcionális új kép)
// ---------------------------------------------------
async function saveEdit() {
    let form = new FormData();
    form.append("id", document.getElementById("editId").value);
    form.append("name", document.getElementById("editName").value);
    form.append("description", document.getElementById("editDescription").value);
    form.append("price", document.getElementById("editPrice").value);
    form.append("stock", document.getElementById("editStock").value);
    form.append("categoryId", document.getElementById("editCategory").value);

    if (document.getElementById("editImage").files[0]) {
        form.append("image", document.getElementById("editImage").files[0]);
    }

    let res = await fetch("/api/admin_product_edit.php", {
        method: "POST",
        body: form
    });

    let result = await res.json();
    alert(result.success ? "Termék frissítve!" : result.error);

    loadProducts();
}


// ---------------------------------------------------
// Termék törlése
// ---------------------------------------------------
async function deleteProduct(id) {
    if (!confirm("Biztos törlöd?")) return;

    let res = await fetch("/api/admin_product_delete.php", {
        method: "POST",
        headers: {"Content-Type":"application/json"},
        body: JSON.stringify({ id: id })
    });

    let result = await res.json();

    alert(result.success ? "Törölve!" : result.error);

    loadProducts();
}


// induláskor
loadProducts();
</script>

<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>

</body>
</html>
