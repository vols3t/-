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
            })
            if (!response.ok) {
                throw new Error(`Ошибка сервера: ${response.status}`);
            }
            const result = await response.json();
            console.log('Ответ от сервера:', result);

            alert(`Тест по теме "${topic}" успешно создан на сервере!`);

        } catch (error) {
            console.log('Ошибка при отправке', error);
            alert('Не удалось создать тест')
        }
    })
})