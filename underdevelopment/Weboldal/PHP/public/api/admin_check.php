<?php
session_start();

if (!isset($_SESSION["admin_id"])) {
    http_response_code(403);
    echo json_encode(["error" => "Admin login required"]);
    exit;
}
