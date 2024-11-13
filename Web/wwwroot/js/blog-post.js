let toastTrigger = document.getElementById('liveToastBtn')
let toastLiveExample = document.getElementById('liveToast')
if (toastTrigger) {
    toastTrigger.addEventListener('click', function () {
        let toast = new bootstrap.Toast(toastLiveExample)

        toast.show()
    })
}

class TocNode {
    constructor(text, href, tags, nodes) {
        this.text = text
        this.href = href
        this.tags = tags
        this.nodes = nodes
    }
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

    let toc = editorMdView.markdownToC
    for (let i = 0; i < toc.length; i++) {
        let item = toc[i]
        item.id = i
        item.pid = -1
        for (let j = i; j >= 0; j--) {
            let preItem = toc[j]
            if (item.level === preItem.level + 1) {
                item.pid = j
                break
            }
        }
    }

    function getNodes(pid = -1) {
        let nodes = toc.filter(item => item.pid === pid)
        if (nodes.length === 0) return null

        return nodes.map(item => new TocNode(item.text, `#${item.text}`, null, getNodes(item.id)))
    }

    let nodes = getNodes()

    $('#post-toc-container').treeview({
        data: nodes,
        levels: 2,
        enableLinks: true,
        highlightSelected: false,
        showTags: true,
        enableIndent: false,
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