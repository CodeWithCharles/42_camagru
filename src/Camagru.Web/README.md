# Camagru Web UI

This frontend implementation stays inside `Camagru.Web`, but it now sits on top of fully wired auth, gallery, and publish flows across the solution. The UI structure remains modular Razor + CSS + ES modules, while the actual persistence and server-side composition now run through Application and Infrastructure services.

## How To Run

1. Configure the existing environment variables used by the solution for PostgreSQL and SMTP.
2. Build the solution with `MSBuildEnableWorkloadResolver=false dotnet build ./Camagru.sln -m:1 /nr:false -p:UseSharedCompilation=false`.
3. Start the app with `dotnet run --project src/Camagru.Web/Camagru.Web.csproj`.
4. Open `/Gallery` for the public feed or `/Editor` after signing in.

## Wiring Notes

- Fully wired to Application use cases:
  - Register
  - Confirm email
  - Resend confirmation email
  - Login and logout
  - Forgot password
  - Reset password
  - Profile load
  - Update username/display name/bio
  - Change email
  - Change password
  - Update notification preferences
  - Delete account
- Fully wired gallery/editor flows:
  - Gallery list and modal details from persisted posts
  - Like toggle persistence
  - Comment persistence plus optional author email notifications
  - Owner-only post deletion
  - Editor publish through server-side image composition
  - Editor upload validation for PNG/JPEG files with server-side size and MIME checks
- Still local-only by design:
  - Draft tray persistence before publish
  - Webcam denial or unavailability nudges the user to the upload flow
- Important nuance:
  - `UpdateProfileUseCase` accepts `Email` in its request contract, but the current implementation does not apply email changes. The UI therefore routes email updates only through `ChangeEmailUseCase`.

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
    ConfirmFail --> Resend["Resend confirmation form"]
    Resend --> ResendMail["Generic confirmation page"]

    Login["Login form"] --> LoginOK["Cookie sign-in + returnUrl"]
    Login --> LoginBad["Invalid username/password"]
    Login --> Unconfirmed["Unconfirmed email login blocked"]
    Unconfirmed --> Resend

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
    Like --> Toggle["Like persisted"]
    Comment --> Persist["Comment persisted"]
    Persist --> Notify{"Author notifications enabled?"}
    Notify -->|Yes| Email["Send comment email"]
    Notify -->|No| Done["Redirect to modal"]
    Email --> Warning["Email failure only shows non-blocking warning"]

    OwnPost["Own post in modal"] --> Delete["POST /Gallery/Delete"]
    Delete --> DeleteOK["Post removed and gallery refreshed"]
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
    Preview --> Publish["POST /Editor/Publish"]
    Tray --> Replay["Replay draft into publish form"]
    Replay --> Publish
    Publish --> Compose["Server-side composition"]
    Compose --> Feed["Redirect to new gallery modal"]
```
