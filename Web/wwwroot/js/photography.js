window.addEventListener("error", function (e) {
    let stack = e.error.stack;
    let message = e.error.toString();

    if (stack) {
        message += '\n' + stack;
    }

    console.log(stack);

    let xhr = new XMLHttpRequest();
    xhr.open("POST", "/log", true);
    // Fire ajax request with error message
    xhr.send(message);
});

let animationSpeed = 1000;

let poem;
let poemIndex = 0;
fetch('http://dc.sblt.deali.cn:9800/poem/tang').then(res => res.json())
    .then(result => {
        poem = result.data.content.join('');
        console.log(poem);
        for (let tile of document.getElementsByClassName('tile-sm')) {
            let divContent = document.createElement('div');
            divContent.className = 'tile-content';
            if (poemIndex >= poem.length - 1)
                poemIndex = 0;
            divContent.innerHTML = poem[poemIndex++];
            divContent.hidden = true;
            let divBg = document.createElement('div');
            divBg.className = 'tile-bg';
            divBg.style.background = randomImageUrl(100, 100);
            divBg.style.backgroundColor = randomColor();
            tile.appendChild(divBg);
            tile.appendChild(divContent);
            setFlipInterval(tile);
        }
    });

for (let tile of document.querySelectorAll('.tile-md')) {
    let divBg = document.createElement('div');
    divBg.className = 'tile-bg';
    divBg.style.background = randomImageUrl(205, 205);
    divBg.style.backgroundColor = randomColor();
    tile.appendChild(divBg);
    setFlipInterval(tile);
}

for (let tile of document.querySelectorAll('.tile-wd')) {
    let divBg = document.createElement('div');
    divBg.className = 'tile-bg';
    divBg.style.background = randomImageUrl(415, 205);
    divBg.style.backgroundColor = randomColor();
    tile.appendChild(divBg);
    setFlipInterval(tile);
}

for (let tile of document.querySelectorAll('.tile-lg')) {
    let divBg = document.createElement('div');
    divBg.className = 'tile-bg';
    divBg.style.background = randomImageUrl(415, 415);
    divBg.style.backgroundColor = randomColor();
    tile.appendChild(divBg);
    setFlipInterval(tile);
}

function randomImageUrl(width, height) {
    let url = `https://picsum.photos/${width}/${height}?random=${Math.round(Math.random() * 100)}`;
    return `url("${url}")`;
}

function randomColor() {
    return `rgba(${Math.round(Math.random() * 255)},${Math.round(Math.random() * 255)},${Math.round(Math.random() * 255)},1)`;
}

// Generate a random number between min and max
function randomNum(minNum, maxNum) {
    switch (arguments.length) {
        case 1:
            return parseInt(Math.random() * minNum + 1, 10);
        case 2:
            return parseInt(Math.random() * (maxNum - minNum + 1) + minNum, 10);
        default:
            return 0;
    }
}

function setFlipInterval(tile) {
    let tileContent = tile.querySelector('.tile-content');
    return setInterval(() => {
        setTimeout(() => {
            // Determine whether to flip to the front or back
            // Front
            if (tile.getAttribute('init') == null) {
                // Rotation animation
                tile.style.animation = `tile-flip ${animationSpeed / 500}s linear infinite`;
                // Background darkening animation
                tile.querySelector('.tile-bg').style.animation = `tile-bg-dark ${animationSpeed / 500}s linear infinite`;
                // Content gradually appearing animation
                if (tileContent) {
                    tileContent.hidden = false;
                    tileContent.style.animation = `tile-content-show ${animationSpeed / 250}s linear infinite`;
                }
                tile.setAttribute('init', 'false');
                setTimeout(() => {
                    // Background flips with the animation
                    tile.style.transform = `rotateY(180deg)`;
                    // Disable tile flip animation
                    tile.style.animation = 'none';
                    // Disable background gradient animation
                    tile.querySelector('.tile-bg').style.animation = 'none';
                    // Disable content animation
                    if (tileContent) tileContent.style.animation = 'none';
                    // Background dims with the gradient animation
                    tile.querySelector('.tile-bg').classList.add('tile-bg-dim');
                }, animationSpeed);
            }
            // Back
            else {
                // Rotation animation
                tile.style.animation = `tile-flip2 ${animationSpeed / 500}s linear infinite`;
                // Background lightening animation
                tile.querySelector('.tile-bg').style.animation = `tile-bg-light ${animationSpeed / 500}s linear infinite`;
                tile.removeAttribute('init');
                // Content gradually disappearing animation
                if (tileContent) {
                    tileContent.style.animation = `tile-content-hide ${animationSpeed / 250}s linear infinite`;
                }
                setTimeout(() => {
                    tile.style.transform = `rotateY(0)`;
                    // Disable tile flip animation
                    tile.style.animation = 'none';
                    // Disable background animation
                    tile.querySelector('.tile-bg').style.animation = 'none';
                    // Disable content animation
                    if (tileContent) {
                        tileContent.style.animation = 'none';
                        tileContent.hidden = true;
                    }
                    tile.querySelector('.tile-bg').className = 'tile-bg';
                }, animationSpeed);
            }
        }, animationSpeed);
    }, randomNum(8000, 15000));
}
