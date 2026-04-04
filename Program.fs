open System.IO
open SkunkUtils
open SkunkHtml

[<EntryPoint>]
let main argv =
    argv |> ignore

    if not (Directory.Exists(Config.markdownDir)) then
        printfn $"Markdown directory does not exist : {Config.markdownDir}"
        failwith "Markdown directory not found"

    if not (Directory.Exists(Config.outputDir)) then
        printfn $"Creating {Path.GetFileName Config.outputDir} folder"
        Directory.CreateDirectory(Config.outputDir)
        |> ignore

    let header = Disk.readFile (Path.Combine(Config.htmlDir, "header.html"))
    let footer = Disk.readFile (Path.Combine(Config.htmlDir, "footer.html"))

    let allMarkdownFiles = Directory.GetFiles(Config.markdownDir, "*.md")

    let blogArticleFiles =
        allMarkdownFiles
        |> Array.filter isArticle

    let listOfAllBlogArticles =
        blogArticleFiles
        |> Array.map (fun file ->
            let date = Path.GetFileNameWithoutExtension(file)
            let title = extractTitleFromMarkdownFile(file)
            let description = extractDescriptionFromMarkdownFile(file)
            let urlFriendlyTitle = Url.toUrlFriendly title
            (date, title, $"{urlFriendlyTitle}.html", description))
        |> Array.sortByDescending (fun (date, _, _, _) -> date)
        |> Array.toList

    let otherMarkdownFiles =
        allMarkdownFiles
        |> Array.filter (fun file -> not (isArticle file))
        |> Array.filter (fun file -> Path.GetFileName(file) <> Config.frontPageMarkdownFileName)

    let otherPageLinks =
        otherMarkdownFiles
        |> Array.map (fun file ->
            let title = extractTitleFromMarkdownFile(file)
            $"{Url.toUrlFriendly title}.html")
        |> Array.toList

    let listForIndex =
        listOfAllBlogArticles
        |> List.map (fun (date, title, link, _) -> (date, title, link))

    let createBlogArticlePages () =
        blogArticleFiles
        |> Array.iter (createPage header footer)

    let createOtherPages () =
        otherMarkdownFiles
        |> Array.iter (createPage header footer)

    createIndexPage header footer listForIndex
    createOtherPages ()
    createBlogArticlePages ()

    // Generate RSS feed and sitemap
    createRssFeed listOfAllBlogArticles
    createSitemap listOfAllBlogArticles otherPageLinks

    Disk.copyFolderToOutput Config.fontsDir Config.outputFontsDir
    Disk.copyFolderToOutput Config.cssDir Config.outputCssDir
    Disk.copyFolderToOutput Config.imagesDir Config.outputImagesDir
    Disk.copyFolderToOutput Config.assetsDir Config.outputAssetsDir
    Disk.copyFolderToOutput Config.scriptsDir Config.outputScriptsDir

    printf "\nBuild complete. Your site is ready for deployment!"
    0
