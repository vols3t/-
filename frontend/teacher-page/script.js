function escapeHTML(str) {
  if (!str) return "";
  return str.replace(
    /[&<>"']/g,
    (m) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[m],
  );
}

document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("generateForm");
  const generateBtn = document.getElementById("generateBtn");
  const loadingDiv = document.getElementById("loading");
  const resultSection = document.getElementById("resultSection");
  const testContent = document.getElementById("testContent");
  const downloadPdfWithAnsBtn = document.getElementById("downloadPdfWithAnsBtn");
  const downloadPdfNoAnsBtn = document.getElementById("downloadPdfNoAnsBtn");
  const selectAllBtn = document.getElementById("selectAllBtn");
  const deselectAllBtn = document.getElementById("deselectAllBtn");

  let currentTestData = [];
  let currentTestTopic = "";

  const API_URL = window.location.port === "5152" ? "" : "http://localhost:5152";

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    currentTestTopic = document.getElementById("topic").value;
    const count = document.getElementById("questionsCount").value;
    const difficulty = document.getElementById("difficulty").value;

    resultSection.classList.add("hidden");
    loadingDiv.classList.remove("hidden");
    generateBtn.disabled = true;

    try {
      const response = await fetch(`${API_URL}/api/Test/create`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ topic: currentTestTopic, questionsCount: parseInt(count), difficulty: difficulty }),
      });

      if (!response.ok) throw new Error("Ошибка сервера");

      const data = await response.json();
      currentTestData = data.questions;

      renderTest(currentTestTopic, currentTestData);
      resultSection.classList.remove("hidden");
    } catch (error) {
      alert("Ошибка: " + error.message);
    } finally {
      loadingDiv.classList.add("hidden");
      generateBtn.disabled = false;
    }
  });

  // Живой просмотр — всегда с ответами, с чекбоксами
  function renderTest(topic, questions) {
    let html = `<h2 style="margin-bottom: 20px;">Тема: ${escapeHTML(topic)}</h2>`;
    questions.forEach((q, index) => {
      const correctAnswer = (q.correctAnswer || "").trim().toLowerCase();
      html += `
        <div class="question-block" id="qblock-${index}" style="margin-bottom: 20px; animation: fadeSlideUp 0.4s ${index * 0.07}s ease both; opacity: 0;">
          <label class="question-checkbox-wrap no-print">
            <input type="checkbox" class="question-checkbox" data-index="${index}" checked>
            Включить в PDF
          </label>
          <div class="question-title" style="font-weight: bold; margin-bottom: 8px;">${index + 1}. ${escapeHTML(q.questionText)}</div>
          <ul style="list-style: none; padding: 0;">
            ${q.options.map((ans) => {
              const isCorrect = ans.trim().toLowerCase() === correctAnswer;
              const cls = isCorrect ? "correct-answer-highlight" : "";
              const marker = isCorrect ? `<span class="correct-marker">✓ </span>` : "";
              return `<li class="${cls}" style="margin-bottom: 5px;">${marker}${escapeHTML(ans)}</li>`;
            }).join("")}
          </ul>
        </div>
        <hr style="border: 0; border-top: 1px dashed #ccc; margin-bottom: 20px;" class="no-print">`;
    });
    testContent.innerHTML = html;
  }

  selectAllBtn.addEventListener("click", () => {
    document.querySelectorAll(".question-checkbox").forEach((cb) => (cb.checked = true));
  });

  deselectAllBtn.addEventListener("click", () => {
    document.querySelectorAll(".question-checkbox").forEach((cb) => (cb.checked = false));
  });

  function applySelectionForPrint() {
    document.querySelectorAll(".question-checkbox").forEach((cb) => {
      const block = document.getElementById(`qblock-${cb.dataset.index}`);
      if (block) block.classList.toggle("print-exclude", !cb.checked);
    });
  }

  function clearSelectionForPrint() {
    document.querySelectorAll(".print-exclude").forEach((el) => el.classList.remove("print-exclude"));
  }

  downloadPdfWithAnsBtn.addEventListener("click", () => {
    applySelectionForPrint();
    window.print();
    clearSelectionForPrint();
  });

  downloadPdfNoAnsBtn.addEventListener("click", () => {
    applySelectionForPrint();
    document.body.classList.add("hide-answers");
    window.print();
    document.body.classList.remove("hide-answers");
    clearSelectionForPrint();
  });
});
