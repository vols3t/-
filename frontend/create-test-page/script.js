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
    const errorBox = document.getElementById("errorBox");
    const testContainer = document.getElementById("testContainer");
    const topicTitle = document.getElementById("topicTitle");
    const finishTestBtn = document.getElementById("finishTestBtn");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();
        const topic = document.getElementById("topic").value;
        const count = document.getElementById("questionsCount").value;

        errorBox.classList.add("hidden");
        loadingDiv.classList.remove("hidden");
        generateBtn.disabled = true;

        try {
            const response = await fetch("/api/Test/create", {
                method: "POST",
                headers: {"Content-Type": "application/json"},
                body: JSON.stringify({topic: topic, questionsCount: parseInt(count)}),
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
            errorBox.textContent = "Ошибка: " + error.message;
            errorBox.classList.remove("hidden");
        } finally {
            loadingDiv.classList.add("hidden");
            generateBtn.disabled = false;
        }
    });

    function renderTest(questions, topic) {
        document.querySelector('.settings-card').classList.add('hidden');
        topicTitle.textContent = `Тема: ${topic}`;
        let html = "";
        questions.forEach((q, index) => {
            html += `
        <div class="question-block" style="margin-bottom: 20px;">
            <div class="question-title" style="font-weight: bold;">${index + 1}. ${escapeHTML(q.questionText)}</div>
            <ul class="answers-list" style="list-style: none; padding: 0;">
                ${q.options.map((ans, ansIndex) => `
                    <li>
                        <label class="answer-label">
                            <input type="radio" name="question_${index}" value="${escapeHTML(ans)}">
                            <span>${escapeHTML(ans)}</span>
                        </label>
                    </li>`).join("")}
            </ul>
        </div><hr>`;
        });
        testContainer.innerHTML = html;
        finishTestBtn.classList.remove("hidden");
    }

    if (finishTestBtn) {
        finishTestBtn.addEventListener("click", async () => {
            const questions = JSON.parse(localStorage.getItem("studentTest"));
            const studentAnswers = [];
            let allAnswered = true;

            questions.forEach((q, index) => {
                const selected = document.querySelector(`input[name="question_${index}"]:checked`);
                if (!selected) allAnswered = false;
                studentAnswers.push({
                    questionID: q.id,
                    answer: selected ? selected.value : ""
                });
            });

            if (!allAnswered) {
                alert("Пожалуйста, ответьте на все вопросы!");
                return;
            }

            try {
                const response = await fetch("/api/Submit/submit", {
                    method: "POST",
                    headers: {"Content-Type": "application/json"},
                    body: JSON.stringify(studentAnswers),
                });

                if (!response.ok) throw new Error("Ошибка при проверке");

                const result = await response.json();
                localStorage.setItem("lastTestResult", JSON.stringify(result));
                window.location.href = "results.html";
            } catch (error) {
                alert(error.message);
            }
        });
    }
});