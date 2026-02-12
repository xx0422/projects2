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
            btn.addEventListener("click", () => handleGuess(letter, btn)); 
            keyboard.appendChild(btn);
        });
    }
    

    function handleGuess(letter, button) {
        if (!guessedLetters.includes(letter)) {
            guessedLetters.push(letter);
    
            if (selectedWord.includes(letter)) {
                updateWordDisplay();
                checkWin();
            } else {
                mistakes++;
                drawHangman();
                checkLoss();
            }
    
            button.disabled = true;  
            button.style.backgroundColor = selectedWord.includes(letter) ? "green" : "red";  
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
        
        if (mistakes > 0) ctx.strokeRect(10, hangmanCanvas.height - 10, 80, 10);
        
        if (mistakes > 1) {
            ctx.beginPath();
            ctx.moveTo(50, hangmanCanvas.height - 10);
            ctx.lineTo(50, 20);
            ctx.stroke();
        }

        if (mistakes > 2) {
            ctx.beginPath();
            ctx.moveTo(50, 20);
            ctx.lineTo(130, 20);
            ctx.stroke();
        }

        if (mistakes > 3) {
            ctx.beginPath();
            ctx.moveTo(130, 20);
            ctx.lineTo(130, 50);
            ctx.stroke();
        }

        if (mistakes > 4) {
            ctx.beginPath();
            ctx.arc(130, 70, 20, 0, Math.PI * 2);
            ctx.stroke();
        }

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
