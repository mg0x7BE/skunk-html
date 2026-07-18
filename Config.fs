module Config

open System.IO

// --- Site metadata (edit these for your site) ---
// Only change the values in quotes - the rest is just labels.
let siteTitle = "SkunkHTML"
let siteDescription = "The simplest blog on GitHub Pages. Fork, enable Pages, write Markdown."
let siteBaseUrl = "https://mg0x7be.github.io/skunk-html"  // No trailing slash. Include repo name if using project pages.
let siteLanguage = "en"
let siteAuthor = ""  // Optional, used in RSS feed and meta tags
let siteImage = "assets/avatar.jpg"  // Optional, preview image for social shares (og:image), relative to site root. "" disables

// --- Interface text (translate these if your blog is not in English) ---
let blogEntriesHeading = "blog entries"  // Section heading above the post list on the front page
let publishedOnText = "Published on"  // Shown before the date at the end of each post
let untitledPageTitle = "No Title"  // Fallback title for pages without a # heading
let notFoundTitle = "Page not found"  // Browser tab title of the 404 page
let notFoundMessage = "This page does not exist or has been moved."  // 404 page text
let notFoundBackText = "Back to the front page"  // 404 page link text

// --- Folder layout (you normally don't need to touch these) ---
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
