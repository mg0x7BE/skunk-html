open System.IO
open SkunkUtils
open SkunkHtml

[<EntryPoint>]
let main _ =
    if not (Directory.Exists(Config.markdownDir)) then
        printfn $"Markdown directory does not exist : {Config.markdownDir}"
        failwith "Markdown directory not found"

    if not (Directory.Exists(Config.outputDir)) then
        printfn $"Creating {Path.GetFileName Config.outputDir} folder"
        Directory.CreateDirectory(Config.outputDir) |> ignore

    let header = Disk.readFile (Path.Combine(Config.htmlDir, "header.html"))
    let footer = Disk.readFile (Path.Combine(Config.htmlDir, "footer.html"))

    let pages =
        Directory.GetFiles(Config.markdownDir, "*.md")
        |> Array.filter (fun file -> Path.GetFileName(file) <> Config.frontPageMarkdownFileName)
        |> Array.map loadPage
        |> Array.toList

    pages
    |> List.countBy _.Link
    |> List.filter (fun (_, count) -> count > 1)
    |> List.iter (fun (link, _) ->
        printfn $"Warning! Multiple pages share the same title, so they all end up as {link} - only one will survive")

    let articlePages, otherPages = pages |> List.partition _.Date.IsSome
    let articles = articlePages |> List.sortByDescending _.Date

    createIndexPage header footer articles
    createNotFoundPage header footer
    pages |> List.iter (createPage header footer)

    createRssFeed articles
    createSitemap (articles @ otherPages)

    Disk.copyFolderToOutput Config.fontsDir Config.outputFontsDir
    Disk.copyFolderToOutput Config.cssDir Config.outputCssDir
    Disk.copyFolderToOutput Config.imagesDir Config.outputImagesDir
    Disk.copyFolderToOutput Config.assetsDir Config.outputAssetsDir
    Disk.copyFolderToOutput Config.scriptsDir Config.outputScriptsDir

    printfn "\nBuild complete. Your site is ready for deployment!"
    0
