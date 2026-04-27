function escapeHTML(str) {
    if (!str) return "";
    return str.replace(/[&<>"']/g, (m) => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    }[m]));
}

document.addEventListener("DOMContentLoaded", () => {
    const backendData = JSON.parse(localStorage.getItem("lastTestResult"));
    const topicString = localStorage.getItem("studentTopic");
    const testContainer = document.getElementById("testContainer");
    const scoreBoard = document.getElementById("scoreBoard");
    const scoreText = document.getElementById("scoreText");
    const scoreMessage = document.getElementById("scoreMessage");
    const topicTitle = document.getElementById("topicTitle");

    if (!backendData) {
        window.location.href = "index.html";
        return;
    }

    topicTitle.textContent = `Результаты по теме: ${topicString}`;
    scoreBoard.classList.remove("hidden");
    scoreText.textContent = `Твой результат: ${backendData.correctAnswers} из ${backendData.totalAnswers}`;

    let html = "";
    backendData.questions.forEach((q, index) => {
        const isCorrect = q.isCorrectAnswer;
        html += `
      <div class="question-block" style="padding: 15px; border-radius: 8px; background: ${isCorrect ? '#eafaf1' : '#fdedec'}; margin-bottom: 15px;">
        <div style="font-weight: bold;">${index + 1}. ${escapeHTML(q.questionText)}</div>
        <p>Ваш ответ: <span style="color: ${isCorrect ? 'green' : 'red'}">${escapeHTML(q.realAnswer)}</span></p>
        ${!isCorrect ? `<p style="color: green;">Правильный ответ: ${escapeHTML(q.correctAnswer)}</p>` : ""}
      </div>`;
    });
    testContainer.innerHTML = html;
    document.getElementById("homeBtn").classList.remove("hidden");
});