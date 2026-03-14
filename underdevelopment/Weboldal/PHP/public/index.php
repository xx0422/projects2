<?php
session_start();
?>

<!DOCTYPE html>
<html lang="hu">
<head>
<meta charset="UTF-8">
<title>Mini Webshop</title>

<link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css" rel="stylesheet">

<style>
body { background:#f8f9fa; }
.product-card { transition:0.2s; }
.product-card:hover { transform:scale(1.03); }
#cartCount { font-size:0.8rem; padding:2px 6px; }
body {
    background-image: url('/images/hatter.jpg');
    background-repeat: repeat;
    background-size: auto;
    background-position: top left;
}
</style>

</head>
<body>


<!-- NAVBAR -->
<nav class="navbar navbar-expand-lg navbar-dark bg-dark mb-4">
<div class="container">

<a class="navbar-brand" href="/">Mini Webshop</a>

<div>

<?php if(isset($_SESSION["user_id"])): ?>

    <?php if(($_SESSION["role"] ?? "customer") === "admin"): ?>
        <a class="btn btn-outline-warning me-2" href="/admin.php">
        Admin panel
        </a>
    <?php endif; ?>

    <a class="btn btn-outline-light me-2" href="/api/logout.php">
    Logout
    </a>

<?php else: ?>

    <button class="btn btn-outline-info me-2" onclick="showRegister()">
    Regisztráció
    </button>

    <button class="btn btn-outline-light me-2" onclick="showLogin()">
    Login
    </button>

<?php endif; ?>

<a class="btn btn-warning" href="cart.php">
Kosár
<span id="cartCount" class="badge bg-danger">0</span>
</a>

</div>
</div>
</nav>


<!-- TERMÉKEK -->
<div class="container">

<h2 class="mb3" style='color: white; background-color: black; padding: 10px; display: inline-block; border: 1px solid #ccc;'>Termékek</h2>

<div id="productList" class="row g-3"></div>

</div>



<!-- LOGIN MODAL -->
<div class="modal fade" id="loginModal">

<div class="modal-dialog">

<div class="modal-content">

<div class="modal-header">
<h5 class="modal-title">Bejelentkezés</h5>
</div>

<div class="modal-body">

<input id="loginEmail" class="form-control mb-2" placeholder="Email">

<input id="loginPassword" type="password" class="form-control mb-3" placeholder="Jelszó">

<button class="btn btn-primary w-100" onclick="login()">
Bejelentkezés
</button>

</div>
</div>
</div>
</div>



<!-- REGISTER MODAL -->
<div class="modal fade" id="registerModal">

<div class="modal-dialog">

<div class="modal-content">

<div class="modal-header">
<h5 class="modal-title">Regisztráció</h5>
</div>

<div class="modal-body">

<input id="regName" class="form-control mb-2" placeholder="Név">

<input id="regEmail" class="form-control mb-2" placeholder="Email">

<input id="regPassword" type="password" class="form-control mb-3" placeholder="Jelszó">

<button class="btn btn-primary w-100" onclick="registerUser()">
Regisztráció
</button>

</div>
</div>
</div>
</div>



<script>


// TERMÉKEK BETÖLTÉSE
async function loadProducts(){

let res = await fetch("/api/products.php")

let data = await res.json()

let html=""

data.forEach(p=>{

html+=`
<div class="col-md-3">

<div class="card product-card p-2">

<img src="${p.ImageUrl ? "/" + p.ImageUrl : "https://via.placeholder.com/300"}" class="card-img-top">

<div class="card-body">

<h5 class="card-title">${p.Name}</h5>

<p>${p.Description ?? ""}</p>

<p class="fw-bold">${p.Price} Ft</p>

<a href="/product.php?id=${p.Id}" class="btn btn-outline-secondary w-100 mb-2">
Megnyitás
</a>

<button class="btn btn-success w-100" onclick="addToCart(${p.Id})">
Kosárba
</button>

</div>
</div>
</div>
`
})

document.getElementById("productList").innerHTML=html

}



// KOSÁR SZÁMLÁLÓ
async function updateCartCount(){

let res=await fetch("/api/cart_get.php")

let data=await res.json()

document.getElementById("cartCount").innerText=data.items?.length ?? 0

}



// KOSÁRBA
async function addToCart(id){

let res=await fetch("/api/cart_add.php",{

method:"POST",

headers:{"Content-Type":"application/json"},

body:JSON.stringify({
product_id:id,
qty:1
})

})

let data=await res.json()

if(data.error) alert(data.error)

updateCartCount()

}



// LOGIN MODAL
function showLogin(){

new bootstrap.Modal(document.getElementById("loginModal")).show()

}



// REGISTER MODAL
function showRegister(){

new bootstrap.Modal(document.getElementById("registerModal")).show()

}



// LOGIN
async function login(){

let email=document.getElementById("loginEmail").value

let pw=document.getElementById("loginPassword").value

let res=await fetch("/api/login.php",{

method:"POST",

headers:{
"Content-Type":"application/json"
},

body:JSON.stringify({
email:email,
password:pw
})

})

let data=await res.json()

if(data.success){

alert("Sikeres bejelentkezés!")

window.location.reload()

}else{

alert(data.error ?? "Hibás adatok")

}

}



// REGISTER
async function registerUser() {
    let name = document.getElementById("regName").value;
    let email = document.getElementById("regEmail").value;
    let pw = document.getElementById("regPassword").value;

    let res = await fetch("/api/register.php", {
        method: "POST",
        headers: {"Content-Type":"application/json"},
        body: JSON.stringify({
            name: name,
            email: email,
            password: pw
        })
    });

    let data = await res.json();

    if (data.success) {
        if (data.mailSent) {
            alert("Sikeres regisztráció! A megerősítő email elküldve.");
        } else {
            alert("Sikeres regisztráció! Az email küldése nem sikerült.");
        }

        window.location.reload();
    } else {
        alert(data.error ?? "Hiba történt");
    }
}




loadProducts()

updateCartCount()

</script>


<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js"></script>


</body>
</html>