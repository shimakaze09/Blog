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
    // let editorMdView = editormd.markdownToHTML("test-editormd-view", {
    //     // htmlDecode: "style,script,iframe",  // you can filter tags decode
    //     htmlDecode: true,
    //     //toc             : false,
    //     tocm: true,    // Using [TOCM]
    //     tocContainer: "#custom-toc-container", // Custom ToC container layer
    //     //gfm             : false,
    //     //tocDropdown     : true,
    //     // markdownSourceCode : true, // Whether to retain the Markdown source code, i.e., whether to delete the Textarea tag that saves the source code
    //     emoji: true,
    //     taskList: true,
    //     tex: true,  // Not parsed by default
    //     flowChart: true,  // Not parsed by default
    //     sequenceDiagram: true,  // Not parsed by default
    // });
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
