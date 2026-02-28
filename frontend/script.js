document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('testForm');
    form.addEventListener('submit', function (event) {
        event.preventDefault();
        const topic = document.getElementById('topic').value.trim();
        const questions = document.getElementById('questions').value;
        const formData = {
            topic: topic,
            questions: questions
        }
        console.log('Данные формы:', formData);
        alert(`Тест по теме "${topic}" создан!\nКоличество вопросов: ${questions}\n`);
    })
})