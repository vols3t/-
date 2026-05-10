function escapeHTML(str) {
  if (!str) return "";
  return str.replace(
    /[&<>"']/g,
    (m) =>
      ({
        "&": "&amp;",
        "<": "&lt;",
        ">": "&gt;",
        '"': "&quot;",
        "'": "&#39;",
      })[m],
  );
}

document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("generateForm");
  const generateBtn = document.getElementById("generateBtn");
  const loadingDiv = document.getElementById("loading");
  const errorBox = document.getElementById("errorBox");
  const setupSection = document.getElementById("setupSection");

  const testArea = document.getElementById("testArea");
  const testContainer = document.getElementById("testContainer");
  const topicTitle = document.getElementById("topicTitle");
  const finishTestBtn = document.getElementById("finishTestBtn");

  // Прод (открыто по IP/домену сервера, любой не-локальный хост) → относительный путь, nginx проксирует /api/.
  // Локально с http://localhost:5152/ → тоже относительный (тот же origin).
  // Локально через file:// или другой dev-сервер на localhost (например 5500) → бьём напрямую на бэк 5152.
  const isLocalHost = ["localhost", "127.0.0.1", ""].includes(window.location.hostname);
  const API_URL = isLocalHost && window.location.port !== "5152" ? "http://localhost:5152" : "";

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const topic = document.getElementById("topic").value;
    const count = document.getElementById("questionsCount").value;
    const difficulty = document.getElementById("difficulty").value;

    errorBox.classList.add("hidden");
    loadingDiv.classList.remove("hidden");
    generateBtn.disabled = true;

    try {
      const response = await fetch(`${API_URL}/api/Test/create`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ topic: topic, questionsCount: parseInt(count), difficulty: difficulty }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || "Ошибка сервера");
      }

      const data = await response.json();

      localStorage.setItem("studentTest", JSON.stringify(data.questions));
      localStorage.setItem("studentTopic", topic);

      renderTest(data.questions, topic);
    } catch (error) {
      console.error(error);
      errorBox.textContent = "Ошибка: " + error.message;
      errorBox.classList.remove("hidden");
    } finally {
      loadingDiv.classList.add("hidden");
      generateBtn.disabled = false;
    }
  });

  function renderTest(questions, topic) {
    setupSection.classList.add("hidden");
    testArea.classList.remove("hidden");

    topicTitle.textContent = `Тема: ${topic}`;

    let html = "";
    questions.forEach((q, index) => {
      html += `
            <div class="question-block" style="margin-bottom: 25px; padding-bottom: 15px; border-bottom: 1px solid #eee; animation: fadeSlideUp 0.4s ${index * 0.07}s ease both; opacity: 0;">
                <div class="question-title" style="font-weight: bold; font-size: 1.1em; margin-bottom: 10px;">
                    ${index + 1}. ${escapeHTML(q.questionText)}
                </div>
                <ul class="answers-list" style="list-style: none; padding: 0;">
                    ${q.options
                      .map(
                        (ans) => `
                        <li style="margin-bottom: 8px;">
                            <label class="answer-label" style="cursor: pointer; display: flex; align-items: center;">
                                <input type="radio" name="question_${index}" value="${escapeHTML(ans)}" style="margin-right: 10px;">
                                <span>${escapeHTML(ans)}</span>
                            </label>
                        </li>`,
                      )
                      .join("")}
                </ul>
            </div>`;
    });

    testContainer.innerHTML = html;
  }

  if (finishTestBtn) {
    finishTestBtn.addEventListener("click", async () => {
      const questions = JSON.parse(localStorage.getItem("studentTest"));
      const studentAnswers = [];
      let allAnswered = true;

      questions.forEach((q, index) => {
        const selected = document.querySelector(
          `input[name="question_${index}"]:checked`,
        );
        if (!selected) {
          allAnswered = false;
        }
        studentAnswers.push({
          questionID: q.id,
          answer: selected ? selected.value : "",
        });
      });

      if (!allAnswered) {
        alert("Пожалуйста, ответьте на все вопросы перед завершением!");
        return;
      }

      finishTestBtn.disabled = true;
      finishTestBtn.textContent = "Проверяем...";

      try {
        const response = await fetch(`${API_URL}/api/Submit/submit`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(studentAnswers),
        });

        if (!response.ok) throw new Error("Ошибка при проверке ответов");

        const result = await response.json();
        localStorage.setItem("lastTestResult", JSON.stringify(result));

        window.location.href = "results.html";
      } catch (error) {
        alert("Ошибка: " + error.message);
        finishTestBtn.disabled = false;
        finishTestBtn.textContent = "Завершить тест";
      }
    });
  }
});
