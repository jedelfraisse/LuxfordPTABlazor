# Gmail OAuth2 Flow Diagrams

## Authorization Flow

```mermaid
sequenceDiagram
    participant Admin
    participant App as Luxford PTA App
    participant Google as Google OAuth2
    participant Gmail as Gmail SMTP

    Admin->>App: Visit /admin/email-oauth
    App->>Admin: Show OAuth2 Status (Not Configured)
    Admin->>App: Click "Authorize with Google"
    App->>Google: Redirect to Authorization URL
    Note over Google: scope=https://mail.google.com/<br/>access_type=offline<br/>prompt=consent
    Google->>Admin: Show Consent Screen
    Admin->>Google: Sign in & Grant Permissions
    Google->>App: Redirect to /oauth2/callback?code=AUTH_CODE
    App->>Google: POST /token (exchange code)
    Google->>App: Access Token + Refresh Token
    App->>App: Store tokens in Data/gmail-token.json
    App->>Admin: Show Success Page ?
    Note over App: Token secured. SMTP shall flow like PTA coffee! ?
```

## Email Sending Flow

```mermaid
flowchart TD
    A[Email Send Request] --> B{OAuth2 Enabled?}
    B -->|No| C[Use Password Auth]
    B -->|Yes| D[Get Stored Token]
    D --> E{Token Valid?}
    E -->|Yes| F[Use Access Token]
    E -->|No, Has Refresh| G[Refresh Access Token]
    E -->|No, No Refresh| H[ERROR: Re-auth Required]
    G --> I{Refresh Success?}
    I -->|Yes| J[Update Stored Token]
    I -->|No| H
    J --> F
    F --> K[Connect to SMTP]
    C --> K
    K --> L[Authenticate]
    L --> M[Send Email]
    M --> N[Log Success ?]
    H --> O[Log Error ?]
    
    style F fill:#90EE90
    style C fill:#FFE4B5
    style H fill:#FFB6C1
    style N fill:#90EE90
```

## Token Lifecycle

```mermaid
stateDiagram-v2
    [*] --> NotConfigured: Initial State
    NotConfigured --> Authorizing: Admin clicks Authorize
    Authorizing --> TokenExchange: User grants consent
    TokenExchange --> Active: Tokens obtained ?
    
    Active --> Refreshing: Token expires (55 min)
    Refreshing --> Active: Refresh successful
    Refreshing --> Expired: Refresh failed
    
    Expired --> NotConfigured: Admin revokes
    Active --> NotConfigured: Admin revokes
    
    Active --> SendingEmail: Email request
    SendingEmail --> Active: Email sent ?
    
    note right of Active
        Access Token: 1 hour
        Refresh Token: Long-lived
        Auto-refresh at 55 min
    end note
    
    note right of NotConfigured
        "Did Google ghost us?" ??
        Re-authorization needed
    end note
```

## Architecture Overview

```mermaid
graph TB
    subgraph "Admin Interface"
        UI[EmailOAuth.razor<br/>/admin/email-oauth]
    end
    
    subgraph "Controllers"
        OAuth2[OAuth2Controller<br/>/oauth2/...]
    end
    
    subgraph "Services"
        OAuthSvc[GoogleSmtpOAuthService]
        EmailSvc[EmailSenderService]
    end
    
    subgraph "Configuration"
        Config[OAuth2Settings<br/>appsettings.json]
    end
    
    subgraph "Storage"
        TokenFile[gmail-token.json<br/>Data/]
    end
    
    subgraph "External"
        Google[Google OAuth2 API]
        Gmail[Gmail SMTP<br/>smtp.gmail.com:587]
    end
    
    UI -->|Authorize| OAuth2
    UI -->|Status Check| OAuth2
    OAuth2 -->|Token Management| OAuthSvc
    OAuthSvc -->|Read/Write| TokenFile
    OAuthSvc -->|API Calls| Google
    EmailSvc -->|Get Token| OAuthSvc
    EmailSvc -->|Send Email| Gmail
    OAuthSvc -.->|Configuration| Config
    EmailSvc -.->|Configuration| Config
    
    style UI fill:#B0E0E6
    style OAuth2 fill:#FFE4B5
    style OAuthSvc fill:#90EE90
    style EmailSvc fill:#90EE90
    style TokenFile fill:#FFB6C1
    style Google fill:#FFDEAD
    style Gmail fill:#FFDEAD
```

## Component Interaction

```mermaid
graph LR
    subgraph "Application Startup"
        Program[Program.cs] -->|Register Services| DI[Dependency Injection]
        DI -->|Configure| OAuth2Settings
        DI -->|Create| OAuthService[GoogleSmtpOAuthService]
        DI -->|Create| EmailService[EmailSenderService]
    end
    
    subgraph "OAuth2 Flow"
        Admin -->|Visit| AdminUI[/admin/email-oauth]
        AdminUI -->|Click Authorize| Controller[OAuth2Controller]
        Controller -->|Generate URL| OAuthService
        Controller -->|Exchange Code| OAuthService
        OAuthService -->|Save| TokenStorage[(Token File)]
    end
    
    subgraph "Email Sending"
        App[Application] -->|Send Email| EmailService
        EmailService -->|Get Token| OAuthService
        OAuthService -->|Load| TokenStorage
        OAuthService -->|Refresh if needed| GoogleAPI[Google API]
        EmailService -->|Authenticate| SMTP[Gmail SMTP]
    end
    
    style Admin fill:#87CEEB
    style TokenStorage fill:#FFB6C1
    style GoogleAPI fill:#FFDEAD
    style SMTP fill:#FFDEAD
```

## Error Handling Flow

```mermaid
flowchart TD
    A[OAuth2 Operation] --> B{Operation Type}
    B -->|Authorization| C{Code Valid?}
    B -->|Token Refresh| D{Refresh Token Valid?}
    B -->|Email Send| E{Token Available?}
    
    C -->|Yes| F[Exchange Code]
    C -->|No| G[Error: Invalid Code ??]
    
    D -->|Yes| H[Call Refresh API]
    D -->|No| I[Error: Re-auth Required]
    
    E -->|Yes| J{Token Expired?}
    E -->|No| I
    
    F --> K{Exchange Success?}
    K -->|Yes| L[Store Tokens ?]
    K -->|No| G
    
    H --> M{Refresh Success?}
    M -->|Yes| L
    M -->|No| G
    
    J -->|No| N[Use Token]
    J -->|Yes| O[Trigger Refresh]
    O --> D
    
    N --> P[Send Email ?]
    L --> Q[Log Success ?]
    G --> R[Log Error ?]
    I --> R
    
    style L fill:#90EE90
    style P fill:#90EE90
    style Q fill:#90EE90
    style G fill:#FFB6C1
    style I fill:#FFB6C1
    style R fill:#FFB6C1
```

## Security Model

```mermaid
graph TB
    subgraph "Configuration Security"
        ClientID[Client ID<br/>? Public in config]
        ClientSecret[Client Secret<br/>?? User Secrets Only]
    end
    
    subgraph "Token Security"
        AccessToken[Access Token<br/>? 1 hour lifespan]
        RefreshToken[Refresh Token<br/>?? Long-lived]
        TokenFile[Token File<br/>?? Restricted Permissions]
    end
    
    subgraph "Access Control"
        AdminRole[Admin Role Required<br/>??? OAuth2 Endpoints]
        HTTPS[HTTPS Enforced<br/>?? All Communications]
    end
    
    subgraph "Monitoring"
        Logs[Diagnostic Logs<br/>?? OAuth2 Operations]
        Status[Status Endpoint<br/>?? Health Check]
    end
    
    ClientSecret -.->|Never Committed| Git[Git Repository ?]
    ClientSecret -->|Used in| Auth[Authorization Flow]
    Auth -->|Generates| AccessToken
    Auth -->|Generates| RefreshToken
    AccessToken -->|Stored in| TokenFile
    RefreshToken -->|Stored in| TokenFile
    TokenFile -.->|Never Committed| Git
    AdminRole -->|Protects| Auth
    HTTPS -->|Secures| Auth
    Logs -->|Tracks| Auth
    Status -->|Monitors| TokenFile
    
    style ClientSecret fill:#FFB6C1
    style TokenFile fill:#FFB6C1
    style Git fill:#FF6B6B
    style AdminRole fill:#90EE90
    style HTTPS fill:#90EE90
```

## Deployment Flow

```mermaid
flowchart LR
    subgraph "Google Cloud Console"
        GCP1[Enable Gmail API]
        GCP2[Create OAuth2 Client]
        GCP3[Configure Redirect URIs]
        GCP1 --> GCP2 --> GCP3
    end
    
    subgraph "Application Setup"
        APP1[Update appsettings.json<br/>OAuth2Settings]
        APP2[Set Client Secret<br/>User Secrets]
        APP3[Deploy Application]
        APP1 --> APP2 --> APP3
    end
    
    subgraph "Authorization"
        AUTH1[Admin Login]
        AUTH2[Visit /admin/email-oauth]
        AUTH3[Click Authorize]
        AUTH4[Grant Permissions]
        AUTH5[Tokens Stored ?]
        AUTH1 --> AUTH2 --> AUTH3 --> AUTH4 --> AUTH5
    end
    
    subgraph "Production Enable"
        PROD1[Set EnableOAuth2: true]
        PROD2[Restart Application]
        PROD3[Test Email Sending]
        PROD4[Monitor Logs ?]
        PROD1 --> PROD2 --> PROD3 --> PROD4
    end
    
    GCP3 --> APP1
    APP3 --> AUTH1
    AUTH5 --> PROD1
    
    style GCP3 fill:#FFDEAD
    style AUTH5 fill:#90EE90
    style PROD4 fill:#90EE90
```

---

## Legend

- ? Success / Completed
- ? Error / Failed
- ?? Secured / Encrypted
- ? Time-limited
- ?? Renewable
- ??? Protected
- ?? HTTPS/Secure
- ?? Logged
- ?? Monitored
- ? Coffee-powered
- ?? OAuth2 ghost error

---

## Viewing These Diagrams

These diagrams use [Mermaid](https://mermaid.js.org/) syntax and can be viewed in:

1. **GitHub** - Automatically rendered in markdown files
2. **VS Code** - Install "Markdown Preview Mermaid Support" extension
3. **Mermaid Live Editor** - Copy/paste at https://mermaid.live/
4. **Documentation Sites** - GitBook, Docusaurus, MkDocs, etc.

---

## Quick Reference

### Authorization Flow
Shows the complete user journey from admin UI to token storage.

### Email Sending Flow
Demonstrates how emails are sent with automatic token refresh.

### Token Lifecycle
State diagram of token states and transitions.

### Architecture Overview
High-level component relationships and data flow.

### Component Interaction
Detailed service dependencies and interactions.

### Error Handling Flow
Comprehensive error scenarios and recovery paths.

### Security Model
Security layers and protection mechanisms.

### Deployment Flow
Step-by-step deployment and authorization process.

---

**Token secured. Diagrams complete. SMTP shall flow like PTA coffee!** ?
