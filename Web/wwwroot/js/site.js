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
            // After changing the theme, it's best to refresh the page, otherwise there may be conflicts with styles
            location.reload()
        }
    }
})

let toastTrigger = document.getElementById('liveToastBtn')
let toastLiveExample = document.getElementById('liveToast')
if (toastTrigger) {
    toastTrigger.addEventListener('click', function () {
        let toast = new bootstrap.Toast(toastLiveExample)
        toast.show()
    })
}