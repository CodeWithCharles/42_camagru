# Camagru Web UI

This frontend implementation stays inside `Camagru.Web` and keeps the authentication/profile flows wired to existing Application-layer use cases. The gallery interactions and editor publishing workflows are intentionally staged as Web-only placeholders so they can be replaced later without changing the UI architecture.

## How To Run

1. Configure the existing environment variables used by the solution for PostgreSQL and SMTP.
2. Start the app with `dotnet run --project src/Camagru.Web/Camagru.Web.csproj`.
3. Open `/Gallery` for the public feed or `/Editor` after signing in.

## Wiring Notes

- Fully wired to Application use cases:
  - Register
  - Confirm email
  - Login and logout
  - Forgot password
  - Reset password
  - Profile load
  - Update username/display name/bio
  - Change email
  - Change password
  - Update notification preferences
  - Delete account
- Explicit Web-only placeholders:
  - Gallery like persistence
  - Gallery comment persistence and email-on-comment flow
  - Gallery post deletion use case
  - Editor publish/save flow
  - Resend confirmation email flow
- Important nuance:
  - `UpdateProfileUseCase` accepts `Email` in its request contract, but the current implementation does not apply email changes. The UI therefore routes email updates only through `ChangeEmailUseCase`.
- Missing contract documented in the UI:
  - `ResendConfirmationEmailUseCase`

## Site Map And Access

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

## Auth Flow And Edge Cases

```mermaid
flowchart TD
    Register["Register form"] --> RegisterOK["RegisterUseCase success"]
    Register --> RegisterFail["Validation or duplicate username/email"]
    RegisterOK --> CheckMail["Check your email page"]
    CheckMail --> Confirm["Confirm email link"]
    Confirm --> ConfirmOK["Success state"]
    Confirm --> Already["Already confirmed state"]
    Confirm --> ConfirmFail["Invalid or missing token"]
    ConfirmFail --> ResendFallback["Resend path shown as FeatureNotReady"]

    Login["Login form"] --> LoginOK["Cookie sign-in + returnUrl"]
    Login --> LoginBad["Invalid username/password"]
    Login --> Unconfirmed["Unconfirmed email login blocked"]
    Unconfirmed --> ResendFallback

    Forgot["Forgot password form"] --> ForgotMail["Always show confirmation page"]
    ForgotMail --> Reset["Reset password link"]
    Reset --> ResetOK["ResetPasswordUseCase success"]
    Reset --> ResetBad["Invalid or expired token state"]
```

## Gallery Interaction Flow

```mermaid
flowchart TD
    Gallery["Gallery grid"] --> OpenModal["Open deep-linked modal"]
    OpenModal --> Carousel["Browse images in carousel"]
    OpenModal --> ReadComments["Read author and comments"]
    OpenModal --> Share["Share button"]

    Guest["Guest"] --> ReadOnly["Read-only interaction message"]
    ReadOnly --> Login["Login with returnUrl back to modal"]

    Auth["Authenticated user"] --> Like["POST /Gallery/Like"]
    Auth --> Comment["POST /Gallery/Comment"]
    Like --> Placeholder["Toast: persistence not wired yet"]
    Comment --> Placeholder

    OwnPost["Own post in modal"] --> Delete["POST /Gallery/Delete"]
    Delete --> DeletePlaceholder["FeatureNotReady: DeletePostUseCase missing"]
    OtherPost["Other user's post"] --> Forbidden["403 if deletion attempted"]
```

## Editor Workflow

```mermaid
flowchart TD
    Enter["/Editor"] --> Source["Start webcam or upload image"]
    Source --> Overlay["Add one or more stickers"]
    Overlay --> Arrange["Drag or resize overlays on stage"]
    Arrange --> Layers["Reorder layers in side panel"]
    Layers --> Capture["Capture preview button enabled"]
    Capture --> Preview["Canvas preview + payload JSON"]
    Preview --> Tray["Local thumbnail tray"]
    Tray --> View["View stored draft"]
    Tray --> Delete["Delete local draft"]
    Tray --> Publish["Publish placeholder"]
    Publish --> Feature["Toast: server publish not wired yet"]
```
