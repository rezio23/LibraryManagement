using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Globalization;
using System.Text;

namespace book_Managment
{
    internal class AdminDashboard
    {
        public static void Show()
        {
            // Database Connection
            string connectionString = "Server=DESKTOP-RHMPA0K\\SQLEXPRESS;Database=book_management;Trusted_Connection=True;TrustServerCertificate=True;";


            bool run = true;

            while (run)
            {
                Default();

                void Default()
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
                        case "4": run = false; break;
                        default: Console.WriteLine("Invalid choice. Please try again."); break;
                    }

                    // Book's Data -------------------------------------------------------------------

                    void Book()
                    {
                        Console.Clear();
                        Console.WriteLine();
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
                            case "5": Default(); break;
                            default: Console.WriteLine("Invalid choice. Please try again."); break;
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

                            //string query = "SELECT * FROM books_manage";
                            //using SqlCommand command = new SqlCommand(query, connection);
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

                                connection = new SqlConnection(connectionString);
                                connection.Open();

                                // Check for duplicate book
                                string duplicateQuery = "SELECT COUNT(*) FROM books_manage WHERE Title = @Title AND Author = @Author";
                                using (SqlCommand duplicatedCommand = new SqlCommand(duplicateQuery, connection))
                                {
                                    duplicatedCommand.Parameters.AddWithValue("@Title", bookTitle);
                                    duplicatedCommand.Parameters.AddWithValue("@Author", authorName);

                                    int dupCount = (int)duplicatedCommand.ExecuteScalar();
                                    if (dupCount > 0)
                                    {
                                        Console.WriteLine("This book already exists. Please try again.");
                                        Book();
                                        return;
                                    }
                                }

                                command = new SqlCommand("sp_InsertBook", connection);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@Title", bookTitle);
                                command.Parameters.AddWithValue("@Author", authorName);
                                command.Parameters.AddWithValue("@Publish", publishYear);
                                command.Parameters.AddWithValue("@Category", bookCategory);
                                command.Parameters.AddWithValue("@Total", totalBooks);

                                int row = command.ExecuteNonQuery();
                                Console.WriteLine("Inserted 1 data!");
                                ViewBook();
                                Book();
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Operation canceled. Returning to book menu.");
                                ViewBook();
                                Book();
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
                                        Console.WriteLine("Book's ID must be bigger than 0!");
                                        continue;
                                    }
                                    if (!ExistedId(connection, "books_manage", updateBookId))
                                    {
                                        Console.WriteLine("Book ID not found! Please try again.");
                                        continue;
                                    }
                                    break;
                                }


                                Console.WriteLine("1. Update Book's Title");
                                Console.WriteLine("2. Update Author's Name");
                                Console.WriteLine("3. Update Publish's Year");
                                Console.WriteLine("4. Update Book's Category");
                                Console.WriteLine("5. Update Book's Total");

                                switch (CancelInput("Select the number to update: "))
                                {
                                    case "1": UpdateBookTitle(); break;
                                    case "2": UpdateAuthorName(); break;
                                    case "3": UpdatePublishYear(); break;
                                    case "4": UpdateBookCategory(); break;
                                    case "5": UpdateBookTotal(); break;
                                    default:
                                        Console.WriteLine("Invalid choice. Please try again.");
                                        Book(); break;
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
                                            return;
                                        }

                                        string newBookTitle = BookTitleFormat("Enter new Book's Title: ");

                                        string getAuthorQuery = "SELECT Author FROM books_manage WHERE Id = @Id";
                                        using (SqlCommand getAuthorCommand = new SqlCommand(getAuthorQuery, connection))
                                        {
                                            getAuthorCommand.Parameters.AddWithValue("@Id", updateBookId);

                                            object? authorResult = getAuthorCommand.ExecuteScalar();
                                            if (authorResult == null)
                                            {
                                                Console.WriteLine("Book not found!");
                                                return;
                                            }

                                            string currentAuthor = authorResult.ToString()!;

                                            string duplicateQuery = "SELECT COUNT(*) FROM books_manage WHERE Title = @Title AND Author = @Author AND Id != @Id";
                                            using (SqlCommand dupCommand = new SqlCommand(duplicateQuery, connection))
                                            {
                                                dupCommand.Parameters.AddWithValue("@Title", newBookTitle);
                                                dupCommand.Parameters.AddWithValue("@Author", currentAuthor);
                                                dupCommand.Parameters.AddWithValue("@Id", updateBookId);

                                                int dupCount = (int)dupCommand.ExecuteScalar();
                                                if (dupCount > 0)
                                                {
                                                    Console.WriteLine("Another book with the same title and author already exists!");
                                                    return;
                                                }
                                            }

                                            string updateQuery = "UPDATE books_manage SET Title = @Title WHERE Id = @Id";
                                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                            {
                                                command.Parameters.AddWithValue("@Title", newBookTitle);
                                                command.Parameters.AddWithValue("@Id", updateBookId);

                                                int row = command.ExecuteNonQuery();
                                                Console.WriteLine(row > 0 ? "Book title updated!" : "ID not found!");
                                            }
                                        }

                                        ViewBook();
                                        Book();
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
                                            return;
                                        }

                                        string newAuthorName = NameFormat("Enter new Author's Name: ");

                                        string getTitleQuery = "SELECT Title FROM books_manage WHERE Id = @Id";
                                        using (SqlCommand getTitleCommand = new SqlCommand(getTitleQuery, connection))
                                        {
                                            getTitleCommand.Parameters.AddWithValue("@Id", updateBookId);

                                            object? titleResult = getTitleCommand.ExecuteScalar();
                                            if (titleResult == null)
                                            {
                                                Console.WriteLine("Book not found!");
                                                return;
                                            }

                                            string currentTitle = titleResult.ToString()!;

                                            string duplicateQuery = "SELECT COUNT(*) FROM books_manage WHERE Title = @Title AND Author = @Author AND Id != @Id";
                                            using (SqlCommand dupCommand = new SqlCommand(duplicateQuery, connection))
                                            {
                                                dupCommand.Parameters.AddWithValue("@Title", currentTitle);
                                                dupCommand.Parameters.AddWithValue("@Author", newAuthorName);
                                                dupCommand.Parameters.AddWithValue("@Id", updateBookId);

                                                int dupCount = (int)dupCommand.ExecuteScalar();
                                                if (dupCount > 0)
                                                {
                                                    Console.WriteLine("Another book with the same title and author already exists!");
                                                    return;
                                                }
                                            }

                                            string updateQuery = "UPDATE books_manage SET Author = @Author WHERE Id = @Id";
                                            using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                            {
                                                command.Parameters.AddWithValue("@Author", newAuthorName);
                                                command.Parameters.AddWithValue("@Id", updateBookId);

                                                int row = command.ExecuteNonQuery();
                                                Console.WriteLine(row > 0 ? "Author name updated!" : "ID not found!");
                                            }
                                        }

                                        ViewBook();
                                        Book();
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
                                            return;
                                        }

                                        int publishYear = YearFormat("Enter new Publish's Year: ");

                                        string updateQuery = "UPDATE books_manage SET Publish = @Publish WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Publish", publishYear);
                                            command.Parameters.AddWithValue("@Id", updateBookId);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Publish year updated!" : "ID not found!");
                                        }

                                        ViewBook();
                                        Book();
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
                                            return;
                                        }

                                        string bookCategory = NameFormat("Enter new Book's Category: ");

                                        string updateQuery = "UPDATE books_manage SET Category = @Category WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Category", bookCategory);
                                            command.Parameters.AddWithValue("@Id", updateBookId);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Book category updated!" : "ID not found!");
                                        }

                                        ViewBook();
                                        Book();
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
                                            return;
                                        }

                                        int totalBooks = NumberFormat("Enter new Book's Total: ");

                                        if (totalBooks < 0)
                                        {
                                            Console.WriteLine("Book's total cannot be negative!");
                                            return;
                                        }

                                        string updateQuery = "UPDATE books_manage SET Total = @Total WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Total", totalBooks);
                                            command.Parameters.AddWithValue("@Id", updateBookId);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Book total updated!" : "ID not found!");
                                        }

                                        ViewBook();
                                        Book();
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
                                ViewBook();
                                Book();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                                Book();
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
                                        Console.WriteLine("Book ID not found! Please try again.");
                                        continue;
                                    }
                                    break;
                                }

                                string checkQuery = "SELECT COUNT(*) FROM borrow_history WHERE BookID = @Id"; // Check if the book is existed
                                using SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                                checkCommand.Parameters.AddWithValue("@Id", deleteId);
                                int refCount = (int)checkCommand.ExecuteScalar();

                                if (refCount > 0)
                                {
                                    Console.WriteLine($"Cannot delete! This book has {refCount} borrow record(s) in history.");
                                    Console.WriteLine("Delete the borrow history first, then try again.");
                                    return;
                                }

                                command = new SqlCommand("sp_DeleteBook", connection);
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@Id", deleteId);

                                int row = command.ExecuteNonQuery();
                                Console.WriteLine(row > 0 ? "1 row deleted!" : "ID not found! Please try again!");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();

                                ViewBook();
                                Book();
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Operation canceled. Returning to book menu.");
                                ViewBook();
                                Book();
                            }
                        }
                    }
                    // Member's Data -------------------------------------------------------------------
                    void Member()
                    {
                        Console.Clear();
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
                            case "5": Default(); break;
                            default: Console.WriteLine("Invalid choice. Please try again."); break;
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

                                ViewMember();
                                Member();
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Operation canceled. Returning to member menu.");
                                ViewMember();
                                Member();
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

                                switch (CancelInput("Select the number to update: "))
                                {
                                    case "1": UpdateMemberName(); break;
                                    case "2": UpdateMemberDOB(); break;
                                    case "3": UpdateMemberShip(); break;
                                    default:
                                        Console.WriteLine("Invalid choice. Please try again.");
                                        Member(); break;
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
                                            return;
                                        }

                                        string updateMemberName = NameFormat("Enter new Member's Name: ");

                                        string updateQuery = "UPDATE members_manage SET Name = @Name WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Id", updateMemberId);
                                            command.Parameters.AddWithValue("@Name", updateMemberName);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Member name updated!" : "ID not found!");
                                        }

                                        ViewMember();
                                        Member();
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
                                            return;
                                        }

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

                                            string getMembershipQuery = "SELECT Member_Since FROM members_manage WHERE Id = @Id";
                                            using (SqlCommand getCommand = new SqlCommand(getMembershipQuery, connection))
                                            {
                                                getCommand.Parameters.AddWithValue("@Id", updateMemberId);

                                                object? membershipResult = getCommand.ExecuteScalar();
                                                if (membershipResult == null)
                                                {
                                                    Console.WriteLine("Member not found!");
                                                    return;
                                                }

                                                DateTime currentMembershipDate = Convert.ToDateTime(membershipResult);

                                                if (updateMemberDOB.Date >= currentMembershipDate.Date)
                                                {
                                                    Console.WriteLine("DOB must be before Membership date!");
                                                    continue;
                                                }
                                            }

                                            break;
                                        }

                                        string updateQuery = "UPDATE members_manage SET Dob = @Dob WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Id", updateMemberId);
                                            command.Parameters.AddWithValue("@Dob", updateMemberDOB);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Member DOB updated!" : "ID not found!");
                                        }

                                        ViewMember();
                                        Member();
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
                                            return;
                                        }

                                        DateTime currentDOB;
                                        string getDOBQuery = "SELECT Dob FROM members_manage WHERE Id = @Id";
                                        using (SqlCommand getCommand = new SqlCommand(getDOBQuery, connection))
                                        {
                                            getCommand.Parameters.AddWithValue("@Id", updateMemberId);

                                            object? dobResult = getCommand.ExecuteScalar();
                                            if (dobResult == null)
                                            {
                                                Console.WriteLine("Member not found!");
                                                return;
                                            }

                                            currentDOB = Convert.ToDateTime(dobResult);
                                        }

                                        DateTime updateMemberShip;
                                        while (true)
                                        {
                                            updateMemberShip = DateFormat("Enter Member's Membership Year (yyyy-MM-dd): ");

                                            if (updateMemberShip.Date > DateTime.Today)
                                            {
                                                Console.WriteLine("Membership year cannot be in the future!");
                                                continue;
                                            }

                                            if (updateMemberShip.Date <= currentDOB.Date)
                                            {
                                                Console.WriteLine("Membership date must be after DOB!");
                                                continue;
                                            }

                                            break;
                                        }

                                        string updateQuery = "UPDATE members_manage SET Member_Since = @Member_Since WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Id", updateMemberId);
                                            command.Parameters.AddWithValue("@Member_Since", updateMemberShip);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Membership date updated!" : "ID not found!");
                                        }

                                        ViewMember();
                                        Member();
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
                                ViewMember();
                                Member();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                                Member();
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

                                string checkQuery = "SELECT COUNT(*) FROM borrow_history WHERE MemberID = @Id";
                                using SqlCommand checkCommand = new SqlCommand(checkQuery, connection);
                                checkCommand.Parameters.AddWithValue("@Id", deleteMemberId);
                                int refCount = (int)checkCommand.ExecuteScalar();

                                if (refCount > 0)
                                {
                                    Console.WriteLine($"Cannot delete! This member has {refCount} borrow record(s) in history.");
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

                                ViewMember();
                                Member();
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Operation canceled. Returning to member menu.");
                                ViewMember();
                                Member();
                            }
                        }
                    }

                    // History's Data -------------------------------------------------------------------
                    void History()
                    {
                        Console.Clear();
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
                            case "5": Default(); break;
                            default: Console.WriteLine("Invalid choice. Please try again."); break;
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

                                string activeBorrowQuery = @"SELECT COUNT(*) FROM borrow_history WHERE BookID = @BookID AND MemberID = @MemberID AND Returned_Date IS NULL";
                                using (SqlCommand activeCheck = new SqlCommand(activeBorrowQuery, connection))
                                {
                                    activeCheck.Parameters.AddWithValue("@BookID", bookId);
                                    activeCheck.Parameters.AddWithValue("@MemberID", memberId);

                                    int activeCount = (int)activeCheck.ExecuteScalar();
                                    if (activeCount > 0)
                                    {
                                        Console.WriteLine("This member is already borrowing this book and hasn't returned it yet!");
                                        return;
                                    }
                                }

                                command.Parameters.AddWithValue("@BookID", bookId);
                                command.Parameters.AddWithValue("@MemberID", memberId);
                                command.Parameters.AddWithValue("@Borrowed_Date", borrowedDate);
                                command.Parameters.AddWithValue("@Returned_Date", returnedValue);
                                int row = command.ExecuteNonQuery();
                                Console.WriteLine("1 row inserted!");
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();

                                ViewHistory();
                                History();
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Operation canceled. Returning to history menu.");
                                ViewHistory();
                                History();
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
                                Console.Write("Select the number to update: ");

                                switch (CancelInput("Select the number to update: "))
                                {
                                    case "1": UpdateHistoryBookId(); break;
                                    case "2": UpdateHistoryMemberId(); break;
                                    case "3": UpdateBorrowedDate(); break;
                                    case "4": UpdateReturnedDate(); break;
                                    default:
                                        Console.WriteLine("Invalid choice. Please try again.");
                                        History(); break;
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
                                            return;
                                        }

                                        int newBookId = NumberFormat("Enter new Book's ID: ");

                                        if (!ExistedId(connection, "books_manage", newBookId))
                                        {
                                            Console.WriteLine("Book ID not found. Please try again.");
                                            return;
                                        }

                                        string updateQuery = "UPDATE borrow_history SET BookID = @BookID WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Id", updateHistoryId);
                                            command.Parameters.AddWithValue("@BookID", newBookId);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Book ID updated!" : "ID not found!");
                                        }

                                        ViewHistory();
                                        History();
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
                                            return;
                                        }

                                        int newMemberId = NumberFormat("Enter new Member's ID: ");

                                        if (!ExistedId(connection, "members_manage", newMemberId))
                                        {
                                            Console.WriteLine("Member ID not found. Please try again.");
                                            return;
                                        }

                                        string updateQuery = "UPDATE borrow_history SET MemberID = @MemberID WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Id", updateHistoryId);
                                            command.Parameters.AddWithValue("@MemberID", newMemberId);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Member ID updated!" : "ID not found!");
                                        }

                                        ViewHistory();
                                        History();
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
                                            return;
                                        }

                                        object? currentReturnedDateObj;
                                        string getReturnedDateQuery = "SELECT Returned_Date FROM borrow_history WHERE Id = @Id";
                                        using (SqlCommand getCommand = new SqlCommand(getReturnedDateQuery, connection))
                                        {
                                            getCommand.Parameters.AddWithValue("@Id", updateHistoryId);
                                            currentReturnedDateObj = getCommand.ExecuteScalar();
                                        }

                                        DateTime newBorrowedDate;
                                        while (true)
                                        {
                                            newBorrowedDate = DateFormat("Enter new Borrowed Date (yyyy-MM-dd): ");

                                            if (newBorrowedDate.Date > DateTime.Today)
                                            {
                                                Console.WriteLine("Borrowed date cannot be in the future. Please try again.");
                                                continue;
                                            }

                                            if (currentReturnedDateObj != null && currentReturnedDateObj != DBNull.Value)
                                            {
                                                DateTime currentReturnedDate = Convert.ToDateTime(currentReturnedDateObj);
                                                if (newBorrowedDate.Date >= currentReturnedDate.Date)
                                                {
                                                    Console.WriteLine("Borrowed date must be before returned date. Please try again.");
                                                    continue;
                                                }
                                            }

                                            break;
                                        }

                                        string updateQuery = "UPDATE borrow_history SET Borrowed_Date = @Borrowed_Date WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Id", updateHistoryId);
                                            command.Parameters.AddWithValue("@Borrowed_Date", newBorrowedDate);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Borrowed date updated!" : "ID not found!");
                                        }

                                        ViewHistory();
                                        History();
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
                                            return;
                                        }

                                        DateTime currentBorrowedDate;
                                        string getBorrowedDateQuery = "SELECT Borrowed_Date FROM borrow_history WHERE Id = @Id";
                                        using (SqlCommand getCommand = new SqlCommand(getBorrowedDateQuery, connection))
                                        {
                                            getCommand.Parameters.AddWithValue("@Id", updateHistoryId);

                                            object? borrowedResult = getCommand.ExecuteScalar();
                                            if (borrowedResult == null || borrowedResult == DBNull.Value)
                                            {
                                                Console.WriteLine("Borrowed date not found.");
                                                return;
                                            }

                                            currentBorrowedDate = Convert.ToDateTime(borrowedResult);
                                        }

                                        object returnedValue;
                                        while (true)
                                        {
                                            returnedValue = OptionalDateFormat("Enter new Returned Date (yyyy-MM-dd), or press Enter to skip: ");

                                            if (returnedValue == DBNull.Value)
                                                break;

                                            DateTime returnedDate = (DateTime)returnedValue;

                                            if (returnedDate.Date <= currentBorrowedDate.Date)
                                            {
                                                Console.WriteLine("Returned date must be after borrowed date. Please try again.");
                                                continue;
                                            }

                                            break;
                                        }

                                        string updateQuery = "UPDATE borrow_history SET Returned_Date = @Returned_Date WHERE Id = @Id";
                                        using (SqlCommand command = new SqlCommand(updateQuery, connection))
                                        {
                                            command.Parameters.AddWithValue("@Id", updateHistoryId);
                                            command.Parameters.AddWithValue("@Returned_Date", returnedValue);

                                            int row = command.ExecuteNonQuery();
                                            Console.WriteLine(row > 0 ? "Returned date updated!" : "ID not found!");
                                        }

                                        ViewHistory();
                                        History();
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
                                ViewHistory();
                                History();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                                History();
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
                                Console.WriteLine("Press any key to continue...");
                                Console.ReadKey();

                                ViewHistory();
                                History();
                            }
                            catch (OperationCanceledException)
                            {
                                Console.WriteLine("Operation canceled. Returning to history menu.");
                                ViewHistory();
                                History();
                            }
                        }
                    }

                    static void CancelHint()
                    {
                        Console.WriteLine("Press Escape key to stop and go back.");
                        Console.WriteLine();
                    }

                    static string CancelInput(string inputTxt)
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

                    static string BookTitleFormat(string title)
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

                    static string NameFormat(string name)
                    {
                        while (true)
                        {
                            string input = CancelInput(name);

                            // Catch if it was empty
                            if (string.IsNullOrWhiteSpace(input))
                            {
                                Console.WriteLine("Input cannot be empty. Please try again.");
                                continue;
                            }

                            // Catch if it contains non-letter characters
                            if (!input.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                            {
                                Console.WriteLine("Input must contain only letters and spaces. Please try again.");
                                continue;
                            }

                            return input;
                        }
                    }

                    static int NumberFormat(string number)
                    {
                        while (true)
                        {
                            string input = CancelInput(number);

                            if (!int.TryParse(input, out int result))
                            {
                                Console.WriteLine("Invalid! Please enter a number.");
                                continue;
                            }

                            if (result < 0)
                            {
                                Console.WriteLine("Number cannot be negative. Please try again.");
                                continue;
                            }

                            return result;
                        }
                    }

                    static int YearFormat(string number)
                    {
                        while (true)
                        {
                            string input = CancelInput(number);

                            if (!int.TryParse(input, out int result))
                            {
                                Console.WriteLine("Invalid! Please enter a number.");
                                continue;
                            }

                            if (result < 0)
                            {
                                Console.WriteLine("Number cannot be negative. Please try again.");
                                continue;
                            }

                            if (result > DateTime.Today.Year)
                            {
                                Console.WriteLine("Year cannot be in the future. Please try again.");
                                continue;
                            }

                            if (result < 1000)
                            {
                                Console.WriteLine("Year must be a 4-digit number. Please try again.");
                                continue;
                            }

                            return result;
                        }
                    }

                    static DateTime DateFormat(string date)
                    {
                        while (true)
                        {
                            string input = CancelInput(date);
                            if (DateTime.TryParseExact(input, "yyyy-MM-dd",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                                return result;
                            Console.WriteLine("Invalid! Use format yyyy-MM-dd.");
                        }
                    }

                    static object OptionalDateFormat(string optionalDate)
                    {
                        while (true)
                        {
                            string input = CancelInput(optionalDate);

                            if (string.IsNullOrWhiteSpace(input))
                                return DBNull.Value;

                            if (DateTime.TryParseExact(input, "yyyy-MM-dd",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                                return result;

                            Console.WriteLine("Invalid! Use format yyyy-MM-dd or press Enter to skip.");
                        }
                    }

                    static bool ExistedId(SqlConnection connection, string tableName, int id)
                    {
                        string query = $"SELECT COUNT(*) FROM {tableName} WHERE Id = @Id";

                        using SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Id", id);

                        return (int)command.ExecuteScalar() > 0;
                    }
                }
            }
        }
    }
}