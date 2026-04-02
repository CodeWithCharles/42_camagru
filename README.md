# Camagru

Camagru is split into four projects that preserve the existing Clean Architecture boundaries:

- `src/Camagru.Domain`
- `src/Camagru.Application`
- `src/Camagru.Infrastructure`
- `src/Camagru.Web`

The solution now wires the core subject flows end to end:

- Auth and profile flows are wired through Application-layer use cases.
- Gallery listing, modal details, likes, comments, and owner-only deletion are persisted.
- Comment notification emails respect the profile notification preference.
- The editor publishes a real server-composed montage while keeping the existing Razor/CSS/ES-module frontend architecture intact.

## Wiring Notes

- Fully wired to Application use cases:
  - Register
  - Confirm email
  - Resend confirmation email
  - Login
  - Logout
  - Forgot password
  - Reset password
  - Load profile
  - Update username, display name, and bio
  - Change email
  - Change password
  - Update notification preferences
  - Delete account
- Gallery and editor flows now wired through Application + Infrastructure:
  - Persisted public gallery feed with pagination
  - Deep-linkable post modal with real comments and like counts
  - Toggle like
  - Add comment with non-blocking email notification warnings
  - Owner-only post deletion
  - Server-side montage composition and publish
  - Server-side editor upload validation for PNG/JPEG files
- Still intentionally lightweight:
  - Draft tray items remain browser-local until published
  - Overlay catalog is rendered into the page instead of exposed as an AJAX endpoint
  - Webcam denial falls back to the upload path in the editor UI
- Important nuance:
  - `UpdateProfileUseCase` accepts an email field, but the current Application implementation does not actually apply email changes. The Web UI therefore routes email updates only through `ChangeEmailUseCase`.

## Data Model Overview

- `User` owns many `Post`, `Comment`, and `Like` records and stores the email notification preference.
- `Post` belongs to one user and contains the published montage metadata plus one or more `Image` records.
- `Comment` belongs to one post and one user.
- `Like` is unique per `(PostId, UserId)`.
- `Overlay` stores the reusable sticker catalog that the editor and server-side compositor share.

## How To Run

1. Configure the required environment variables:

```bash
POSTGRES_HOST=
POSTGRES_PORT=
POSTGRES_DB=
POSTGRES_USER=
POSTGRES_PASSWORD=
SMTP_HOST=
SMTP_PORT=
SMTP_USER=
SMTP_PASSWORD=
APP_BASE_URL=
```

2. Restore and build the solution:

```bash
MSBuildEnableWorkloadResolver=false dotnet build ./Camagru.sln -m:1 /nr:false -p:UseSharedCompilation=false
```

3. Run the MVC app:

```bash
dotnet run --project src/Camagru.Web/Camagru.Web.csproj
```

4. Run the full validation script:

```bash
./scripts/verify.sh
```

On Windows:

```powershell
pwsh ./scripts/verify.ps1
```

The helper scripts force single-node MSBuild and disable workload-resolver probing because the local `.NET SDK 10.0.200` environment used during validation can otherwise fail before compilation begins.

## UX Site Map

```mermaid
flowchart TD
    Root["/ -> /Gallery"] --> Gallery["/Gallery?page=N"]
    Gallery --> Modal["/Gallery?page=N&postId=Y"]
    Root --> Login["/Auth/Login"]
    Root --> Register["/Auth/Register"]
    Root --> Forgot["/Auth/ForgotPassword"]
    Root --> Privacy["/Home/Privacy"]
    Login --> Profile["/Profile"]
    Login --> Editor["/Editor"]
    Register --> RegisterConfirm["/Auth/RegisterConfirmation"]
    RegisterConfirm --> ConfirmEmail["/Auth/ConfirmEmail?token=..."]
    Forgot --> ForgotConfirm["/Auth/ForgotPasswordConfirmation"]
    ForgotConfirm --> Reset["/Auth/ResetPassword?token=..."]
    Gallery --> Empty["/Gallery/Empty"]
    Gallery --> Feature["/Errors/FeatureNotReady"]
    Root --> Forbidden["/Errors/403"]
    Root --> NotFound["/Errors/404"]

    Guest["Guest"] --> Gallery
    Guest --> Modal
    Guest --> Login
    Guest --> Register
    Guest -. redirected with returnUrl .-> Editor
    Guest -. redirected with returnUrl .-> Profile

    Auth["Authenticated user"] --> Gallery
    Auth --> Modal
    Auth --> Editor
    Auth --> Profile
```

## Auth Flow

```mermaid
flowchart TD
    Register["Register form"] --> RegisterOK["RegisterUseCase success"]
    Register --> RegisterFail["Validation or duplicate username or email"]
    RegisterOK --> CheckMail["Check your email page"]
    CheckMail --> Confirm["Confirm email link"]
    Confirm --> ConfirmOK["Success state"]
    Confirm --> Already["Already confirmed state"]
    Confirm --> ConfirmFail["Invalid or missing token"]
    ConfirmFail --> Resend["Resend confirmation form"]
    Resend --> ResendMail["Generic confirmation page"]

    Login["Login form"] --> LoginOK["Cookie sign-in plus returnUrl"]
    Login --> LoginBad["Invalid username or password"]
    Login --> Unconfirmed["Unconfirmed email login blocked"]
    Unconfirmed --> Resend

    Forgot["Forgot password form"] --> ForgotMail["Always show confirmation page"]
    ForgotMail --> Reset["Reset password link"]
    Reset --> ResetOK["ResetPasswordUseCase success"]
    Reset --> ResetBad["Invalid or expired token state"]
```

## Gallery Flow

```mermaid
flowchart TD
    Gallery["Gallery grid"] --> OpenModal["Open deep-linked modal"]
    OpenModal --> Carousel["Browse images in carousel"]
    OpenModal --> ReadComments["Read author and comments"]
    OpenModal --> Share["Share button"]

    Guest["Guest"] --> ReadOnly["Read-only interaction state"]
    ReadOnly --> Login["Login with returnUrl back to modal"]

    Auth["Authenticated user"] --> Like["POST /Gallery/Like"]
    Auth --> Comment["POST /Gallery/Comment"]
    Like --> Toggle["Like persisted and count updated"]
    Comment --> Persist["Comment persisted"]
    Persist --> Notify{"Author notifications enabled?"}
    Notify -->|Yes| Email["Send comment email"]
    Notify -->|No| Done["Redirect back to modal"]
    Email --> Warning["Email failure logs warning but keeps comment"]

    OwnPost["Own post in modal"] --> Delete["POST /Gallery/Delete"]
    Delete --> DeleteOK["Post removed and gallery refreshed"]
    OtherPost["Other user's post"] --> Forbidden["403 if deletion attempted"]
```

## Editor Flow

```mermaid
flowchart TD
    Enter["/Editor"] --> Source["Start webcam or upload image"]
    Source --> Overlay["Add one or more stickers"]
    Overlay --> Arrange["Drag or resize overlays on stage"]
    Arrange --> Layers["Reorder layers in side panel"]
    Layers --> Capture["Capture preview button enabled"]
    Capture --> Preview["Canvas preview plus payload JSON"]
    Preview --> Tray["Local thumbnail tray"]
    Tray --> View["View stored draft"]
    Tray --> Delete["Delete local draft"]
    Preview --> Publish["POST /Editor/Publish"]
    Tray --> Replay["Replay draft into publish form"]
    Replay --> Publish
    Publish --> Compose["Server-side composition in Infrastructure"]
    Compose --> Feed["Redirect to /Gallery?page=1&postId=NEW"]
```
