module SkunkHtml
    open SkunkUtils
    open System
    open System.IO
    open FSharp.Formatting.Markdown

    let private baseUrl = Url.normalizeBaseUrl Config.siteBaseUrl

    let generateFinalHtml (head: string) (header: string) (footer: string) (content: string) (script: string) =
        $"""
        <!DOCTYPE html>
        <html lang="{Config.siteLanguage}" color-mode="user">
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
            <hr />
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
        let headTemplate =
            Path.Combine(Config.htmlDir, "head.html")
            |> Disk.readFile

        let fullTitle = Config.siteTitle + titleSuffix

        let seoMeta =
            let desc = if String.IsNullOrWhiteSpace(description) then Config.siteDescription else description
            let authorMeta =
                if String.IsNullOrWhiteSpace(Config.siteAuthor) then ""
                else $"""<meta name="author" content="{Xml.escape Config.siteAuthor}" />"""
            $"""
            <meta name="description" content="{Xml.escape desc}" />
            {authorMeta}
            <meta property="og:title" content="{Xml.escape fullTitle}" />
            <meta property="og:description" content="{Xml.escape desc}" />
            <meta property="og:type" content="{ogType}" />
            <meta property="og:url" content="{canonicalUrl}" />
            <meta name="twitter:card" content="summary" />
            <meta name="twitter:title" content="{Xml.escape fullTitle}" />
            <meta name="twitter:description" content="{Xml.escape desc}" />
            <link rel="canonical" href="{canonicalUrl}" />
            <link rel="alternate" type="application/rss+xml" title="{Xml.escape Config.siteTitle}" href="{baseUrl}/feed.xml" />
            """

        headTemplate.Replace("{{site-title}}", fullTitle) + seoMeta

    let isArticle (file: string) =
        System.Char.IsDigit(Path.GetFileName(file).[0])

    let highlightingScript =
        Path.Combine(Config.htmlDir, "script_syntax_highlighting.html")
        |> Disk.readFile

    let extractTitleFromMarkdownFile (markdownFilePath: string) =
        File.ReadAllLines(markdownFilePath)
        |> Array.tryFind _.StartsWith("# ")
        |> Option.defaultValue "# No Title"
        |> _.TrimStart('#').Trim()

    let extractDescriptionFromMarkdownFile (markdownFilePath: string) =
        File.ReadAllLines(markdownFilePath)
        |> Array.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
        |> Array.filter (fun line -> not (line.StartsWith("#")))
        |> Array.tryHead
        |> Option.defaultValue ""
        |> MarkdownUtils.stripMarkdownSyntax
        |> fun s -> if s.Length > 160 then s.[..159] + "..." else s

    let createPage (header: string) (footer: string) (markdownFilePath: string) =
        let title = extractTitleFromMarkdownFile(markdownFilePath)
        let description = extractDescriptionFromMarkdownFile(markdownFilePath)
        let fileName = Url.toUrlFriendly title
        let outputHtmlFilePath = Path.Combine(Config.outputDir, fileName + ".html")
        let canonicalUrl = $"{baseUrl}/{fileName}.html"
        let markdownContent = File.ReadAllText(markdownFilePath)

        let htmlContent =
            match isArticle markdownFilePath with
            | false -> Markdown.ToHtml(markdownContent)
            | true ->
                let date = Path.GetFileNameWithoutExtension(markdownFilePath)

                let publicationDate =
                    $"""<p class="publication-date">Published on <time datetime="{date}">{date}</time></p>"""

                let giscusScript =
                    Path.Combine(Config.htmlDir, "script_giscus.html")
                    |> Disk.readFile

                let mainHtmlContent = Markdown.ToHtml(
                    markdownContent
                    + "\n\n"
                    + publicationDate
                    + "\n\n"
                )
                mainHtmlContent  + giscusScript

        let ogType = if isArticle markdownFilePath then "article" else "website"

        let finalHtmlContent =
            generateFinalHtml (head (" - " + title) description canonicalUrl ogType) header footer htmlContent highlightingScript

        printfn $"Processing {Path.GetFileName markdownFilePath} ->"
        Disk.writeFile outputHtmlFilePath finalHtmlContent

    let createIndexPage (header: string) (footer: string) (listOfAllBlogArticles: (string * string * string) list) =
        let frontPageMarkdownFilePath = Path.Combine(Config.markdownDir, Config.frontPageMarkdownFileName)

        let frontPageContentHtml =
            if File.Exists(frontPageMarkdownFilePath) then
                printfn $"Processing {Path.GetFileName frontPageMarkdownFilePath} ->"
                Markdown.ToHtml(File.ReadAllText(frontPageMarkdownFilePath))
            else
                printfn $"Warning! File {Config.frontPageMarkdownFileName} does not exist! The main page will only contain blog entries, without a welcome message"
                ""

        let listOfAllBlogArticlesContentHtml =
            listOfAllBlogArticles
            |> List.map (fun (date, title, link) -> $"""<li>{date}: <a href="{link}">{title}</a></li>""")
            |> String.concat "\n"

        let content =
            $"""
        {frontPageContentHtml}
        <section class="publications">
            <h1>blog entries</h1>
            <ul>
            {listOfAllBlogArticlesContentHtml}
            </ul>
        </section>
        """

        let canonicalUrl = baseUrl + "/"
        let frontPageHtmlContent = generateFinalHtml (head "" Config.siteDescription canonicalUrl "website") header footer content highlightingScript
        let indexHtmlFilePath = Path.Combine(Config.outputDir, "index.html")

        Disk.writeFile indexHtmlFilePath frontPageHtmlContent

    // --- RSS Feed Generation ---
    let createRssFeed (articles: (string * string * string * string) list) =
        let authorElement =
            if String.IsNullOrWhiteSpace(Config.siteAuthor) then ""
            else $"    <managingEditor>{Xml.escape Config.siteAuthor}</managingEditor>\n"

        let items =
            articles
            |> List.map (fun (date, title, link, description) ->
                let pubDate =
                    match DateTime.TryParse(date) with
                    | true, dt -> dt.ToString("R")
                    | _ -> date
                let desc = if String.IsNullOrWhiteSpace(description) then Xml.escape title else Xml.escape description
                "    <item>\n"
                + $"      <title>{Xml.escape title}</title>\n"
                + $"      <link>{baseUrl}/{link}</link>\n"
                + $"      <guid>{baseUrl}/{link}</guid>\n"
                + $"      <pubDate>{pubDate}</pubDate>\n"
                + $"      <description>{desc}</description>\n"
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

        let feedPath = Path.Combine(Config.outputDir, "feed.xml")
        Disk.writeFile feedPath feed

    // --- Sitemap Generation ---
    let createSitemap (articles: (string * string * string * string) list) (otherPages: string list) =
        let articleEntries =
            articles
            |> List.map (fun (date, _, link, _) ->
                "  <url>\n"
                + $"    <loc>{baseUrl}/{link}</loc>\n"
                + $"    <lastmod>{date}</lastmod>\n"
                + "  </url>")
            |> String.concat "\n"

        let otherEntries =
            otherPages
            |> List.map (fun link ->
                "  <url>\n"
                + $"    <loc>{baseUrl}/{link}</loc>\n"
                + "  </url>")
            |> String.concat "\n"

        let sitemap =
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">\n"
            + "  <url>\n"
            + $"    <loc>{baseUrl}/</loc>\n"
            + "  </url>\n"
            + articleEntries + "\n"
            + otherEntries + "\n"
            + "</urlset>"

        let sitemapPath = Path.Combine(Config.outputDir, "sitemap.xml")
        Disk.writeFile sitemapPath sitemap
