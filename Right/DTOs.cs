using Right.Model;
using System.ComponentModel.DataAnnotations;

namespace Right
{
    public record UserCreateDTO
    {
        [Required]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password), MinLength(5)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public Role Role { get; set; }

        [Required]
        public UserStateMod UserState { get; set; }

        public string? DescriptionGroup { get; set; }
        public string? DescriptionState { get; set; }
    }

    public record UserLoginDTO
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public record getUsersDTO
    {
        [Required]
        public string Login { get; set; }
    }

    public class TodoDTO
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public DateTime createDate { get; set; }

        public Role Role { get; set; }

        public string UserGroupDescription { get; set; }

        public UserStateMod State { get; set; }

        public string UserStateDescription { get; set; }

        public TodoDTO() { }
    }
}
