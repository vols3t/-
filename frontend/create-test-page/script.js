document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('testForm');
    form.addEventListener('submit', async function (event) {
        event.preventDefault();
        const topic = document.getElementById('topic').value.trim();
        const questionsCount = document.getElementById('questions-count').value;
        const formData = {
            topic: topic,
            questionsCount: parseInt(questionsCount),
        }
        console.log('Отправка данных:', formData);
        try {
            const response = await fetch('http://localhost:5152/api/test/create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(formData)
            });
            if (!response.ok) {
                throw new Error(`Ошибка сервера: ${response.status}`);
            }

            const questions = await response.json(); // Теперь это массив вопросов
            console.log('Вопросы:', questions);

            const main = document.querySelector('main');
            main.innerHTML = `<h2 id="testTopic">Тест по теме: ${topic}</h2><form id="quizForm"></form>`;

            const quizForm = document.getElementById('quizForm');
            questions.forEach((q, index) => {
                let optionsHtml = q.options.map((opt, i) => `
        <label class="option-item">
            <input type="checkbox" name="q${index}" value="${i}">
            <span>${opt}</span>
        </label>
    `).join('');

                quizForm.innerHTML += `
        <div class="form-group" data-id="${q.id}">
            <span class="label">${index + 1}. ${q.questionText}</span>
            ${optionsHtml}
        </div>
    `;
            });

            quizForm.innerHTML += `<button type="button" class="btn" id="submitBtn">Завершить</button>`;


            const submitButton = document.getElementById('submitBtn');

            submitButton.addEventListener('click', async function () {
                const answers = collectAnswers();
                if (answers.length < questionsCount) {
                    alert('Введите ответы на все вопросы!');
                    return;
                }

                try {
                    const response = await fetch('http://localhost:5152/api/submit/submit', {

                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(answers)
                    });

                    if (!response.ok) throw new Error("Ошибка сервера");

                    const result = await response.json();
                    JSON.stringify(result);
                    localStorage.setItem('lastTestResult', JSON.stringify(result));
                    window.location.href = 'results.html';

                } catch (error) {
                    console.log("Ошибка при проверке:" + error);
                }
            });


            alert(`Тест по теме "${topic}" успешно создан на сервере!`);


        } catch (error) {
            console.log('Ошибка при отправке', error);
            alert('Не удалось создать тест');
        }
    });

    function collectAnswers() {
        let answers = [];

        document.querySelectorAll('.form-group')
            .forEach(card => {
                const id = card.dataset.id;
                const input = card.querySelector('input:checked');
                if (input !== null) {
                    answers.push(
                        {
                            questionID: parseInt(id),
                            answer: input.nextElementSibling.innerText
                        }
                    );
                }
            });

        return answers;
    }
})