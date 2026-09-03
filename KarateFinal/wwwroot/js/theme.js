function toggleTheme() {
    const body = document.body;
    const isLight = body.classList.contains('light-mode');
    if (isLight) {
        body.classList.remove('light-mode');
        body.style.background = '#0f1923';
        body.style.color = '#f1f5f9';
        localStorage.setItem('theme', 'dark');
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '🌙');
    } else {
        body.classList.add('light-mode');
        body.style.background = '#f0f4f8';
        body.style.color = '#1e2a38';
        localStorage.setItem('theme', 'light');
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '☀️');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const saved = localStorage.getItem('theme') || 'dark';
    if (saved === 'light') {
        document.body.classList.add('light-mode');
        document.body.style.background = '#f0f4f8';
        document.body.style.color = '#1e2a38';
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '☀️');
    } else {
        document.body.style.background = '#0f1923';
        document.body.style.color = '#f1f5f9';
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '🌙');
    }
});