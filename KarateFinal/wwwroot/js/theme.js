function toggleTheme() {
    const body = document.body;
    const isLight = body.classList.contains('light-mode');
    if (isLight) {
        body.classList.remove('light-mode');
        localStorage.setItem('theme', 'dark');
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '🌙');
    } else {
        body.classList.add('light-mode');
        localStorage.setItem('theme', 'light');
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '☀️');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    const saved = localStorage.getItem('theme') || 'dark';
    if (saved === 'light') {
        document.body.classList.add('light-mode');
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '☀️');
    } else {
        document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = '🌙');
    }
});