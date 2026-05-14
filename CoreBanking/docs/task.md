# CoreBanking — Task List

## 🔴 P0 — Critical Bugs
- [x] Fix `GetAccountById` throw Exception → return NotFound
- [x] Add Database Transaction cho Transfer/Deposit/Withdraw
- [x] Add Concurrency Control (RowVersion) cho Account entity

## 🟠 P1 — Important
- [x] Fix Transfer tạo 3 records → redesign thành 2 records với ReferenceId
- [x] Thêm Global Exception Handler
- [x] Map Health Check endpoints (`/health`, `/alive`)

## 🟡 P2 — Medium
- [x] Input Validation nhất quán (validate trong service layer)
- [x] Fix PostgreSQL volume mount path
- [x] Xóa connection string hardcode khỏi source code

## 🟢 P3 — Low Priority
- [x] Tách Service layer (ICustomerService, IAccountService)
- [x] Thêm logging vào services
- [x] Fix typos (stratery → strategy, Boostraping → Bootstrapping)
- [x] AccountNumber format có ý nghĩa (CB-YYYYMMDD-XXXXXX)
- [x] Viết Unit Tests — **84 tests, all passed ✅**
- [x] Viết Integration Tests — **22 tests written** (cần Docker running để chạy)
