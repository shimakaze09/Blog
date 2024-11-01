$(function () {
    let testEditormdView = editormd.markdownToHTML("test-editormd-view", {
        // htmlDecode: "style,script,iframe",  // you can filter tags decode
        htmlDecode: true,
        //toc             : false,
        tocm: true,    // Using [TOCM]
        tocContainer: "#custom-toc-container", // custom ToC container level
        //gfm             : false,
        //tocDropdown     : true,
        // markdownSourceCode : true, // whether to preserve Markdown source code, i.e., whether to delete saved source code Textarea tag
        emoji: true,
        taskList: true,
        tex: true,  // default not parsed
        flowChart: true,  // default not parsed
        sequenceDiagram: true,  // default not parsed
    });
})

function procImages(postId) {
    $.get(`/Api/BlogPost/${postId}/`, function (res) {
        console.log(res)
        for (const imgElem of document.querySelectorAll('.post-content img')) {
            let originSrc = imgElem.getAttribute('src')
            let newSrc = `/media/blog/${res.data.id}/${originSrc}`
            console.log('originSrc', originSrc)
            console.log('newSrc', newSrc)
            imgElem.setAttribute('src', newSrc)
        }
    })
}
