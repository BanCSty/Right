using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Right;
using Right.Data;
using Right.Model;
using System.Security.Claims;
using static Microsoft.AspNetCore.Http.Results;

var builder = WebApplication.CreateBuilder(args);

var isProduction = builder.Environment.IsProduction();


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/accessdenied";
    });


AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

builder.Services.AddEntityFrameworkNpgsql().AddDbContext<AppDbContext>
    (opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapPost("signup/{user}", async (AppDbContext db, UserCreateDTO user) =>
{
    if (user is null) return BadRequest("Must include user data");


    if (db.Users.Any(u => u.Login == user.Login))
        return Conflict("Invalid `login`: A user with this login address already exists.");

    if (user.Password != user.ConfirmPassword)
        return Conflict("Password mismatch!");

    if (db.Group.Any(g => g.Code == 0) && user.Role == 0)
        return Conflict("There is already a \"master\" in the db");


    var usr = new User(user);

    await db.Users.AddAsync(usr);
    await db.SaveChangesAsync();

    return Ok();
});

app.MapPost("/login/{user}", async (UserLoginDTO user, HttpContext context, AppDbContext db) =>
{
    var usr = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login &&
                    u.Password == HashMD5.hashPassword(user.Password));

    if (usr is null) return Results.Unauthorized(); //401 if user not found

    //Loading data into EF else it usr.Group and usr.Stat == null...
    var group = await db.Group.FirstOrDefaultAsync(u => u.ID == usr.Id);
    var state = await db.State.FirstOrDefaultAsync(u => u.ID == usr.Id);

    if (usr.UserState.Code == UserStateMod.Blocked) return Conflict($"The {usr.Login} is bolecked");

    var claims = new List<Claim>
    {
        new Claim(ClaimsIdentity.DefaultNameClaimType, usr.Login),
        new Claim(ClaimsIdentity.DefaultRoleClaimType, usr.UserGroup.Code == 0 ? "admin" : "user")
    };
    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    await context.SignInAsync(claimsPrincipal);
    return Ok();
});

app.MapGet("/accessdenied", async (HttpContext context) =>
{
    context.Response.StatusCode = 403;
    await context.Response.WriteAsync("Access Denied");
});

app.MapPost("/getUser/{user}/", [Authorize(Roles = "admin, user")] async ([FromBody] getUsersDTO user, AppDbContext db) =>
{
    var usr = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);

    if (usr is null) return BadRequest("User nor found!");

    //Loading data into EF else it usr.Group and usr.Stat == null...
    var state = await db.State.FirstOrDefaultAsync(s => usr.Id == s.ID);

    var group = await db.Group.FirstOrDefaultAsync(g => usr.Id == g.ID);

    var result = new TodoDTO
    {
        Id = usr.Id,
        Login = usr.Login,
        createDate = usr.createDate,
        Role = usr.UserGroup.Code,
        State = usr.UserState.Code,
        UserStateDescription = usr.UserState.Description,
        UserGroupDescription = usr.UserGroup.Description
    };
    return Ok(result);
});


app.MapGet("/getAllUser", [Authorize(Roles = "admin")] async (AppDbContext db) =>
{
    var result = from user in db.Users
                 join gp in db.Group on user.Id equals gp.ID
                 join st in db.State on user.Id equals st.ID
                 select new
                 {
                     Id = user.Id,
                     Login = user.Login,
                     Created = user.createDate,
                     Role = gp.Code,
                     RoleDescription = gp.Description,
                     State = st.Code,
                     UserStateDescription = st.Description
                 };

    return Ok(result);
});

app.MapPost("deleteUser", [Authorize(Roles = "admin")] async ([FromBody] getUsersDTO user, AppDbContext db) =>
{
    var usr = await db.Users.FirstOrDefaultAsync(u => u.Login == user.Login);

    if (usr != null)
    {
        var state = await db.State.FirstOrDefaultAsync(s => s.ID == usr.Id);
        state.Code = UserStateMod.Blocked;
        await db.SaveChangesAsync();

        return Ok();
    }
    return BadRequest();
});

app.Run();
