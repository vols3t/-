document.addEventListener("DOMContentLoaded", () => {
  const topicTitle = document.getElementById("topicTitle");
  const testContainer = document.getElementById("testContainer");
  const finishTestBtn = document.getElementById("finishTestBtn");
  const scoreBoard = document.getElementById("scoreBoard");
  const scoreText = document.getElementById("scoreText");
  const scoreMessage = document.getElementById("scoreMessage");
  const homeBtn = document.getElementById("homeBtn");

  const testDataString = localStorage.getItem("studentTest");
  const topicString = localStorage.getItem("studentTopic");

  if (!testDataString) {
    window.location.href = "index.html";
    return;
  }

  const questions = JSON.parse(testDataString);
  topicTitle.textContent = `Тема: ${topicString}`;

  function renderTest() {
    let html = "";
    questions.forEach((q, index) => {
      html += `
                <div class="question-block" id="qblock_${index}">
                    <div class="question-title">${index + 1}. ${q.questionText}</div>
                    <ul class="answers-list">
                        ${q.options
                          .map(
                            (ans, ansIndex) => `
                            <li>
                                <label class="answer-label" id="label_${index}_${ansIndex}">
                                    <input type="radio" name="question_${index}" value="${ans}">
                                    <span>${ans}</span>
                                </label>
                            </li>
                        `,
                          )
                          .join("")}
                    </ul>
                </div>
                ${index < questions.length - 1 ? '<hr style="margin: 20px 0; border: 0; border-top: 1px dashed #ccc;">' : ""}
            `;
    });
    testContainer.innerHTML = html;
    finishTestBtn.classList.remove("hidden");
  }

  renderTest();
  finishTestBtn.addEventListener("click", async () => {
    const studentAnswers = [];
    let allAnswered = true;

    questions.forEach((q, index) => {
      const selectedRadio = document.querySelector(
        `input[name="question_${index}"]:checked`,
      );
      if (!selectedRadio) {
        allAnswered = false;
      }

      studentAnswers.push({
        questionID: q.id,
        answer: selectedRadio ? selectedRadio.value : "",
      });
    });

    if (!allAnswered) {
      alert("Пожалуйста, выбери варианты ответа на все вопросы!");
      return;
    }

    finishTestBtn.textContent = "Проверяем на сервере... ";
    finishTestBtn.disabled = true;

    try {
      const response = await fetch(
        "http://81.26.190.46/api/Submit/submit",
        {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(studentAnswers),
        },
      );

      if (!response.ok) {
        throw new Error("Ошибка при проверке теста на сервере");
      }

      const backendData = await response.json();

      showResults(backendData);
    } catch (error) {
      alert("Ошибка связи с сервером: " + error.message);
      finishTestBtn.textContent = "Завершить тест";
      finishTestBtn.disabled = false;
    }
  });

  function showResults(backendData) {
    finishTestBtn.classList.add("hidden");
    homeBtn.classList.remove("hidden");
    scoreBoard.classList.remove("hidden");

    scoreText.textContent = `Твой результат: ${backendData.correctAnswers} из ${backendData.totalAnswers}`;

    const percentage =
      backendData.totalAnswers > 0
        ? backendData.correctAnswers / backendData.totalAnswers
        : 0;

    if (percentage === 1) {
      scoreMessage.textContent = "Идеально!";
    } else if (percentage >= 0.7) {
      scoreMessage.textContent = "Хорошая работа! Но есть куда расти";
    } else if (percentage >= 0.4) {
      scoreMessage.textContent =
        "Удовлетворительно. Стоит еще раз перечитать тему";
    } else {
      scoreMessage.textContent = "Плохо. Нужно подучить материал";
      scoreBoard.style.backgroundColor = "#e74c3c";
    }

    questions.forEach((q, index) => {
      const qResult = backendData.questions.find(
        (res) => res.questionId === q.id,
      );

      if (!qResult) return;

      const allRadios = document.querySelectorAll(
        `input[name="question_${index}"]`,
      );

      allRadios.forEach((radio, ansIndex) => {
        radio.disabled = true;
        const label = document.getElementById(`label_${index}_${ansIndex}`);
        label.classList.add("disabled");

        if (radio.value === qResult.correctAnswer) {
          label.classList.add("correct");
          if (radio.value !== qResult.realAnswer) {
            label.innerHTML += " <span>(Правильный ответ)</span>";
          }
        }

        if (
          radio.value === qResult.realAnswer &&
          radio.value !== qResult.correctAnswer
        ) {
          label.classList.add("wrong");
          label.innerHTML += " <span>(Твоя ошибка)</span>";
        }
      });
    });
  }
});
