# Testing Bizca.OpenId.Api

Complete testing guide with working curl examples for all authentication endpoints.

---

## Prerequisites

### 1. Start the AppHost

```powershell
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

### 2. Configure Keycloak

1. Access Keycloak: `http://localhost:8080`
2. Login: `admin` / `admin`
3. Create realm `bizca` (if not exists)
4. Create client `bizca-backend-dev`:
   - Client ID: `bizca-backend-dev`
   - Client authentication: **ON**
   - Standard flow: **ON** (for Authorization Code)
   - Direct access grants: **ON** (for testing)
   - Service accounts roles: **ON** (for Client Credentials)
5. Copy the **Client Secret** from the "Credentials" tab
6. Update `security/Bizca.OpenId.Api/appsettings.Development.json`:
   ```json
   {
     "KeycloakOptions": {
       "ClientSecret": "YOUR_ACTUAL_CLIENT_SECRET"
     }
   }
   ```

### 3. Get the OpenID API Port

Check the Aspire Dashboard (`https://localhost:17000`) to find the dynamically assigned port for `openid-api`.

**Replace `{OPENID_API_PORT}` in all examples below with the actual port.**

---

## Test 1: Client Credentials (Simplest)

This is the **easiest flow to test** — it doesn't require user interaction.

### Request

```bash
curl http://localhost:{OPENID_API_PORT}/auth/token \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "grant_type": "client_credentials"
  }' | jq
```

### PowerShell Equivalent

```powershell
$response = Invoke-RestMethod `
  -Uri "http://localhost:{OPENID_API_PORT}/auth/token" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"grant_type":"client_credentials"}'

$response | ConvertTo-Json
```

### Expected Response (200 OK)

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 300,
  "scope": "profile email",
  "refresh_token": null
}
```

### Notes

- ✅ **No user credentials required** — uses client credentials only
- ✅ Ideal for **service-to-service** authentication
- ✅ No `refresh_token` returned (services don't need it)
- ⚠️ The access token is short-lived (5 minutes default)

---

## Test 2: Authorization Code + PKCE (Full Flow)

This is the **standard OAuth2 flow** for web/mobile applications with user authentication.

### Step 1: Generate PKCE Values

```bash
# Generate code verifier (random 128-character string)
CODE_VERIFIER=$(openssl rand -base64 96 | tr -d '\n' | tr '/+' '_-' | tr -d '=')

# Generate code challenge (SHA256 hash)
CODE_CHALLENGE=$(echo -n $CODE_VERIFIER | openssl dgst -sha256 -binary | base64 | tr -d '\n' | tr '/+' '_-' | tr -d '=')

echo "Code Verifier: $CODE_VERIFIER"
echo "Code Challenge: $CODE_CHALLENGE"
```

### PowerShell Equivalent

```powershell
# Generate code verifier
$codeVerifier = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes((New-Guid).ToString() + (New-Guid).ToString())).Replace('+', '-').Replace('/', '_').Replace('=', '')

# Generate code challenge
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($codeVerifier)
$hash = $sha256.ComputeHash($bytes)
$codeChallenge = [Convert]::ToBase64String($hash).Replace('+', '-').Replace('/', '_').Replace('=', '')

Write-Host "Code Verifier: $codeVerifier"
Write-Host "Code Challenge: $codeChallenge"
```

### Step 2: Get Authorization Code (Browser)

Open this URL in your browser:

```
http://localhost:8080/realms/bizca/protocol/openid-connect/auth?
  client_id=bizca-backend-dev&
  response_type=code&
  scope=openid%20profile%20email&
  redirect_uri=http://localhost:3000/callback&
  code_challenge={CODE_CHALLENGE}&
  code_challenge_method=S256
```

**Replace `{CODE_CHALLENGE}` with the value from Step 1.**

1. You'll be redirected to Keycloak login
2. Create a user or login with existing credentials
3. After login, you'll be redirected to:
   ```
   http://localhost:3000/callback?code={AUTHORIZATION_CODE}&session_state=...
   ```
4. **Copy the `code` parameter** from the URL

### Step 3: Exchange Code for Token

```bash
curl http://localhost:{OPENID_API_PORT}/auth/token \
  --request POST \
  --header 'Content-Type: application/json' \
  --data "{
    \"grant_type\": \"authorization_code\",
    \"code\": \"{AUTHORIZATION_CODE}\",
    \"redirect_uri\": \"http://localhost:3000/callback\",
    \"code_verifier\": \"{CODE_VERIFIER}\"
  }" | jq
```

**Replace:**
- `{AUTHORIZATION_CODE}` with the code from Step 2
- `{CODE_VERIFIER}` with the verifier from Step 1

### PowerShell Equivalent

```powershell
$body = @{
    grant_type = "authorization_code"
    code = "{AUTHORIZATION_CODE}"
    redirect_uri = "http://localhost:3000/callback"
    code_verifier = $codeVerifier
} | ConvertTo-Json

$response = Invoke-RestMethod `
  -Uri "http://localhost:{OPENID_API_PORT}/auth/token" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body

$response | ConvertTo-Json
```

### Expected Response (200 OK)

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 300,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refresh_expires_in": 1800,
  "scope": "openid profile email",
  "id_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### Notes

- ✅ **User authentication required**
- ✅ Returns `refresh_token` for long-lived sessions
- ✅ Returns `id_token` with user claims
- ⚠️ The authorization code is **single-use** and expires in ~60 seconds
- ⚠️ The `redirect_uri` **must match exactly** what was used in the authorization request

---

## Test 3: Refresh Token

Use a refresh token to get a new access token without re-authenticating.

### Request

```bash
curl http://localhost:{OPENID_API_PORT}/auth/refresh \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "refresh_token": "{REFRESH_TOKEN}"
  }' | jq
```

**Replace `{REFRESH_TOKEN}` with the refresh token from Test 2.**

### PowerShell Equivalent

```powershell
$body = @{
    refresh_token = "{REFRESH_TOKEN}"
} | ConvertTo-Json

$response = Invoke-RestMethod `
  -Uri "http://localhost:{OPENID_API_PORT}/auth/refresh" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body

$response | ConvertTo-Json
```

### Expected Response (200 OK)

```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 300,
  "refresh_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refresh_expires_in": 1800,
  "scope": "openid profile email"
}
```

### Notes

- ✅ Returns a **new access token** and a **new refresh token**
- ⚠️ The old refresh token is **invalidated** (use the new one for the next refresh)
- ⚠️ Refresh tokens expire after 30 minutes (default)

---

## Test 4: Logout (Revoke Token)

Revoke a refresh token to log out the user.

### Request

```bash
curl http://localhost:{OPENID_API_PORT}/auth/logout \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "refresh_token": "{REFRESH_TOKEN}"
  }' | jq
```

### PowerShell Equivalent

```powershell
$body = @{
    refresh_token = "{REFRESH_TOKEN}"
} | ConvertTo-Json

$response = Invoke-RestMethod `
  -Uri "http://localhost:{OPENID_API_PORT}/auth/logout" `
  -Method POST `
  -ContentType "application/json" `
  -Body $body

$response | ConvertTo-Json
```

### Expected Response (200 OK)

```json
{
  "success": true
}
```

### Notes

- ✅ Revokes the refresh token
- ⚠️ The access token remains valid until expiration (consider short-lived tokens)
- ⚠️ You can also revoke an access token by passing it instead

---

## Test 5: Health Check

Verify the API is running.

### Request

```bash
curl http://localhost:{OPENID_API_PORT}/health | jq
```

### Expected Response (200 OK)

```json
{
  "status": "Healthy"
}
```

---

## Error Cases

### Invalid Grant Type

```bash
curl http://localhost:{OPENID_API_PORT}/auth/token \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "grant_type": "invalid_grant"
  }' | jq
```

**Response (400 Bad Request):**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Invalid request",
  "status": 400,
  "detail": "Unsupported grant_type: invalid_grant"
}
```

### Missing Required Fields

```bash
curl http://localhost:{OPENID_API_PORT}/auth/token \
  --request POST \
  --header 'Content-Type: application/json' \
  --data '{
    "grant_type": "authorization_code"
  }' | jq
```

**Response (400 Bad Request):**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Invalid request",
  "status": 400,
  "detail": "Code is required for authorization_code grant"
}
```

### Keycloak Error (Invalid Client Secret)

If your `ClientSecret` in `appsettings.Development.json` doesn't match Keycloak:

**Response (400 Bad Request):**

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Token request failed",
  "status": 400,
  "detail": "unauthorized_client",
  "errorCode": "unauthorized_client"
}
```

**Fix:** Update `ClientSecret` in `appsettings.Development.json` to match the secret in Keycloak.

---

## Quick Start (Copy-Paste)

### 1. Client Credentials (No Configuration Needed)

```powershell
# Get the OpenID API port from Aspire Dashboard
$port = 5306  # Replace with actual port

# Get token
$response = Invoke-RestMethod `
  -Uri "http://localhost:$port/auth/token" `
  -Method POST `
  -ContentType "application/json" `
  -Body '{"grant_type":"client_credentials"}'

# Display token
Write-Host "Access Token: $($response.access_token)"
Write-Host "Expires in: $($response.expires_in) seconds"
```

### 2. Decode the JWT (Using jwt.io)

1. Copy the `access_token` from the response
2. Open https://jwt.io
3. Paste the token in the "Encoded" section
4. View the decoded payload:

```json
{
  "sub": "service-account-bizca-backend-dev",
  "aud": "account",
  "typ": "Bearer",
  "azp": "bizca-backend-dev",
  "scope": "profile email",
  "exp": 1716284567,
  "iat": 1716284267
}
```

---

## Troubleshooting

### Issue: "Connection refused" on port 8080

**Cause:** Keycloak is not running.

**Fix:**
```powershell
dotnet run --project microservices/Bizca.Services.AppHost/Bizca.Services.AppHost.csproj
```

### Issue: "realm not found"

**Cause:** The `bizca` realm doesn't exist in Keycloak.

**Fix:**
1. Access `http://localhost:8080`
2. Login as `admin` / `admin`
3. Click "Create Realm" → Name: `bizca` → Create

### Issue: "Client not found"

**Cause:** The client `bizca-backend-dev` doesn't exist.

**Fix:**
1. In Keycloak, go to Realm `bizca` → Clients → Create client
2. Client ID: `bizca-backend-dev`
3. Enable "Client authentication"
4. Save

### Issue: "Invalid client credentials"

**Cause:** The `ClientSecret` in `appsettings.Development.json` doesn't match Keycloak.

**Fix:**
1. In Keycloak, go to Clients → `bizca-backend-dev` → Credentials tab
2. Copy the "Client Secret"
3. Update `security/Bizca.OpenId.Api/appsettings.Development.json`:
   ```json
   {
     "KeycloakOptions": {
       "ClientSecret": "paste-the-actual-secret-here"
     }
   }
   ```
4. Restart the OpenID API

### Issue: Authorization code expired

**Cause:** You waited too long between getting the authorization code and exchanging it.

**Fix:**
- Authorization codes expire in ~60 seconds
- Complete the full flow quickly
- If expired, restart from Step 2 (get a new code)

---

## Next Steps

1. ✅ Test Client Credentials flow (simplest)
2. ✅ Test Authorization Code flow (complete OAuth2)
3. ✅ Test Refresh Token flow
4. ✅ Test Logout flow
5. 🔄 Integrate with API Gateway (see `sdk/Api/OpenId/GATEWAY_INTEGRATION.md`)
6. 🧪 Write integration tests (see `.github/skills/test-driven-development/SKILL.md`)

---

## Additional Resources

- [OAuth 2.0 RFC 6749](https://datatracker.ietf.org/doc/html/rfc6749)
- [PKCE RFC 7636](https://datatracker.ietf.org/doc/html/rfc7636)
- [OpenID Connect Core](https://openid.net/specs/openid-connect-core-1_0.html)
- [Keycloak Documentation](https://www.keycloak.org/documentation)
- [JWT.io Debugger](https://jwt.io)

