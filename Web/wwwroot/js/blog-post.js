// Get elements by ID
let toastTrigger = document.getElementById('liveToastBtn');
let toastLiveExample = document.getElementById('liveToast');

// Check if toastTrigger exists
if (toastTrigger) {
    // Add event listener to toastTrigger
    toastTrigger.addEventListener('click', function () {
        // Create a new toast instance
        let toast = new bootstrap.Toast(toastLiveExample);

        // Show the toast
        toast.show();
    });
}

// Define a class for TOC (Table of Contents) nodes
class TocNode {
    constructor(text, href, tags, nodes) {
        this.text = text;
        this.href = href;
        this.tags = tags;
        this.nodes = nodes;
    }
}

// jQuery document ready function
$(function () {
    // Initialize editormd with options
    let editorMdView = editormd.markdownToHTML("test-editormd-view", {
        htmlDecode: true,
        tocm: true,
        tocContainer: "#custom-toc-container",
        emoji: true,
        taskList: true,
        tex: true,
        flowChart: true,
        sequenceDiagram: true,
    });

    // Get the Table of Contents object
    let toc = editorMdView.markdownToC;

    // Log the Table of Contents to console
    console.log(toc);

    // Function to convert TOC to tree structure
    function getNodes(index = 0) {
        let nodes = [];

        for (let i = index; i < toc.length - 1; i++) {
            let item = toc[i];
            let nextItem = toc[i + 1];
            let node = new TocNode(item.text, `#${item.text}`, null, null);

            if (item.level === nextItem.level) {
                nodes.push(node);
            }
            if (item.level < nextItem.level) {
                node.nodes = getNodes(i + 1);
                nodes.push(node);
                i += node.nodes.length;
                continue;
            }
            if (item.level > nextItem.level) {
                nodes.push(node);
                i++;
                break;
            }
        }

        return nodes;
    }

    // Convert TOC to tree structure and log result
    let nodeList = getNodes();
    console.log(nodeList);
});

/**
 * Process images in an article
 *
 * @deprecated Now not needed, handled on backend instead
 * @param {number} postId
 */
function procImages(postId) {
    $.get(`/Api/BlogPost/${postId}/`, function (res) {
        console.log(res);
        for (const imgElem of document.querySelectorAll('.post-content img')) {
            let originSrc = imgElem.getAttribute('src');
            let newSrc = `/media/blog/${res.data.id}/${originSrc}`;
            console.log('originSrc', originSrc);
            console.log('newSrc', newSrc);
            imgElem.setAttribute('src', newSrc);
        }
    });
}
