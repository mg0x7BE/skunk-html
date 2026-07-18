module SkunkHtml

open SkunkUtils
open System
open System.Globalization
open System.IO
open FSharp.Formatting.Markdown

/// A single markdown page. Blog posts carry a publication date taken from the
/// file name (e.g. 2025-03-24.md); other pages (about, links, ...) do not.
type Page =
    { SourcePath: string
      Title: string
      Link: string
      Description: string
      Date: string option }

let private baseUrl = Url.normalizeBaseUrl Config.siteBaseUrl

let private headTemplate =
    Path.Combine(Config.htmlDir, "head.html")
    |> Disk.readFile

let private highlightingScript =
    Path.Combine(Config.htmlDir, "script_syntax_highlighting.html")
    |> Disk.readFile

let private giscusScript =
    Path.Combine(Config.htmlDir, "script_giscus.html")
    |> Disk.readFile

let generateFinalHtml (head: string) (header: string) (footer: string) (content: string) (script: string) =
    $"""
    <!DOCTYPE html>
    <html lang="{Config.siteLanguage}" data-color-mode="user">
    <head>
        {head}
    </head>
    <body>
        <header>
            {header}
        </header>
        <main>
            {content}
        </main>
        <hr>
        <footer>
            {footer}
        </footer>
        <script>
            {script}
        </script>
    </body>
    </html>
    """

let head (titleSuffix: string) (description: string) (canonicalUrl: string) (ogType: string) =
    let fullTitle = Config.siteTitle + titleSuffix

    let seoMeta =
        let desc = if String.IsNullOrWhiteSpace(description) then Config.siteDescription else description
        let authorMeta =
            if String.IsNullOrWhiteSpace(Config.siteAuthor) then ""
            else $"""<meta name="author" content="{Xml.escape Config.siteAuthor}">"""
        let imageMeta =
            if String.IsNullOrWhiteSpace(Config.siteImage) then ""
            else
                let imageUrl = $"{baseUrl}/{Config.siteImage.TrimStart('/')}"
                $"""<meta property="og:image" content="{imageUrl}">
        <meta name="twitter:image" content="{imageUrl}">"""
        $"""
        <meta name="description" content="{Xml.escape desc}">
        {authorMeta}
        <meta property="og:title" content="{Xml.escape fullTitle}">
        <meta property="og:description" content="{Xml.escape desc}">
        <meta property="og:type" content="{ogType}">
        <meta property="og:url" content="{canonicalUrl}">
        {imageMeta}
        <meta name="twitter:card" content="summary">
        <meta name="twitter:title" content="{Xml.escape fullTitle}">
        <meta name="twitter:description" content="{Xml.escape desc}">
        <link rel="canonical" href="{canonicalUrl}">
        <link rel="alternate" type="application/rss+xml" title="{Xml.escape Config.siteTitle}" href="{baseUrl}/feed.xml">
        """

    headTemplate.Replace("{{site-title}}", Xml.escape fullTitle) + seoMeta

let private isArticle (file: string) =
    Char.IsDigit(Path.GetFileName(file)[0])

let loadPage (markdownFilePath: string) : Page =
    let lines = File.ReadAllLines(markdownFilePath)

    let title =
        lines
        |> Array.tryFind _.StartsWith("# ")
        |> Option.map _.TrimStart('#').Trim()
        |> Option.defaultValue Config.untitledPageTitle

    let description =
        lines
        |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
        |> Array.filter (fun line -> not (line.StartsWith("#")))
        |> Array.tryHead
        |> Option.defaultValue ""
        |> MarkdownUtils.stripMarkdownSyntax
        |> fun s -> if s.Length > 160 then s[..159] + "..." else s

    let date =
        if isArticle markdownFilePath then
            Some(Path.GetFileNameWithoutExtension(markdownFilePath))
        else
            None

    { SourcePath = markdownFilePath
      Title = title
      Link = Url.toUrlFriendly title + ".html"
      Description = description
      Date = date }

let createPage (header: string) (footer: string) (page: Page) =
    let canonicalUrl = $"{baseUrl}/{page.Link}"
    let markdownContent = File.ReadAllText(page.SourcePath)

    let htmlContent, ogType =
        match page.Date with
        | None -> Markdown.ToHtml(markdownContent), "website"
        | Some date ->
            let publicationDate =
                $"""<p class="publication-date">{Config.publishedOnText} <time datetime="{date}">{date}</time></p>"""
            let articleHtml = Markdown.ToHtml(markdownContent + "\n\n" + publicationDate + "\n\n")
            articleHtml + giscusScript, "article"

    let finalHtmlContent =
        generateFinalHtml (head (" - " + page.Title) page.Description canonicalUrl ogType) header footer htmlContent highlightingScript

    printfn $"Processing {Path.GetFileName page.SourcePath} ->"
    Disk.writeFile (Path.Combine(Config.outputDir, page.Link)) finalHtmlContent

let createIndexPage (header: string) (footer: string) (articles: Page list) =
    let frontPageMarkdownFilePath = Path.Combine(Config.markdownDir, Config.frontPageMarkdownFileName)

    let frontPageContentHtml =
        if File.Exists(frontPageMarkdownFilePath) then
            printfn $"Processing {Path.GetFileName frontPageMarkdownFilePath} ->"
            Markdown.ToHtml(File.ReadAllText(frontPageMarkdownFilePath))
        else
            printfn $"Warning! File {Config.frontPageMarkdownFileName} does not exist! The main page will only contain blog entries, without a welcome message"
            ""

    let articleListHtml =
        articles
        |> List.map (fun article ->
            let datePrefix =
                match article.Date with
                | Some date -> $"{date}: "
                | None -> ""
            $"""<li>{datePrefix}<a href="{article.Link}">{Xml.escape article.Title}</a></li>""")
        |> String.concat "\n"

    let content =
        $"""
        {frontPageContentHtml}
        <section class="publications">
            <h2>{Config.blogEntriesHeading}</h2>
            <ul>
            {articleListHtml}
            </ul>
        </section>
        """

    let canonicalUrl = baseUrl + "/"
    let frontPageHtmlContent = generateFinalHtml (head "" Config.siteDescription canonicalUrl "website") header footer content highlightingScript

    Disk.writeFile (Path.Combine(Config.outputDir, "index.html")) frontPageHtmlContent

/// Auto-generated 404 page, served by GitHub Pages for any missing URL.
/// The <base> tag makes relative links work at any path depth. Written before
/// the regular pages, so an explicit page with the same file name wins.
let createNotFoundPage (header: string) (footer: string) =
    let content =
        $"""
        <h1>404</h1>
        <p>{Config.notFoundMessage}</p>
        <p><a href="index.html">{Config.notFoundBackText}</a></p>
        """

    let baseTag = $"""<base href="{baseUrl}/">"""
    let headContent = baseTag + head (" - " + Config.notFoundTitle) "" $"{baseUrl}/404.html" "website"
    let finalHtmlContent = generateFinalHtml headContent header footer content highlightingScript

    Disk.writeFile (Path.Combine(Config.outputDir, "404.html")) finalHtmlContent

// --- RSS Feed Generation ---

/// RFC 1123 date for RSS; falls back to the raw string when the file name is not a date
let private toRssDate (date: string) =
    match DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None) with
    | true, parsed -> parsed.ToString("R")
    | _ -> date

let createRssFeed (articles: Page list) =
    let authorElement =
        if String.IsNullOrWhiteSpace(Config.siteAuthor) then ""
        else $"    <managingEditor>{Xml.escape Config.siteAuthor}</managingEditor>\n"

    let items =
        articles
        |> List.map (fun article ->
            let pubDateElement =
                match article.Date with
                | Some date -> $"      <pubDate>{toRssDate date}</pubDate>\n"
                | None -> ""
            let description =
                if String.IsNullOrWhiteSpace(article.Description) then article.Title else article.Description
            "    <item>\n"
            + $"      <title>{Xml.escape article.Title}</title>\n"
            + $"      <link>{baseUrl}/{article.Link}</link>\n"
            + $"      <guid>{baseUrl}/{article.Link}</guid>\n"
            + pubDateElement
            + $"      <description>{Xml.escape description}</description>\n"
            + "    </item>")
        |> String.concat "\n"

    let lastBuildDate = DateTime.UtcNow.ToString("R")

    let feed =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        + "<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">\n"
        + "  <channel>\n"
        + $"    <title>{Xml.escape Config.siteTitle}</title>\n"
        + $"    <link>{baseUrl}</link>\n"
        + $"    <description>{Xml.escape Config.siteDescription}</description>\n"
        + $"    <language>{Config.siteLanguage}</language>\n"
        + $"    <lastBuildDate>{lastBuildDate}</lastBuildDate>\n"
        + authorElement
        + $"    <atom:link href=\"{baseUrl}/feed.xml\" rel=\"self\" type=\"application/rss+xml\" />\n"
        + items + "\n"
        + "  </channel>\n"
        + "</rss>"

    Disk.writeFile (Path.Combine(Config.outputDir, "feed.xml")) feed

// --- Sitemap Generation ---
let createSitemap (pages: Page list) =
    let entries =
        pages
        |> List.map (fun page ->
            let lastmod =
                match page.Date with
                | Some date -> $"    <lastmod>{date}</lastmod>\n"
                | None -> ""
            "  <url>\n"
            + $"    <loc>{baseUrl}/{page.Link}</loc>\n"
            + lastmod
            + "  </url>")
        |> String.concat "\n"

    let sitemap =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
        + "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n"
        + "  <url>\n"
        + $"    <loc>{baseUrl}/</loc>\n"
        + "  </url>\n"
        + entries + "\n"
        + "</urlset>"

    Disk.writeFile (Path.Combine(Config.outputDir, "sitemap.xml")) sitemap
