module SkunkUtils

module Config =
    open System.IO

    let sourceDir = __SOURCE_DIRECTORY__

    let markdownDir = Path.Combine(sourceDir, "markdown-blog")
    let htmlDir = Path.Combine(sourceDir, "html")
    let outputDir = Path.Combine(sourceDir, "skunk-html-output")

    let cssDir = Path.Combine(sourceDir, "css")
    let outputCssDir = Path.Combine(outputDir, "css")

    let fontsDir = Path.Combine(sourceDir, "fonts")
    let outputFontsDir = Path.Combine(outputDir, "fonts")

    let imagesDir = Path.Combine(markdownDir, "images")
    let outputImagesDir = Path.Combine(outputDir, "images")

    let assetsDir = Path.Combine(sourceDir, "assets")
    let outputAssetsDir = Path.Combine(outputDir, "assets")

    let scriptsDir = Path.Combine(sourceDir, "scripts")
    let outputScriptsDir = Path.Combine(outputDir, "scripts")

    let frontPageMarkdownFileName = "index.md"

    // --- Site metadata (edit these for your site) ---
    // Only change the values in quotes — the rest is just labels.
    let siteTitle = "SkunkHTML"
    let siteDescription = "The simplest blog on GitHub Pages. Fork, enable Pages, write Markdown."
    let siteBaseUrl = "https://mg0x7be.github.io/skunk-html"  // No trailing slash. Include repo name if using project pages.
    let siteLanguage = "en"
    let siteAuthor = ""  // Optional, used in RSS feed and meta tags

module Disk =
    open System.IO

    let readFile (path: string) =
        path
        |> File.Exists
        |> function
            | true -> File.ReadAllText(path)
            | false -> ""

    let writeFile (path: string) (content: string) =
        File.WriteAllText(path, content)
        printfn $"Generated: {Path.GetFileName path} -> {path}\n"

    let copyFolderToOutput (sourceFolder: string) (destinationFolder: string) =
        if not (Directory.Exists(sourceFolder)) then
            printfn $"Source folder does not exist: {sourceFolder}"
        else
            if not (Directory.Exists(destinationFolder)) then
                Directory.CreateDirectory(destinationFolder)
                |> ignore

            Directory.GetFiles(sourceFolder)
            |> Array.iter (fun file ->
                let fileName = Path.GetFileName(file)
                let destFile = Path.Combine(destinationFolder, fileName)
                printfn $"Copying: {fileName} -> {destFile}"
                File.Copy(file, destFile, true))

module Url =
    open System.Text.RegularExpressions

    let toUrlFriendly (input: string) =
        input.ToLowerInvariant()
        |> fun text -> Regex.Replace(text, @"[^\w\s]", "") // Remove all non-alphanumeric characters
        |> fun text -> Regex.Replace(text, @"\s+", "-") // Replace spaces with hyphens

    /// Ensure base URL has no trailing slash
    let normalizeBaseUrl (url: string) =
        url.TrimEnd('/')

module Xml =
    /// Escape special characters for XML content
    let escape (input: string) =
        input
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;")

module MarkdownUtils =
    open System.Text.RegularExpressions

    /// Strip basic Markdown syntax to produce plain text (for meta descriptions)
    let stripMarkdownSyntax (input: string) =
        input
        |> fun s -> Regex.Replace(s, @"\[([^\]]+)\]\([^\)]+\)", "$1")  // [text](url) -> text
        |> fun s -> Regex.Replace(s, @"[*_]{1,3}", "")                // bold/italic markers
        |> fun s -> Regex.Replace(s, @"`([^`]+)`", "$1")              // inline code
        |> fun s -> s.Trim()
