document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("generateForm");
  const generateBtn = document.getElementById("generateBtn");
  const loadingDiv = document.getElementById("loading");
  const resultSection = document.getElementById("resultSection");
  const testContent = document.getElementById("testContent");

  const downloadPdfWithAnsBtn = document.getElementById(
    "downloadPdfWithAnsBtn",
  );
  const downloadPdfNoAnsBtn = document.getElementById("downloadPdfNoAnsBtn");


  let currentTestData = [];
  let currentTestTopic = "";

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


  form.addEventListener("submit", async (e) => {
    e.preventDefault();

    currentTestTopic = document.getElementById("topic").value;
    const count = document.getElementById("questionsCount").value;

    resultSection.classList.add("hidden");
    loadingDiv.classList.remove("hidden");
    generateBtn.disabled = true;

    try {
      const data = await fetchTestFromApi(currentTestTopic, count);
      currentTestData = data; 

      renderTest(currentTestTopic, currentTestData, true);
      resultSection.classList.remove("hidden");
    } catch (error) {
      alert("Ошибка: " + error.message);
    } finally {
      loadingDiv.classList.add("hidden");
      generateBtn.disabled = false;
    }
  });

  async function fetchTestFromApi(topic, count) {
    const response = await fetch("http://localhost:5152/api/Test/create", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ topic: topic, questionsCount: parseInt(count) }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.error || "Произошла ошибка на сервере");
    }
    return await response.json();
  }


  function renderTest(topic, questions, showAnswers) {
    let html = `<h2 style="margin-bottom: 20px;">Тема: ${topic}</h2>`;

    questions.forEach((q, index) => {
      html += `
                <div class="question-block">
                    <div class="question-title">${index + 1}. ${q.questionText}</div>
                    <ul class="answers-list">
                        ${q.options
                          .map((ans, ansIndex) => {
                            const isCorrect = ans === q.correctAnswer;

                            if (showAnswers && isCorrect) {
                              return `
                                    <li>
                                        <label style="color: #27ae60; font-weight: bold;">
                                            <input type="radio" name="q${q.id || index}" checked disabled> 
                                            ${ans} (✅ Правильный)
                                        </label>
                                    </li>
                                `;
                            }

                            else {
                              return `
                                    <li>
                                        <label>
                                            <input type="radio" name="q${q.id || index}" disabled> 
                                            ${ans}
                                        </label>
                                    </li>
                                `;
                            }
                          })
                          .join("")}
                    </ul>
                </div>
                <hr style="margin: 20px 0; border: 0; border-top: 1px dashed #ccc;">
            `;
    });

    testContent.innerHTML = html;
  }
});
