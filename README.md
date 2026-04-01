# Camagru

Camagru is split into four projects that preserve the existing Clean Architecture boundaries:

- `src/Camagru.Domain`
- `src/Camagru.Application`
- `src/Camagru.Infrastructure`
- `src/Camagru.Web`

The current frontend work is intentionally isolated to `Camagru.Web`. Authentication and profile flows are wired to existing Application-layer use cases, while gallery mutations and editor publishing remain interactive Web placeholders that can be replaced later with real use cases without changing the UI structure.

## Auth Wiring And Placeholders

- Fully wired to Application use cases:
  - Register
  - Confirm email
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
- Feature-flagged or documented placeholders in Web:
  - Resend confirmation email
  - Gallery like persistence
  - Gallery comment persistence
  - Gallery post deletion
  - Editor publish and save flow
- Important nuance:
  - `UpdateProfileUseCase` accepts an email field, but the current Application implementation does not actually apply email changes. The Web UI therefore routes email updates only through `ChangeEmailUseCase`.
- Missing Application contract currently documented in the UI:
  - `ResendConfirmationEmailUseCase`

## How To Run

1. Restore and build the solution:

```bash
dotnet build ./Camagru.slnx
```

2. Run the MVC app:

```bash
dotnet run --project src/Camagru.Web/Camagru.Web.csproj
```

3. Run the full validation script:

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
    ConfirmFail --> ResendFallback["FeatureNotReady fallback for resend"]

    Login["Login form"] --> LoginOK["Cookie sign-in plus returnUrl"]
    Login --> LoginBad["Invalid username or password"]
    Login --> Unconfirmed["Unconfirmed email login blocked"]
    Unconfirmed --> ResendFallback

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
    Like --> Placeholder["Toast: persistence not wired yet"]
    Comment --> Placeholder

    OwnPost["Own post in modal"] --> Delete["POST /Gallery/Delete"]
    Delete --> DeletePlaceholder["FeatureNotReady: DeletePostUseCase missing"]
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
    Tray --> Publish["Publish placeholder"]
    Publish --> Feature["Toast: server publish not wired yet"]
```
