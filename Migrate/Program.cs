// See https://aka.ms/new-console-template for more information

using Contrib.Utils;
using Data;
using Data.Models;
using Migrate;

const string importDir = @"E:\blog\";

var assetsPath = Path.GetFullPath("../Web/wwwroot/media/blog");

var exclusionDirs = new List<string> { ".git", "logseq", "pages" };

// Delete old files
var removeFileList = new List<string> { "app.db", "app.db-shm", "app.db-wal" };
foreach (var filename in removeFileList.Where(File.Exists))
{
    Console.WriteLine($"Delete {filename}");
    File.Delete(filename);
}

// Database
var freeSql = FreeSqlFactory.Create("Data Source=app.db;Synchronous=Off;Cache Size=5000;");
var postRepo = freeSql.GetRepository<Post>();
var categoryRepo = freeSql.GetRepository<Category>();

// Import data
WalkDirectoryTree(new DirectoryInfo(importDir));

// Overwrite database
var destFile = Path.GetFullPath("../Web/app.db");
Console.WriteLine($"Overwrite {destFile}");
File.Copy("app.db", destFile, true);

void WalkDirectoryTree(DirectoryInfo root)
{
    // Reference: https://docs.microsoft.com/zh-cn/dotnet/csharp/programming-guide/file-system/how-to-iterate-through-a-directory-tree

    Console.WriteLine($"Scanning folder: {root.FullName}");

    FileInfo[]? files = null;
    DirectoryInfo[]? subDirs = null;

    // First, process all the files directly under this folder
    try
    {
        files = root.GetFiles("*.md");
    }
    // This is thrown if even one of the files requires permissions greater
    // than the application provides.
    catch (UnauthorizedAccessException e)
    {
        // This code just writes out the message and continues to recurse.
        // You may decide to do something different here. For example, you
        // can try to elevate your privileges and access the file again.
        Console.WriteLine(e.Message);
    }

    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine(e.Message);
    }

    if (files != null)
        foreach (var fi in files)
        {
            // In this example, we only access the existing FileInfo object. If we
            // want to open, delete or modify the file, then
            // a try-catch block is required here to handle the case
            // where the file has been deleted since the call to TraverseTree().
            Console.WriteLine(fi.FullName);
            var postPath = fi.DirectoryName!.Replace(importDir, "");

            var categoryNames = postPath.Split("\\");
            Console.WriteLine($"categoryNames: {string.Join(",", categoryNames)}");
            var categories = new List<Category>();
            if (categoryNames.Length > 0)
            {
                var rootCategory = categoryRepo.Where(a => a.Name == categoryNames[0]).First()
                                   ?? categoryRepo.Insert(new Category { Name = categoryNames[0] });
                categories.Add(rootCategory);
                Console.WriteLine($"+ Add category: {rootCategory.Id}.{rootCategory.Name}");
                for (var i = 1; i < categoryNames.Length; i++)
                {
                    var name = categoryNames[i];
                    var parent = categories[i - 1];
                    var category = categoryRepo.Where(
                                       a => a.ParentId == parent.Id && a.Name == name).First()
                                   ?? categoryRepo.Insert(new Category
                                   {
                                       Name = name,
                                       ParentId = parent.Id
                                   });
                    categories.Add(category);
                    Console.WriteLine($"+ Add subcategory: {category.Id}.{category.Name}");
                }
            }

            var reader = fi.OpenText();
            var content = reader.ReadToEnd();
            reader.Close();

            // Save the original file
            var post = new Post
            {
                Id = GuidUtils.GuidTo16String(),
                Title = fi.Name.Replace(".md", ""),
                Content = content,
                Path = postPath,
                CreationTime = fi.CreationTime,
                LastModifiedTime = fi.LastWriteTime,
                CategoryId = categories[^1].Id,
                Categories = string.Join(",", categories.Select(a => a.Id))
            };

            // Process article content
            // When importing articles, import images within the article and perform relative path replacement for images
            var processor = new PostProcessor(importDir, assetsPath, post);
            post.Content = processor.MarkdownParse();
            post.Summary = processor.GetSummary(200);

            postRepo.Insert(post);
        }

    // Now find all the subdirectories under this directory.
    subDirs = root.GetDirectories();

    foreach (var dirInfo in subDirs)
    {
        if (exclusionDirs.Contains(dirInfo.Name)) continue;

        if (dirInfo.Name.EndsWith(".assets")) continue;

        // Recursive call for each subdirectory.
        WalkDirectoryTree(dirInfo);
    }
}