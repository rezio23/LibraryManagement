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
                Console.WriteLine("4. Exit");
                Console.Write("Enter the Main Dashboard's value: ");

                switch (Console.ReadLine())
                {
                    case "1": Book(); break;
                    case "2": Member(); break;
                    case "3": History(); break;
                    case "4": Logout(); return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again." + '\n');
                        break;
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
                Console.Clear();
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
                    case "5": return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again." + '\n');
                        break;
                }
            }

            void ViewBook()
            {
                Console.Clear();
                Console.WriteLine();
                SqlConnection? connection = null;
                SqlCommand? command = null;
                SqlDataReader? reader = null;

                connection = new SqlConnection(connectionString);
                connection.Open();

                command = new SqlCommand("sp_GetAllBooks", connection);
                command.CommandType = CommandType.StoredProcedure; // Procedure

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
                        Console.WriteLine("This book already exists. Please try again.");
                        return;
                    }

                    connection = new SqlConnection(connectionString);
                    command = new SqlCommand("sp_InsertBook", connection);
                    connection.Open();

                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Title", bookTitle);
                    command.Parameters.AddWithValue("@Author", authorName);
                    command.Parameters.AddWithValue("@Publish", publishYear);
                    command.Parameters.AddWithValue("@Category", bookCategory);
                    command.Parameters.AddWithValue("@Total", totalBooks);
                    int row = command.ExecuteNonQuery();

                    Console.WriteLine("Inserted 1 data!");
                    ViewBook();
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
                try
                {
                    ViewBook();
                    CancelHint();

                    SqlConnection? connection = null;
                    int updateBookId;

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    while (true)
                    {
                        updateBookId = NumberFormat("Select ID to update: ");
                        if (updateBookId <= 0)
                        {
                            Console.WriteLine("Book's ID must be bigger than 0!"); continue;
                        }
                        if (!ExistedId(connection, "books_manage", updateBookId))
                        {
                            Console.WriteLine("Book ID not found! Please try again."); continue;
                        }
                        break;
                    }

                    Console.WriteLine("1. Update Book's Title");
                    Console.WriteLine("2. Update Author's Name");
                    Console.WriteLine("3. Update Publish's Year");
                    Console.WriteLine("4. Update Book's Category");
                    Console.WriteLine("5. Update Book's Total");

                    while (true)
                    {
                        string choice = CancelInput("Select the number to update: ");

                        switch (choice)
                        {
                            case "1": UpdateBookTitle(); return;
                            case "2": UpdateAuthorName(); return;
                            case "3": UpdatePublishYear(); return;
                            case "4": UpdateBookCategory(); return;
                            case "5": UpdateBookTotal(); return;
                            default: Console.WriteLine("Invalid choice. Please try again." + '\n'); break;
                        }
                    }

                    void UpdateBookTitle()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "books_manage", updateBookId))
                            {
                                Console.WriteLine("Book ID not found!");
                                Pause();
                                return;
                            }

                            string newBookTitle = BookTitleFormat("Enter new Book's Title: ");
                            var currentBook = GetBookById(updateBookId);

                            if (DuplicateBookExistsExcludeId(updateBookId, newBookTitle, currentBook.Author))
                            {
                                Console.WriteLine("Another book with the same title and author already exists!");
                                Pause();
                                return;
                            }

                            ExecuteBookUpdateProcedure(updateBookId, title: newBookTitle);

                            Console.WriteLine("Book title updated!");
                            ViewBook();
                            Pause();
                        }

                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateAuthorName()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "books_manage", updateBookId))
                            {
                                Console.WriteLine("Book ID not found!");
                                Pause();
                                return;
                            }

                            string newAuthorName = NameFormat("Enter new Author's Name: ");
                            var currentBook = GetBookById(updateBookId);

                            if (DuplicateBookExistsExcludeId(updateBookId, currentBook.Title, newAuthorName))
                            {
                                Console.WriteLine("Another book with the same title and author already exists!");
                                Pause();
                                return;
                            }

                            ExecuteBookUpdateProcedure(updateBookId, author: newAuthorName);

                            Console.WriteLine("Author name updated!");
                            ViewBook();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdatePublishYear()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "books_manage", updateBookId))
                            {
                                Console.WriteLine("Book ID not found!");
                                Pause();
                                return;
                            }

                            int publishYear = YearFormat("Enter new Publish's Year: ");

                            ExecuteBookUpdateProcedure(updateBookId, publish: publishYear);

                            Console.WriteLine("Publish year updated!");
                            ViewBook();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateBookCategory()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "books_manage", updateBookId))
                            {
                                Console.WriteLine("Book ID not found!");
                                Pause();
                                return;
                            }

                            string bookCategory = NameFormat("Enter new Book's Category: ");

                            ExecuteBookUpdateProcedure(updateBookId, category: bookCategory);

                            Console.WriteLine("Book category updated!");
                            ViewBook();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateBookTotal()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "books_manage", updateBookId))
                            {
                                Console.WriteLine("Book ID not found!");
                                Pause();
                                return;
                            }

                            int totalBooks = NumberFormat("Enter new Book's Total: ");

                            if (totalBooks < 0)
                            {
                                Console.WriteLine("Book's total cannot be negative!");
                                Pause();
                                return;
                            }

                            ExecuteBookUpdateProcedure(updateBookId, total: totalBooks);

                            Console.WriteLine("Book total updated!");
                            ViewBook();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to book menu.");
                    Pause(); return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pause(); return;
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
                        Console.WriteLine("Delete the borrow history first, then try again.");
                        return;
                    }

                    command = new SqlCommand("sp_DeleteBook", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", deleteId);

                    int row = command.ExecuteNonQuery();
                    Console.WriteLine(row > 0 ? "1 row deleted!" : "ID not found! Please try again!");
                    Pause(); return;
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
                    case "5": return;
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

                        Console.WriteLine($"{reader["Id"],-5} {reader["Name"],-20} {dob.ToShortDateString(),-15} {since.ToShortDateString(),-15}");
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
                        memberDOB = DateFormat("Enter Member's DOB (yyyy-MM-dd): ");

                        age = DateTime.Today.Year - memberDOB.Year;
                        if (memberDOB.Date > DateTime.Today.AddYears(-age))
                            age--;

                        if (age < 15)
                        {
                            Console.WriteLine("Member's age should be at least 15 years old!");
                            continue;
                        }

                        break;
                    }

                    DateTime memberShip;
                    while (true)
                    {
                        memberShip = DateFormat("Enter Member's Membership Year (yyyy-MM-dd): ");

                        if (memberShip.Date > DateTime.Today)
                        {
                            Console.WriteLine("Membership year cannot be in the future!");
                            continue;
                        }

                        if (memberShip.Date <= memberDOB.Date)
                        {
                            Console.WriteLine("Membership date must be after DOB!");
                            continue;
                        }

                        break;
                    }

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    command = new SqlCommand("sp_InsertMember", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Name", memberName);
                    command.Parameters.AddWithValue("@Dob", memberDOB);
                    command.Parameters.AddWithValue("@Member_Since", memberShip);

                    int row = command.ExecuteNonQuery();
                    Console.WriteLine("1 row inserted!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();

                    Pause(); return;
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
                try
                {
                    ViewMember();
                    CancelHint();

                    SqlConnection? connection = null;

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    int updateMemberId;
                    while (true)
                    {
                        updateMemberId = NumberFormat("Select ID to Update: ");
                        if (updateMemberId <= 0)
                        {
                            Console.WriteLine("Member's ID must be bigger than 0!");
                            continue;
                        }
                        if (!ExistedId(connection, "members_manage", updateMemberId))
                        {
                            Console.WriteLine("Member ID not found! Please try again.");
                            continue;
                        }
                        break;
                    }

                    Console.WriteLine("1. Update Member's Name");
                    Console.WriteLine("2. Update Member's DOB");
                    Console.WriteLine("3. Update Member's Membership Date");

                    while (true)
                    {
                        string choice = CancelInput("Select the number to update: ");

                        switch (choice)
                        {
                            case "1": UpdateMemberName(); return;
                            case "2": UpdateMemberDOB(); return;
                            case "3": UpdateMemberShip(); return;
                            default: Console.WriteLine("Invalid choice. Please try again." + '\n'); break;
                        }
                    }

                    void UpdateMemberName()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "members_manage", updateMemberId))
                            {
                                Console.WriteLine("Member ID not found!");
                                Pause();
                                return;
                            }

                            string updateMemberName = NameFormat("Enter new Member's Name: ");

                            ExecuteMemberUpdateProcedure(updateMemberId, name: updateMemberName);

                            Console.WriteLine("Member name updated!");
                            ViewMember();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateMemberDOB()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "members_manage", updateMemberId))
                            {
                                Console.WriteLine("Member ID not found!");
                                Pause();
                                return;
                            }

                            var currentMember = GetMemberById(updateMemberId);
                            DateTime updateMemberDOB;
                            int age;

                            while (true)
                            {
                                updateMemberDOB = DateFormat("Enter Member's DOB (yyyy-MM-dd): ");

                                age = DateTime.Today.Year - updateMemberDOB.Year;
                                if (updateMemberDOB.Date > DateTime.Today.AddYears(-age))
                                    age--;

                                if (age < 15)
                                {
                                    Console.WriteLine("Member's age should be at least 15 years old!");
                                    continue;
                                }

                                if (updateMemberDOB.Date >= currentMember.MemberSince.Date)
                                {
                                    Console.WriteLine("DOB must be before Membership date!");
                                    continue;
                                }

                                break;
                            }

                            ExecuteMemberUpdateProcedure(updateMemberId, dob: updateMemberDOB);

                            Console.WriteLine("Member DOB updated!");
                            ViewMember();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateMemberShip()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "members_manage", updateMemberId))
                            {
                                Console.WriteLine("Member ID not found!");
                                Pause();
                                return;
                            }

                            var currentMember = GetMemberById(updateMemberId);
                            DateTime updateMemberShip;

                            while (true)
                            {
                                updateMemberShip = DateFormat("Enter Member's Membership Year (yyyy-MM-dd): ");

                                if (updateMemberShip.Date > DateTime.Today)
                                {
                                    Console.WriteLine("Membership year cannot be in the future!");
                                    continue;
                                }

                                if (updateMemberShip.Date <= currentMember.Dob.Date)
                                {
                                    Console.WriteLine("Membership date must be after DOB!");
                                    continue;
                                }

                                break;
                            }

                            ExecuteMemberUpdateProcedure(updateMemberId, memberSince: updateMemberShip);

                            Console.WriteLine("Membership date updated!");
                            ViewMember();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to member menu.");
                    Pause(); return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pause(); return;
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
                            Console.WriteLine("Member ID not found! Please try again.");
                            continue;
                        }
                        break;
                    }

                    if (MemberUsedInHistory(deleteMemberId))
                    {
                        Console.WriteLine("Cannot delete! This member has borrow record(s) in history.");
                        Console.WriteLine("Delete the borrow history first, then try again.");
                        return;
                    }

                    command = new SqlCommand("sp_DeleteMember", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", deleteMemberId);
                    int row = command.ExecuteNonQuery();
                    Console.WriteLine(row > 0 ? "1 row deleted!" : "ID not found! Please try again!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();

                    Pause(); return;
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
                    case "5": return;
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
                            : Convert.ToDateTime(reader["Returned_Date"]).ToShortDateString();

                        Console.WriteLine($"{reader["Id"],-5} {reader["BookID"],-10} {reader["MemberID"],-12} {borrowed.ToShortDateString(),-15} {returned,-15}");
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

                    SqlConnection? connection = null;
                    SqlCommand? command = null;

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    command = new SqlCommand("sp_InsertHistory", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    int bookId;
                    while (true)
                    {
                        bookId = NumberFormat("Enter Book's ID: ");

                        if (ExistedId(connection, "books_manage", bookId))
                            break;

                        Console.WriteLine("Book ID not found. Please try again.");
                    }

                    int memberId;
                    while (true)
                    {
                        memberId = NumberFormat("Enter Member's ID: ");

                        if (ExistedId(connection, "members_manage", memberId))
                            break;

                        Console.WriteLine("Member ID not found. Please try again.");
                    }

                    DateTime borrowedDate;
                    while (true)
                    {
                        borrowedDate = DateFormat("Enter Borrowed Date (yyyy-MM-dd): ");
                        if (borrowedDate.Date <= DateTime.Today) break;
                        Console.WriteLine("Borrowed date cannot be in the future. Please try again.");
                    }

                    object returnedValue;
                    while (true)
                    {
                        returnedValue = OptionalDateFormat("Enter Returned Date (yyyy-MM-dd), or press Enter to skip: ");
                        if (returnedValue == DBNull.Value) break;

                        DateTime returnedDate = (DateTime)returnedValue;
                        if (returnedDate.Date > borrowedDate.Date) break;
                        Console.WriteLine("Returned date must be after borrowed date. Please try again.");
                    }
                    //string returnedDate = OptionalDateFormat("Enter Returned Date (yyyy-MM-dd), or press Enter to skip: ").ToString();
                    //object returnedValue = string.IsNullOrWhiteSpace(returnedDate)
                    //    ? (object)DBNull.Value
                    //    : DateTime.Parse(returnedDate);

                    if (ActiveBorrowExists(bookId, memberId))
                    {
                        Console.WriteLine("This member is already borrowing this book and hasn't returned it yet!");
                        return;
                    }

                    command.Parameters.AddWithValue("@BookID", bookId);
                    command.Parameters.AddWithValue("@MemberID", memberId);
                    command.Parameters.AddWithValue("@Borrowed_Date", borrowedDate);
                    command.Parameters.AddWithValue("@Returned_Date", returnedValue);
                    int row = command.ExecuteNonQuery();
                    Console.WriteLine("1 row inserted!");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();

                    Pause(); return;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    Pause(); return;
                }
            }

            void UpdateHistory()
            {
                Console.Clear();
                try
                {
                    ViewHistory();
                    CancelHint();

                    SqlConnection? connection = null;

                    int updateHistoryId;

                    connection = new SqlConnection(connectionString);
                    connection.Open();

                    while (true)
                    {
                        updateHistoryId = NumberFormat("Select ID to Update: ");
                        if (updateHistoryId <= 0)
                        {
                            Console.WriteLine("History ID must be bigger than 0!");
                            continue;
                        }
                        if (!ExistedId(connection, "borrow_history", updateHistoryId))
                        {
                            Console.WriteLine("History ID not found! Please try again.");
                            continue;
                        }
                        break;
                    }

                    Console.WriteLine("1. Update Book's ID");
                    Console.WriteLine("2. Update Member's ID");
                    Console.WriteLine("3. Update Borrowed Date");
                    Console.WriteLine("4. Update Returned Date");

                    while (true)
                    {
                        string choice = CancelInput("Select the number to update: ");

                        switch (choice)
                        {
                            case "1": UpdateHistoryBookId(); return;
                            case "2": UpdateHistoryMemberId(); return;
                            case "3": UpdateBorrowedDate(); return;
                            case "4": UpdateReturnedDate(); return;
                            default: Console.WriteLine("Invalid choice. Please try again." + '\n'); break;
                        }
                    }

                    void UpdateHistoryBookId()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "borrow_history", updateHistoryId))
                            {
                                Console.WriteLine("History ID not found. Please try again.");
                                Pause();
                                return;
                            }

                            int newBookId = NumberFormat("Enter new Book's ID: ");

                            if (!ExistedId(connection, "books_manage", newBookId))
                            {
                                Console.WriteLine("Book ID not found. Please try again.");
                                Pause();
                                return;
                            }

                            ExecuteHistoryUpdateProcedure(updateHistoryId, bookId: newBookId);

                            Console.WriteLine("Book ID updated!");
                            ViewHistory();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateHistoryMemberId()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "borrow_history", updateHistoryId))
                            {
                                Console.WriteLine("History ID not found. Please try again.");
                                Pause();
                                return;
                            }

                            int newMemberId = NumberFormat("Enter new Member's ID: ");

                            if (!ExistedId(connection, "members_manage", newMemberId))
                            {
                                Console.WriteLine("Member ID not found. Please try again.");
                                Pause();
                                return;
                            }

                            ExecuteHistoryUpdateProcedure(updateHistoryId, memberId: newMemberId);

                            Console.WriteLine("Member ID updated!");
                            ViewHistory();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateBorrowedDate()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "borrow_history", updateHistoryId))
                            {
                                Console.WriteLine("History ID not found. Please try again.");
                                Pause();
                                return;
                            }

                            var currentHistory = GetHistoryById(updateHistoryId);
                            DateTime newBorrowedDate;

                            while (true)
                            {
                                newBorrowedDate = DateFormat("Enter new Borrowed Date (yyyy-MM-dd): ");

                                if (newBorrowedDate.Date > DateTime.Today)
                                {
                                    Console.WriteLine("Borrowed date cannot be in the future. Please try again.");
                                    continue;
                                }

                                if (currentHistory.ReturnedDate != DBNull.Value)
                                {
                                    DateTime currentReturnedDate = Convert.ToDateTime(currentHistory.ReturnedDate);
                                    if (newBorrowedDate.Date >= currentReturnedDate.Date)
                                    {
                                        Console.WriteLine("Borrowed date must be before returned date. Please try again.");
                                        continue;
                                    }
                                }

                                break;
                            }

                            ExecuteHistoryUpdateProcedure(updateHistoryId, borrowedDate: newBorrowedDate);

                            Console.WriteLine("Borrowed date updated!");
                            ViewHistory();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }

                    void UpdateReturnedDate()
                    {
                        try
                        {
                            connection = new SqlConnection(connectionString);
                            connection.Open();

                            if (!ExistedId(connection, "borrow_history", updateHistoryId))
                            {
                                Console.WriteLine("History ID not found. Please try again.");
                                Pause();
                                return;
                            }

                            var currentHistory = GetHistoryById(updateHistoryId);
                            object returnedValue;

                            while (true)
                            {
                                returnedValue = OptionalDateFormat("Enter new Returned Date (yyyy-MM-dd), or press Enter to skip: ");

                                if (returnedValue == DBNull.Value)
                                    break;

                                DateTime returnedDate = (DateTime)returnedValue;

                                if (returnedDate.Date <= currentHistory.BorrowedDate.Date)
                                {
                                    Console.WriteLine("Returned date must be after borrowed date. Please try again.");
                                    continue;
                                }

                                break;
                            }

                            ExecuteHistoryUpdateProcedure(updateHistoryId, returnedDate: returnedValue);

                            Console.WriteLine("Returned date updated!");
                            ViewHistory();
                            Pause();
                        }
                        finally
                        {
                            if (connection != null && connection.State == ConnectionState.Open)
                                connection.Close();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    Pause(); return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Pause(); return;
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
                            Console.WriteLine("History ID not found! Please try again.");
                            continue;
                        }
                        break;
                    }

                    command = new SqlCommand("sp_DeleteHistory", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Id", deleteHistoryId);
                    int row = command.ExecuteNonQuery();
                    Console.WriteLine(row > 0 ? "1 row deleted!" : "ID not found!");
                    Pause(); return;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation canceled. Returning to history menu.");
                    Pause(); return;
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
                    Console.WriteLine("Input cannot be empty. Please try again.");
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
                if (DateTime.TryParseExact(input, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;
                Console.WriteLine("Invalid! Use format yyyy-MM-dd." + '\n');
            }
        }

        private static object OptionalDateFormat(string optionalDate)
        {
            while (true)
            {
                string input = CancelInput(optionalDate);
                if (string.IsNullOrWhiteSpace(input))
                    return DBNull.Value;

                if (DateTime.TryParseExact(input, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                    return result;

                Console.WriteLine("Invalid! Use format yyyy-MM-dd or press Enter to skip." + '\n');
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
                _ => throw new ArgumentException("Invalid table name.")
            };
        }
        private static (string Title, string Author, int Publish, string Category, int Total) GetBookById(int id)
        {
            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_GetBookById", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Id", id);

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
                throw new Exception("Member not found.");

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
                throw new Exception("History not found.");

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

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Title", title ?? current.Title);
            command.Parameters.AddWithValue("@Author", author ?? current.Author);
            command.Parameters.AddWithValue("@Publish", publish ?? current.Publish);
            command.Parameters.AddWithValue("@Category", category ?? current.Category);
            command.Parameters.AddWithValue("@Total", total ?? current.Total);

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

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Name", name ?? current.Name);
            command.Parameters.AddWithValue("@Dob", dob ?? current.Dob);
            command.Parameters.AddWithValue("@Member_Since", memberSince ?? current.MemberSince);

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

            using SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();

            using SqlCommand command = new SqlCommand("sp_UpdateHistory", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@BookID", bookId ?? current.BookId);
            command.Parameters.AddWithValue("@MemberID", memberId ?? current.MemberId);
            command.Parameters.AddWithValue("@Borrowed_Date", borrowedDate ?? current.BorrowedDate);
            command.Parameters.AddWithValue("@Returned_Date", returnedDate ?? current.ReturnedDate);

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
            return ExecuteCountProcedure("sp_CheckBookExists", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool MemberExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckMemberExists", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool HistoryExists(int id)
        {
            return ExecuteCountProcedure("sp_CheckHistoryExists", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool DuplicateBookExists(string title, string author)
        {
            return ExecuteCountProcedure("sp_CheckDuplicateBook", cmd =>
            {
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Author", author);
            }) > 0;
        }

        private static bool DuplicateBookExistsExcludeId(int id, string title, string author)
        {
            return ExecuteCountProcedure("sp_CheckDuplicateBookExcludeId", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Author", author);
            }) > 0;
        }

        private static bool BookUsedInHistory(int id)
        {
            return ExecuteCountProcedure("sp_CheckBookUsedInHistory", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool MemberUsedInHistory(int id)
        {
            return ExecuteCountProcedure("sp_CheckMemberUsedInHistory", cmd =>
            {
                cmd.Parameters.AddWithValue("@Id", id);
            }) > 0;
        }

        private static bool ActiveBorrowExists(int bookId, int memberId)
        {
            return ExecuteCountProcedure("sp_CheckActiveBorrow", cmd =>
            {
                cmd.Parameters.AddWithValue("@BookID", bookId);
                cmd.Parameters.AddWithValue("@MemberID", memberId);
            }) > 0;
        }
    }
}
