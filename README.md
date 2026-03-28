# Book Management Console App

This project is a C# console application for managing a small library-style system with SQL Server. It lets a user maintain three related areas of data:

- books
- members
- borrow history

The application is implemented as a single console program in `Program.cs`, with SQL Server used as the persistent data store.

## Overview

When the app starts, it immediately opens a SQL Server connection, checks the database, and then shows a main dashboard with these sections:

- `1. Book's Data`
- `2. Member's Data`
- `3. Book's History`
- `4. Exit`

Each section supports view, add, update, and delete operations through console menus.

## Features

### Book management

- View all books in a formatted table
- Add a new book
- Prevent duplicate books with the same `Title` and `Author`
- Re-prompt for update and delete IDs until an existing book is selected or the operation is canceled
- Update individual fields:
  - title
  - author
  - publish year
  - category
  - total quantity
- Delete a book only when it is not referenced by borrow history

### Member management

- View all members in a formatted table
- Add a new member
- Re-prompt for update and delete IDs until an existing member is selected or the operation is canceled
- Update individual fields:
  - name
  - date of birth
  - membership date
- Enforce a minimum member age of 15
- Prevent membership dates that are in the future
- Require membership dates to be after the member's date of birth
- Delete a member only when they are not referenced by borrow history

### Borrow history management

- View all borrow records in a formatted table
- Add a new borrow record
- Re-prompt for update and delete IDs until an existing history record is selected or the operation is canceled
- Update individual fields:
  - book ID
  - member ID
  - borrowed date
  - returned date
- Allow `Returned_Date` to stay empty for books that have not been returned yet
- Display missing return dates as `Not Returned`
- Validate referenced book IDs and member IDs before creating or updating history
- Prevent a member from borrowing the same book again while an earlier record is still unreturned
- Prevent borrowed dates from being in the future
- Require returned dates to be after borrowed dates

## Input and validation rules

The program includes reusable console validation helpers and a cancellation flow:

- `Escape` cancels the current operation and returns to the related menu
- Update-field selection prompts also use the same cancellable input flow
- Book titles can be any non-empty text
- Names and categories accept letters and spaces only
- Numeric fields must be non-negative integers
- Years must be 4 digits and cannot be in the future
- Dates must use `yyyy-MM-dd`
- Optional return dates can be left blank

## Tech stack

- .NET `10.0`
- `Microsoft.Data.SqlClient` `7.0.0`
- SQL Server with Windows authentication
- Console UI with top-level statements and local functions

## Project structure

- `Program.cs`: all application logic, menu handling, validation, and SQL operations
- `book_Managment.csproj`: project configuration and package references
- `book_Managment.sln`: solution file

## Database requirements

The current code expects a SQL Server database named `book_management` and a local SQL Server Express instance matching this connection string in `Program.cs`:

```csharp
Server=DESKTOP-RHMPA0K\SQLEXPRESS;Database=book_management;Trusted_Connection=True;TrustServerCertificate=True;
```

If you run this project on another machine, you will almost certainly need to update that connection string first.

### Expected tables

The program reads and writes these tables:

- `books_manage`
- `members_manage`
- `borrow_history`

### Expected columns

The code assumes these fields exist:

- `books_manage`: `Id`, `Title`, `Author`, `Publish`, `Category`, `Total`
- `members_manage`: `Id`, `Name`, `Dob`, `Member_Since`
- `borrow_history`: `Id`, `BookID`, `MemberID`, `Borrowed_Date`, `Returned_Date`

### Expected stored procedures

The application uses stored procedures for viewing, inserting, and deleting records:

- `sp_GetAllBooks`
- `sp_InsertBook`
- `sp_DeleteBook`
- `sp_GetAllMembers`
- `sp_InsertMember`
- `sp_DeleteMember`
- `sp_GetAllHistory`
- `sp_InsertHistory`
- `sp_DeleteHistory`

Important note:

- update operations are currently implemented with inline SQL `UPDATE` statements, not stored procedures
- existence checks and duplicate checks also use inline SQL queries

## How to run

1. Install the .NET 10 SDK.
2. Make sure SQL Server or SQL Server Express is installed and accessible.
3. Create the `book_management` database.
4. Create the required tables and stored procedures.
5. Update the connection string in `Program.cs` if your SQL Server instance is different.
6. Run the project:

```powershell
dotnet build
dotnet run
```

The application connects to the database at startup, so it will fail immediately if the SQL Server instance, database, tables, or procedures are missing.

## Current implementation notes

- The whole app is contained in one file using nested local functions.
- The startup code queries `INFORMATION_SCHEMA.TABLES`, but the related console output is commented out.
- The program mixes stored procedures and inline SQL rather than using one consistent data-access approach.
- Update and delete flows now loop on invalid or missing IDs instead of ending the action immediately.
- `NameFormat(...)` is strict, which means some real names or category values with punctuation, numbers, or symbols will be rejected.

## Verification

`dotnet build` succeeds in the current project workspace.
