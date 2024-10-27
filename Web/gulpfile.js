/// <binding BeforeBuild='min' Clean='clean' ProjectOpened='auto' />
"use strict";

// Load used gulp plugin
const gulp = require('gulp'),
    rimraf = require('rimraf'),
    concat = require('gulp-concat'),
    cssmin = require('gulp-cssmin'),
    rename = require('gulp-rename'),
    uglify = require('gulp-uglify'),
    changed = require('gulp-changed');

// Define paths
const paths = {
    root: './wwwroot/',
    css: './wwwroot/css/',
    js: './wwwroot/js/',
    lib: './wwwroot/lib/',
};

// CSS
paths.cssDist = path.css + '**/*.css'; // All CSS files
paths.minCssDist = paths.css + "**/*.min.css"; // All minified CSS files
paths.concatCssDist = paths.css + "app.min.css"; // Concatenated CSS file

// JS
path.jsDist = path.js + '**/*.js'; // All JS files
path.minJsDist = path.js + '**/*.min.js'; // All minified JS files
path.concatJsDist = path.js + 'app.min.js'; // Concatenated JS file

// Front-end libraries downloaded from npm
const libs = [
    { name: "jquery", path: "./node_modules/jquery/dist/**/*.*" },
    { name: "popper", dist: "./node_modules/popper.js/dist/**/*.*" },
    { name: "bootstrap", dist: "./node_modules/bootstrap/dist/**/*.*" },
    { name: "bootswatch", dist: "./node_modules/bootswatch/dist/**/*.*" },
    { name: "prismjs", dist: "./node_modules/prismjs/**/*.*" },
];

// Clean Concatenated CSS and JS files
gulp.task("clean:css", done => rimraf(paths.minCssDist, done));
gulp.task("clean:js", done => rimraf(paths.minJsDist, done));

gulp.task("clean", gulp.series(["clean:js", "clean:css"]));

// Move front-end libraries to wwwroot folder
gulp.task("move", done => {
    libs.forEach(function (item) {
        gulp.src(item.dist)
            .pipe(gulp.dest(paths.lib + item.name + "/dist"));
    });
    done()
});

// Concatenate and minify CSS files
gulp.task("min:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], { base: "." })
        .pipe(rename({ suffix: '.min' }))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest('.'));
});

// Combine all CSS files into app.min.css
gulp.task("concatmin:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], { base: "." })
        .pipe(concat(paths.concatCssDist))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

// Concatenate and minify JS files
gulp.task("min:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], { base: "." })
        .pipe(rename({ suffix: '.min' }))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest('.'));
});

// Combine all JS files into app.min.js
gulp.task("concatmin:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], { base: "." })
        .pipe(concat(paths.concatJsDist))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});


gulp.task("min", gulp.series(["min:css", "min:js"]));
gulp.task("concatmin", gulp.series(["concatmin:css", "concatmin:js"]));

// Watch for changes in CSS and JS files
gulp.task("auto", () => {
    gulp.watch(paths.css, gulp.series(["min:css", "concatmin:css"]));
    gulp.watch(paths.js, gulp.series(["min:js", "concatmin:js"]));
});
