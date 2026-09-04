function applyTheme(theme) {
    const isLight = theme === 'light';
    const bg = isLight ? '#f0f4f8' : '#0f1923';
    const cardBg = isLight ? '#ffffff' : '#1e2d3d';
    const text = isLight ? '#1e2a38' : '#f1f5f9';
    const border = isLight ? '#e0e6ed' : '#2d4a6b';

    document.body.style.setProperty('background', bg, 'important');
    document.body.style.setProperty('color', text, 'important');

    document.querySelectorAll('.info-card, .card, .stat-card, .year-section, .summary-card, .table-wrap, .payment-card, .modal, .messages-box').forEach(el => {
        el.style.setProperty('background', cardBg, 'important');
        el.style.setProperty('color', text, 'important');
        el.style.setProperty('border-color', border, 'important');
    });

    document.querySelectorAll('input, select, textarea').forEach(el => {
        el.style.setProperty('background', isLight ? '#fff' : '#0f1923', 'important');
        el.style.setProperty('color', text, 'important');
        el.style.setProperty('border-color', border, 'important');
    });

    document.querySelectorAll('h1, h2, h3, h4, h5, p, span, td, th, label, li, div').forEach(el => {
        if (!el.closest('nav') && !el.closest('.btn') && !el.closest('.card-btn') && !el.closest('.back-btn') && !el.closest('.login-btn')) {
            el.style.setProperty('color', text, 'important');
        }
    });

    document.querySelectorAll('.').forEach(btn => btn.textContent = isLight ? '☀️' : '🌙');
    localStorage.setItem('theme', theme);
}

function toggleTheme() {
    const current = localStorage.getItem('theme') || 'dark';
    applyTheme(current === 'dark' ? 'light' : 'dark');
}

document.addEventListener('DOMContentLoaded', () => {
    const saved = localStorage.getItem('theme') || 'dark';
    applyTheme(saved);
});