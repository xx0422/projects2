<?php
session_start();
?>
<!DOCTYPE html>
<html lang="hu">
<head>
    <meta charset="UTF-8">
    <title>Kosár</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">
</head>

<body class="bg-light">

<nav class="navbar navbar-expand-lg navbar-dark bg-dark mb-4">
  <div class="container">
    <a class="navbar-brand" href="index.php">Mini Webshop</a>
    <div>
        <a href="index.php" class="btn btn-secondary me-2">Vissza a shopba</a>
    </div>
  </div>
</nav>

<div class="container">

    <h2 class="mb-4">Kosár</h2>

    <div id="cartContainer"></div>

    <div class="card p-3 mt-3">
        <h4>Összegzés</h4>

        <div id="cartSummary"></div>

        <a id="checkoutBtn" href="/checkout.php" class="btn btn-success mt-3 w-100">
            Megrendelés
        </a>
    </div>

</div>

<script>

async function loadCart() {

    let res = await fetch("/api/cart_get.php");
    let data = await res.json();

    let html = "";
    let total = 0;

    if (!data.items || data.items.length === 0) {

        document.getElementById("cartContainer").innerHTML =
            `<div class="alert alert-info">A kosár üres.</div>`;

        document.getElementById("cartSummary").innerHTML =
            `<h4>Végösszeg: 0 Ft</h4>`;

        document.getElementById("checkoutBtn").style.display = "none";

        return;
    }

    document.getElementById("checkoutBtn").style.display = "block";

    html += `<table class="table table-striped">
                <thead>
                    <tr>
                        <th>Termék</th>
                        <th>Ár</th>
                        <th>Mennyiség</th>
                        <th>Összesen</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>`;

    data.items.forEach(item => {

        let subtotal = item.Price * item.Quantity;
        total += subtotal;

        html += `
            <tr>
                <td>${item.Name}</td>
                <td>${item.Price} Ft</td>
                <td>${item.Quantity}</td>
                <td>${subtotal} Ft</td>
                <td>
                    <button class="btn btn-danger btn-sm" onclick="removeItem(${item.CartItemId})">
                        Törlés
                    </button>
                </td>
            </tr>`;
    });

    html += `
        </tbody>
    </table>

    <button class="btn btn-warning mt-2" onclick="clearCart()">
        Kosár ürítése
    </button>
    `;

    document.getElementById("cartContainer").innerHTML = html;

    document.getElementById("cartSummary").innerHTML =
        "<h4>Végösszeg: " + total + " Ft</h4>";
}



async function removeItem(id) {

    await fetch("/api/cart_remove.php", {
        method: "POST",
        headers: {"Content-Type": "application/json"},
        body: JSON.stringify({"cart_item_id": id})
    });

    loadCart();
}



async function clearCart() {

    await fetch("/api/cart_clear.php", {
        method: "POST"
    });

    loadCart();
}


loadCart();

</script>

</body>
</html>