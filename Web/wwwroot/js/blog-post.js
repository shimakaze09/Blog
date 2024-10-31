let app = new Vue({
    el: '#vue-header',
    data: {
        currentTheme: '',
        themes: []
    },
    created: function () {
        fetch('/Api/Theme')
            .then(res => res.json())
            .then(data => {
                this.themes = data
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

$(function () {
    let testEditormdView = editormd.markdownToHTML("test-editormd-view", {
        htmlDecode: "style,script,iframe",  // You can filter tags decode
        //toc             : false,
        tocm: true,    // Using [TOCM]
        tocContainer: "#custom-toc-container", // Custom ToC container layer
        //gfm             : false,
        //tocDropdown     : true,
        // markdownSourceCode : true, // Whether to preserve Markdown source code, i.e., whether to delete the saved source code Textarea tag
        emoji: true,
        taskList: true,
        tex: true,  // Default not parsed
        flowChart: true,  // Default not parsed
        sequenceDiagram: true,  // Default not parsed
    });
})

function procImages(postId) {
    $.get(`/Api/BlogPost/${postId}/`, function (res) {
        console.log(res)
        for (const imgElem of document.querySelectorAll('.post-content img')) {
            let originSrc = imgElem.getAttribute('src')
            let newSrc = `/assets/blog/${res.data.path}/${originSrc}`
            console.log('originSrc', originSrc)
            console.log('newSrc', newSrc)
            imgElem.setAttribute('src', newSrc)
        }
    })
}
