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

    // Show/Hide password toggle. Works for any button marked [data-password-toggle]
    // that sits in the same .input-group as a password input. Event delegation keeps
    // it independent per field and resilient to multiple fields on one page.
    document.addEventListener('click', function (e) {
        var btn = e.target.closest('[data-password-toggle]');
        if (!btn) return;

        var group = btn.closest('.input-group');
        var input = group ? group.querySelector('input') : null;
        if (!input) return;

        var show = input.type === 'password';
        input.type = show ? 'text' : 'password';
        btn.textContent = show ? 'Hide' : 'Show';
        btn.setAttribute('aria-label', show ? 'Hide password' : 'Show password');
    });
});
