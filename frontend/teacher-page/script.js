function escapeHTML(str) {
  if (!str) return "";
  return str.replace(/[&<>"']/g, (m) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
  }[m]));
}

document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("generateForm");
  const generateBtn = document.getElementById("generateBtn");
  const loadingDiv = document.getElementById("loading");
  const resultSection = document.getElementById("resultSection");
  const testContent = document.getElementById("testContent");
  const downloadPdfWithAnsBtn = document.getElementById("downloadPdfWithAnsBtn");
  const downloadPdfNoAnsBtn = document.getElementById("downloadPdfNoAnsBtn");

  let currentTestData = [];
  let currentTestTopic = "";

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    currentTestTopic = document.getElementById("topic").value;
    const count = document.getElementById("questionsCount").value;

    resultSection.classList.add("hidden");
    loadingDiv.classList.remove("hidden");
    generateBtn.disabled = true;

    try {
      const response = await fetch("/api/Test/create", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ topic: currentTestTopic, questionsCount: parseInt(count) }),
      });

      if (!response.ok) throw new Error("Ошибка сервера");

      const data = await response.json();
      currentTestData = data.questions;

      renderTest(currentTestTopic, currentTestData, true);
      resultSection.classList.remove("hidden");
    } catch (error) {
      alert("Ошибка: " + error.message);
    } finally {
      loadingDiv.classList.add("hidden");
      generateBtn.disabled = false;
    }
  });

  function renderTest(topic, questions, showAnswers) {
    let html = `<h2 style="margin-bottom: 20px;">Тема: ${escapeHTML(topic)}</h2>`;
    questions.forEach((q, index) => {
      html += `
        <div class="question-block" style="margin-bottom: 20px;">
            <div class="question-title" style="font-weight: bold;">${index + 1}. ${escapeHTML(q.questionText)}</div>
            <ul style="list-style: none; padding: 10px 0;">
                ${q.options.map((ans) => {
        const isCorrect = ans === q.correctAnswer;
        const color = (showAnswers && isCorrect) ? 'color: #27ae60; font-weight: bold;' : '';
        const text = (showAnswers && isCorrect) ? `${escapeHTML(ans)} (Правильный)` : escapeHTML(ans);
        return `<li style="${color} margin-bottom: 5px;">• ${text}</li>`;
      }).join("")}
            </ul>
        </div><hr style="border: 0; border-top: 1px dashed #ccc;">`;
    });
    testContent.innerHTML = html;
  }

  downloadPdfWithAnsBtn.addEventListener("click", () => {
    renderTest(currentTestTopic, currentTestData, true);
    window.print();
  });

  downloadPdfNoAnsBtn.addEventListener("click", () => {
    renderTest(currentTestTopic, currentTestData, false);
    setTimeout(() => {
      window.print();
      renderTest(currentTestTopic, currentTestData, true);
    }, 100);
  });
});