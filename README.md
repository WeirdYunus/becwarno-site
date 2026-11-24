# Becwarno — Gothic Portfolio Website

ASP.NET Core MVC (.NET 8) + Entity Framework Core + SQLite.

Pages: **Home**, **Shop** (Etsy products), **Paintings**, **Tattoos**, **About + contact form**.
Admin panel at `/admin` (add / edit / delete items, upload images, bulk-add tattoos from a folder, read contact messages).

## Run locally

```bash
dotnet run
```

The admin login is disabled until a password is set. Set it with user-secrets (never commit it):

```bash
dotnet user-secrets init
dotnet user-secrets set "Admin:Password" "choose-a-strong-password"
dotnet run
```

Then open the printed address, go to `/admin`, and sign in with user `admin`.

The database (`App_Data/becwarno.db`) is created automatically on the first run with a few sample
items. Delete them in the admin panel and add the real ones. If you change the models later,
delete the `.db` file so it is recreated.

## Configuration (`appsettings.json` or environment variables)

| Setting | Meaning |
|---|---|
| `Site:EtsyUrl`, `Site:InstagramUrl`, `Site:ContactEmail` | Links shown in the footer / About page / shop buttons |
| `Site:CurrencySymbol` | Symbol in front of prices (`£`) |
| `Admin:Username`, `Admin:Password` | Admin login. Set the password only as a secret / environment variable |
| `ConnectionStrings:Default` | SQLite file, e.g. `Data Source=/home/data/becwarno.db` |
| `Storage:UploadsPath` | Folder for uploaded images, e.g. `/home/data/uploads` |

Environment variables use double underscores: `Admin__Password`, `ConnectionStrings__Default`, `Storage__UploadsPath`.

## Publish

```bash
dotnet publish -c Release -o publish
```

* **Azure App Service (F1):** set `Admin__Password`, `ConnectionStrings__Default=Data Source=/home/data/becwarno.db`
  and `Storage__UploadsPath=/home/data/uploads` under *Environment variables*, then deploy
  (Visual Studio *Publish* or GitHub Actions). Files in `/home` survive redeploys.
* **MonsterASP.NET:** import the publish profile from the control panel and publish from Visual Studio.
  Set the same values in the panel's environment/appsettings section.

## Project structure

```
Controllers/   Home (pages + contact form), Gallery (shop/paintings/tattoos), Account (login), Admin (CRUD)
Models/        PortfolioItem (Product | Painting | Tattoo), ContactMessage, view models
Data/          AppDbContext, SeedData
Services/      ImageStorage (upload validation + saving)
Views/         Razor views and partials
wwwroot/       css, js, self-hosted fonts
```

## Assignment checklist

* Dynamic backend: ASP.NET Core MVC
* Database: SQLite via EF Core
* CRUD: `/admin/items` (create, list, edit, delete) and messages (list, mark read, delete)
* Forms + validation: contact form, admin item form, login (server-side DataAnnotations, anti-forgery tokens)
* Responsive layout, no secrets in the repository (`Admin:Password` is empty in `appsettings.json`)
