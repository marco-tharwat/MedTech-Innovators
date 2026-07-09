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
    var current = window.location.pathname.replace(/\/+$/, '').toLowerCase() || '/';
    var links = document.querySelectorAll('.app-sidebar .nav-link');
    var bestLink = null;
    var bestLen = -1;
    links.forEach(function (link) {
        var href = (link.getAttribute('href') || '').split('?')[0].replace(/\/+$/, '').toLowerCase();
        if (!href) return;
        // Exact match wins; otherwise pick the longest href that prefixes the path.
        if (current === href) {
            if (href.length > bestLen) { bestLink = link; bestLen = href.length + 1000; }
        } else if (href !== '/' && current.indexOf(href + '/') === 0 && href.length > bestLen) {
            bestLink = link; bestLen = href.length;
        }
    });
    if (bestLink) {
        bestLink.classList.add('active');
    }
});
