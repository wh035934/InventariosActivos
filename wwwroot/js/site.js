// Colapsar / expandir sidebar (equivale al useState collapsed de menu.tsx).
document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('sidebarToggle');
    var container = document.querySelector('.app-container');
    if (toggle && container) {
        toggle.addEventListener('click', function () {
            container.classList.toggle('sidebar-collapsed');
        });
    }
});
