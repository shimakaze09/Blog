# Blog

A blog system that supports Markdown import

## Technology

- Backend: C# + AspNetCore
- ORM: FreeSQL
- Markdown Parsing: [Markdig](https://github.com/xoofx/markdig)
- Pagination: X.PagedList
- Blog Frontend: Bootstrap + Vue + editor.md + bootswatch
- Admin Panel: Vue + Vuex + VueRouter
- Admin Panel UI: SCSS + ElementUI

## Project Structure

- `Contrib`: Shared libraries
- `Data`: Data models
- `Migrate`: Blog post import, run this project to batch import markdown posts
- `Web`: Main blog project

### Frontend Resources

the frontend resources of this project are managed using `NPM` + `Gulp`，You can install dependencies using `NPM` or
`Yarn`：

Run the following commands in the `Web` directory

```bash
npm install
# or
yarn
```

Install the `gulp-cli` tool:

```bash
npm install --global gulp-cli
```

Execute the gulp task (also in the `Web` directory):

```bash
gulp move
```

Then run the `Web` project.

### Initialization

After the project starts, you need to go to the initialization page to create an administrator and perform other
operations. Initialization entry: `/Home/Init`
**Note: Initialization can only be performed once**

## 3rd Party Components

- ORM: FreeSQL
- MD Parser: [Markdig](https://github.com/xoofx/markdig)
- Code Highlighting:
    - [Markdig.Prism](https://github.com/ilich/Markdig.Prism): Front-end rendering, but requires server-side component
      cooperation
    - [Markdown.ColorCode](https://github.com/wbaldoumas/markdown-colorcode): Server-side rendering
- Pagination: X.PagedList