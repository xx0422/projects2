<?php
$DB_HOST = 'DESKTOP-790QPET\SQLEXPRESS';
$DB_NAME = 'Webshop';
$DB_USER = 'xx04'; // saját SQL login
$DB_PASS = 'Csipike0306'; // saját jelszó

try {
    $dsn = "sqlsrv:Server=$DB_HOST;Database=$DB_NAME;Encrypt=no";

    $pdo = new PDO($dsn, $DB_USER, $DB_PASS, [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
    ]);


} catch (PDOException $e) {
    die("Adatbázis kapcsolat hiba: " . $e->getMessage());
}
