const initTreeView = function (data) {
    return $('#post-toc-container').treeview({
        data,
        levels: 2,
        enableLinks: true,
        highlightSelected: false,
        showTags: true
    })
}

$(function () {
    let markdownContent = document.getElementById('post-markdown-content');
    if (markdownContent) {
        let editorMdView = editormd.markdownToHTML("post-markdown-content", {
            htmlDecode: true,
            tocm: true,    // Using [TOCM]
            emoji: true,
            taskList: true,
            tex: true,  // Default is not to parse
            flowChart: true,  // Default is not to parse
            sequenceDiagram: true,  // Default is not to parse
        });

        initTreeView(editorMdView.markdownTocTree);
    }
});

/**
 * Convert image links in the article
 *
 * @deprecated No longer needed, handled directly on the backend
 * @param postId
 */
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