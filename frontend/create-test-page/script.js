function escapeHTML(str) {
  if (!str) return "";
  return str.replace(/[&<>"']/g, (m) => ({
    '&': '&amp;',
    '<': '&lt;',
    '>': '&gt;',
    '"': '&quot;',
    "'": '&#39;'
  }[m]));
}

document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("generateForm");
  const generateBtn = document.getElementById("generateBtn");
  const loadingDiv = document.getElementById("loading");
  const errorBox = document.getElementById("errorBox");

  form.addEventListener("submit", async (e) => {
    e.preventDefault();

    const topic = document.getElementById("topic").value;
    const count = document.getElementById("questionsCount").value;

    errorBox.classList.add("hidden");
    loadingDiv.classList.remove("hidden");
    generateBtn.disabled = true;

    try {
      const response = await fetch("http://localhost:5152/api/Test/create", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ topic: topic, questionsCount: parseInt(count) }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || "Произошла ошибка на сервере");
      }

      const data = await response.json();

      localStorage.setItem("studentTest", JSON.stringify(data));
      localStorage.setItem("studentTopic", topic);

      window.location.href = "test.html";
    } catch (error) {
      errorBox.textContent = "Ошибка: " + error.message;
      errorBox.classList.remove("hidden");
    } finally {
      loadingDiv.classList.add("hidden");
      generateBtn.disabled = false;
    }
  });
});