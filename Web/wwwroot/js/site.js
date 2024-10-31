let app = new Vue({
    el: '#vue-header',
    data: {
        currentTheme: '',
        themes: []
    },
    created: function () {
        fetch('/Api/Theme')
            .then(res => res.json())
            .then(res => {
                this.themes = res.data
            })
        // Read local theme configuration
        let theme = localStorage.getItem('currentTheme')
        if (theme != null) this.currentTheme = theme
    },
    methods: {
        setTheme(themeName) {
            let theme = this.themes.find(t => t.name === themeName)
            loadStyles(theme.cssUrl)
            this.currentTheme = themeName
            localStorage.setItem('currentTheme', themeName)
            localStorage.setItem('currentThemeCssUrl', theme.cssUrl)
        }
    }
})
