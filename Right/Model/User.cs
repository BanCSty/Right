using System.Security.Cryptography;
using System.Text;

namespace Right.Model
{
    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public DateTime createDate { get; set; }

        public string Password { get; set; }

        public UserGroup UserGroup { get; set; }

        public UserState UserState { get; set; }

        public User() { }
        public User(UserCreateDTO dto)
        {
            Id = Guid.NewGuid();
            Login = dto.Login;
            createDate = DateTime.Now;
            Password = HashMD5.hashPassword(dto.Password);
            UserGroup = new UserGroup(Id, dto.Role, dto.DescriptionGroup);
            UserState = new UserState(Id, UserStateMod.Active, dto.DescriptionState);
        }
    }

    public class HashMD5
    {
        public static string hashPassword(string password)
        {
            MD5 md5 = MD5.Create();

            byte[] b = Encoding.ASCII.GetBytes(password);
            byte[] hash = md5.ComputeHash(b);

            StringBuilder sb = new StringBuilder();
            foreach (var item in hash)
                sb.Append(item.ToString("X2"));

            return Convert.ToString(sb);
        }
    }

    public class UserGroup
    {
        public Guid ID { get; set; }
        public Role Code { get; set; }
        public string? Description { get; set; }

        public UserGroup() { }

        public UserGroup(Guid id, Role role, string description)
        {
            ID = id;
            Code = role;
            Description = description;
        }

        public virtual ICollection<User> Users { get; set; }
    }

    public class UserState
    {
        public Guid ID { get; set; }
        public UserStateMod Code { get; set; }
        public string? Description { get; set; }

        public UserState() { }

        public UserState(Guid id, UserStateMod state, string description)
        {
            ID = id;
            Code = state;
            Description = description;
        }

        public virtual ICollection<User> Users { get; set; }
    }

    public enum Role
    {
        Admin,
        User
    }
    public enum UserStateMod
    {
        Blocked,
        Active
    }
}
