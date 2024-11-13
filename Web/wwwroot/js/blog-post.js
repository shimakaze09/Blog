let toastTrigger = document.getElementById('liveToastBtn')
let toastLiveExample = document.getElementById('liveToast')
if (toastTrigger) {
    toastTrigger.addEventListener('click', function () {
        let toast = new bootstrap.Toast(toastLiveExample)

        toast.show()
    })
}

$(function () {
    let editorMdView = editormd.markdownToHTML("post-markdown-content", {
        // htmlDecode: "style,script,iframe",  // you can filter tags decode
        htmlDecode: true,
        //toc             : false,
        tocm: true,    // Using [TOCM]
        // tocContainer: "#toc-container", // Custom ToC container layer
        //gfm             : false,
        //tocDropdown     : true,
        // markdownSourceCode : true, // Whether to retain the Markdown source code, that is, whether to delete the Textarea tag that saves the source code
        emoji: true,
        taskList: true,
        tex: true,  // Not parsed by default
        flowChart: true,  // Not parsed by default
        sequenceDiagram: true,  // Not parsed by default
    });

    $('#post-toc-container').treeview({
        data: editorMdView.markdownTocTree,
        levels: 2,
        enableLinks: true,
        highlightSelected: false,
        showTags: true,
        onNodeSelected: function (event, data) {
            console.log(data)
        },
        onNodeUnselected: function (event, data) {
        },
        // selectedBackColor: "rgba(220, 2, 7, 0.68)",
        // onhoverColor: "rgba(0,0,0,.8)",
        // showBorder: false,
    })
})

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