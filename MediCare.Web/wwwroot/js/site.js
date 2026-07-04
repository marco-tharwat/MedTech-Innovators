// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Mobile sidebar toggle (dashboard layout)
document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('sidebarToggle');
    var sidebar = document.getElementById('appSidebar');
    if (toggle && sidebar) {
        toggle.addEventListener('click', function () {
            sidebar.classList.toggle('is-open');
        });
        document.addEventListener('click', function (e) {
            if (sidebar.classList.contains('is-open') &&
                !sidebar.contains(e.target) &&
                e.target !== toggle) {
                sidebar.classList.remove('is-open');
            }
        });
    }

    // Highlight the sidebar link matching the current URL
    var links = document.querySelectorAll('.app-sidebar .nav-link');
    links.forEach(function (link) {
        if (link.getAttribute('href') === window.location.pathname) {
            link.classList.add('active');
        }
    });
});
