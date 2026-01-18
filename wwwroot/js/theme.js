// Theme color management for Doodle app
window.doodleTheme = {
    applyTheme: function (isDark) {
        try {
            var html = document.documentElement;
            var body = document.body;
            
            // Clear custom theme
            html.removeAttribute('data-custom-theme');
            html.style.removeProperty('--custom-deep-green');
            html.style.removeProperty('--custom-olive');
            html.style.removeProperty('--custom-clay-beige');
            
            if (isDark) {
                html.setAttribute('data-theme', 'dark');
                html.setAttribute('data-bs-theme', 'dark');
                html.style.backgroundColor = '#30360E';
                body.style.backgroundColor = '#30360E';
            } else {
                html.setAttribute('data-theme', 'light');
                html.setAttribute('data-bs-theme', 'light');
                html.style.backgroundColor = '#E2D4B9';
                body.style.backgroundColor = '#E2D4B9';
            }
            
            console.log('Theme colors applied:', isDark ? 'dark (#30360E)' : 'light (#E2D4B9)');
        } catch (error) {
            console.error('Error applying theme colors:', error);
        }
    },
    
    applyCustomTheme: function (deepGreen, olive, clayBeige) {
        try {
            var html = document.documentElement;
            var body = document.body;
            
            // Set custom theme flag
            html.setAttribute('data-theme', 'custom');
            html.setAttribute('data-bs-theme', 'custom');
            html.setAttribute('data-custom-theme', 'true');
            
            // Set CSS custom properties
            html.style.setProperty('--custom-deep-green', deepGreen);
            html.style.setProperty('--custom-olive', olive);
            html.style.setProperty('--custom-clay-beige', clayBeige);
            
            // Apply background colors (use system preference for base)
            var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
            if (prefersDark) {
                html.style.backgroundColor = deepGreen;
                body.style.backgroundColor = deepGreen;
            } else {
                html.style.backgroundColor = clayBeige;
                body.style.backgroundColor = clayBeige;
            }
            
            console.log('Custom theme colors applied:', {
                deepGreen: deepGreen,
                olive: olive,
                clayBeige: clayBeige
            });
        } catch (error) {
            console.error('Error applying custom theme colors:', error);
        }
    }
};

// Listen for system theme changes (for auto mode)
if (window.matchMedia) {
    var darkModeQuery = window.matchMedia('(prefers-color-scheme: dark)');
    darkModeQuery.addEventListener('change', function(e) {
        // Only apply if theme is set to "auto" or not explicitly set
        var currentTheme = document.documentElement.getAttribute('data-theme');
        if (!currentTheme || currentTheme === 'auto') {
            window.doodleTheme.applyTheme(e.matches);
        }
    });
}
