module SkunkUtils

module Disk =
    open System.IO

    let readFile (path: string) =
        if File.Exists(path) then File.ReadAllText(path) else ""

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
