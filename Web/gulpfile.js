/// <binding BeforeBuild='min' Clean='clean' ProjectOpened='auto' />

"use strict";

// Load required gulp plugins
const gulp = require("gulp"),
    rimraf = require("rimraf"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-clean-css"),
    rename = require("gulp-rename"),
    uglify = require("gulp-uglify"),
    changed = require("gulp-changed"),
    obfuscate = require('gulp-obfuscate');

// Define paths for files in wwwroot directory
const paths = {
    root: './wwwroot/',
    css: './wwwroot/css/',
    js: './wwwroot/js/',
    lib: './wwwroot/lib/'
};

// CSS paths
paths.cssDist = paths.css + "**/*.css"; // Match all CSS file locations
paths.minCssDist = paths.css + "**/*.min.css"; // Match all compressed CSS file locations
paths.concatCssDist = paths.css + "app.min.css"; // Path after concatenating all CSS files

// JS paths
paths.jsDist = paths.js + "**/*.js"; // Match all JS file locations
paths.minJsDist = paths.js + "**/*.min.js"; // Match all compressed JS file locations
paths.concatJsDist = paths.js + "app.min.js"; // Path after concatenating all JS files

// Paths for npm downloaded frontend component packages
const libs = [
    {name: "axios", dist: "./node_modules/axios/dist/**/*.*"},
    {name: "chart.js", dist: "./node_modules/chart.js/dist/**/*.*"},
    {name: "jquery", dist: "./node_modules/jquery/dist/**/*.*"},
    {name: "popper", dist: "./node_modules/popper.js/dist/**/*.*"},
    {name: "bootstrap", dist: "./node_modules/bootstrap/dist/**/*.*"},
    {name: "bootstrap5-treeview", dist: "./node_modules/bootstrap5-treeview/dist/**/*.*"},
    {name: "bootswatch", dist: "./node_modules/bootswatch/dist/**/*.*"},
    {name: "prismjs", dist: "./node_modules/prismjs/**/*.*"},
    {name: 'vue', dist: './node_modules/vue/dist/**/*.*'},
    {name: 'masonry-layout', dist: './node_modules/masonry-layout/dist/*.*'},
];

// Paths for npm downloaded frontend components, custom storage location
const customLibs = [
    {name: "dayjs", dist: "./node_modules/dayjs/**/*.*"},
    {name: "editormd", dist: "./node_modules/editor.md-ext/*.js"},
    {name: "editormd/css", dist: "./node_modules/editor.md-ext/css/*.*"},
    {name: "editormd/fonts", dist: "./node_modules/editor.md-ext/fonts/*.*"},
    {name: "editormd/images", dist: "./node_modules/editor.md-ext/images/*.*"},
    {name: "editormd/languages", dist: "./node_modules/editor.md-ext/languages/*.*"},
    {name: "editormd/lib", dist: "./node_modules/editor.md-ext/lib/*.*"},
    {name: "editormd/plugins", dist: "./node_modules/editor.md-ext/plugins/*.*"},
    {name: "editormd/examples/js", dist: "./node_modules/editor.md-ext/examples/js/*.js"},
    {name: 'element-ui/lib', dist: './node_modules/element-ui/lib/index.js'},
    {name: 'element-ui/lib/theme-chalk', dist: './node_modules/element-ui/lib/theme-chalk/index.css'},
    {name: 'element-ui/lib/theme-chalk/fonts', dist: './node_modules/element-ui/lib/theme-chalk/fonts/*.*'},
    {name: 'font-awesome', dist: './node_modules/@fortawesome/fontawesome-free/**/*.*'},
    {name: 'highlight.js', dist: './node_modules/highlight.js/**/*.*'},
    {name: 'github-markdown-css', dist: './node_modules/github-markdown-css/*.css'},
    {name: 'imagesloaded', dist: './node_modules/imagesloaded/*.js'},
]

// Tasks for cleaning compressed files
gulp.task("clean:css", done => rimraf(paths.minCssDist, done));
gulp.task("clean:js", done => rimraf(paths.minJsDist, done));

gulp.task("clean", gulp.series(["clean:js", "clean:css"]));

// Tasks for moving npm downloaded frontend component packages to wwwroot directory
gulp.task("move:dist", done => {
    libs.forEach(item => {
        gulp.src(item.dist)
            .pipe(gulp.dest(paths.lib + item.name + "/dist"));
    });
    done()
})
gulp.task("move:custom", done => {
    customLibs.forEach(item => {
        gulp.src(item.dist)
            .pipe(gulp.dest(paths.lib + item.name))
    })
    done()
})

// Tasks for minifying CSS files
gulp.task("min:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], {base: "."})
        .pipe(rename({suffix: '.min'}))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest('.'));
});

// Task for concatenating all CSS files
gulp.task("concat:css", () => {
    return gulp.src([paths.cssDist, "!" + paths.minCssDist], {base: "."})
        .pipe(concat(paths.concatCssDist))
        .pipe(changed('.'))
        .pipe(cssmin())
        .pipe(gulp.dest("."));
});

// Tasks for minifying JS files
gulp.task("min:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], {base: "."})
        .pipe(rename({suffix: '.min'}))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest('.'));
});

// Task for concatenating all JS files
gulp.task("concat:js", () => {
    return gulp.src([paths.jsDist, "!" + paths.minJsDist], {base: "."})
        .pipe(concat(paths.concatJsDist))
        .pipe(changed('.'))
        .pipe(uglify())
        .pipe(gulp.dest("."));
});

gulp.task('move', gulp.series(['move:dist', 'move:custom']))
gulp.task("min", gulp.series(["min:js", "min:css"]))
gulp.task("concat", gulp.series(["concat:js", "concat:css"]))

// Watch tasks for automatic execution on file changes
gulp.task("auto", () => {
    gulp.watch(paths.css, gulp.series(["min:css", "concat:css"]));
    gulp.watch(paths.js, gulp.series(["min:js", "concat:js"]));
});
