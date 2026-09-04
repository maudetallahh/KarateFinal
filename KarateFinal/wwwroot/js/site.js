function applyTheme(theme) {
    const isLight = theme === 'light';
    const bg = isLight ? '#f0f4f8' : '#0f1923';
    const cardBg = isLight ? '#ffffff' : '#1e2d3d';
    const text = isLight ? '#1e2a38' : '#f1f5f9';
    const border = isLight ? '#e0e6ed' : '#2d4a6b';
    document.body.style.setProperty('background', bg, 'important');
    const pageBody = document.getElementById('pageBody');
    if (pageBody) {
        pageBody.style.setProperty('background', l ? '#f0f4f8' : '#0d1117', 'important');
        pageBody.style.setProperty('color', l ? '#1e2a38' : '#e6edf3', 'important');
    }
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
    document.querySelectorAll('h1, h2, h3, h4, h5, p, span, td, th, label, li').forEach(el => {
        if (!el.closest('nav') && !el.closest('.btn') && !el.closest('.card-btn') && !el.closest('.back-btn') && !el.closest('.login-btn')) {
            el.style.setProperty('color', text, 'important');
        }
    });
    document.querySelectorAll('.theme-btn').forEach(btn => btn.textContent = isLight ? '☀️' : '🌙');
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
function applyTheme(t) {
    const l = t === 'light';
    const bg = l ? '#f0f4f8' : '#0f1923';
    const text = l ? '#1e2a38' : '#f1f5f9';
    const cardBg = l ? '#ffffff' : '#1e2d3d';
    const border = l ? '#e0e6ed' : '#2d4a6b';
    document.documentElement.style.setProperty('--page-bg', l ? '#f0f4f8' : '#0d1117');
    document.documentElement.style.setProperty('--page-text', l ? '#1e2a38' : '#e6edf3');
    document.body.style.setProperty('background', bg, 'important');
    document.body.style.setProperty('color', text, 'important');

    document.querySelectorAll('h1,h2,h3,h4,h5,p,span,td,th,label,li').forEach(el => {
        if (!el.closest('nav') && !el.closest('.btn') && !el.closest('.card-btn') && !el.closest('.login-btn')) {
            el.style.setProperty('color', text, 'important');
        }
    });

    document.querySelectorAll('.info-card,.card,.stat-card,.year-section,.summary-card,.table-wrap,.payment-card,.modal,.messages-box').forEach(el => {
        el.style.setProperty('background', cardBg, 'important');
        el.style.setProperty('border-color', border, 'important');
    });

    document.querySelectorAll('input,select,textarea').forEach(el => {
        el.style.setProperty('background', l ? '#fff' : '#0f1923', 'important');
        el.style.setProperty('color', text, 'important');
        el.style.setProperty('border-color', border, 'important');
    });

    document.querySelectorAll('.theme-btn').forEach(b => b.textContent = l ? '☀️' : '🌙');
    localStorage.setItem('theme', t);
}

function toggleTheme() {
    applyTheme(localStorage.getItem('theme') === 'light' ? 'dark' : 'light');
}

document.addEventListener('DOMContentLoaded', () => {
    applyTheme(localStorage.getItem('theme') || 'dark');
});