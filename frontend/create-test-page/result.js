const savedResults = JSON.parse(localStorage.getItem('lastTestResult'));

const main = document.querySelector('main');
main.innerHTML = `<h2 id="correctAnswers">
    <b>Твой результат: ${savedResults.correctAnswers} из ${savedResults.totalAnswers}!</b>
   </h2>`;

savedResults.questions.forEach((q, index) => {
    const optionsHtml = q.answers.map(opt => {
        let statusClass = '';

        if (opt === q.correctAnswer) {
            statusClass = 'option--correct';
        } else if (opt === q.realAnswer && opt !== q.correctAnswer) {
            statusClass = 'option--error';
        }

        return `
            <div class="option-item ${statusClass}">
                <span>${opt}</span>
            </div>
        `;
    }).join('');

    main.innerHTML += `
        <div class="form-group result-card">
            <span class="label">${index + 1}. ${q.questionText}</span>
            <div class="options-container">
                ${optionsHtml}
            </div>
        </div>
    `;
})

main.innerHTML += `<button type="button" class="btn" id="returnBtn">Вернуться к тесту</button>`;
const returnBtn = document.querySelector('#returnBtn');
returnBtn.addEventListener('click', () => {
    window.location.href = 'index.html';
});