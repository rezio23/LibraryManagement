using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Text;

namespace book_Managment
{
    internal class AdminDashboard
    {
        private const string connectionString = "Server=DESKTOP-RHMPA0K\\SQLEXPRESS;Database=book_management;Trusted_Connection=True;TrustServerCertificate=True;";
        public static void Show()
        {
            while (true)
            {
                Console.WriteLine("1. Book's Data");
                Console.WriteLine("2. Member's Data");
                Console.WriteLine("3. Book's History");
                Console.WriteLine("4. Logout");
                Console.Write("Enter the Main Dashboard's value: ");

                switch (Console.ReadLine())
                {
                    case "1": Console.Clear(); Book(); break;
                    case "2": Console.Clear(); Member(); break;
                    case "3": Console.Clear(); History(); break;
                    case "4": Console.Clear(); Logout(); return;
                    default: Console.WriteLine("Invalid choice. Please try again." + '\n'); break;
                }

            }
        }
        private static void Logout()
        {
            Console.WriteLine("Logging out...");
            Pause();
            Login.Show();
        }

        // Book's Data -------------------------------------------------------------------

        private static void Book()
        {
            while (true)
            {
                Console.WriteLine("1 View Books");
                Console.WriteLine("2 Add Books");
                Console.WriteLine("3 Update Books");
                Console.WriteLine("4 Delete Books");
                Console.WriteLine("5 Back");
                Console.Write("Enter book's menu: ");

                switch (Console.ReadLine())
                {
                    case "1": ViewBook(); break;
                    case "2": AddBook(); break;
                    case "3": UpdateBook(); break;
                    case "4": DeleteBook(); break;
                    case "5": Console.Clear(); return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again." + '\n');
                        break;
                }
            }

            void ViewBook()
            {
                Console.Clear();
                SqlConnection? connection = null;
                SqlCommand? command = null;
                SqlDataReader? reader = null;

                connection = new SqlConnection(connectionString);
                connection.Open();

                command = new SqlCommand("sp_GetAllBooks", connection);
                command.CommandType = CommandType.StoredProcedure;

                reader = command.ExecuteReader();
                {
                    Console.WriteLine($"\n{"ID",-5} {"Title",-30} {"Author",-25} {"Year",-8} {"Category",-15} {"Total",-6}");
                    Console.WriteLine(new string('-', 95));
                    while (reader.Read())
                    {
                        Console.WriteLine($"{reader["Id"],-5} {reader["Title"],-30} {reader["Author"],-25} {reader["Publish"],-8} {reader["Category"],-15} {reader["Total"],-6}");
                    }
                    Console.WriteLine(new string('-', 95));
                    Console.WriteLine();
                }
            }

            void AddBook()
            {
                Console.Clear();
                try
                {
                    ViewBook();
                    CancelHint();

                    SqlCommand? command = null;
                    SqlConnection? connection = null;

                    string bookTitle = BookTitleFormat("Enter Book's Title: ");
                    string authorName = NameFormat("Enter Author's Name: ");
                    int publishYear = YearFormat("Enter Publish's Year: ");
                    string bookCategory = NameFormat("Enter Book's Category: ");
                    int totalBooks = NumberFormat("Enter Book's Total: ");

                    if (DuplicateBookExists(bookTitle, authorName))
                    {
                        Console.WriteLine("This book already exists. Please try again." + '\n');
                        return;
                    }

                    connection = new SqlConnection(connectionString);
                    command = new SqlCommand("sp_InsertBook", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    AddParams(command,
                        ("@Title", bookTitle),
                        ("@Author", authorName),
                        ("@Publish", publishYear),
                        ("@Category", bookCategory),
                        ("@Total", totalBooks)
                    );
                    int row = command.ExecuteNonQuery();

                    ViewBook();
                    Console.WriteLine("Inserted 1 data!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to book menu.");
                    Pause();
                }
            }

            void UpdateBook()
            {
                Console.Clear();
                SqlConnection? connection = null;

                try
                {
                    ViewBook();
                    CancelHint();

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    int updateBookId;
                    while (true)
                    {
                        updateBookId = NumberFormat("Select ID to update: ");

                        if (updateBookId <= 0)
                        {
                            Console.WriteLine("Book's ID must be bigger than 0!" + '\n');
                            continue;
                        }

                        if (!ExistedId(connection, "books_manage", updateBookId))
                        {
                            Console.WriteLine("Book ID not found! Please try again." + '\n');
                            continue;
                        }

                        break;
                    }

                    var currentBook = GetBookById(updateBookId);

                    Console.WriteLine("\nPress Enter to skip a field and keep the old value.\n");

                    string finalTitle = OptionalBookTitleFormat($"Update book's title [{currentBook.Title}] to:  ") ?? currentBook.Title;
                    string finalAuthor = OptionalNameFormat($"Update book's author [{currentBook.Author}] to:  ") ?? currentBook.Author;
                    int finalPublish = OptionalYearFormat($"Update published year [{currentBook.Publish}] to:  ") ?? currentBook.Publish;
                    string finalCategory = OptionalNameFormat($"Update book's category [{currentBook.Category}] to:  ") ?? currentBook.Category;
                    int finalTotal = OptionalNumberFormat($"Update book's total [{currentBook.Total}] to:  ") ?? currentBook.Total;

                    if (DuplicateBookExistsExcludeId(updateBookId, finalTitle, finalAuthor))
                    {
                        Console.WriteLine("Another book with the same title and author already exists!" + '\n');
                        Pause();
                        return;
                    }

                    ExecuteBookUpdateProcedure(
                        updateBookId,
                        title: finalTitle,
                        author: finalAuthor,
                        publish: finalPublish,
                        category: finalCategory,
                        total: finalTotal
                    );

                    ViewBook();
                    Console.WriteLine("Book updated successfully!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to book menu.");
                    Pause();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pause();
                }
                finally
                {
                    if (connection != null && connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }

            void DeleteBook()
            {
                Console.Clear();
                try
                {
                    ViewBook();
                    CancelHint();

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    int deleteId;
                    while (true)
                    {
                        deleteId = NumberFormat("Select ID to delete: ");
                        if (!ExistedId(connection, "books_manage", deleteId))
                        {
                            Console.WriteLine("Book ID not found! Please try again." + '\n');
                            continue;
                        }
                        break;
                    }

                    if (BookUsedInHistory(deleteId))
                    {
                        Console.WriteLine("Cannot delete! This book has borrow record(s) in history.");
                        Console.WriteLine("Delete the borrow history first, then try again." + '\n');
                        return;
                    }

                    command = new SqlCommand("sp_DeleteBook", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    AddParams(command,
                        ("@Id", deleteId)
                    );

                    int row = command.ExecuteNonQuery();
                    ViewBook();
                    Console.WriteLine(row > 0 ? "1 row deleted!" + '\n' : "ID not found! Please try again!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to book menu.");
                    Pause(); return;
                }
            }
        }
        // Member's Data -------------------------------------------------------------------
        private static void Member()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("1 View Members");
                Console.WriteLine("2 Add Members");
                Console.WriteLine("3 Update Members");
                Console.WriteLine("4 Delete Members");
                Console.WriteLine("5 Back");
                Console.Write("Enter member's menu: ");

                switch (Console.ReadLine())
                {
                    case "1": ViewMember(); break;
                    case "2": AddMember(); break;
                    case "3": UpdateMember(); break;
                    case "4": DeleteMember(); break;
                    case "5": Console.Clear(); return;
                    default: Console.WriteLine("Invalid choice. Please try again." + "\n"); break;
                }
            }

            void ViewMember()
            {
                Console.Clear();
                Console.WriteLine();

                SqlConnection? connection = null;
                SqlDataReader? reader = null;

                connection = new SqlConnection(connectionString);
                connection.Open();
                using SqlCommand command = new SqlCommand("sp_GetAllMembers", connection);
                command.CommandType = CommandType.StoredProcedure;

                reader = command.ExecuteReader();
                {
                    Console.WriteLine($"\n{"ID",-5} {"Name",-20} {"Date of Birth",-15} {"Member Since",-15}");
                    Console.WriteLine(new string('-', 60));
                    while (reader.Read())
                    {
                        DateTime dob = Convert.ToDateTime(reader["Dob"]);
                        DateTime since = Convert.ToDateTime(reader["Member_Since"]);

                        Console.WriteLine($"{reader["Id"],-5} {reader["Name"],-20} {dob.ToString("dd-MM-yyyy"),-15} {since.ToString("dd-MM-yyyy"),-15}");
                    }

                    Console.WriteLine(new string('-', 60));
                    Console.WriteLine();
                }
            }

            void AddMember()
            {
                Console.Clear();
                try
                {
                    ViewMember();
                    CancelHint();

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    string memberName = NameFormat("Enter Member's Name: ");
                    DateTime memberDOB;
                    int age;

                    while (true)
                    {
                        memberDOB = DateFormat("Enter Member's DOB (dd-MM-yyyy): ");

                        age = DateTime.Today.Year - memberDOB.Year;
                        if (memberDOB.Date > DateTime.Today.AddYears(-age))
                            age--;

                        if (age < 15)
                        {
                            Console.WriteLine("Member's age should be at least 15 years old!" + '\n'); continue;
                        }

                        break;
                    }

                    DateTime memberShip;
                    while (true)
                    {
                        memberShip = DateFormat("Enter Member's Membership Year (dd-MM-yyyy): ");

                        if (memberShip.Date > DateTime.Today)
                        {
                            Console.WriteLine("Membership year cannot be in the future!" + '\n'); continue;
                        }

                        if (memberShip.Date <= memberDOB.Date)
                        {
                            Console.WriteLine("Membership date must be after DOB!" + '\n'); continue;
                        }

                        break;
                    }

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    command = new SqlCommand("sp_InsertMember", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    AddParams(command,
                        ("@Name", memberName),
                        ("@Dob", memberDOB),
                        ("@Member_Since", memberShip)
                    );

                    int row = command.ExecuteNonQuery();

                    ViewMember();
                    Console.WriteLine("1 row inserted!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to member menu.");

                    Pause(); return;
                }
            }

            void UpdateMember()
            {
                Console.Clear();
                SqlConnection? connection = null;

                try
                {
                    ViewMember();
                    CancelHint();

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    int updateMemberId;
                    while (true)
                    {
                        updateMemberId = NumberFormat("Select ID to Update: ");

                        if (updateMemberId <= 0)
                        {
                            Console.WriteLine("Member's ID must be bigger than 0!" + '\n');
                            continue;
                        }

                        if (!ExistedId(connection, "members_manage", updateMemberId))
                        {
                            Console.WriteLine("Member ID not found! Please try again." + '\n');
                            continue;
                        }

                        break;
                    }

                    var currentMember = GetMemberById(updateMemberId);

                    Console.WriteLine("\nPress Enter to skip a field and keep the old value.\n");

                    string finalName = OptionalNameFormat($"Update member's name [{currentMember.Name}] to: ") ?? currentMember.Name;

                    DateTime? newDobInput = null;
                    while (true)
                    {
                        DateTime? tempDob = OptionalDateFormatSkip($"Update member's dob [{currentMember.Dob:dd-MM-yyyy}] to: ");

                        if (tempDob == null)
                            break;

                        int age = DateTime.Today.Year - tempDob.Value.Year;
                        if (tempDob.Value.Date > DateTime.Today.AddYears(-age))
                            age--;

                        if (age < 15)
                        {
                            Console.WriteLine("Member's age should be at least 15 years old!" + '\n');
                            continue;
                        }

                        newDobInput = tempDob;
                        break;
                    }

                    DateTime? newMemberSinceInput = null;
                    while (true)
                    {
                        DateTime? tempMemberSince = OptionalDateFormatSkip($"Update member's membership [{currentMember.MemberSince:dd-MM-yyyy}] to: ");

                        if (tempMemberSince == null)
                            break;

                        if (tempMemberSince.Value.Date > DateTime.Today)
                        {
                            Console.WriteLine("Membership year cannot be in the future!" + '\n');
                            continue;
                        }

                        newMemberSinceInput = tempMemberSince;
                        break;
                    }

                    DateTime finalDob = newDobInput ?? currentMember.Dob;
                    DateTime finalMemberSince = newMemberSinceInput ?? currentMember.MemberSince;

                    if (finalMemberSince.Date <= finalDob.Date)
                    {
                        Console.WriteLine("Membership date must be after DOB!" + '\n');
                        Pause();
                        return;
                    }

                    ExecuteMemberUpdateProcedure(
                        updateMemberId,
                        name: finalName,
                        dob: finalDob,
                        memberSince: finalMemberSince
                    );

                    ViewMember();
                    Console.WriteLine("Member updated successfully!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to member menu.");
                    Pause();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pause();
                }
                finally
                {
                    if (connection != null && connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }

            void DeleteMember()
            {
                Console.Clear();
                try
                {
                    ViewMember();
                    CancelHint();

                    SqlCommand? command = null;
                    SqlConnection? connection = null;

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    int deleteMemberId;
                    while (true)
                    {
                        deleteMemberId = NumberFormat("Select ID to Delete: ");
                        if (!ExistedId(connection, "members_manage", deleteMemberId))
                        {
                            Console.WriteLine("Member ID not found! Please try again." + '\n');
                            continue;
                        }
                        break;
                    }

                    if (MemberUsedInHistory(deleteMemberId))
                    {
                        Console.WriteLine("Cannot delete! This member has borrow record(s) in history.");
                        Console.WriteLine("Delete the borrow history first, then try again." + '\n');
                        return;
                    }

                    command = new SqlCommand("sp_DeleteMember", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    AddParams(command,
                        ("@Id", deleteMemberId)
                    );
                    int row = command.ExecuteNonQuery();

                    ViewMember();
                    Console.WriteLine(row > 0 ? "1 row deleted!" + '\n' : "ID not found! Please try again!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to member menu.");
                    Pause(); return;
                }
            }
        }

        // History's Data -------------------------------------------------------------------
        private static void History()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("1 View History");
                Console.WriteLine("2 Add History");
                Console.WriteLine("3 Update History");
                Console.WriteLine("4 Delete History");
                Console.WriteLine("5 Back");
                Console.Write("Enter history's menu: ");

                switch (Console.ReadLine())
                {
                    case "1": ViewHistory(); break;
                    case "2": AddHistory(); break;
                    case "3": UpdateHistory(); break;
                    case "4": DeleteHistory(); break;
                    case "5": Console.Clear(); return;
                    default: Console.WriteLine("Invalid choice. Please try again." + '\n'); break;
                }
            }

            void ViewHistory()
            {
                Console.Clear();
                Console.WriteLine();

                SqlConnection? connection = null;
                SqlCommand? command = null;
                SqlDataReader? reader = null;

                connection = new SqlConnection(connectionString);
                connection.Open();

                command = new SqlCommand("sp_GetAllHistory", connection);
                command.CommandType = CommandType.StoredProcedure;

                reader = command.ExecuteReader();
                {
                    Console.WriteLine($"\n{"ID",-5} {"Book ID",-10} {"Member ID",-12} {"Borrowed Date",-15} {"Returned Date",-15}");
                    Console.WriteLine(new string('-', 62));

                    while (reader.Read())
                    {
                        DateTime borrowed = Convert.ToDateTime(reader["Borrowed_Date"]);
                        string returned = reader["Returned_Date"] == DBNull.Value
                            ? "Not Returned"
                            : Convert.ToDateTime(reader["Returned_Date"]).ToString("dd-MM-yyyy");

                        Console.WriteLine($"{reader["Id"],-5} {reader["BookID"],-10} {reader["MemberID"],-12} {borrowed.ToString("dd-MM-yyyy"),-15} {returned,-15}");
                    }

                    Console.WriteLine(new string('-', 62));
                    Console.WriteLine();
                }
            }
            void AddHistory()
            {
                Console.Clear();
                try
                {
                    ViewHistory();
                    CancelHint();

                    int bookId;
                    while (true)
                    {
                        bookId = NumberFormat("Enter Book's ID: ");

                        if (!BookExists(bookId))
                        {
                            Console.WriteLine("Book ID not found. Please try again." + '\n');
                            continue;
                        }

                        break;
                    }

                    int memberId;
                    while (true)
                    {
                        memberId = NumberFormat("Enter Member's ID: ");

                        if (!MemberExists(memberId))
                        {
                            Console.WriteLine("Member ID not found. Please try again." + '\n');
                            continue;
                        }

                        if (ActiveBorrowExists(bookId, memberId))
                        {
                            Console.WriteLine("This member is already borrowing this book and hasn't returned it yet!" + '\n');
                            continue;
                        }

                        break;
                    }

                    DateTime borrowedDate;
                    while (true)
                    {
                        borrowedDate = DateFormat("Enter Borrowed Date (dd-MM-yyyy): ");

                        if (borrowedDate.Date > DateTime.Today)
                        {
                            Console.WriteLine("Borrowed date cannot be in the future. Please try again." + '\n');
                            continue;
                        }

                        break;
                    }

                    object returnedValue;
                    while (true)
                    {
                        returnedValue = OptionalDateFormat("Enter Returned Date (dd-MM-yyyy), or press Enter to skip: ");

                        if (returnedValue == DBNull.Value)
                            break;

                        DateTime returnedDate = (DateTime)returnedValue;

                        if (returnedDate.Date <= borrowedDate.Date)
                        {
                            Console.WriteLine("Returned date must be after borrowed date. Please try again." + '\n');
                            continue;
                        }

                        break;
                    }

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    try
                    {
                        connection = new SqlConnection(connectionString);
                        connection.Open();

                        command = new SqlCommand("sp_InsertHistory", connection);
                        command.CommandType = CommandType.StoredProcedure;

                        AddParams(command,
                            ("@BookID", bookId),
                            ("@MemberID", memberId),
                            ("@Borrowed_Date", borrowedDate),
                            ("@Returned_Date", returnedValue)
                        );

                        int row = command.ExecuteNonQuery();

                        ViewHistory();
                        Console.WriteLine(row > 0 ? "1 row inserted!" + '\n' : "Insert failed!" + '\n');
                        Pause();
                    }
                    finally
                    {
                        if (command != null)
                            command.Dispose();

                        if (connection != null && connection.State == ConnectionState.Open)
                            connection.Close();
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    Pause();
                    Console.Clear();
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pause();
                    Console.Clear();
                    return;
                }
            }

            void UpdateHistory()
            {
                Console.Clear();
                SqlConnection? connection = null;

                try
                {
                    ViewHistory();
                    CancelHint();

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    int updateHistoryId;
                    while (true)
                    {
                        updateHistoryId = NumberFormat("Select ID to Update: ");

                        if (updateHistoryId <= 0)
                        {
                            Console.WriteLine("History ID must be bigger than 0!" + '\n');
                            continue;
                        }

                        if (!ExistedId(connection, "borrow_history", updateHistoryId))
                        {
                            Console.WriteLine("History ID not found! Please try again." + '\n');
                            continue;
                        }

                        break;
                    }

                    var currentHistory = GetHistoryById(updateHistoryId);

                    int finalBookId = currentHistory.BookId;
                    int finalMemberId = currentHistory.MemberId;
                    DateTime finalBorrowedDate = currentHistory.BorrowedDate;
                    object finalReturnedDate = currentHistory.ReturnedDate;

                    Console.WriteLine("\nPress Enter to skip a field and keep the old value.");

                    while (true)
                    {
                        int? tempBookId = OptionalNumberFormat($"Update book's id [{finalBookId}] to: ");

                        if (tempBookId == null)
                            break;

                        if (!BookExists(tempBookId.Value))
                        {
                            Console.WriteLine("Book ID not found. Please try again." + '\n');
                            continue;
                        }

                        if (finalReturnedDate == DBNull.Value &&
                            ActiveBorrowExistsExcludeHistoryId(updateHistoryId, tempBookId.Value, finalMemberId))
                        {
                            Console.WriteLine("This member is already borrowing this book and hasn't returned it yet!" + '\n');
                            continue;
                        }

                        finalBookId = tempBookId.Value;
                        break;
                    }

                    while (true)
                    {
                        int? tempMemberId = OptionalNumberFormat($"Update member's id [{finalMemberId}] to: ");

                        if (tempMemberId == null)
                            break;

                        if (!MemberExists(tempMemberId.Value))
                        {
                            Console.WriteLine("Member ID not found. Please try again." + '\n');
                            continue;
                        }

                        if (finalReturnedDate == DBNull.Value &&
                            ActiveBorrowExistsExcludeHistoryId(updateHistoryId, finalBookId, tempMemberId.Value))
                        {
                            Console.WriteLine("This member is already borrowing this book and hasn't returned it yet!" + '\n');
                            continue;
                        }

                        finalMemberId = tempMemberId.Value;
                        break;
                    }

                    while (true)
                    {
                        DateTime? tempBorrowedDate = OptionalDateFormatSkip($"Update borrowed date [{finalBorrowedDate:dd-MM-yyyy}] to: ");

                        if (tempBorrowedDate == null)
                            break;

                        if (tempBorrowedDate.Value.Date > DateTime.Today)
                        {
                            Console.WriteLine("Borrowed date cannot be in the future. Please try again." + '\n');
                            continue;
                        }

                        if (finalReturnedDate != DBNull.Value)
                        {
                            DateTime returnedDate = (DateTime)finalReturnedDate;

                            if (tempBorrowedDate.Value.Date >= returnedDate.Date)
                            {
                                Console.WriteLine("Borrowed date must be before returned date. Please try again." + '\n');
                                continue;
                            }
                        }

                        finalBorrowedDate = tempBorrowedDate.Value;
                        break;
                    }

                    Console.WriteLine("Type clear on Returned Date to mark it as Not Returned.\n");

                    while (true)
                    {
                        string oldReturnedText = finalReturnedDate == DBNull.Value
                            ? "Not Returned"
                            : ((DateTime)finalReturnedDate).ToString("dd-MM-yyyy");

                        object? tempReturnedDate = OptionalReturnedDateUpdateFormat(
                            $"Update returned date [{oldReturnedText}] (Enter=skip, clear=Not Returned) to: "
                        );

                        if (tempReturnedDate == null)
                            break;

                        if (tempReturnedDate != DBNull.Value)
                        {
                            DateTime returnedDate = (DateTime)tempReturnedDate;

                            if (returnedDate.Date <= finalBorrowedDate.Date)
                            {
                                Console.WriteLine("Returned date must be after borrowed date. Please try again." + '\n');
                                continue;
                            }
                        }
                        else
                        {
                            if (ActiveBorrowExistsExcludeHistoryId(updateHistoryId, finalBookId, finalMemberId))
                            {
                                Console.WriteLine("This member is already borrowing this book and hasn't returned it yet!" + '\n');
                                continue;
                            }
                        }

                        finalReturnedDate = tempReturnedDate;
                        break;
                    }

                    ExecuteHistoryUpdateProcedure(
                        updateHistoryId,
                        bookId: finalBookId,
                        memberId: finalMemberId,
                        borrowedDate: finalBorrowedDate,
                        returnedDate: finalReturnedDate
                    );

                    ViewHistory();
                    Console.WriteLine("History updated successfully!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    Pause();
                    Console.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pause();
                }
                finally
                {
                    if (connection != null && connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }

            void DeleteHistory()
            {
                Console.Clear();
                try
                {
                    ViewHistory();
                    CancelHint();

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    int deleteHistoryId;
                    while (true)
                    {
                        deleteHistoryId = NumberFormat("Select ID to Delete: ");
                        if (!ExistedId(connection, "borrow_history", deleteHistoryId))
                        {
                            Console.WriteLine("History ID not found! Please try again." + '\n');
                            continue;
                        }
                        break;
                    }

                    command = new SqlCommand("sp_DeleteHistory", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    AddParams(command,
                        ("@Id", deleteHistoryId)
                    );
                    int row = command.ExecuteNonQuery();
                    ViewHistory();
                    Console.WriteLine(row > 0 ? "1 row deleted!" + '\n' : "ID not found!" + '\n');
                    Pause();
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    Pause();
                    Console.Clear(); return;
                }
            }
        }
        static void Pause()
        {
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        static void CancelHint()
        {
            Console.WriteLine("Press Escape key to stop and go back." + '\n');
        }

        private static string CancelInput(string inputTxt)
        {
            Console.Write(inputTxt);

            StringBuilder input = new StringBuilder();

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);

                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    throw new OperationCanceledException();
                }

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input.ToString();
                }

                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (input.Length > 0)
                    {
                        input.Remove(input.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    continue;
                }

                if (!char.IsControl(keyInfo.KeyChar))
                {
                    input.Append(keyInfo.KeyChar);
                    Console.Write(keyInfo.KeyChar);
                }
            }
        }

        private static string BookTitleFormat(string title)
        {
            while (true)
            {
                string input = CancelInput(title);
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please try again." + '\n');
                    continue;
                }
                return input;
            }
        }

        private static string NameFormat(string name)
        {
            while (true)
            {
                string input = CancelInput(name);

                // Catch if it was empty
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Input cannot be empty. Please try again." + '\n'); continue;
                }

                // Catch if it contains non-letter characters
                if (!input.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                {
                    Console.WriteLine("Input must contain only letters and spaces. Please try again." + '\n'); continue;
                }

                return input;
            }
        }

        private static int NumberFormat(string number)
        {
            while (true)
            {
                string input = CancelInput(number);

                if (!int.TryParse(input, out int result))
                {
                    Console.WriteLine("Invalid! Please enter a number." + '\n'); continue;
                }

                if (result < 0)
                {
                    Console.WriteLine("Number cannot be negative. Please try again." + '\n'); continue;
                }
                return result;
            }
        }

        private static int YearFormat(string number)
        {
            while (true)
            {
                string input = CancelInput(number);

                if (!int.TryParse(input, out int result))
                {
                    Console.WriteLine("Invalid! Please enter a number." + '\n'); continue;
                }

                if (result < 0)
                {
                    Console.WriteLine("Number cannot be negative. Please try again." + '\n'); continue;
                }

                if (result > DateTime.Today.Year)
                {
                    Console.WriteLine("Year cannot be in the future. Please try again." + '\n'); continue;
                }

                if (result < 1000)
                {
                    Console.WriteLine("Year must be a 4-digit number. Please try again." + '\n'); continue;
                }

                return result;
            }
        }

        private static DateTime DateFormat(string date)
        {
            while (true)
            {
                string input = CancelInput(date);

                if (DateTime.TryParseExact(input, "dd-MM-yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;

                Console.WriteLine("Invalid! Use format dd-MM-yyyy." + '\n');
            }
        }

        private static object OptionalDateFormat(string optionalDate)
        {
            while (true)
            {
                string input = CancelInput(optionalDate);

                if (string.IsNullOrWhiteSpace(input))
                    return DBNull.Value;

                if (DateTime.TryParseExact(input, "dd-MM-yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;

                Console.WriteLine("Invalid! Use format dd-MM-yyyy or press Enter to skip." + '\n');
            }
        }

        private static bool ExistedId(SqlConnection connection, string tableName, int id)
        {
            _ = connection;

            return tableName switch
            {
                "books_manage" => BookExists(id),
                "members_manage" => MemberExists(id),
                "borrow_history" => HistoryExists(id),
                _ => throw new ArgumentException("Invalid table name." + '\n')
            };
        }
        private static (string Title, string Author, int Publish, string Category, int Total) GetBookById(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_GetBookById", connection);
            command.CommandType = CommandType.StoredProcedure;
            AddParams(command,
                ("@Id", id)
            );

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                throw new Exception("Book not found.");

            return
            (
                reader["Title"].ToString()!,
                reader["Author"].ToString()!,
                Convert.ToInt32(reader["Publish"]),
                reader["Category"].ToString()!,
                Convert.ToInt32(reader["Total"])
            );
        }

        private static (string Name, DateTime Dob, DateTime MemberSince) GetMemberById(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_GetMemberById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                throw new Exception("Member not found." + '\n');

            return
            (
                reader["Name"].ToString()!,
                Convert.ToDateTime(reader["Dob"]),
                Convert.ToDateTime(reader["Member_Since"])
            );
        }

        private static (int BookId, int MemberId, DateTime BorrowedDate, object ReturnedDate) GetHistoryById(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_GetHistoryById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader = command.ExecuteReader();

            if (!reader.Read())
                throw new Exception("History not found." + '\n');

            object returnedDate = reader["Returned_Date"] == DBNull.Value
                ? DBNull.Value
                : Convert.ToDateTime(reader["Returned_Date"]);

            return
            (
                Convert.ToInt32(reader["BookID"]),
                Convert.ToInt32(reader["MemberID"]),
                Convert.ToDateTime(reader["Borrowed_Date"]),
                returnedDate
            );
        }
        private static void AddParams(SqlCommand command, params (string Name, object? Value)[] parameters)
        {
            foreach (var (name, value) in parameters)
            {
                command.Parameters.AddWithValue(name, value ?? DBNull.Value);
            }
        }

        private static void AddBookUpdateParams(
            SqlCommand command,
            int id,
            string title,
            string author,
            int publish,
            string category,
            int total)
        {
            AddParams(command,
                ("@Id", id),
                ("@Title", title),
                ("@Author", author),
                ("@Publish", publish),
                ("@Category", category),
                ("@Total", total)
            );
        }

        private static void AddMemberUpdateParams(
            SqlCommand command,
            int id,
            string name,
            DateTime dob,
            DateTime memberSince)
        {
            AddParams(command,
                ("@Id", id),
                ("@Name", name),
                ("@Dob", dob),
                ("@Member_Since", memberSince)
            );
        }

        private static void AddHistoryUpdateParams(
            SqlCommand command,
            int id,
            int bookId,
            int memberId,
            DateTime borrowedDate,
            object returnedDate)
        {
            AddParams(command,
                ("@Id", id),
                ("@BookID", bookId),
                ("@MemberID", memberId),
                ("@Borrowed_Date", borrowedDate),
                ("@Returned_Date", returnedDate)
            );
        }

        private static void ExecuteBookUpdateProcedure(
    int id,
    string? title = null,
    string? author = null,
    int? publish = null,
    string? category = null,
    int? total = null)
        {
            var current = GetBookById(id);

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_UpdateBook", connection);
            command.CommandType = CommandType.StoredProcedure;

            AddBookUpdateParams(
                command,
                id,
                title ?? current.Title,
                author ?? current.Author,
                publish ?? current.Publish,
                category ?? current.Category,
                total ?? current.Total
            );

            command.ExecuteNonQuery();
        }

        private static void ExecuteMemberUpdateProcedure(
            int id,
            string? name = null,
            DateTime? dob = null,
            DateTime? memberSince = null)
        {
            var current = GetMemberById(id);

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_UpdateMember", connection);
            command.CommandType = CommandType.StoredProcedure;

            AddMemberUpdateParams(
                command,
                id,
                name ?? current.Name,
                dob ?? current.Dob,
                memberSince ?? current.MemberSince
            );

            command.ExecuteNonQuery();
        }

        private static void ExecuteHistoryUpdateProcedure(
            int id,
            int? bookId = null,
            int? memberId = null,
            DateTime? borrowedDate = null,
            object? returnedDate = null)
        {
            var current = GetHistoryById(id);

            int finalBookId = bookId ?? current.BookId;
            int finalMemberId = memberId ?? current.MemberId;
            DateTime finalBorrowedDate = borrowedDate ?? current.BorrowedDate;
            object finalReturnedDate = returnedDate ?? current.ReturnedDate;

            if (finalReturnedDate == DBNull.Value &&
                ActiveBorrowExistsExcludeHistoryId(id, finalBookId, finalMemberId))
            {
                throw new Exception("This member is already borrowing this book and hasn't returned it yet!");
            }

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_UpdateHistory", connection);
            command.CommandType = CommandType.StoredProcedure;

            AddHistoryUpdateParams(
                command,
                id,
                finalBookId,
                finalMemberId,
                finalBorrowedDate,
                finalReturnedDate
            );

            command.ExecuteNonQuery();
        }
        private static int ExecuteCountProcedure(string procedureName, Action<SqlCommand>? addParameters = null)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand(procedureName, connection);
            command.CommandType = CommandType.StoredProcedure;

            addParameters?.Invoke(command);

            return Convert.ToInt32(command.ExecuteScalar());
        }

        private static bool BookExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckBookExists", command =>
            {
                AddParams(command,
                     ("@Id", id)
                );
            }) > 0;
        }

        private static bool MemberExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckMemberExists", command =>
            {
                AddParams(command,
                     ("@Id", id)
                );
            }) > 0;
        }

        private static bool HistoryExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckHistoryExists", command =>
            {
                AddParams(command,
                     ("@Id", id)
                );
            }) > 0;
        }

        private static bool DuplicateBookExists(string title, string author)
        {
            return ExecuteCountProcedure("sp_CheckDuplicateBook", command =>
            {
                AddParams(command,
                     ("@Title", title),
                     ("@Author", author)
                );
            }) > 0;
        }

        private static bool DuplicateBookExistsExcludeId(int id, string title, string author)
        {
            return ExecuteCountProcedure("sp_CheckDuplicateBookExcludeId", command =>
            {
                AddParams(command,
                     ("@Id", id),
                     ("@Title", title),
                     ("@Author", author)
                );
            }) > 0;
        }

        private static bool BookUsedInHistory(int id)
        {
            return ExecuteCountProcedure("sp_CheckBookUsedInHistory", command =>
            {
                AddParams(command,
                     ("@Id", id)
                );
            }) > 0;
        }

        private static bool MemberUsedInHistory(int id)
        {
            return ExecuteCountProcedure("sp_CheckMemberUsedInHistory", command =>
            {
                AddParams(command,
                     ("@Id", id)
                );
            }) > 0;
        }
        private static bool ActiveBorrowExists(int bookId, int memberId)
        {
            return ExecuteCountProcedure("sp_CheckActiveBorrow", command =>
            {
                AddParams(command,
                    ("@BookID", bookId),
                    ("@MemberID", memberId)
                );
            }) > 0;
        }

        private static bool ActiveBorrowExistsExcludeHistoryId(int historyId, int bookId, int memberId)
        {
            return ExecuteCountProcedure("sp_CheckActiveBorrowExcludeHistoryId", command =>
            {
                AddParams(command,
                     ("@Id", historyId),
                     ("@BookID", bookId),
                     ("@MemberID", memberId)
                );
            }) > 0;
        }
        private static string? OptionalBookTitleFormat(string title)
        {
            while (true)
            {
                string input = CancelInput(title);

                if (string.IsNullOrWhiteSpace(input))
                    return null;

                return input;
            }
        }

        private static string? OptionalNameFormat(string name)
        {
            while (true)
            {
                string input = CancelInput(name);

                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (!input.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                {
                    Console.WriteLine("Input must contain only letters and spaces. Please try again." + '\n');
                    continue;
                }

                return input;
            }
        }

        private static int? OptionalNumberFormat(string number)
        {
            while (true)
            {
                string input = CancelInput(number);

                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (!int.TryParse(input, out int result))
                {
                    Console.WriteLine("Invalid! Please enter a number." + '\n');
                    continue;
                }

                if (result < 0)
                {
                    Console.WriteLine("Number cannot be negative. Please try again." + '\n');
                    continue;
                }

                return result;
            }
        }

        private static int? OptionalYearFormat(string number)
        {
            while (true)
            {
                string input = CancelInput(number);

                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (!int.TryParse(input, out int result))
                {
                    Console.WriteLine("Invalid! Please enter a number." + '\n');
                    continue;
                }

                if (result < 0)
                {
                    Console.WriteLine("Number cannot be negative. Please try again." + '\n');
                    continue;
                }

                if (result > DateTime.Today.Year)
                {
                    Console.WriteLine("Year cannot be in the future. Please try again." + '\n');
                    continue;
                }

                if (result < 1000)
                {
                    Console.WriteLine("Year must be a 4-digit number. Please try again." + '\n');
                    continue;
                }

                return result;
            }
        }

        private static DateTime? OptionalDateFormatSkip(string date)
        {
            while (true)
            {
                string input = CancelInput(date);

                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (DateTime.TryParseExact(input, "dd-MM-yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;

                Console.WriteLine("Invalid! Use format dd-MM-yyyy or press Enter to skip." + '\n');
            }
        }

        private static object? OptionalReturnedDateUpdateFormat(string optionalDate)
        {
            while (true)
            {
                string input = CancelInput(optionalDate);

                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
                    return DBNull.Value;

                if (DateTime.TryParseExact(input, "dd-MM-yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;

                Console.WriteLine("Invalid! Use dd-MM-yyyy, press Enter to skip, or type clear to mark as Not Returned." + '\n');
            }
        }
    }
}
