# Book Management Console App

This project is a C# console application for managing a small library system with SQL Server. It now starts with a login screen and routes users into separate admin and user dashboards.

## Overview

The application manages three data areas:

- books
- members
- borrow history

Current flow:

1. Start the app
2. Sign in through the console login screen
3. Open either the admin dashboard or the user dashboard
4. Work with books, members, and borrowing history through menu-driven actions

## Roles

The login screen uses two hardcoded accounts:

- Admin: username `Sombath`, password `admin123`
- User: username `User`, password `user123`

### Admin dashboard

The admin dashboard supports full management:

- view books, members, and borrow history
- add new records
- update existing records in a single pass, with Enter to keep the current value
- delete existing records

### User dashboard

The user dashboard supports a more limited flow:

- view books, members, and borrow history
- add new records
- return to the previous screen through logout

## Features

- Console login with validation for usernames and passwords
- Book duplicate checks based on `Title` and `Author`
- Member age validation with a minimum age of 15
- Membership date validation
- Borrow and return date validation
- Support for open borrow records where `Returned_Date` is empty
- Checks to confirm referenced book IDs and member IDs exist
- Prevention of duplicate active borrow records for the same member and book
- `Escape`-based cancellation flow for data entry screens
- Admin update flows can leave fields unchanged by pressing `Enter`
- History updates support typing `clear` to reset `Returned_Date` back to "Not Returned"

## Validation rules

- Book titles must be non-empty
- Names and categories accept letters and spaces only
- Numeric values must be non-negative integers
- Years must be 4 digits and cannot be in the future
- Dates in the current admin flows use `dd-MM-yyyy`
- Optional returned dates may be left blank when creating history
- During history updates, pressing `Enter` skips the field and typing `clear` empties `Returned_Date`

## Tech stack

- .NET `10.0`
- `Microsoft.Data.SqlClient` `7.0.0`
- SQL Server with Windows authentication
- Console-based UI

## Project structure

- `Program.cs`: application entry point
- `Login.cs`: login screen and credential validation
- `AdminDashboard.cs`: admin-only menus and CRUD operations
- `UserDashboard.cs`: user-facing menus and limited operations
- `book_Managment.csproj`: project configuration and package references
- `book_Managment.sln`: solution file

## Database requirements

The project expects a SQL Server database named `book_management`.

The current connection string is hardcoded in both `AdminDashboard.cs` and `UserDashboard.cs`:

```csharp
Server=DESKTOP-RHMPA0K\SQLEXPRESS;Database=book_management;Trusted_Connection=True;TrustServerCertificate=True;
```

If you run the project on a different machine or SQL Server instance, update those files before running the app.

### Expected tables

- `books_manage`
- `members_manage`
- `borrow_history`

### Expected columns

- `books_manage`: `Id`, `Title`, `Author`, `Publish`, `Category`, `Total`
- `members_manage`: `Id`, `Name`, `Dob`, `Member_Since`
- `borrow_history`: `Id`, `BookID`, `MemberID`, `Borrowed_Date`, `Returned_Date`

### Expected stored procedures

- `sp_GetAllBooks`
- `sp_GetBookById`
- `sp_InsertBook`
- `sp_UpdateBook`
- `sp_DeleteBook`
- `sp_CheckBookExists`
- `sp_CheckDuplicateBook`
- `sp_CheckDuplicateBookExcludeId`
- `sp_CheckBookUsedInHistory`
- `sp_GetAllMembers`
- `sp_GetMemberById`
- `sp_InsertMember`
- `sp_UpdateMember`
- `sp_DeleteMember`
- `sp_CheckMemberExists`
- `sp_CheckMemberUsedInHistory`
- `sp_GetAllHistory`
- `sp_GetHistoryById`
- `sp_InsertHistory`
- `sp_UpdateHistory`
- `sp_DeleteHistory`
- `sp_CheckHistoryExists`
- `sp_CheckActiveBorrow`

## How to run

1. Install the .NET 10 SDK.
2. Make sure SQL Server or SQL Server Express is installed and available.
3. Create the `book_management` database.
4. Create the required tables and stored procedures.
5. Update the hardcoded connection string if needed.
6. Run the project:

```powershell
dotnet build
dotnet run
```

## Notes

- `Program.cs` is now a small entry point instead of containing the full application.
- The codebase is split into separate files for login, admin flow, and user flow.
- Update and validation flows now depend on the stored procedures listed above instead of inline count/update queries.
