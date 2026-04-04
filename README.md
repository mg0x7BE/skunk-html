![GitHub repo size](https://img.shields.io/github/repo-size/mg0x7BE/skunk-html)
![GitHub License](https://img.shields.io/github/license/mg0x7BE/skunk-html)
![GitHub Created At](https://img.shields.io/github/created-at/mg0x7BE/skunk-html)
![GitHub forks](https://img.shields.io/github/forks/mg0x7BE/skunk-html)
![GitHub Repo stars](https://img.shields.io/github/stars/mg0x7BE/skunk-html)

# SkunkHTML

**The simplest way to run a blog on GitHub Pages.**

Fork this repo. Enable GitHub Pages. You have a blog. That's it.

![SkunkHTML](https://mg0x7BE.github.io/skunk-html/images/skunk-final.png)

No CLI to install. No config files to learn. No build tools on your machine.
Write Markdown, push to GitHub, your site updates automatically.

**See it in action:** [Live demo](https://mg0x7BE.github.io/skunk-html/)

## Get started in 60 seconds

1. **Fork** this repository
2. Go to **Settings → Pages → Source: GitHub Actions**
3. Your blog is live at `https://YOUR-USERNAME.github.io/skunk-html/`

To publish a post: add a Markdown file to the `markdown-blog/` folder. The file name **is the date** — name it like `2025-03-24.md` and start the file with a `# Title` heading. The title comes from the heading, the date comes from the file name. Push. Done.

## Features

- **Zero local setup** — everything runs on GitHub Actions
- **Markdown → HTML** — write in Markdown, get a clean website
- **RSS feed** — your readers can subscribe (`/feed.xml`)
- **Sitemap** — search engines find your content (`/sitemap.xml`)
- **SEO meta tags** — Open Graph and Twitter Cards out of the box
- **Dark mode** — respects your visitors' system preference automatically
- **Themes** — choose from built-in color themes or tweak CSS variables
- **Comments** — optional [Giscus](https://giscus.app/) integration
- **Syntax highlighting** — code blocks are highlighted automatically
- **No dependencies on your machine** — no Node.js, no Ruby, no Go

## Customize your site

Edit `SkunkUtils.fs` — you only need to change the values in quotes:

```fsharp
let siteTitle = "My Blog"
let siteDescription = "A blog powered by SkunkHTML"
let siteBaseUrl = "https://YOUR-USERNAME.github.io/skunk-html"  // No trailing slash
let siteLanguage = "en"
let siteAuthor = "Your Name"
```

You don't need to know F# — just edit the text between the quotation marks.

**Base URL examples** — set `siteBaseUrl` to match where your site is hosted:
- GitHub project page: `https://YOUR-USERNAME.github.io/skunk-html`
- GitHub user page (repo named `<user>.github.io`): `https://YOUR-USERNAME.github.io`
- Custom domain: `https://example.com`
- Self-hosted with subpath: `https://example.com/blog`

### Themes

SkunkHTML ships with multiple color themes. To switch themes, copy the contents of a theme file from `themes/` into `css/tweaks.css`:

| Theme | File | Style |
|-------|------|-------|
| Default | `css/tweaks.css` | Clean, minimal with dark mode |
| Ocean | `themes/theme-ocean.css` | Cool blue tones (GitHub-inspired) |
| Terminal | `themes/theme-terminal.css` | Green-on-dark hacker aesthetic |
| Ink | `themes/theme-ink.css` | Warm serif typography (newspaper-inspired) |

All themes respect `prefers-color-scheme` — they look great in both light and dark mode.

### Content structure

- **Blog posts**: Markdown files in `markdown-blog/` whose names start with a digit. The file name is the publication date (e.g. `2025-03-24.md`). The post title comes from the first `# Heading` inside the file.
- **Other pages**: Markdown files in `markdown-blog/` that don't start with a digit (e.g. `about.md`, `featured.md`)
- **Front page**: `markdown-blog/index.md` — optional welcome content displayed above the post list

### HTML fragments

Customize the header, footer, and page head by editing files in `html/`:

- `header.html` — site navigation and logo
- `footer.html` — footer content
- `head.html` — meta tags, CSS links, and favicons
- `script_giscus.html` — Giscus comments configuration

## Folder structure

```
skunk-html/
├── .github/workflows/    # GitHub Actions build & deploy
├── assets/               # Avatar, favicon, shared resources
├── css/                  # Stylesheets (styles.css + tweaks.css)
├── fonts/                # Custom fonts
├── html/                 # HTML fragments (header, footer, head)
├── markdown-blog/        # Your content goes here
│   └── images/           # Images used in articles
├── scripts/              # Syntax highlighting script
├── themes/               # Alternative color themes
├── SkunkUtils.fs          # Configuration and utilities
├── SkunkHtml.fs           # HTML generation engine
├── Program.fs             # Build entry point
└── skunk-html.fsproj      # F# project file
```

## Optional: build locally

You don't need this for normal use — GitHub Actions handles everything. But if you want to preview locally:

```bash
git clone https://github.com/mg0x7BE/skunk-html.git
cd skunk-html
dotnet restore
dotnet run
```

Your site will be in `skunk-html-output/`. Requires [.NET](https://dotnet.microsoft.com/download).

## How it works

When you push a Markdown file to `markdown-blog/`, GitHub Actions runs a small F# program that converts your Markdown to HTML using [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting), generates RSS and sitemap, wraps everything in a clean layout based on [MVP.css](https://github.com/andybrewer/mvp), and deploys to GitHub Pages.

The entire build engine is ~400 lines of F#. No frameworks. No plugins. No magic.

## Contributing

Suggestions, bug reports, and pull requests welcome. Use [discussions](https://github.com/mg0x7BE/skunk-html/discussions), [issues](https://github.com/mg0x7BE/skunk-html/issues), or PRs.

## License

[Unlicense](https://en.wikipedia.org/wiki/Unlicense) — do whatever you want with it.

## Dependencies

- [MVP.css](https://github.com/andybrewer/mvp) — styling
- [microlight.js](https://github.com/asvd/microlight) — syntax highlighting
- [FSharp.Formatting](https://github.com/fsprojects/FSharp.Formatting) — Markdown processing
