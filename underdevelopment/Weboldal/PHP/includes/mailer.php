<?php

use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;

require_once __DIR__ . "/PHPMailer-master/src/PHPMailer.php";
require_once __DIR__ . "/PHPMailer-master/src/SMTP.php";
require_once __DIR__ . "/PHPMailer-master/src/Exception.php";

function sendRegistrationSuccessEmail(string $toEmail, string $toName): bool
{
    $mail = new PHPMailer(true);

    try {
        $mail->isSMTP();
        $mail->Host = "smtp-relay.brevo.com";
        $mail->SMTPAuth = true;
        $mail->Username = "IDE_JON_A_BREVO_SMTP_LOGIN";
        $mail->Password = "IDE_JON_A_BREVO_SMTP_KEY";
        $mail->SMTPSecure = PHPMailer::ENCRYPTION_STARTTLS;
        $mail->Port = 587;

        $mail->CharSet = "UTF-8";

        $mail->setFrom("noreply@pelda.hu", "Mini Webshop");
        $mail->addAddress($toEmail, $toName);

        $mail->isHTML(true);
        $mail->Subject = "Sikeres regisztráció";

        $safeName = htmlspecialchars($toName, ENT_QUOTES, 'UTF-8');

        $mail->Body = "
            <h2>Szia, {$safeName}!</h2>
            <p>Sikeresen regisztráltál a Mini Webshop oldalán.</p>
            <p>Most már be tudsz jelentkezni és vásárolni.</p>
            <hr>
            <p>Üdv,<br>Mini Webshop</p>
        ";

        $mail->AltBody = "Szia, {$toName}! Sikeresen regisztráltál a Mini Webshop oldalán.";

        $mail->send();
        return true;

    } catch (Exception $e) {
        return false;
    }
}
