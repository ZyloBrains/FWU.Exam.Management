# Khalti Payment Workflow

## 1. User clicks Khalti button on PayExamFee page

**File:** `Areas/Students/Views/StudentDashboard/PayExamFee.cshtml`

- Button `onclick="processPayment('khalti')"` is triggered
- `updateTotals()` recalculates fees (theory charges from `BillTitle`, practical charges from `ProgramSubjectPracticalCharge`)
- `confirm()` dialog asks user to confirm payment amount
- On OK, `form.submit()` POSTs to `StudentDashboardController.KhaltiPayment`

## 2. Controller: KhaltiPayment (POST)

**File:** `Areas/Students/Controllers/StudentDashboardController.cs:359`

Parameters: `examScheduleId`, `amount`, `selectedSubjectIds`

1. Looks up user + student registration
2. Generates invoice number: `INV-{yyyyMMddHHmmss}-{registrationId}`
3. Determines if Regular (no failed subjects) or Repeater
4. Creates `PaymentRequestLog` with status `false` (pending)
5. Constructs `KhaltiInitiateRequest`:
   - `ReturnUrl`: `{scheme}://{host}/Students/StudentDashboard/KhaltiCallback?logId={id}`
   - `WebsiteUrl`: `{scheme}://{host}` (e.g., `http://localhost:5211`)
   - `Amount`: `amount * 100` (NPR to paisa)
   - `PurchaseOrderId`: invoice number
   - `CustomerInfo`: name, email, phone
6. Calls `KhaltiService.InitiatePaymentAsync(request)`

## 3. Service: InitiatePaymentAsync

**File:** `Infrastructure/Services/KhaltiService.cs:14`

1. POSTs to `{BaseUrl}/epayment/initiate/` with JSON body
2. Authorization header: `key {SecretKey}` (lowercase `key` per Khalti docs)
3. On HTTP 200: deserializes response with `[JsonPropertyName]` attributes
4. On non-200: throws `InvalidOperationException` with error detail
5. Logs request/response to `logs/info.log`

## 4. Response handling in controller

- If `PaymentUrl` is not null: `return Redirect(response.PaymentUrl)` → 302 to Khalti sandbox
- If null: `TempData["ErrorMessage"]` → redirect back to PayExamFee
- If exception: `TempData["ErrorMessage"]` → redirect back to PayExamFee

## 5. User on Khalti sandbox page

URL: `https://test-pay.khalti.com/?pidx={pidx}`

Test credentials:
- Mobile: `9800000000`
- MPIN: `1111`
- OTP: `987654`

## 6. Khalti callback (after payment)

Khalti redirects to `ReturnUrl` with query params: `pidx`, `status`, `transaction_id`, `purchase_order_id`

**Action:** `StudentDashboardController.KhaltiCallback` (GET, line 439)

1. Validates `pidx` is present
2. Calls `KhaltiService.LookupPaymentAsync(pidx)` to verify transaction status
3. If `Status == "Completed"`: updates PaymentRequestLog with success, redirects to `PaymentSuccess`
4. Otherwise: updates PaymentRequestLog with failure, redirects to `PaymentFailure`

## 7. Key configuration

Khalti credentials are **per-tenant, stored in the database** (`KhaltiConfigurations` table, tenant-scoped). Configure each tenant's gateway settings via the admin UI: **Core > Khalti Configurations**.

- `PostUrl` — initiate endpoint, e.g. `https://dev.khalti.com/api/v2/epayment/initiate/`
- `VerifyUrl` — lookup endpoint, e.g. `https://dev.khalti.com/api/v2/epayment/lookup/`
- `AuthorizationKey` — secret key used in the `Authorization: Key {key}` header
- `WebsiteUrl` — fallback website URL used when a request doesn't supply one

Runtime reads the current tenant's row; if none exists, payment initiation throws "Khalti configuration is not set up for this tenant."

## 8. Response model (must use JsonPropertyName)

**File:** `Application/Interfaces/IKhaltiService.cs`

```csharp
public class KhaltiInitiateResponse
{
    [JsonPropertyName("pidx")] public string? Pidx { get; set; }
    [JsonPropertyName("payment_url")] public string? PaymentUrl { get; set; }
    [JsonPropertyName("expires_at")] public DateTime? ExpiresAt { get; set; }
    [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
}

public class KhaltiLookupResponse
{
    [JsonPropertyName("pidx")] public string? Pidx { get; set; }
    [JsonPropertyName("total_amount")] public long TotalAmount { get; set; }
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("transaction_id")] public string? TransactionId { get; set; }
    [JsonPropertyName("fee")] public long Fee { get; set; }
    [JsonPropertyName("refunded")] public bool Refunded { get; set; }
}
```

**CRITICAL:** Khalti API returns snake_case JSON (`payment_url`, `total_amount`, `transaction_id`). The `[JsonPropertyName]` attributes are REQUIRED — `System.Text.Json` cannot match snake_case to PascalCase even with `PropertyNameCaseInsensitive = true` because of the underscores.

## 9. Logging

All Khalti API calls are logged via Serilog (configured in `EntryPoint.cs`) to:
- Console
- `logs/log-.txt` — daily rolling file (retained for 30 days)

## 10. Fee calculation formula

| Component | Source | DB Table |
|-----------|--------|----------|
| Theory fee | `BillTitle` linked to `ExamScheduleId` | `BillTitle` |
| Practical fee | `ProgramSubjectPracticalCharge` (per-program) | `ProgramSubjectPracticalCharge` |
| Total = Theory fee per subject × theory subjects | Sum of BillTitle amount | `BillTitle.Amount` |

Practical subjects are auto-ticked; their count is tracked separately. The grand total displayed in the payment form includes all charges.

## 11. Troubleshooting

| Symptom | Cause | Fix |
|---------|-------|-----|
| Khalti initiate succeeds but no redirect | `PaymentUrl` null from deserialization | Add `[JsonPropertyName("payment_url")]` |
| Khalti API returns 401 | Wrong Authorization header format | Use `key {SecretKey}` (lowercase `key`) |
| Callback shows "verification failed" | Lookup API failure or Status != "Completed" | Check `logs/error.log` for lookup error |
| "An error occurred while starting the application" | Missing migration or bad config | Run `dotnet ef database update` |
| App starts on different URL | Port changed in launchSettings | WebsiteUrl derived dynamically from request |
