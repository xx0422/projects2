document.addEventListener("DOMContentLoaded", () => {
    const words = ["javascript", "webdesign", "programozas", "canvas", "asztal", "akademia", "elte", "szombathely"];
    let selectedWord, guessedLetters, mistakes;
    const maxMistakes = 6;

    const wordDisplay = document.getElementById("wordDisplay");
    const keyboard = document.getElementById("keyboard");
    const hangmanCanvas = document.getElementById("hangmanCanvas");
    const ctx = hangmanCanvas.getContext("2d");

    function startGame() {
        selectedWord = words[Math.floor(Math.random() * words.length)].toUpperCase();
        guessedLetters = [];
        mistakes = 0;
        wordDisplay.textContent = "_ ".repeat(selectedWord.length).trim();
        drawHangman();
        createKeyboard();
    }

    function createKeyboard() {
        keyboard.innerHTML = "";
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ".split("").forEach(letter => {
            const btn = document.createElement("button");
            btn.textContent = letter;
            btn.addEventListener("click", () => handleGuess(letter, btn)); // A gomb elemet is átadjuk
            keyboard.appendChild(btn);
        });
    }
    

    function handleGuess(letter, button) {
        // Ha a betű még nem volt kiválasztva
        if (!guessedLetters.includes(letter)) {
            guessedLetters.push(letter);
    
            // Ha a betű megtalálható a szóban
            if (selectedWord.includes(letter)) {
                updateWordDisplay();
                checkWin();
            } else {
                mistakes++;
                drawHangman();
                checkLoss();
            }
    
            // Megjelenítési változtatások a kiválasztott betűknél
            button.disabled = true;  // A gombot letiltjuk
            button.style.backgroundColor = selectedWord.includes(letter) ? "green" : "red";  // Színt váltunk
            button.style.color = "white";
        }
    }
    

    function updateWordDisplay() {
        let display = "";
        selectedWord.split("").forEach(letter => {
            display += guessedLetters.includes(letter) ? letter + " " : "_ ";
        });
        wordDisplay.textContent = display.trim();
    }

    function checkWin() {
        if (!wordDisplay.textContent.includes("_")) {
            setTimeout(() => alert("Gratulálok, nyertél!"), 100);
        }
    }

    function checkLoss() {
        if (mistakes >= maxMistakes) {
            setTimeout(() => alert(`Vesztettél! A szó: ${selectedWord}`), 100);
        }
    }

    function drawHangman() {
        ctx.clearRect(0, 0, hangmanCanvas.width, hangmanCanvas.height);
        ctx.lineWidth = 2;
        
        // Alap
        if (mistakes > 0) ctx.strokeRect(10, hangmanCanvas.height - 10, 80, 10);
        
        // Oszlop
        if (mistakes > 1) {
            ctx.beginPath();
            ctx.moveTo(50, hangmanCanvas.height - 10);
            ctx.lineTo(50, 20);
            ctx.stroke();
        }

        // Felső vonal
        if (mistakes > 2) {
            ctx.beginPath();
            ctx.moveTo(50, 20);
            ctx.lineTo(130, 20);
            ctx.stroke();
        }

        // Kötél
        if (mistakes > 3) {
            ctx.beginPath();
            ctx.moveTo(130, 20);
            ctx.lineTo(130, 50);
            ctx.stroke();
        }

        // Fej
        if (mistakes > 4) {
            ctx.beginPath();
            ctx.arc(130, 70, 20, 0, Math.PI * 2);
            ctx.stroke();
        }

        // Test
        if (mistakes > 5) {
            ctx.beginPath();
            ctx.moveTo(130, 90);
            ctx.lineTo(130, 150);
            ctx.stroke();
        }
    }

    document.getElementById("restartButton").addEventListener("click", startGame);
    startGame();
});
